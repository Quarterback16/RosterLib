using System;
using System.IO;
using System.Xml;
using RosterLib.Models;

namespace RosterLib
{
	public class HillenMaster : XmlCache
	{
		/// <summary>
		///   HillenMaster will keep a hash table of Power Ratings
		///      so they dont get re-created all the time.
		///   Key is season + week + TeamCode
		///     2012:01:SF  30.5
		/// </summary>
		public HillenMaster( string name, string fileName )
			: base( name )
		{
			Filename = string.Format( "{0}XML\\{1}", Utility.OutputDirectory(), fileName );
			LoadCache();
			IsDirty = false;  //  we r starting from the xml
		}

		private void LoadCache()
		{
			try
			{
				XmlDoc = new XmlDocument();
				XmlDoc.Load( Filename );
				var listNode = XmlDoc.ChildNodes[ 2 ];
				foreach ( XmlNode node in listNode.ChildNodes )
					AddXmlRating( node );
			}
			catch ( IOException e )
			{
				Utility.Announce( string.Format( "Unable to open {1} xmlfile - {0}", e.Message, Filename ) );
			}
		}

		private void AddXmlRating( XmlNode node )
		{
			PutRating( new HillenPowerRating( node ) );
		}

		public override decimal GetStat( string theKey )
		{
			var season = theKey.Substring( 0, 4 );
			var week = theKey.Substring( 5, 2 );
			var teamCode = theKey.Substring( 8, 2 );

			var stat = new HillenPowerRating
			{
				Season = season,
				Week = week,
				TeamCode = teamCode,
				Quantity = 0.0M
			};

#if DEBUG
			Utility.Announce( string.Format( "HillenMaster:Getting Stat {0}", stat.FormatKey() ) );
#endif

			var key = stat.FormatKey();
			if ( TheHt.ContainsKey( key ) )
			{
				stat = (HillenPowerRating) TheHt[ key ];
				CacheHits++;
			}
			else
			{
				//  new it up
#if DEBUG
				Utility.Announce( string.Format( "HillenMaster:Instantiating Stat {0}", stat.FormatKey() ) );
#endif
				PutRating( stat );
				IsDirty = true;
				CacheMisses++;
			}
			return stat.Quantity;
		}

		public void PutRating( HillenPowerRating stat )
		{
			if ( stat.Quantity == 0.0M ) return;

			IsDirty = true;
			if ( TheHt.ContainsKey( stat.FormatKey() ) )
			{
				TheHt[ stat.FormatKey() ] = stat;
#if DEBUG
				//Utility.Announce( string.Format( "HillenMaster:Putting Stat {0}", stat.FormatKey() ) );
#endif
				return;
			}
			TheHt.Add( stat.FormatKey(), stat );
#if DEBUG
			//Utility.Announce( string.Format( "HillenMaster:Adding Stat {0}", stat.FormatKey() ) );
#endif
		}

		#region  Persistence

		public void Dump2Xml()
		{
			if ( ( TheHt.Count <= 0 ) || !IsDirty ) return;

			Utility.EnsureDirectory( Filename );  //  will create the dir if its not there

			var writer = new XmlTextWriter( Filename, null );

			writer.WriteStartDocument();
			writer.WriteComment( "Comments: " + Name );
			writer.WriteStartElement( "stat-list" );

			var myEnumerator = TheHt.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var t = (HillenPowerRating) myEnumerator.Value;
				WriteStatNode( writer, t );
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Close();
			Utility.Announce( Filename + " created" );
		}

		private static void WriteStatNode( XmlWriter writer, HillenPowerRating stat )
		{
			writer.WriteStartElement( "stat" );
			writer.WriteAttributeString( "season", stat.Season );
			writer.WriteAttributeString( "week", stat.Week );
			writer.WriteAttributeString( "team", stat.TeamCode );
			writer.WriteAttributeString( "qty", stat.Quantity.ToString() );
			writer.WriteEndElement();
		}

		#endregion


		public void Calculate( string season, string week )
		{
			if ( week.Equals( "01" ) )
			{
				SetupSeason( season );
				Dump2Xml();
				return;
			}

			var hp = new HillinPredictor();

			var theSeason = new NflSeason( season );

			foreach ( var team in theSeason.TeamList )
			{
				//  get teams game for the week
				Utility.Announce( string.Format( "  Doing {0}", team.TeamCode ) );
				var upcomingWeek = new NFLWeek( season, week );
				var previousWeek = upcomingWeek.PreviousWeek( upcomingWeek, loadgames:false, regularSeasonGamesOnly:true );
				var prevWeek = string.Format( "{0:0#}", previousWeek.WeekNo );
				var oldPowerRating = team.GetPowerRating( prevWeek );

				if ( oldPowerRating == 0 )
				{
					return;
				}
				var newRating = new HillenPowerRating
				{
					Season = season,
					TeamCode = team.TeamCode,
					Week = week,
					Quantity = oldPowerRating
				};

				var game = Utility.GetGameFor( season, prevWeek, team.TeamCode );

				if ( game.GameDate != new DateTime( 1, 1, 1 ) )
				{
					var predictedResult = hp.PredictGame( game, null, game.GameDate );
					var predictedMarginForTeam = predictedResult.MarginForTeam( team.TeamCode );
					//Utility.Announce( string.Format( "Predicted Margin for {0} is {1}", TeamCode, predictedMarginForTeam ) );
					var actualMarginForTeam = game.Result.MarginForTeam( team.TeamCode );
					//Utility.Announce( string.Format( "   Result of {2} means Actual Margin for {0} is {1}",
					var newPowerRating = AdjustedPower( oldPowerRating, predictedMarginForTeam, actualMarginForTeam );
					newRating.Quantity = newPowerRating;
				}
				PutRating( newRating );
			}
			Dump2Xml();
		}

		private static decimal AdjustedPower( decimal teamsPowerRating, decimal predictedMarginForTeam,
			decimal actualMarginForTeam )
		{
			var difference = actualMarginForTeam - predictedMarginForTeam;
			var absDiff = Math.Abs( difference );

			//  look up modifier
			var modifier = 0.0M;
			if ( absDiff > 14.5M )
				modifier = 3.0M;
			else if ( absDiff > 9.5M )
				modifier = 2.0M;
			else if ( absDiff > 4.5M )
				modifier = 1.0M;

			if ( difference < 0 )
			{
				//Utility.Announce( string.Format( "Modifier for {0} is {1}",
				//   TeamCode, -modifier ) );
				return teamsPowerRating -= modifier;
			}
			//Utility.Announce( string.Format( "Modifier for {0} is {1}",
			//   TeamCode, modifier ) );
			return teamsPowerRating += modifier;
		}

		private void SetupSeason( string season )
		{
			if ( season.Equals( "2012" ) )
			{
				PutRating( new HillenPowerRating( season, "01", "AC", 19.5M ) );
				PutRating( new HillenPowerRating( season, "01", "AF", 24.0M ) );
				PutRating( new HillenPowerRating( season, "01", "BR", 26.5M ) );
				PutRating( new HillenPowerRating( season, "01", "BB", 22.5M ) );
				PutRating( new HillenPowerRating( season, "01", "CP", 21.0M ) );
				PutRating( new HillenPowerRating( season, "01", "CH", 22.5M ) );
				PutRating( new HillenPowerRating( season, "01", "CI", 21.0M ) );
				PutRating( new HillenPowerRating( season, "01", "CL", 15.5M ) );
				PutRating( new HillenPowerRating( season, "01", "DC", 21.0M ) );
				PutRating( new HillenPowerRating( season, "01", "DB", 21.0M ) );
				PutRating( new HillenPowerRating( season, "01", "DL", 24.0M ) );
				PutRating( new HillenPowerRating( season, "01", "GB", 38.0M ) );
				PutRating( new HillenPowerRating( season, "01", "HT", 24.0M ) );
				PutRating( new HillenPowerRating( season, "01", "IC", 13.0M ) );
				PutRating( new HillenPowerRating( season, "01", "JJ", 15.5M ) );
				PutRating( new HillenPowerRating( season, "01", "KC", 22.5M ) );
				PutRating( new HillenPowerRating( season, "01", "MD", 18.0M ) );
				PutRating( new HillenPowerRating( season, "01", "MV", 15.5M ) );
				PutRating( new HillenPowerRating( season, "01", "NE", 31.5M ) );
				PutRating( new HillenPowerRating( season, "01", "NO", 24.0M ) );
				PutRating( new HillenPowerRating( season, "01", "NG", 21.0M ) );
				PutRating( new HillenPowerRating( season, "01", "NJ", 22.5M ) );
				PutRating( new HillenPowerRating( season, "01", "OR", 19.5M ) );
				PutRating( new HillenPowerRating( season, "01", "PE", 26.5M ) );
				PutRating( new HillenPowerRating( season, "01", "PS", 22.5M ) );
				PutRating( new HillenPowerRating( season, "01", "SD", 22.5M ) );
				PutRating( new HillenPowerRating( season, "01", "SF", 31.5M ) );
				PutRating( new HillenPowerRating( season, "01", "SL", 15.5M ) );
				PutRating( new HillenPowerRating( season, "01", "SS", 19.5M ) );
				PutRating( new HillenPowerRating( season, "01", "TB", 15.5M ) );
				PutRating( new HillenPowerRating( season, "01", "TT", 22.5M ) );
				PutRating( new HillenPowerRating( season, "01", "WR", 19.5M ) );
			}
		}

		public void Calculate( string season )
		{
			var theSeason = new NflSeason( season );
			theSeason.LoadRegularWeeksToDate();
			foreach ( var week in theSeason.RegularWeeks )
			{
				var theWeek = string.Format( "{0:0#}", week.WeekNo );
				Utility.Announce( string.Format( "Doing week {0} of {1}", week.WeekNo, season ) );
				Calculate( theSeason.Year, theWeek );
			}
		}
	}


}


