using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;

namespace RosterLib
{
	/// <summary>
	/// Summary description for WizSeason.
	/// </summary>
	public class WizSeason
	{
		#region  Privates

		private readonly string _season;
		private string _startWeek;
		private string _endWeek;

		private readonly ArrayList _gameList;

		private int _nGames;
		private static GameStats _total;

		private readonly ArrayList _matrixList;
		DataTable _teams;

		private const decimal KPointsWin = 3.0M;
		private const decimal KPointsDraw = 2.0M;
		private const decimal KPointsLoss = 1.0M;

		private Ladder _poLadder; //  put these in a hash table
		private Ladder _roLadder;
		private Ladder _ppLadder;
		private Ladder _prLadder;
		private Ladder _rdLadder;
		private Ladder _pdLadder;

		//  options
#if DEBUG
		private const bool bOutputMatrixs = true; //  turning this on will produce the unit_TT.htm
		private const bool bFrequencyTables = true;
		private const bool bOutputLadders = true;
		private const bool bOutputPlayerEP = true;
		private const bool bExplain = true;

#else
		private bool bOutputMatrixs   = true;
		private bool bFrequencyTables = true;
		private bool bOutputLadders   = true;
		private bool bOutputPlayerEP  = true;
		private static bool bExplain  = false;

#endif
		private const bool BCsvOutput = true;
		private Hashtable _ladderHash;

		//  Frequency Tables
		FrequencyTable _ftYDp;
		FrequencyTable _ftYDr;
		FrequencyTable _ftSaKallowed;
		FrequencyTable _ftTDp;
		FrequencyTable _ftTDr;

		#endregion

		#region  Accessors

		public static GameStats Total
		{
			get { return _total; }
			set { _total = value; }
		}

		public static int GamesPlayed { get; set; }

		public Hashtable PlayerHt { get; set; }

		#endregion

		#region  Constructors

		public WizSeason( string seasonIn )
		{
			_season = seasonIn;
			_matrixList = new ArrayList(); //   Ratings Matrix for all the teams
			if ( bOutputPlayerEP ) PlayerHt = new Hashtable(); //  table of EP

			InitialiseMatrix();
			foreach ( UnitMatrix u in _matrixList )
			{
				u.Team.LoadGames( u.Team.TeamCode, _season );
				foreach (NFLGame g in u.Team.GameList)
					ProcessGame(u, g);
			}
			OutputPhase( seasonIn );
		}

		private static void ProcessGame(UnitMatrix u, NFLGame g)
		{
			var weekOffset = Int32.Parse(g.Week) - 1;
			Utility.Announce(string.Format("Game {0}", g.GameCodeOut()));
			if ((weekOffset < 0) || (weekOffset > 20))
				Utility.Announce(string.Format("Game {0} has week {1}", g.GameCodeOut(), g.Week));
			else
			{
				if (!g.MetricsCalculated) g.TallyMetrics(String.Empty);

				decimal metric = (g.IsHome(u.Team.TeamCode) ? g.HomeTDp : g.AwayTDp);
				GetEp("PO", u.PoExp, g, u, weekOffset, metric);

				metric = (g.IsHome(u.Team.TeamCode) ? g.HomeTDr : g.AwayTDr);
				GetEp("RO", u.RoExp, g, u, weekOffset, metric);

				metric = (g.IsHome(u.Team.TeamCode) ? g.HomeSaKa : g.AwaySaKa);
				GetEp("PP", u.PpExp, g, u, weekOffset, metric);

				metric = (g.IsHome(u.Team.TeamCode) ? g.AwaySaKa : g.HomeSaKa);
				GetEp("PR", u.PrExp, g, u, weekOffset, metric);

				metric = (g.IsHome(u.Team.TeamCode) ? g.AwayTDr : g.HomeTDr);
				GetEp("RD", u.RdExp, g, u, weekOffset, metric);

				metric = (g.IsHome(u.Team.TeamCode) ? g.AwaySaKa : g.HomeSaKa);
				GetEp("PD", u.PdExp, g, u, weekOffset, metric);
			}
		}

		public WizSeason( string seasonIn, string startWeekIn, string endWeekIn )
		{
			//  The constructor will read all the results and crunch the 
			//  experience points for each unit.
			_season = seasonIn;
			_startWeek = startWeekIn;
			_endWeek = endWeekIn;

			_gameList = new ArrayList(); //  stats for all the games
			_matrixList = new ArrayList(); //   Ratings Matrix for all the teams
			if ( bOutputPlayerEP ) PlayerHt = new Hashtable(); //  table of EP

			_total = new GameStats( seasonIn, "tot", "tot", 0 ); //  League Totals

			InitialiseMatrix();

			LoadSeason( _season, _startWeek, _endWeek ); //  loads game stats for a season used for averages
			Utility.Announce( string.Format( "WizSeason:Loaded and tallied {0} games", _gameList.Count ) );

			if ( seasonIn != Utility.CurrentSeason() )
			{
				if ( Int32.Parse( Utility.CurrentWeek() ) > 0 )
				{
					//  This adds current season games giving a cumulative affect
					Utility.Announce( "Adding ratings for up to " + Utility.CurrentWeek() );
					LoadSeason( Utility.CurrentSeason(), "01", Utility.CurrentWeek() );
				}
			}

			AnnounceTotals();

			DetermineExperience();

			OutputPhase( seasonIn );
		}


		private static void GetEp( string unitCode, decimal[,] exp, NFLGame g, UnitMatrix u, int weekOffset,
											decimal metric )
		{
			var ep = g.ExperiencePoints( unitCode, u.Team.TeamCode );
			IncrementMatrix( ep, weekOffset, exp, metric );
			return;
		}

		private static void IncrementMatrix( decimal ep, int weekOffset, decimal[,] exp, decimal metric )
		{
			exp[ weekOffset, 0 ] = ep;
			exp[ weekOffset, 1 ] = metric;
			exp[ weekOffset, 2 ] = 1.0M;
			if ( weekOffset < Constants.K_WEEKS_IN_REGULAR_SEASON )
			{
				// only include regular season stuff in totals
				exp[ 21, 0 ] += ep;
				exp[ 21, 1 ] += metric;
				exp[ 21, 2 ] = 0.0M;
			}
		}

		private void OutputPhase( string seasonIn )
		{
			if ( bOutputMatrixs ) OutputMatrixs( seasonIn );

			if ( bOutputLadders )
			{
				if ( _startWeek == null ) _startWeek = "01";
				if ( _endWeek == null ) _endWeek = "17";
				var suf = " - " + seasonIn + " " + _startWeek + " to " + _endWeek;
				_ladderHash = new Hashtable();
				_poLadder = new Ladder( UnitMatrix.KUnitNamePassingOffence + suf, 32 );
				_ladderHash.Add( UnitMatrix.KUnitNamePassingOffence, _poLadder );
				_roLadder = new Ladder( UnitMatrix.KUnitNameRushingOffence + suf, 32 );
				_ladderHash.Add( UnitMatrix.KUnitNameRushingOffence, _roLadder );
				_ppLadder = new Ladder( UnitMatrix.KUnitNamePassProtection + suf, 32 );
				_ladderHash.Add( UnitMatrix.KUnitNamePassProtection, _ppLadder );

				_prLadder = new Ladder( UnitMatrix.KUnitNamePassRush + suf, 32 );
				_ladderHash.Add( UnitMatrix.KUnitNamePassRush, _prLadder );
				_rdLadder = new Ladder( UnitMatrix.KUnitNameRushingDefence + suf, 32 );
				_ladderHash.Add( UnitMatrix.KUnitNameRushingDefence, _rdLadder );
				_pdLadder = new Ladder( UnitMatrix.KUnitNamePassingDefence + suf, 32 );
				_ladderHash.Add( UnitMatrix.KUnitNamePassingDefence, _pdLadder );
				OutputLadders();
			}

			if ( bFrequencyTables )
				if ( _gameList != null ) FrequencyTables();

			if ( BCsvOutput ) SendToCsv( );

			if ( bOutputPlayerEP ) OutputPlayerEp();

			SendToXml( seasonIn );
		}

		private void SendToXml( string season )
		{
			var writer = new
                XmlTextWriter(string.Format("{0}xml\\TeamEP{1}.xml", 
					 Utility.OutputDirectory(), season), null);

			writer.WriteStartDocument();
			writer.WriteComment( "Comments: Team Unit Experience Points" );
			writer.WriteStartElement( "teamList" );
			//  All the players
			foreach ( UnitMatrix m in _matrixList )
				SpitTeam( m, writer );

			writer.WriteEndDocument();
			writer.Close();
		}

		private static void SpitTeam( UnitMatrix m, XmlTextWriter writer )
		{
			m.Team.ExperiencePoints = TallyExperience(m);
			writer.WriteStartElement( "team" );

			WriteElement( writer, "teamcode", m.Team.TeamCode );
			WriteElement( writer, "name", m.Team.NameOut() );
			WriteElement( writer, "ep", m.Team.ExperiencePoints.ToString() );

			UnitList( m, writer );

			writer.WriteEndElement();
		}

		private static decimal TallyExperience(UnitMatrix m )
		{
			var tot = 0.0M;
			if (Utility.UnitList == null) Utility.LoadUnits();
			foreach (TeamUnit u in Utility.UnitList)
			{
				var exp = m.GetUnit(u.UnitCode);
				tot += exp[RosterLib.Constants.K_WEEKS_IN_A_SEASON, 0];
			}
			return tot;
		}

		private static void UnitList( UnitMatrix m, XmlWriter writer )
		{
			writer.WriteStartElement( "unit-list" );
			foreach ( TeamUnit u in Utility.UnitList )
			{
				WriteElement( writer, "unitcode", u.UnitCode );
				var exp = m.GetUnit( u.UnitCode );
				WriteElement( writer, "ep", string.Format( "{0:#0.0}", exp[ Constants.K_WEEKS_IN_A_SEASON, 0 ] ) );
			}
			writer.WriteEndElement();
		}

		private static void WriteElement( XmlWriter writer, string name, string text )
		{
			writer.WriteStartElement( name );
			writer.WriteString( text );
			writer.WriteEndElement();
		}

		#endregion

		#region  Ladders

		private void OutputLadders()
		{
			OutputLadder();
			OutputPoLadder();
			OutputRoLadder();
			OutputPpLadder();
			OutputPrLadder();
			OutputRdLadder();
			OutputPdLadder();
		}

		private void OutputLadder()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "Team", typeof ( String ) );
			cols.Add( "Offence", typeof ( Decimal ) );
			cols.Add( "PO", typeof ( Decimal ) );
			cols.Add( "RO", typeof ( Decimal ) );
			cols.Add( "PP", typeof ( Decimal ) );
			cols.Add( "Defence", typeof ( Decimal ) );
			cols.Add( "PR", typeof ( Decimal ) );
			cols.Add( "RD", typeof ( Decimal ) );
			cols.Add( "PD", typeof ( Decimal ) );
			cols.Add( "Total", typeof ( Decimal ) );

			foreach ( var t in _matrixList )
			{
				var matrix = (UnitMatrix) t;
				var dr = dt.NewRow();
				dr[ "Team" ] = Masters.Tm.TeamFor( matrix.TeamCode, _season );
				dr[ "Offence" ] = matrix.TotalOffPoints;
				dr[ "PO" ] = matrix.PoPoints;
				dr[ "RO" ] = matrix.RoPoints;
				dr[ "PP" ] = matrix.PpPoints;
				dr[ "Defence" ] = matrix.TotalDefPoints;
				dr[ "PR" ] = matrix.PrPoints;
				dr[ "RD" ] = matrix.RdPoints;
				dr[ "PD" ] = matrix.PdPoints;
				dr[ "Total" ] = matrix.TotalPoints;
				dt.Rows.Add( dr );
			}
			SpitLadder( dt );
			SpitDefence( dt );
			SpitOffence( dt );
		}

		private static string TitleOut()
		{
			return "Exp Ladder " + Utility.CurrentSeason() + " to week " + Utility.CurrentWeek();
		}

		private void OutputPoLadder()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "Team", typeof ( String ) );
			cols.Add( "PassOff", typeof ( Decimal ) );
			cols.Add( "Metric", typeof ( Decimal ) );

			for ( var i = 0; i < _matrixList.Count; i++ )
			{
				var matrix = (UnitMatrix) _matrixList[ i ];
				var dr = dt.NewRow();
				dr[ "Team" ] = Masters.Tm.TeamFor( matrix.TeamCode, _season ).Trim();
				dr[ "PassOff" ] = matrix.PoPoints;
				dr[ "Metric" ] = matrix.PoMetrics;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = "PassOff DESC";
			var st = new SimpleTableReport( TitleOut(), "" ) {ColumnHeadings = true};

			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Pass Offence", "PassOff", "{0:#0.0}" ) );
			st.AddColumn( new ReportColumn( "Pass TDs", "Metric", "{0:#0}" ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
			st.RenderAsHtml( LadderFileName( "PO", _season ), true );

			_poLadder.Load( dt, "Team", UnitMatrix.KUnitNamePassingOffence );
		}

		private static string LadderFileName( string unitCode, string season )
		{
            return string.Format("{0}Exp_{1}_Ladder_{2}.htm", Utility.OutputDirectory(), unitCode, season);
		}

		private void OutputRoLadder()
		{
			const string K_COL = "RushOff";
			const string K_COL_METRIC = "Metric";

			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "Team", typeof ( String ) );
			cols.Add( K_COL, typeof ( Decimal ) );
			cols.Add( K_COL_METRIC, typeof ( Decimal ) );

			for ( var i = 0; i < _matrixList.Count; i++ )
			{
				var matrix = (UnitMatrix) _matrixList[ i ];
				var dr = dt.NewRow();
				dr[ "Team" ] = Masters.Tm.TeamFor( matrix.TeamCode, _season ).Trim();
				dr[ K_COL ] = matrix.RoPoints;
				dr[ K_COL_METRIC ] = matrix.RoMetrics;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = K_COL + " DESC";

			var st = new SimpleTableReport( "Exp Ladder " + _season, "" ) {ColumnHeadings = true};

			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Rush Offence", K_COL, "{0:#0.0}" ) );
			st.AddColumn( new ReportColumn( "Rushing YDs", K_COL_METRIC, "{0:#0}" ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
			st.RenderAsHtml( LadderFileName( "RO", _season ), true );

			_roLadder.Load( dt, "Team", UnitMatrix.KUnitNameRushingOffence );
		}

		private void OutputPpLadder()
		{
			const string K_COL = "PassProt";
			const string K_COL_METRIC = "Metric";

			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "Team", typeof ( String ) );
			cols.Add( K_COL, typeof ( Decimal ) );
			cols.Add( K_COL_METRIC, typeof ( Decimal ) );

			UnitMatrix matrix;
			for ( int i = 0; i < _matrixList.Count; i++ )
			{
				matrix = (UnitMatrix) _matrixList[ i ];
				DataRow dr = dt.NewRow();
				dr[ "Team" ] = Masters.Tm.TeamFor( matrix.TeamCode, _season ).Trim();
				dr[ K_COL ] = matrix.PpPoints;
				dr[ K_COL_METRIC ] = matrix.PpMetrics;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = K_COL + " DESC";
			var st = new SimpleTableReport( "Exp Ladder " + _season, "" ) {ColumnHeadings = true};

			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Pass Protection", K_COL, "{0:#0.0}" ) );
			st.AddColumn( new ReportColumn( "Sacks Allowed", K_COL_METRIC, "{0:#0.0}" ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
			st.RenderAsHtml( LadderFileName( "PP", _season ), true );
			_ppLadder.Load( dt, "Team", UnitMatrix.KUnitNamePassProtection );
		}

		private void OutputPrLadder()
		{
			const string K_COL = "PassRush";
			const string K_COL_METRIC = "Metric";

			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "Team", typeof ( String ) );
			cols.Add( K_COL, typeof ( Decimal ) );
			cols.Add( K_COL_METRIC, typeof ( Decimal ) );

			UnitMatrix matrix;
			for ( int i = 0; i < _matrixList.Count; i++ )
			{
				matrix = (UnitMatrix) _matrixList[ i ];
				DataRow dr = dt.NewRow();
				dr[ "Team" ] = Masters.Tm.TeamFor( matrix.TeamCode, _season ).Trim();
				dr[ K_COL ] = matrix.PrPoints;
				dr[ K_COL_METRIC ] = matrix.PrMetrics;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = K_COL + " DESC";
			var st = new SimpleTableReport( "Exp Ladder " + _season, "" ) {ColumnHeadings = true};

			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Pass Rush", K_COL, "{0:#0.0}" ) );
			st.AddColumn( new ReportColumn( "Sacks", K_COL_METRIC, "{0:#0.0}" ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
			st.RenderAsHtml( LadderFileName( "PR", _season ), true );

			_prLadder.Load( dt, "Team", UnitMatrix.KUnitNamePassRush );
		}

		private void OutputRdLadder()
		{
			const string K_COL = "RunDef";
			const string K_COL_METRIC = "Metric";

			DataTable dt = new DataTable();
			DataColumnCollection cols = dt.Columns;
			cols.Add( "Team", typeof ( String ) );
			cols.Add( K_COL, typeof ( Decimal ) );
			cols.Add( K_COL_METRIC, typeof ( Decimal ) );

			UnitMatrix matrix;
			for ( int i = 0; i < _matrixList.Count; i++ )
			{
				matrix = (UnitMatrix) _matrixList[ i ];
				DataRow dr = dt.NewRow();
				dr[ "Team" ] = Masters.Tm.TeamFor( matrix.TeamCode, _season ).Trim();
				dr[ K_COL ] = matrix.RdPoints;
				dr[ K_COL_METRIC ] = matrix.RdMetrics;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = K_COL + " DESC";
			var st = new SimpleTableReport( "Exp Ladder " + _season, "" );

			st.ColumnHeadings = true;
			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Run Defence", K_COL, "{0:#0.0}" ) );
			st.AddColumn( new ReportColumn( "Rushing TDs Allowed", K_COL_METRIC, "{0:#0}" ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
			st.RenderAsHtml( LadderFileName( "RD", _season ), true );
			_rdLadder.Load( dt, "Team", UnitMatrix.KUnitNameRushingDefence );
		}

		private void OutputPdLadder()
		{
			const string K_COL = "PassDef";
			const string K_COL_METRIC = "Metric";

			DataTable dt = new DataTable();
			DataColumnCollection cols = dt.Columns;
			cols.Add( "Team", typeof ( String ) );
			cols.Add( K_COL, typeof ( Decimal ) );
			cols.Add( K_COL_METRIC, typeof ( Decimal ) );

			UnitMatrix matrix;
			for ( int i = 0; i < _matrixList.Count; i++ )
			{
				matrix = (UnitMatrix) _matrixList[ i ];
				DataRow dr = dt.NewRow();
				dr[ "Team" ] = Masters.Tm.TeamFor( matrix.TeamCode, _season ).Trim();
				dr[ K_COL ] = matrix.PdPoints;
				dr[ K_COL_METRIC ] = matrix.PdMetrics;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = K_COL + " DESC";

			SimpleTableReport st = new SimpleTableReport( "Exp Ladder " + _season, "" );

			st.ColumnHeadings = true;
			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Pass Defence", K_COL, "{0:#0.0}" ) );
			st.AddColumn( new ReportColumn( "Pass TDs allowed", K_COL_METRIC, "{0:#0}" ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
			st.RenderAsHtml( LadderFileName( "PD", _season ), true );
			_pdLadder.Load( dt, "Team", UnitMatrix.KUnitNamePassingDefence );
		}

		private void SpitDefence( DataTable dt )
		{
			dt.DefaultView.Sort = "Defence DESC";
			var st = new SimpleTableReport( "Defense Ladder " + _season, "" ) {ColumnHeadings = true};

			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Defence", "Defence", "{0:#0.0}", true ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
            st.RenderAsHtml(string.Format("{0}Exp_Defence_{1}.htm", Utility.OutputDirectory(), _season), true);
		}

		private void SpitOffence( DataTable dt )
		{
			dt.DefaultView.Sort = "Offence DESC";
			var st = new SimpleTableReport( "Offence Ladder " + _season, "" ) {ColumnHeadings = true};

			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Offence", "Offence", "{0:#0.0}", true ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
            st.RenderAsHtml(string.Format("{0}Exp_Offence_{1}.htm", Utility.OutputDirectory(), _season), true);
		}

		private void SpitLadder( DataTable dt )
		{
			dt.DefaultView.Sort = "Total DESC";
			var st = new SimpleTableReport( "Exp Ladder " + _season, "" ) {ColumnHeadings = true};

			st.AddColumn( new ReportColumn( "Team", "Team", "{0}" ) );
			st.AddColumn( new ReportColumn( "Offence", "Offence", "{0:#0.0}", true ) );
			st.AddColumn( new ReportColumn( "PO", "PO", "{0:#0.0}", true ) );
			st.AddColumn( new ReportColumn( "RO", "RO", "{0:#0.0}", true ) );
			st.AddColumn( new ReportColumn( "PP", "PP", "{0:#0.0}", true ) );
			st.AddColumn( new ReportColumn( "Defence", "Defence", "{0:#0.0}", true ) );
			st.AddColumn( new ReportColumn( "PR", "PR", "{0:#0.0}", true ) );
			st.AddColumn( new ReportColumn( "RD", "RD", "{0:#0.0}", true ) );
			st.AddColumn( new ReportColumn( "PD", "PD", "{0:#0.0}", true ) );
			st.AddColumn( new ReportColumn( "Total", "Total", "{0:#0.0}", true ) );
			st.LoadBody( dt );
			st.DoRowNumbers = true;
			st.ShowElapsedTime = false;
            st.RenderAsHtml(string.Format("{0}Exp_Ladder_{1}.htm", Utility.OutputDirectory(), _season), true);
		}

		#endregion

		/// <summary>
		///  Creates a CSV file with all the data.
		///  Note: Unicode wont work for ListPro
		/// </summary>
		private void SendToCsv()
		{
			using ( var fs = File.Create(Utility.OutputDirectory() + "ExpPt.csv"))
			using ( var sw = new StreamWriter( fs ) )
				foreach ( UnitMatrix m in _matrixList )
					sw.WriteLine( m.CsvLine() );
		}

		private void OutputMatrixs( string seasonIn )
		{
			foreach (var t in _matrixList)
			{
				var matrix = (UnitMatrix)t;
				matrix.RenderAsHtml( "Unit " + seasonIn, true, true, true, true );
			}
		}

		public string GetRank( string teamCode, string unitName )
		{
			var ladder = (Ladder) _ladderHash[ unitName ];
			var teamName = Masters.Tm.TeamFor( teamCode, _season );
			var pos = ladder.PosFor( teamName.Trim() );
			return pos;
		}

		/// <summary>
		///   Each Team is the season gets a set of stats.
		/// </summary>
		private void InitialiseMatrix()
		{
			Utility.Announce( "Initializing Matrix " );
			var ds = Utility.TflWs.GetTeams( _season, "" );
			_teams = ds.Tables[ "teams" ];
			foreach ( DataRow dr in _teams.Rows )
			{
				var teamCode = dr[ "TEAMID" ].ToString();
				var team = Masters.Tm.GetTeam( _season, teamCode );
				var unit = new UnitMatrix( team );
				_matrixList.Add( unit );
			}
		}

		/// <summary>
		///  Returns the Matrix for a particular team from
		///  the Matrix list.
		/// </summary>
		/// <param name="teamCode">The team code.</param>
		/// <returns>Teams unitmatrix object</returns>
		public UnitMatrix GetMatrix( string teamCode )
		{
			UnitMatrix matrix = null;
			foreach ( var t in _matrixList )
			{
				matrix = (UnitMatrix) t;
				if ( matrix.TeamCode == teamCode )
					break;
			}
			return matrix;
		}

		/// <summary>
		///   For each game in the PerformanceList - Determines the experience.
		/// </summary>
		private void DetermineExperience()
		{
#if DEBUG
			Utility.Announce( "Calculating Experience" );
#endif
			_gameList.Sort();

			foreach ( var t in _gameList )
			{
				var game = (GameStats) t;
				if (!game.Played()) continue;
				var homeMatrix = GetMatrix( game.HomeTeam );
				var awayMatrix = GetMatrix( game.AwayTeam );
				IncrementExpPts( homeMatrix, awayMatrix, game );
			}
#if DEBUG
			Utility.Announce( "Finished Calculating Experience" );
#endif
		}

		/// <summary>
		///   Crunches a game then increments the exp PTS for
		///   the 6 units on each team.
		/// </summary>
		/// <param name="homeMatrix">The home Matrix.</param>
		/// <param name="awayMatrix">The away Matrix.</param>
		/// <param name="game">The game.</param>
		private void IncrementExpPts( UnitMatrix homeMatrix, UnitMatrix awayMatrix, GameStats game )
		{
			//RosterLib.Utility.Announce( string.Format( "Evaluating game {0} {1}@{2}",
			//                                    game.Code, game.AwayTeam, game.HomeTeam ) );

#if DEBUG
			homeMatrix.DumpToLog();
			awayMatrix.DumpToLog();
#endif
			EvaluatePo( game, homeMatrix, awayMatrix, true );
			EvaluatePo( game, awayMatrix, homeMatrix, false );
			EvaluateRo( game, homeMatrix, awayMatrix, true );
			EvaluateRo( game, awayMatrix, homeMatrix, false );
			EvaluatePp( game, homeMatrix, awayMatrix, true );
			EvaluatePp( game, awayMatrix, homeMatrix, false );
		}

		private static decimal DivByZeroCheck( decimal metric1, decimal divisor )
		{
			if ( ( divisor == 0 ) || ( divisor.ToString() == "" ) )
				return 0.0M;

			return ( metric1/divisor );
		}

		private void EvaluatePp( GameStats game, UnitMatrix offMatrix, UnitMatrix defMatrix, bool bHome )
		{
			decimal avgMetric = AverageMetric( ( Total.AwaySacksAllowed + Total.HomeSacksAllowed )/2 );
#if DEBUG
			Utility.Announce( string.Format( "League Average Sacks Allowed is {0:###.#} (tot={1:###.#})",
															avgMetric, Total.AwaySacksAllowed + Total.HomeSacksAllowed ) );
#endif
			//  Multipliers
			var sakAllowed = offMatrix.PpMetrics;
			var saks = defMatrix.PrMetrics;

			var offMult = Multiplier( avgMetric, DivByZeroCheck( sakAllowed, offMatrix.GamesPlayed ) );
#if DEBUG            
			Utility.Announce( string.Format( "Defence {1} averages {0:###.#} Sacks/game",
															DivByZeroCheck( defMatrix.PrMetrics, defMatrix.GamesPlayed ),
															defMatrix.TeamCode ) );
			Utility.Announce( string.Format( "Offense {1} averages {0:###.#} Sacks allowed/game",
															DivByZeroCheck( sakAllowed, offMatrix.GamesPlayed ), offMatrix.TeamCode ) );
#endif            
			var defMult = Multiplier( DivByZeroCheck( saks, defMatrix.GamesPlayed ), avgMetric );

			if ( offMult == 0.0M ) offMult = 1.0M;
			if ( defMult == 0.0M ) defMult = 1.0M;
#if DEBUG
			Utility.Announce( string.Format( "Offensive multiplier is {0:##.##}", offMult ) );
			Utility.Announce( string.Format( "Defensive multiplier is {0:##.##}", defMult ) );
#endif
			var metric = ( bHome ) ? game.HomeSacksAllowed : game.AwaySacksAllowed;
			decimal offPoints;
			decimal defPoints;
			var result = ResultOf( metric, 2.0M, 4.0M );
			//RosterLib.Utility.Announce( "Result=" + result );
			switch ( result )
			{
				case "W":
					offPoints = KPointsLoss;
					defPoints = KPointsWin;
					break;
				case "D":
					offPoints = KPointsDraw;
					defPoints = KPointsDraw;
					break;
				default:
					offPoints = KPointsWin;
					defPoints = KPointsLoss;
					break;
			}

			offMatrix.AddPpPoints( offPoints, defMult, game );
			defMatrix.AddPrPoints( defPoints, offMult, game );
			DistributeEp( "PP", offPoints, defMult, game, bHome );
			DistributeEp( "PR", defPoints, offMult, game, bHome );
#if DEBUG            
			Explain( offMatrix, defMatrix, "Pass Protection", offPoints,
						defPoints, "PP", "PR", offMult, defMult, result,
						( offMatrix.PpPoints - offPoints ), ( defMatrix.PrPoints - defPoints ), metric, bHome );
#endif            
		}

		/// <summary>
		/// Evaluates the Passing Offense and Defence, based on passing yards.
		/// </summary>
		/// <param name="game">The game.</param>
		/// <param name="offMatrix">The off Matrix.</param>
		/// <param name="defMatrix">The def Matrix.</param>
		/// <param name="bHome">if set to <c>true</c> [b home].</param>
		private void EvaluatePo( GameStats game, UnitMatrix offMatrix, UnitMatrix defMatrix, bool bHome )
		{
			//  What is the league average for Tdp
			var avgMetric = AverageMetric( ( Total.AwayTDpasses + Total.HomeTDpasses )/2 );

			//  How does the offence compare against the League
			var offMult = Multiplier( offMatrix.PoMetrics/offMatrix.GamesPlayed, avgMetric );
#if DEBUG
			Utility.Announce( string.Format( "League Average Tdp is {0:###.#} (tot={1:###.#})",
															avgMetric, Total.AwayTDpasses + Total.HomeTDpasses ) );
			Utility.Announce( string.Format( "Defence {1} gives up an average of {0:###.#} Tdp/game",
															defMatrix.PpMetrics/defMatrix.GamesPlayed, defMatrix.TeamCode ) );
			Utility.Announce( string.Format( "Offense {1} averages {0:###.#} Tdp/game",
															offMatrix.PoMetrics/offMatrix.GamesPlayed, offMatrix.TeamCode ) );
#endif
			var defMult = Multiplier( avgMetric, defMatrix.PdMetrics/defMatrix.GamesPlayed ); //  Avg Def = 1.0

			var metric = ( bHome ) ? game.HomeTDpasses : game.AwayTDpasses;

			decimal offPoints;
			decimal defPoints;
			var result = ResultOf( metric, 0.0M, 1.0M );

			//Utility.Announce( string.Format( "  WizSeason.EvaluatePO Result for {1} = {0}", result, offMatrix.Team.Name ) );

			#region  tally

			switch ( result )
			{
				case "L":
					offPoints = KPointsLoss;
					defPoints = KPointsWin;
					break;
				case "D":
					offPoints = KPointsDraw;
					defPoints = KPointsDraw;
					break;
				default:
					offPoints = KPointsWin;
					defPoints = KPointsLoss;
					break;
			}

			offMatrix.AddPoPoints( offPoints, defMult, game );
			defMatrix.AddPdPoints( defPoints, offMult, game );
			//  Give each member of the passing unit the experience too
			DistributeEp( "PO", offPoints, defMult, game, bHome );
			DistributeEp( "PD", defPoints, offMult, game, bHome );

			#endregion

			Explain( offMatrix, defMatrix, "Pass Offence", offPoints,
						defPoints, "PO", "PD", offMult, defMult, result,
						( offMatrix.PoPoints - ( offPoints*defMult ) ), ( defMatrix.PdPoints - ( defPoints*offMult ) ),
						metric, bHome );
		}

		private void EvaluateRo( GameStats game, UnitMatrix offMatrix, UnitMatrix defMatrix, bool bHome )
		{
			decimal avgMetric = AverageMetric( ( Total.AwayTDruns + Total.HomeTDruns )/2 );
//			RosterLib.Utility.Announce( string.Format( "League Average Tdr is {0:###.#} (tot={1:###.#})", 
//				avgMetric, Total.AwayTDruns + Total.HomeTDruns ) );

			decimal gp = offMatrix.GamesPlayed;
			decimal tdr = offMatrix.RoMetrics;
			decimal offMult = Multiplier( tdr/gp, avgMetric );
			decimal defMult = Multiplier( avgMetric, defMatrix.RdMetrics/defMatrix.GamesPlayed );

//			RosterLib.Utility.Announce( string.Format( "Offense {1} averages {0:###.#} Tdr/game", 
//				offMatrix.ROMetrics / offMatrix.GamesPlayed, offMatrix.TeamCode ) );
//			RosterLib.Utility.Announce( string.Format( "Defence {1} averages {0:###.#} Tdr allowed/game", 
//				defMatrix.RDMetrics / defMatrix.GamesPlayed, defMatrix.TeamCode ) );

			decimal metric = ( bHome ) ? game.HomeTDruns : game.AwayTDruns;
			//string result = ResultOf( metric, 100.0M, 125.0M );
			string result = ResultOf( metric, 0.0M, 1.0M );
//			RosterLib.Utility.Announce( "Result=" + result );

			decimal offPoints;
			decimal defPoints;

			switch ( result )
			{
				case "L":
					offPoints = KPointsLoss;
					defPoints = KPointsWin;
					break;
				case "D":
					offPoints = KPointsDraw;
					defPoints = KPointsDraw;
					break;
				default:
					offPoints = KPointsWin;
					defPoints = KPointsLoss;
					break;
			}

			offMatrix.AddRoPoints( offPoints, defMult, game );
			defMatrix.AddRdPoints( defPoints, offMult, game );
			DistributeEp( "RO", offPoints, defMult, game, bHome );
			DistributeEp( "RD", defPoints, offMult, game, bHome );
			Explain( offMatrix, defMatrix, "Rush Offence", offPoints,
						defPoints, "RO", "RD", offMult, defMult, result,
						( offMatrix.RoPoints - offPoints ), ( defMatrix.RdPoints - defPoints ), metric, bHome );
		}

		/// <summary>
		///   Determines if you winklose or draw.
		/// </summary>
		/// <param name="metric">The metric.</param>
		/// <param name="lossQuota">The loss quota - you lose if you dont get this much.</param>
		/// <param name="drawQuota">The draw quota - you win if you get more than this.</param>
		/// <returns></returns>
		private static string ResultOf( decimal metric, decimal lossQuota, decimal drawQuota )
		{
			if ( metric <= lossQuota )
				return "L";

			return metric <= drawQuota ? "D" : "W";
		}

		/// <summary>
		///   Calculates a multiplier which is the Teams Average over the League Average
		/// </summary>
		/// <param name="metric">The metric.</param>
		/// <param name="average">The average.</param>
		/// <returns></returns>
		private static decimal Multiplier( decimal metric, decimal average )
		{
			return DivByZeroCheck( metric, average );
		}

		#region Explanations

		public static void Explain( UnitMatrix offMatrix, UnitMatrix defMatrix,
											 string area, decimal offEp, decimal defEp, string offUnit, string defUnit,
											 decimal offMult, decimal defMult, string result, decimal offPtsBefore,
											 decimal defPtsBefore,
											 decimal metric, bool bHome )
		{
			if ( bExplain )
			{
				RosterLib.Utility.Announce( string.Format( "Considering {0} {1} team", area, bHome ? "Home" : "Away" ) );
				RosterLib.Utility.Announce( MultiplierLine( "Off team " + offMatrix.TeamCode, offUnit, metric,
																 offMult ) );
				RosterLib.Utility.Announce( MultiplierLine( "Def team " + defMatrix.TeamCode, defUnit, metric,
																 defMult ) );
				RosterLib.Utility.Announce( PointsLine( offMatrix.TeamCode, offUnit, offPtsBefore, offEp ) );
				RosterLib.Utility.Announce( PointsLine( defMatrix.TeamCode, defUnit, defPtsBefore, defEp ) );
			}
		}

		private static string MultiplierLine( string team, string unit, decimal metric, decimal multiplier )
		{
			return string.Format( "  " + team + " {0} tot is {1:##0.0}, multiplier = {2:#0.0#}",
										 unit, metric, multiplier );
		}

		private static string PointsLine( string team, string unit, decimal fromPts, decimal points )
		{
			return string.Format( "  Adding {0:##0.#} {1} points to {2} to take it from {3:##0.#} to {4:##0.#}",
										 points, unit, team, fromPts, fromPts + points );
		}

		#endregion

		private void LoadSeason( string seasonIn, string startWeekIn, string endWeekIn )
		{
#if DEBUG
			Utility.Announce( "WizSeason.LoadSeason " + seasonIn + " from " + startWeekIn + " to " + endWeekIn );
#endif
			//  for each game in the season
			var ds = Utility.TflWs.GetSeason( seasonIn, startWeekIn, endWeekIn );
			//  calculate gamestat totals for the season
			var dt = ds.Tables[ "sched" ];
			foreach ( DataRow dr in dt.Rows )
			{
				if ( dr[ "HOMESCORE" ].ToString() != "0" || dr[ "AWAYSCORE" ].ToString() != "0" )
				{
					_nGames++;
					var game = new GameStats(
						string.Format("{0}:{1}:{2}", dr["SEASON"], dr["WEEK"], dr["GAMENO"]),
						dr["HOMETEAM"].ToString(), dr["AWAYTEAM"].ToString(),
						Int32.Parse(dr["WEEK"].ToString()))
					           	{
					           		HomePassingYds = Utility.TflWs.TeamStats("S",
					           		                                         _season, dr["WEEK"].ToString(),
					           		                                         dr["GAMENO"].ToString(),
					           		                                         dr["HOMETEAM"].ToString()),
					           		AwayPassingYds = Utility.TflWs.TeamStats("S",
					           		                                         _season, dr["WEEK"].ToString(),
					           		                                         dr["GAMENO"].ToString(),
					           		                                         dr["AWAYTEAM"].ToString())
					           	};

					#region  Passing Yards

					//  Increment totals
					_total.HomePassingYds += game.HomePassingYds;
					_total.AwayPassingYds += game.AwayPassingYds;

					#endregion

					#region SacksAllowed 

					game.HomeSacksAllowed = Utility.TflWs.TeamStats( "Q",
																						 _season, dr[ "WEEK" ].ToString(),
																						 dr[ "GAMENO" ].ToString(),
																						 dr[ "AWAYTEAM" ].ToString() );
					game.AwaySacksAllowed = Utility.TflWs.TeamStats( "Q",
																						 _season, dr[ "WEEK" ].ToString(),
																						 dr[ "GAMENO" ].ToString(),
																						 dr[ "HOMETEAM" ].ToString() );
					game.HomeSacks = game.AwaySacksAllowed;
					game.AwaySacks = game.HomeSacksAllowed;
					//  Increment totals
					_total.HomeSacksAllowed += game.HomeSacksAllowed;
					_total.AwaySacksAllowed += game.AwaySacksAllowed;
					_total.HomeSacks += game.HomeSacks;
					_total.AwaySacks += game.AwaySacks;

					#endregion

					#region TD passes 

					game.HomeTDpasses = Utility.TflWs.TeamScores( "P",
																					 _season, dr[ "WEEK" ].ToString(),
																					 dr[ "GAMENO" ].ToString(),
																					 dr[ "HOMETEAM" ].ToString() );
					IsTopEffort( "TD passes", game.HomeTDpasses, dr[ "HOMETEAM" ].ToString(), dr[ "WEEK" ].ToString() );

					game.AwayTDpasses = Utility.TflWs.TeamScores( "P",
																					 _season, dr[ "WEEK" ].ToString(),
																					 dr[ "GAMENO" ].ToString(),
																					 dr[ "AWAYTEAM" ].ToString() );
					IsTopEffort("TD passes", game.AwayTDpasses, dr["AWAYTEAM"].ToString(), dr["WEEK"].ToString());


					//  Increment totals
					_total.HomeTDpasses += game.HomeTDpasses;
					_total.AwayTDpasses += game.AwayTDpasses;

					#endregion

					#region TD runs 

					game.HomeTDruns = Utility.TflWs.TeamScores( "R",
																				  _season, dr[ "WEEK" ].ToString(),
																				  dr[ "GAMENO" ].ToString(),
																				  dr[ "HOMETEAM" ].ToString() );
					IsTopEffort("TD runs", game.HomeTDruns, dr["HOMETEAM"].ToString(), dr["WEEK"].ToString());

					game.AwayTDruns = Utility.TflWs.TeamScores( "R",
																				  _season, dr[ "WEEK" ].ToString(),
																				  dr[ "GAMENO" ].ToString(),
																				  dr[ "AWAYTEAM" ].ToString() );
					IsTopEffort("TD runs", game.AwayTDruns, dr["AWAYTEAM"].ToString(), dr["WEEK"].ToString());

					//  Increment totals
					_total.HomeTDruns += game.HomeTDruns;
					_total.AwayTDruns += game.AwayTDruns;

					#endregion

					#region FGs 

					game.HomeFGs = Utility.TflWs.TeamScores( "3",
																			  _season, dr[ "WEEK" ].ToString(),
																			  dr[ "GAMENO" ].ToString(),
																			  dr[ "HOMETEAM" ].ToString() );
					game.AwayFGs = Utility.TflWs.TeamScores( "3",
																			  _season, dr[ "WEEK" ].ToString(),
																			  dr[ "GAMENO" ].ToString(),
																			  dr[ "AWAYTEAM" ].ToString() );

					//  Increment totals
					_total.HomeFGs += game.HomeFGs;
					_total.AwayFGs += game.AwayFGs;

					#endregion

					#region  Rushing Yards

					game.HomeRushingYds = Utility.TflWs.TeamStats( "Y",
																					  _season, dr[ "WEEK" ].ToString(),
																					  dr[ "GAMENO" ].ToString(),
																					  dr[ "HOMETEAM" ].ToString() );
					game.AwayRushingYds = Utility.TflWs.TeamStats( "Y",
																					  _season, dr[ "WEEK" ].ToString(),
																					  dr[ "GAMENO" ].ToString(),
																					  dr[ "AWAYTEAM" ].ToString() );

					//  Increment totals
					_total.HomeRushingYds += game.HomeRushingYds;
					_total.AwayRushingYds += game.AwayRushingYds;

					#endregion

					#region  Matrix Loading

					//  Wack the results into the team Matrix
					var homeMatrix = GetMatrix( game.HomeTeam );
					var awayMatrix = GetMatrix( game.AwayTeam );

					homeMatrix.SetOpponent( game.Week, "v" + awayMatrix.TeamCode );
					awayMatrix.SetOpponent( game.Week, "@" + homeMatrix.TeamCode );
					if ( game.Played() )
					{
						homeMatrix.AddMetrics( game, true );
						awayMatrix.AddMetrics( game, false );
						GamesPlayed++;
					}

					#endregion

					_gameList.Add( game );
				}
			}
		}

		private static void IsTopEffort( string metric, decimal quantity, string team, string week )
		{
			if (metric.Equals("TD passes") || metric.Equals("TD runs"))
				if (quantity > 2.0M)
					Utility.Announce(
						string.Format("TOP effort {0} - {1} by {2} in week {3}", metric, quantity, team, week));
		}

		private void AnnounceTotals()
		{
			Utility.Announce( string.Format( "Loaded and totaled {0} games", _gameList.Count ) );
			Utility.Announce(
				string.Format( "Total {0} = {1}", "PassingYds", _total.HomePassingYds + _total.AwayPassingYds ) );
			Utility.Announce( string.Format( "Total {0} = {1}", "Tdp", _total.HomeTDpasses + _total.AwayTDpasses ) );
			Utility.Announce( string.Format( "Total {0} = {1}", "Tdr", _total.HomeTDruns + _total.AwayTDruns ) );
			Utility.Announce(
				string.Format( "Total {0} = {1}", "RushingYds", _total.HomeRushingYds + _total.AwayRushingYds ) );
			Utility.Announce( string.Format( "Total {0} = {1}", "FGs", _total.HomeFGs + _total.AwayFGs ) );
			Utility.Announce( string.Format( "Total {0} = {1}", "SAK", _total.HomeSacks + _total.AwaySacks ) );
		}

		private void FrequencyTables()
		{
			if (_gameList.Count > 0)
			{
				GameStats game;

				#region frequency table for Passing Yards

				_ftYDp = new FrequencyTable("Passing Yards" + _season);

				foreach ( object t in _gameList )
				{
					game = (GameStats) t;
					_ftYDp.Add(game.AwayPassingYds);
					_ftYDp.Add(game.HomePassingYds);
				}

				_ftYDp.Calculate();
				_ftYDp.RenderAsHtml("Freq_YDp", _season );

				#endregion

				#region frequency table for Rushing Yards

				_ftYDr = new FrequencyTable("Rushing Yards " + _season);

				foreach ( var t in _gameList )
				{
					game = (GameStats) t;
					if (!game.isPlayoff())
					{
						_ftYDr.Add( game.AwayRushingYds );
						_ftYDr.Add( game.HomeRushingYds );
					}
				}

				_ftYDr.Calculate();
				_ftYDr.RenderAsHtml("Freq_YDr", _season );

				#endregion

				#region frequency table for Sacks Allowed

				_ftSaKallowed = new FrequencyTable("Sacks Allowed " + _season);

				foreach ( var t in _gameList )
				{
					game = (GameStats) t;
					if (!game.isPlayoff())
					{
						_ftSaKallowed.Add( game.AwaySacksAllowed );
						_ftSaKallowed.Add( game.HomeSacksAllowed );
					}
				}

				_ftSaKallowed.Calculate();
				_ftSaKallowed.RenderAsHtml("Freq_SAK_allwd", _season );

				#endregion

				#region frequency table for Tdp

				_ftTDp = new FrequencyTable(string.Format("TD passes {0} week {1} to {2}", _season, _startWeek, _endWeek ) );

				foreach ( var t in _gameList )
				{
					game = (GameStats) t;
					if ( game.isPlayoff() ) continue;

					_ftTDp.Add( game.AwayTDpasses );
					_ftTDp.Add( game.HomeTDpasses );
				}

				_ftTDp.Calculate();
				_ftTDp.RenderAsHtml("Freq_TDp", _season );

				#endregion

				#region frequency table for TD runs

				_ftTDr = new FrequencyTable(string.Format("TD runs {0} week {1} to {2}", _season, _startWeek, _endWeek ));

				foreach (var t in _gameList)
				{
					game = (GameStats)t;
					if (!game.isPlayoff())
					{
						_ftTDr.Add( game.AwayTDruns );
						_ftTDr.Add( game.HomeTDruns );
					}
				}

				_ftTDr.Calculate();
				_ftTDr.RenderAsHtml("Freq_TDr", _season );

				#endregion
			}
			else
				Utility.Announce( "No games to analyse" );
		}

		public void SummaryStats()
		{
			//  put the statistics into a data table and
			//  spit it out using SimpleTableReport
			if ( _gameList == null ) return;

			var minYDp = 999.9M;
			var maxYDp = 0.0M;
			var minYDpCode = "";
			var maxYDpCode = "";
			GameStats game;
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "METRIC", typeof ( String ) );
			cols.Add( "TOTAL", typeof ( Decimal ) );
			cols.Add( "AVG", typeof ( Decimal ) );

			#region  Max and Min Passing yards

			for ( var i = 0; i < _gameList.Count; i++ )
			{
				game = (GameStats) _gameList[ i ];
				if ( game.HomePassingYds < minYDp )
				{
					minYDp = game.HomePassingYds;
					minYDpCode = game.Code;
				}
				if ( game.AwayPassingYds < minYDp )
				{
					minYDp = game.AwayPassingYds;
					minYDpCode = game.Code;
				}
				if ( game.HomePassingYds > maxYDp )
				{
					maxYDp = game.HomePassingYds;
					maxYDpCode = game.Code;
				}
				if ( game.AwayPassingYds > maxYDp )
				{
					maxYDp = game.AwayPassingYds;
					maxYDpCode = game.Code;
				}

				//  Wack in a report line for each game
				//AddStat( dt, "Game " + game.Code + " Home Passing", game.HomePassingYds, 0 );
			}
			AddStat( dt, "Max Passing Yards " + maxYDpCode, maxYDp, maxYDp );
			AddStat( dt, "Min Passing Yards " + minYDpCode, minYDp, minYDp );

			#endregion

			#region  Passing yards

			AddStat( dt, "Home Passing Yards", _total.HomePassingYds,
						Average( _total.HomePassingYds, _nGames ) );
			AddStat( dt, "Away Passing Yards", _total.AwayPassingYds,
						Average( _total.AwayPassingYds, _nGames ) );
			AddStat( dt, "Total Passing Yards", _total.AwayPassingYds + _total.HomePassingYds,
						Average( _total.AwayPassingYds + _total.HomePassingYds, _nGames ) );
			AddStat( dt, "Average Passing Yards", ( _total.AwayPassingYds + _total.HomePassingYds )/2,
						Average( _total.AwayPassingYds + _total.HomePassingYds, _nGames*2 ) );

			#endregion

			#region  Rushing yards

			AddStat( dt, "Home Rushing Yards", _total.HomeRushingYds,
						Average( _total.HomeRushingYds, _nGames ) );
			AddStat( dt, "Away Rushing Yards", _total.AwayRushingYds,
						Average( _total.AwayRushingYds, _nGames ) );
			AddStat( dt, "Total Rushing Yards", _total.AwayRushingYds + _total.HomeRushingYds,
						Average( _total.AwayRushingYds + _total.HomeRushingYds, _nGames ) );
			AddStat( dt, "Average Rushing Yards", ( _total.AwayRushingYds + _total.HomeRushingYds )/2,
						Average( _total.AwayRushingYds + _total.HomeRushingYds, _nGames*2 ) );

			#endregion

			AddStat( dt, "Total Games", _nGames, _nGames );

			#region  Simple Table stuff

			SimpleTableReport st = new SimpleTableReport(
				"Season Total Stats " + _season + " week " + _startWeek + " to " + _endWeek, "" );

			st.ColumnHeadings = true;
			st.AddColumn( new ReportColumn( "Metric", "METRIC", "{0,-20}" ) );
			st.AddColumn( new ReportColumn( "Total", "TOTAL", "{0:000}" ) );
			st.AddColumn( new ReportColumn( "Average", "AVG", "{0:000.0}" ) );

			st.LoadBody( dt );
			st.ShowElapsedTime = false;
            st.RenderAsHtml(Utility.OutputDirectory() + "SeasonStats" + _season + ".htm", true);

			#endregion
		}

		private static void AddStat( DataTable dt, string statName, decimal stat, decimal avg )
		{
			DataRow dr = dt.NewRow();
			dr[ "METRIC" ] = statName;
			dr[ "TOTAL" ] = stat;
			dr[ "AVG" ] = avg;
			dt.Rows.Add( dr );
		}

		private static decimal Average( decimal quotient, int divisor )
		{
			//  need to do decimal other wise INT() will occur
			if (divisor == 0) return 0.0M;
			return ( quotient/Decimal.Parse( divisor.ToString() ) );
		}

		public void PassingDifferentials()
		{
			if ( _matrixList == null ) return;

			ArrayList diff = new ArrayList();
			for ( int i = 0; i < _matrixList.Count; i++ )
			{
				UnitMatrix matrixA = (UnitMatrix)_matrixList[i];

				for ( int j = 0; j < _matrixList.Count; j++ )
				{
					UnitMatrix matrixB = (UnitMatrix)_matrixList[j];
					diff.Add( ( matrixA.PoPoints - matrixB.PdPoints ) );
				}
			}

			FrequencyTable ftp = new FrequencyTable( "Passing differential " + _season +
																  " week " + _startWeek + " to " + _endWeek, diff );
			ftp.Calculate();
			ftp.RenderAsHtml( "PassingDiff", _season );
		}

		public void RunningDifferentials()
		{
			if ( _matrixList == null ) return;
			ArrayList diff = new ArrayList();
			for ( int i = 0; i < _matrixList.Count; i++ )
			{
				UnitMatrix matrixA = (UnitMatrix)_matrixList[i];

				for ( int j = 0; j < _matrixList.Count; j++ )
				{
					UnitMatrix matrixB = (UnitMatrix)_matrixList[j];
					diff.Add( ( matrixA.RoPoints - matrixB.RdPoints ) );
				}
			}

			FrequencyTable ftp = new FrequencyTable(
				string.Format( "Running differential {0} week {1} to {2}", _season, _startWeek, _endWeek ), diff );

			ftp.Calculate();
			ftp.RenderAsHtml( "RunningDiff", _season );
		}

		public static decimal AverageMetric( decimal tot )
		{
			return ( tot / GamesPlayed );
		}

		#region  Player Experience

		private void DistributeEp( string unitCode, decimal points, decimal multiplier, GameStats game, bool bHome )
		{
			if ( bOutputPlayerEP )
			{
				var teamCode = ( bHome ) ? game.HomeTeam : game.AwayTeam;
				var ep = points*multiplier;
#if DEBUG
				Utility.Announce( 
					string.Format( "Distributing {0:##.##} points to the {1} unit from game {2}", 
					ep, unitCode, game.Code ) );
#endif
				var ds = Utility.TflWs.GetLineup( teamCode, game.Season, game.Week );
				//  For each player in the game in the particular unit tally points
				if ( ds.Tables[ 0 ].Rows.Count > 0 )
				{
					var dt = ds.Tables[ "lineup" ];
					foreach ( DataRow dr in dt.Rows )
					{
						var playerId = dr[ "PLAYERID" ].ToString();
						var startVal = dr[ "START" ].ToString();
						var starter = ( startVal == "True" ) ? true : false;
						var p = new NFLPlayer( playerId );
						if ( p.IsInUnit( unitCode ) )
							TallyEp( playerId, ep, starter );
					}
				}
			}
		}

		private void TallyEp( string playerId, decimal ep, bool starter )
		{
			if ( ! starter ) ep = ep/2.0M; // Half it for backups

#if DEBUG
			Utility.Announce( string.Format( "Distributing {0:##.##} points to {1}", ep, playerId ) );  		
#endif
			if ( PlayerHt.ContainsKey( playerId ) )
				PlayerHt[ playerId ] = (decimal) PlayerHt[ playerId ] + ep;
			else
				PlayerHt.Add( playerId, ep );

			//update EP master
		}

		/// <summary>
		///   Use a simple player Report to dump the results
		/// </summary>
		private void OutputPlayerEp()
		{
			var str = new SimpleTableReport("Experience Points : By Player " + _season)
			          	{ColumnHeadings = true, DoRowNumbers = true};
			str.AddColumn( new ReportColumn( "Player", "PLAYER", "{0,-22}" ) );
			str.AddColumn( new ReportColumn( "Pos", "POS", "{0}" ) );
			str.AddColumn( new ReportColumn( "Team", "TEAMCODE", "{0}" ) );
			str.AddColumn( new ReportColumn( "Unit", "UNITCODE", "{0}" ) );
			str.AddColumn( new ReportColumn( "EP", "EP", "{0:00.0}", true ) );
			str.LoadBody( BuildTable() );
			str.RenderAsHtml(
                string.Format("{0}//Experience Points//Players//ep{1}-{2}.htm", 
						Utility.OutputDirectory(), _season, Utility.CurrentWeek()),
				true );
			RenderPos( "QB", str );
			RenderPos( "RB", str );
			RenderPos( "WR", str );
			RenderPos( "DE", str );
			RenderPos( "DT", str );
			RenderPos( "CB", str );
			RenderPos( "FS", str );
			RenderPos( "SS", str );
			RenderUnit( "PR", str );
			RenderTeams( str );
		}

		private void RenderTeams( SimpleTableReport str )
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "TEAMNAME", typeof ( String ) );
			cols.Add( "EP", typeof ( Int32 ) );
			foreach ( DataRow dr in _teams.Rows )
			{
				var teamCode = dr[ "TEAMID" ].ToString();
				str.SetFilter( string.Format( "TEAMCODE='{0}'", teamCode ) );
				str.RenderAsHtml(
                    string.Format("{0}Experience Points//ep-t-{3}-{1}-{2}.htm", Utility.OutputDirectory(), _season,
										Utility.CurrentWeek(), teamCode ), true );
				//  ask str what the tot was
				var ep = string.Format( "{0}", str.LastTotal );
				//  store it in a DataTable
				var tdr = dt.NewRow();
				tdr[ "TEAMNAME" ] = Utility.TflWs.TeamFor( teamCode, _season );
				tdr[ "EP" ] = ep;
				dt.Rows.Add( tdr );
			}
			//  throw the dt at a Reporter
			TeamExperience( dt );
		}

		private void TeamExperience( DataTable dt )
		{
			var str = new SimpleTableReport("Experience Points : By Team " + _season)
			          	{ColumnHeadings = true, DoRowNumbers = true};
			str.AddColumn( new ReportColumn( "Team", "TEAMNAME", "{0,-20}" ) );
			str.AddColumn( new ReportColumn( "EP", "EP", "{0,3}", true ) );
			str.LoadBody( dt );
			str.SetSortOrder( "EP DESC" );
			str.RenderAsHtml(
                string.Format("{0}\\Experience Points\\Teams\\Team-ep{1}-{2}.htm", 
					 Utility.OutputDirectory(), _season, Utility.CurrentWeek()),
				true );
		}

		private void RenderPos( string position, SimpleTableReport str )
		{
			str.SetFilter( string.Format( "POS like '%{0}%'", position ) );
			str.SetSortOrder( "EP DESC" );
			str.RenderAsHtml(
                string.Format("{0}Experience Points\\Positional\\ep{3}-{1}-{2}.htm", 
					 Utility.OutputDirectory(), _season, Utility.CurrentWeek(), position ), true );
		}

		private void RenderUnit( string unitCode, SimpleTableReport str )
		{
			str.SetFilter( string.Format( "UNITCODE='{0}'", unitCode ) );
			str.RenderAsHtml(
					 string.Format( "{0}Experience Points\\Units\\ep{3}-{1}-{2}.htm", 
						Utility.OutputDirectory(), _season, Utility.CurrentWeek(), unitCode ), true );
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "PLAYER", typeof ( String ) );
			cols.Add( "POS", typeof ( String ) );
			cols.Add( "TEAMCODE", typeof ( String ) );
			cols.Add( "CAT", typeof ( String ) );
			cols.Add( "UNITCODE", typeof ( String ) );
			cols.Add( "EP", typeof ( Decimal ) );

			var myEnumerator = PlayerHt.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
#if DEBUG
				Utility.Announce( string.Format( "\t{0}\t{1}", myEnumerator.Key, myEnumerator.Value ) );
#endif
				var p = new NFLPlayer( myEnumerator.Key.ToString() );
				var ep = (decimal) myEnumerator.Value;
				var dr = dt.NewRow();
				dr[ "PLAYER" ] = p.PlayerName;
				dr[ "POS" ] = p.PlayerPos;
				dr[ "TEAMCODE" ] = p.TeamCode;
				dr[ "CAT" ] = p.PlayerCat;
				dr[ "UNITCODE" ] = p.Unit();
				dr[ "EP" ] = ep;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = "EP DESC";
			return dt;
		}

		#endregion
	}
}
