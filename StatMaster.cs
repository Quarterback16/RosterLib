using RosterLib.Models;
using System.IO;
using System.Linq;
using System.Xml;

namespace RosterLib
{
	public class StatMaster : XmlCache
	{
		/// <summary>
		///   StatMaster will keep a hash table of Stats so they dont get re-created all the time.
		///   Key is season + teamcode + week + StatType
		///     2012:01:SF:K  2.5
		/// </summary>
		public StatMaster( string name, string fileName ) : base( name )
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
					AddXmlStat( node );
			}
			catch ( IOException e )
			{
				Utility.Announce( string.Format( "Unable to open {1} xmlfile - {0}", e.Message, Filename ) );
			}
		}

		private void AddXmlStat( XmlNode node )
		{
			PutStat( new NflStat( node ) );
		}

		public NflStat GetStat( string season, string week, string teamCode, string statType )
		{
			var stat = new NflStat
			{
				Season = season,
				TeamCode = teamCode,
				Week = week,
				StatType = statType,
				Quantity = 0.0M
			};

#if DEBUG
			Utility.Announce( $"StatMaster:Getting Stat {stat.FormatKey()}" );
#endif

			var key = stat.FormatKey();
			if ( TheHt.ContainsKey( key ) )
			{
				stat = ( NflStat ) TheHt[ key ];
				CacheHits++;
			}
			else
			{
				//  new it up
#if DEBUG
				Utility.Announce( $"StatMaster:Instantiating Stat {stat.FormatKey()}" );
#endif
				PutStat( stat );
				IsDirty = true;
				CacheMisses++;
			}
			return stat;
		}

		public void PutStat( NflStat stat )
		{
			if ( stat.Quantity == 0.0M ) return;

			IsDirty = true;
			if ( TheHt.ContainsKey( stat.FormatKey() ) )
			{
				TheHt[ stat.FormatKey() ] = stat;
				return;
			}
			TheHt.Add( stat.FormatKey(), stat );
		}

		#region Persistence

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
				var t = ( NflStat ) myEnumerator.Value;
				WriteStatNode( writer, t );
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Close();
			Utility.Announce( Filename + " created" );
		}

		private static void WriteStatNode( XmlWriter writer, NflStat stat )
		{
			writer.WriteStartElement( "stat" );
			writer.WriteAttributeString( "season", stat.Season );
			writer.WriteAttributeString( "week", stat.Week );
			writer.WriteAttributeString( "team", stat.TeamCode );
			writer.WriteAttributeString( "statType", stat.StatType );
			writer.WriteAttributeString( "qty", stat.Quantity.ToString() );
			writer.WriteAttributeString( "opp", stat.Opponent );
			writer.WriteEndElement();
		}

		#endregion Persistence

		public void Calculate( string season, string week )
		{
			var theWeek = new NFLWeek( season, week );
			if ( !theWeek.HasPassed() ) return;

			theWeek.LoadGameList();
			foreach ( var nflStat in theWeek.GameList().Cast<NFLGame>()
				.Select( game => game.GenerateStats() ).SelectMany( statList => statList ) )
				PutStat( nflStat );
		}

		public void Calculate( string season )
		{
			var theSeason = new NflSeason( season );
			theSeason.LoadRegularWeeksToDate();
			foreach ( var week in theSeason.RegularWeeks )
				Calculate( theSeason.Year, week.Week );
		}
	}
}