using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace RosterLib
{
	/// <summary>
	/// Summary description for NflSeason.
	/// </summary>
	public class NflSeason
	{
		public Logger Logger { get; set; }

		private int _mWeeks = 21;

		private ArrayList _mTeamKeyList;
		public readonly List<NflTeam> _mTeamList;
		private readonly List<NflConference> _mConferenceList;

		public List<NFLGame> GameList { get; set; }

		public List<NFLWeek> RegularWeeks { get; set; }

		public int WeeksIntheRegularSeason { get; set; }

		public NflSeason( string yearIn )
		{
			WeeksIntheRegularSeason = 17;
			Year = yearIn;
			TeamKeyList = new ArrayList();
			_mTeamList = new List<NflTeam>();
			GameList = new List<NFLGame>();
			_mConferenceList = new List<NflConference>();

			LoadTeamList( yearIn );
			LoadGameList( yearIn );
			LoadDivisionList( yearIn );
		}

		public NflSeason( string yearIn, bool teamsOnly )
		{
			WeeksIntheRegularSeason = 17;
			Year = yearIn;
			TeamKeyList = new ArrayList();
			_mTeamList = new List<NflTeam>();

			LoadTeamList( yearIn );
			if ( !teamsOnly )
			{
				GameList = new List<NFLGame>();
				_mConferenceList = new List<NflConference>();
				LoadGameList( yearIn );
				LoadDivisionList( yearIn );
			}
		}

		public int CurrentWeek( DateTime dateInFocus )
		{
			//  Sort games by date

			var currentWeek = 0;
			foreach ( var game in GameList )
			{
				if ( game.GameDate <= dateInFocus ) continue;
				currentWeek = game.WeekNo;
				break;
			}
			return currentWeek;
		}

		public NflSeason( string yearIn, bool loadGames, bool loadDivisions )
		{
			WeeksIntheRegularSeason = 17;
			Year = yearIn;
			TeamKeyList = new ArrayList();
			_mTeamList = new List<NflTeam>();

			LoadTeamList( yearIn );

			if ( loadGames )
			{
				GameList = new List<NFLGame>();
				LoadGameList( yearIn );
			}
			if ( !loadDivisions ) return;
			_mConferenceList = new List<NflConference>();
			LoadDivisionList( yearIn );
		}

		public void DumpLineups( string week )
		{
			foreach ( var t in _mTeamList )
			{
				var l = t.Lineup( Year, week );
				l.DumpKeyPlayers();
				if ( l.MissingKeys > 0 )
					Logger.Trace( $"{t.NameOut()} is missing {l.MissingKeys} key players" );
			}
		}

		#region Load

		private void LoadDivisionList( string yearIn )
		{
			Logger.Trace( $"LoadDivisionList: Loading {yearIn} division List..." );
			var nfc = new NflConference( "NFC", yearIn );
			ConferenceList.Add( nfc );
			LoadNfc( nfc );
			var afc = new NflConference( "AFC", yearIn );
			ConferenceList.Add( afc );
			LoadAfc( afc );
		}

		public void LoadNfc( NflConference conf )
		{
			Logger.Trace( "NewRosterReport:LoadNFC Loading NFC" );
			conf.AddDiv( "East", "A" );
			conf.AddDiv( "North", "B" );
			conf.AddDiv( "South", "C" );
			conf.AddDiv( "West", "D" );
			Logger.Trace( "NewRosterReport:LoadNFC Loading NFC - finished" );
		}

		public void LoadAfc( NflConference conf )
		{
			Logger.Trace( "NewRosterReport:LoadAFC Loading AFC" );

			conf.AddDiv( "East", "E" );
			conf.AddDiv( "North", "F" );
			conf.AddDiv( "South", "G" );
			conf.AddDiv( "West", "H" );

			Logger.Trace( "NewRosterReport:LoadAFC Loading AFC - finished" );
		}

		public void LoadRegularWeeks()
		{
			RegularWeeks = new List<NFLWeek>();
			for ( var i = 1; i <= WeeksIntheRegularSeason; i++ )
			{
				var week = new NFLWeek( Year, i );
				RegularWeeks.Add( week );
			}
		}

		public void LoadRegularWeeksToDate()
		{
			RegularWeeks = new List<NFLWeek>();
			for ( var i = 1; i <= WeeksIntheRegularSeason; i++ )
			{
				var week = new NFLWeek( Year, i );
				var gameList = week.GameList();
				foreach ( NFLGame game in gameList )
				{
					if ( !game.Played() )
						break;
				}
				RegularWeeks.Add( week );
			}
		}

		public void LoadGames()
		{
			LoadGameList( Year );
		}

		private void LoadGameList( string yearIn )
		{
			Logger.Trace( string.Format( "LoadGameList: Loading {0} game List...", yearIn ) );

			if ( GameList == null ) GameList = new List<NFLGame>();
			var gameDt = Utility.TflWs.GetSeasonDt( yearIn );
			foreach ( var g in from DataRow dr in gameDt.Rows select new NFLGame( dr ) )
				GameList.Add( g );

			Logger.Trace( $"LoadGameList: Loaded {GameList.Count} games." );
		}

		private void LoadTeamList( string yearIn )
		{
			Logger = LogManager.GetCurrentClassLogger();

			Logger.Trace( $"LoadTeamList: Loading {yearIn} team List" );

			var ds = Utility.TflWs.GetTeams( yearIn, "" );
			var teams = ds.Tables[ "team" ];
			foreach ( DataRow dr in teams.Rows )
			{
				TeamKeyList.Add( yearIn + dr[ "TEAMID" ] );
				TeamList.Add( new NflTeam( dr[ "TEAMID" ].ToString(), yearIn ) );
			}

			Logger.Trace( $"LoadTeamList: Loaded {TeamList.Count} teams." );
		}

		#endregion Load

		#region XML stuff

		public NflSeason( XmlNode node )
		{
			TeamKeyList = new ArrayList();
			foreach ( XmlNode n in node.ChildNodes )
			{
				Logger.Trace( "processing " + n.Name );
				switch ( n.Name )
				{
					case "year":
						Year = n.InnerText;
						break;

					case "weeks":
						Weeks = Int32.Parse( n.InnerText );
						break;

					case "team-list":
						ProcessTeams( n );
						break;
				}
			}
		}

		private void ProcessTeams( XmlNode n )
		{
			foreach ( XmlNode tn in n.ChildNodes )
			{
				if ( tn.Attributes != null )
				{
					var teamKey = tn.Attributes[ "key" ].Value;
					TeamKeyList.Add( teamKey );
					foreach ( XmlNode rn in tn.ChildNodes )
					{
						if ( rn.Name.Equals( "rating-list" ) )
							ProcessRatings( rn, teamKey );
					}
				}
			}
		}

		static private void ProcessRatings( XmlNode n, string teamKey )
		{
			foreach ( XmlNode tn in n.ChildNodes )
			{
				if ( !tn.Name.Equals( "at-week" ) ) continue;
				if ( tn.Attributes == null ) continue;
				var w = Int32.Parse( tn.Attributes[ "week" ].Value );
				var t = Masters.Tm.GetTeam( teamKey );
				t.LetterRating[ w ] = tn.Attributes[ "letter" ].Value;
				t.NumberRating[ w ] = Decimal.Parse( tn.Attributes[ "number" ].Value );
				t.NibbleRating[ w, 0 ] = Int32.Parse( tn.Attributes[ "offence" ].Value );
				t.NibbleRating[ w, 1 ] = Int32.Parse( tn.Attributes[ "defence" ].Value );
			}
		}

		public decimal AverageScoreAfterWeek( int weekNo )
		{
			if ( AverageScores == null )
				CalculateAverageScores( Weeks );  // do them all to start off with
			else if ( AverageScores[ weekNo - 1, 0 ] == 0 )
				CalculateAverageScores( weekNo );

			return AverageScores != null ? AverageScores[ weekNo - 1, 0 ] : 0;
		}

		private void ZeroiseAverageScores()
		{
			for ( int w = 1; w < Weeks; w++ )
				AverageScores[ w - 1, 0 ] = 0;
		}

		private void CalculateAverageScores( int toWeek )
		{
			Logger.Trace( string.Format( " Calculating Average Scores for {0} upto week {1}", Year, toWeek ) );

			if ( AverageScores == null )
			{
				AverageScores = new decimal[ Weeks, 4 ];
				ZeroiseAverageScores();
			}

			AverageScores[ 0, 0 ] = 21;

			for ( int w = 1; w < toWeek + 1; w++ )
			{
				decimal totalPoints = 0.0M;  //  accumulate these
				decimal totalScores = 0.0M;
				decimal nGames = 0.0M;

				//  for the previous weeks
				for ( var i = 0; i < w; i++ )
				{
					var ds = Utility.TflWs.GetGames( Int32.Parse( Year ), i + 1 );
					var dt = ds.Tables[ "sched" ];
					foreach ( DataRow dr in dt.Rows )
					{
						var game = new NFLGame( dr );
						totalPoints += game.TotalPoints;
						if ( game.Played() )
						{
							totalScores += 2.0M;
							nGames++;
						}
					}
				}
				if ( totalScores > 0 )
					AverageScores[ w - 1, 0 ] = ( int ) Math.Round( totalPoints / totalScores );
				else
					AverageScores[ w - 1, 0 ] = 21;
				AverageScores[ w - 1, 1 ] = totalScores / 2;
				AverageScores[ w - 1, 2 ] = totalPoints;
				AverageScores[ w - 1, 3 ] = nGames;

				Logger.Trace(
				   string.Format( "   Week {0:#0} - {1:#0} games - Average score {2:#0} Total Points {3:####0}",
					   w, AverageScores[ w - 1, 1 ], AverageScores[ w - 1, 0 ], totalPoints ) );
			}
		}

		public void DumpTeams()
		{
			foreach ( string key in TeamKeyList )
			{
				var teamLine = $"\t[{key}]";
				Logger.Trace( teamLine );
#if DEBUG
				Console.WriteLine(teamLine);
#endif
			}
		}

		#region Schedule<yyyy>.xml

		public void SpitSchedule()
		{
			if ( ( GameList.Count > 0 ) )
			{
				var fileName = string.Format( "schedule{0}.xml", Year );
				var writer = new
				   XmlTextWriter( string.Format( "{0}{1}", Utility.OutputDirectory() + "xml\\", fileName ), null )
				{ Formatting = Formatting.Indented };

				writer.WriteStartDocument();
				const string pItext = "type='text/xsl' href='../xsl/schedule2teamhtml.xsl'";
				writer.WriteProcessingInstruction( "xml-stylesheet", pItext );

				writer.WriteComment( "Comments: NFL Season Game List" );
				writer.WriteStartElement( "game-list" );

				// Game List
				foreach ( var g in GameList )
					WriteGameNode( writer, g );

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Close();

				Logger.Trace( fileName + " created" );
			}
		}

		private static void WriteGameNode( XmlWriter writer, NFLGame g )
		{
			var winner = g.WinningTeamCode();
			if ( winner.Equals( "HO" ) ) winner = "TT";

			if ( ( g.HomeScore + g.AwayScore == 0 ) && ( g.GameDate < DateTime.Now ) )
				Utility.Announce( String.Format( "0-0 tie {0} {1} @ {2}", g.GameKey(), g.AwayTeamName, g.HomeTeamName ) );

			writer.WriteStartElement( "game" );
			writer.WriteAttributeString( "winner", winner );
			writer.WriteAttributeString( "type", g.GameType() );
			decimal spread = g.Spread;
			if ( spread == 0.0M )
				writer.WriteAttributeString( "off-the-board", "true" );
			else if ( spread == 0.5M ) spread = 0.0M;

			if ( g.WentIntoOvertime() )
				writer.WriteAttributeString( "overtime", "true" );

			WriteElement( writer, "date", g.GameDate.ToString( "yyyy-MM-dd" ) );
			WriteElement( writer, "week-number", g.WeekNo.ToString() );

			WriteTeams( writer, g );
			WriteElement( writer, "spread", spread.ToString() );
			writer.WriteEndElement();
		}

		private static void WriteTeams( XmlWriter writer, NFLGame g )
		{
			writer.WriteStartElement( "team-list" );

			WriteTeamNode( writer, g.HomeTeam, g.HomeScore, true );
			WriteTeamNode( writer, g.AwayTeam, g.AwayScore, false );

			writer.WriteEndElement();
		}

		private static void WriteTeamNode( XmlWriter writer, string teamCode, int score, bool isHome )
		{
			if ( teamCode.Equals( "HO" ) ) teamCode = "TT";

			writer.WriteStartElement( "team" );
			writer.WriteAttributeString( "id", teamCode );
			if ( isHome )
				writer.WriteAttributeString( "home", "true" );
			WriteElement( writer, "score", score.ToString() );
			writer.WriteEndElement();
		}

		#endregion Schedule<yyyy>.xml

		public void SpitSeason()
		{
			var fileName = string.Format( "Season{0}.xml", Year );
			var writer = new
			   XmlTextWriter( string.Format( "{0}{1}", Utility.OutputDirectory() + "xml\\", fileName ), null )
			{ Formatting = Formatting.Indented };
			writer.WriteStartDocument();
			writer.WriteComment( "Comments: NFL Season Structure" );
			WriteSeasonNode( writer, this );
			writer.WriteEndDocument();
			writer.Close();
			Utility.Announce( fileName + " created" );
		}

		private static void WriteSeasonNode( XmlWriter writer, NflSeason s )
		{
			writer.WriteStartElement( "season" );
			WriteElement( writer, "year", s.Year );
			WriteElement( writer, "weeks", s.Weeks.ToString() );

			WriteConferenceList( writer, s );

			WriteTeamList( writer, s );

			writer.WriteEndElement();
		}

		private static void WriteConferenceList( XmlWriter writer, NflSeason s )
		{
			writer.WriteStartElement( "conferences" );
			//  Division List
			foreach ( var c in s.ConferenceList )
				WriteConferenceNode( writer, c );
			writer.WriteEndElement();
		}

		private static void WriteConferenceNode( XmlWriter writer, NflConference c )
		{
			writer.WriteStartElement( "conference" );
			WriteElement( writer, "name", c.NameOut() );

			WriteDivisionList( writer, c );

			writer.WriteEndElement();
		}

		private static void WriteDivisionList( XmlWriter writer, NflConference c )
		{
			writer.WriteStartElement( "division-list" );
			//  Division List
			foreach ( NFLDivision d in c.DivList )
				WriteDivisionNode( writer, d );
			writer.WriteEndElement();
		}

		private static void WriteDivisionNode( XmlWriter writer, NFLDivision d )
		{
			writer.WriteStartElement( "division" );
			WriteElement( writer, "name", d.NameOut() );
			WriteElement( writer, "id", d.Code );

			writer.WriteEndElement();
		}

		private static void WriteTeamList( XmlWriter writer, NflSeason s )
		{
			writer.WriteStartElement( "team-list" );
			foreach ( var t in s.TeamList )
			{
				writer.WriteStartElement( "team" );
				WriteElement( writer, "id", t.TeamCode );
				WriteElement( writer, "name", t.Name );
				WriteElement( writer, "division", t.Division() );

				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		private static void WriteElement( XmlWriter writer, string name, string text )
		{
			writer.WriteStartElement( name );
			writer.WriteString( text );
			writer.WriteEndElement();
		}

		#endregion XML stuff

		#region Accessors

		public decimal[,] AverageScores { get; set; }

		public string Year { get; set; }

		public ArrayList TeamKeyList
		{
			get { return _mTeamKeyList; }
			set { _mTeamKeyList = value; }
		}

		public int NumberOfTeams()
		{
			return _mTeamKeyList.Count;
		}

		public int NumberOfGames()
		{
			return GameList.Count;
		}

		public int Weeks
		{
			get { return _mWeeks; }
			set { _mWeeks = value; }
		}

		public List<NflTeam> TeamList
		{
			get { return _mTeamList; }
		}

		public List<NflConference> ConferenceList
		{
			get { return _mConferenceList; }
		}

		#endregion Accessors

		public void Predict()
		{
			var rr = new NFLRosterReport( Year );
			rr.SeasonProjection( "Spread", Year, "0", Utility.StartOfSeason( Year ) );
		}

		public void DumpPowerRatings()
		{
			var compareByPower = new Comparison<NflTeam>( CompareTeamsByPower );
			TeamList.Sort( compareByPower );
			foreach ( var t in TeamList )
				Logger.Trace( string.Format( "{0,-20} : {1:00.0} : {2:00.0} : {3:'+'00.0;'-'00.0'}  ",
				   t.NameOut(), t.StartingPowerRating, t.PowerRating, t.PowerRating - t.StartingPowerRating ) );
		}

		private static int CompareTeamsByPower( NflTeam x, NflTeam y )
		{
			if ( x == null )
			{
				if ( y == null )
					return 0;
				return -1;
			}
			return y == null ? 1 : y.PowerRating.CompareTo( x.PowerRating );
		}

		public override string ToString()
		{
			return Year;
		}
	}
}