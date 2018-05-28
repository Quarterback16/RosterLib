using System.IO;
using System.Linq;
using System.Xml;

namespace RosterLib
{
	/// <summary>
	///   GridStatsMaster will keep a hash table of GridStats Score Output so they dont get re-created all the time.
	///   Key is season + week + PlayerId
	///     2012:01:AKERDA01  3
	/// </summary>
	public class GridStatsMaster : XmlCache
	{
		public GridStatsMaster( string name, string fileName ) : base( name )
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
			PutStat( new GridStatsOutput( node ) );
		}

		public override decimal GetStat( string theKey )
		{
			var season = theKey.Substring( 0, 4 );
			var week = theKey.Substring( 5, 2 );
			var playerId = theKey.Substring( 8, 8 );

			var stat = new GridStatsOutput
			{
				Season = season,
				Week = week,
				PlayerId = playerId,
				Quantity = 0.0M
			};

#if DEBUG
			Utility.Announce( string.Format( "GridStatsMaster:Getting Stat {0}", stat.FormatKey() ) );
#endif

			var key = stat.FormatKey();
			if ( TheHt.ContainsKey( key ) )
			{
				stat = (GridStatsOutput) TheHt[ key ];
				CacheHits++;
			}
			else
			{
				//  new it up
#if DEBUG
				Utility.Announce( string.Format( "GridStatsMaster:Instantiating Stat {0}", stat.FormatKey() ) );
#endif
				PutStat( stat );
				IsDirty = true;
				CacheMisses++;
			}
			return stat.Quantity;
		}

		public void PutStat( GridStatsOutput stat )
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
				var t = (GridStatsOutput) myEnumerator.Value;
				WriteStatNode( writer, t );
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Close();
			Utility.Announce( Filename + " created" );
		}

		private static void WriteStatNode( XmlWriter writer, GridStatsOutput stat )
		{
			writer.WriteStartElement( "stat" );
			writer.WriteAttributeString( "season", stat.Season );
			writer.WriteAttributeString( "week", stat.Week );
			writer.WriteAttributeString( "id", stat.PlayerId );
			writer.WriteAttributeString( "qty", stat.Quantity.ToString() );
			writer.WriteAttributeString( "opp", stat.Opponent );
			writer.WriteEndElement();
		}

		#endregion

		public void Calculate( string season, string week )
		{
			var theWeek = new NFLWeek( season, week );
			theWeek.LoadGameList();
			foreach ( var nflStat in theWeek.GameList().Cast<NFLGame>()
				.Select( game => game.GenerateGridStatsOutput() ).SelectMany( statList => statList ) )
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
