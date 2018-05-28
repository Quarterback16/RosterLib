using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace RosterLib
{
	/// <summary>
	/// Summary description for NFLTeam.
	/// </summary>
	public class NflTeam : IComparable
	{
		public Logger Logger { get; private set; }

		#region Constants

		public const string KUnitcodePo = "PO";
		public const string KUnitcodeRo = "RO";
		public const string KUnitcodePp = "PP";

		public const string KUnitcodePr = "PR";
		public const string KUnitcodeRd = "RD";
		public const string KUnitcodePd = "PD";

		#endregion Constants

		public string Season;
		public string Name;

		public string Div;
		public string TeamCode;

		public string ApCode { get; set; }

		public bool IsPlayoffBound { get; set; }

		#region Collections

		public ArrayList PlayerList;

		public ArrayList StarterList;
		public ArrayList BackupList;
		public ArrayList OtherList;
		public ArrayList InjuryList;

		public ArrayList FaList;

		public ArrayList PassingUnit;
		public ArrayList RunningUnit;
		public ArrayList ProtectionUnit;
		public ArrayList KickingUnit;
		public ArrayList PassRushUnit;
		public ArrayList PassDefenceUnit;
		public ArrayList RunDefenceUnit;

		#endregion Collections

		public KickUnit KickUnit { get; set; }

		public RushUnit RunUnit { get; set; }

		public PassUnit PassUnit { get; set; }

		public NFLPlayer Kicker { get; set; }

		public int Games { get; set; }

		public int PtsFor;
		public int PtsAgin;
		public int MVictoryPoints;
		private decimal _clip;

		public int OffRate;
		public int DefRate;
		private string _ratings = "";

		//  Score Counts
		public int Tdp;

		public int Tdr;
		public int Fg;

		private HillenMaster _hillenMaster;

		#region Team Stats

		private int _totYdp;

		public int TotYdp { get; set; }

		public int TotYDpAllowed { get; set; }

		public int TotTDpAllowed { get; set; }

		public int TotIntercepts { get; set; }

		public int TotYdrAllowed { get; set; }

		public int TotTDrAllowed { get; set; }

		public decimal TotSacks { get; set; }

		public int TotTDr { get; set; }

		public int TotYdr { get; set; }

		public int TotInterceptions { get; set; }

		public int TotTDp { get; set; }

		public int TotTDs { get; set; }

		public int TotTDt { get; set; }

		public int TotTDk { get; set; }

		public int TotTDi { get; set; }

		public int TotTDf { get; set; }

		public decimal TotSacksAllowed { get; set; }

		public int DefensiveScores { get; set; }

		public decimal FantasyPoints { get; set; }

		#endregion Team Stats

		//  Game log
		private NFLPlayer _kicker;

		private ArrayList _fgResultList;

		private ArrayList _divOpponentList;

		private NFLSchedule _sched;
		private NFLSchedule _prevSched;
		public int PrevWins;
		public int PrevLosses;

		private DefensiveTeamCategory _myCat; //  for rating Defensive teams

		public UnitMatrix Matrix;

		#region GordanRanks

		public String[] LetterRating;
		public decimal[] NumberRating;

		#endregion GordanRanks

		public decimal StartingPowerRating { get; set; }

		public decimal PowerRating { get; set; }

		//  Nibble Ranks
		private int[,] _nibbleRating;

		//  Team card

		public ICachePlayers PlayerCache { get; set; }

		public ArrayList ProjectionList;

		//
		//  Starting Spots on the team, loaded when adding players to the team
		//
		private ArrayList _spotList;

		#region Constructors

		public NflTeam()
		{
			//  dummy constructor for testing only
		}

		/// <summary>
		///    Constructor for the NFL Team object.
		/// </summary>
		/// <param name="nameIn"></param>
		/// <param name="divIn"></param>
		/// <param name="codeIn"></param>
		/// <param name="catIn"></param>
		/// <param name="shortNameIn"></param>
		/// <param name="seasonIn"></param>
		public NflTeam( string nameIn, string divIn, string codeIn, string catIn, string shortNameIn,
					   string seasonIn )
		{
			PlayersLost = "";
			PlayersGot = "";
			// Constructor for old grids
			PlayerList = new ArrayList();
			//RosterLib.Utility.Announce("  6. Old Constructor Team " + codeIn);

			Season = seasonIn;
			Name = nameIn;
			NickName = shortNameIn;
			Div = divIn;
			TeamCode = codeIn;

			LoadOldGrid( catIn, "" );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NflTeam"/> class.
		/// Very quick.
		/// </summary>
		/// <param name="codeIn">The code in.</param>
		public NflTeam( string codeIn )
		{
			if ( string.IsNullOrEmpty( codeIn ) ) return;
			if ( codeIn == "??" )
			{
				TeamCode = codeIn;
				return;
			}

			if ( codeIn == "BYE" )
			{
				TeamCode = "BYE";
				Ratings = "XXXXXX";
			}
			else
			{
				PlayersLost = "";
				PlayersGot = "";
#if DEBUG
				//Utility.Announce( "  1. Old Constructor Team " + codeIn );
#endif
				PlayerList = new ArrayList();
				TeamCode = codeIn;
				Season = Utility.CurrentSeason();

				var teamData = Utility.TflWs.TeamDataFor( codeIn, Season );
				GetTeamValuesFromData( teamData );
				_spotList = new ArrayList();
				InjuryList = new ArrayList();
				FaList = new ArrayList();
				StarterList = new ArrayList();
				SetRecord( Season, skipPostseason: false );
				SetRatings();
				if ( Ratings.Substring( 0, 1 ) == "?" ) GetTeamValuesFromData( teamData );
			}
		}

		private void SetRatings()
		{
			var urs = new UnitRatingsService2();
			Ratings = urs.GetUnitRatingsFor( TeamCode, DateTime.Now );
		}

		//  Do not use a single K for place kickers now
		public NFLPlayer SetKicker()
		{
			var ds = Utility.TflWs.GetPlayer(
			   TeamCode, strRole: Constants.K_ROLE_STARTER, strPos: "PK" );

			if ( ds.Tables[ 0 ].Rows.Count == 1 )
			{
				var kickerId = ds.Tables[ 0 ].Rows[ 0 ][ "PLAYERID" ].ToString();
				Kicker = new NFLPlayer( kickerId );
			}
			else if ( ds.Tables[ 0 ].Rows.Count > 1 )
			{
				Utility.Announce( string.Format( "Too many Kickers found for {0}", Name ) );
				var dt = ds.Tables[ 0 ];
				foreach ( DataRow dr in dt.Rows )
				{
					Utility.Announce( string.Format( "   {0} {1}", dr[ "PLAYERID" ].ToString(), dr[ "POSDESC" ].ToString() ) );
				}
			}
			else
				Utility.Announce( string.Format( "No Kicker found for {0}", Name ) );
			return Kicker;
		}

		public NflTeam( string codeIn, string seasonIn )
		{
			PlayersLost = "";
			PlayersGot = "";
			//RosterLib.Utility.Announce(string.Format("  2. Constructing team {0} for {1}", codeIn, seasonIn));

			PlayerList = new ArrayList();
			Season = seasonIn;
			TeamCode = codeIn;
			var dr = Utility.TflWs.TeamDataFor( codeIn, seasonIn );
			GetTeamValuesFromData( dr );
			_spotList = new ArrayList();

			if ( Config.DoProjections() || Config.DoMatchups() )
			{
				//  Load Schedule
				LoadSchedule();
				ProjectionList = new ArrayList();
			}

			if ( Config.ReportType == "New" )
			{
				//  New stuff
				SetRecord( seasonIn, skipPostseason: false );

				//  Team record

				LoadGames( codeIn, seasonIn );
				if ( Config.DoExperience() )
				{
					if ( Utility.Wz == null ) Utility.Wz = new WizSeason( Utility.LastSeason(), "01", "17" );
					Matrix = Utility.Wz.GetMatrix( TeamCode );
				}
			}

			InitialiseRatings();
		}

		private void GetTeamValuesFromData( DataRow dr )
		{
			if ( dr == null ) return;
			Name = string.Format( "{0} {1}",
								  dr[ "CITY" ].ToString().Trim(), dr[ "TEAMNAME" ].ToString().Trim() );
			PowerRating = Decimal.Parse( dr[ "POWER" ].ToString() );
			StartingPowerRating = Decimal.Parse( dr[ "POWER" ].ToString() );
			Ratings = dr[ "RATE" ].ToString();
			IsPlayoffBound = dr[ "PLAYOFFS" ].ToString().Trim().Length > 0;
			ApCode = dr[ "APCODE" ].ToString();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NflTeam"/> class.
		/// Quick constructor.
		/// Used for Victory points.
		/// </summary>
		/// <param name="codeIn">The code in.</param>
		/// <param name="season">The season.</param>
		/// <param name="wins">The wins.</param>
		/// <param name="name">The name.</param>
		public NflTeam( string codeIn, string season, int wins, string name )
		{
			PlayersLost = "";
			PlayersGot = "";
			//  Constructor for Victory Points
			//RosterLib.Utility.Announce("  5. VP Constructor Team " + codeIn);
			PlayerList = new ArrayList();
			TeamCode = codeIn;
			Name = name;
			Season = season;
			Wins = wins;
			LoadGames( codeIn, season );
		}

		public NflTeam( string nameIn, string divIn, string codeIn, string shortNameIn,
					   string seasonIn )
		{
			PlayersLost = "";
			PlayersGot = "";
			//
			//  constructor for new roster 5 paras
			//
			//RosterLib.Utility.Announce("  5. Constructing New Roster Team " + codeIn);
			PlayerList = new ArrayList();

			Season = seasonIn;
			Name = nameIn;
			NickName = shortNameIn;
			Div = divIn;
			TeamCode = codeIn;

			if ( nameIn == "" ) Name = Utility.TflWs.TeamFor( codeIn, seasonIn );

			_spotList = new ArrayList();

			if ( Config.DoProjections() && ( Season != "" ) )
				InitialiseProjections();

			//  New stuff
			SetRecord( seasonIn, skipPostseason: false );

			//  Team record
			GameList = new ArrayList();
			LoadGames( codeIn, seasonIn );

			if ( Config.DoExperience() || Config.DoMatchups() )
			{
				if ( Utility.Wz == null )
					Utility.Wz = new WizSeason( Utility.LastSeason(), "01", "17" );
				Matrix = Utility.Wz.GetMatrix( TeamCode );
			}

			LoadDivisionalOpponents();

			SetDefence();

			LoadTeamCard();
		}

		public NflTeam( XmlNode node )
		{
			PlayersLost = "";
			PlayersGot = "";
			//RosterLib.Utility.Announce( "Instantiating new Team via xml" );
			if ( node.Attributes != null )
			{
				Season = node.Attributes[ "season" ].Value;
				TeamCode = node.Attributes[ "id" ].Value;
				Name = node.Attributes[ "name" ].Value.Trim();
				Div = node.Attributes[ "division" ].Value;
			}
			InitialiseRatings();
			//RosterLib.Utility.Announce( string.Format( "Instantiated {0} via xml", Name ) );
		}

		#endregion Constructors

		private void InitialiseRatings()
		{
			LetterRating = new String[ Constants.K_WEEKS_IN_A_SEASON + 1 ];
			NumberRating = new decimal[ Constants.K_WEEKS_IN_A_SEASON + 1 ];
			_nibbleRating = new int[ Constants.K_WEEKS_IN_A_SEASON + 1, 2 ];
			for ( var w = 0; w < Constants.K_WEEKS_IN_A_SEASON; w++ )
			{
				LetterRating[ w ] = "?";
				NumberRating[ w ] = 0.0M;
				NibbleRating[ w, 0 ] = 0;
				NibbleRating[ w, 1 ] = 0;
			}
		}

		public void LoadOldGrid( string catIn, string strPos )
		{
			//RosterLib.Utility.Announce( "Loading starters");
			LoadStarters( catIn, strPos );

			//RosterLib.Utility.Announce( "Loading backups");
			LoadBackups( catIn, strPos );

			//RosterLib.Utility.Announce( "Loading others");
			LoadOthers( catIn, strPos );
#if DEBUG
			Utility.Announce( "Loading Injuries");
#endif
			LoadInjuries( catIn, strPos );
		}

		public void SetDefence()
		{
			//  34 defence?
			//         NFLPlayer nosetackle = GetPlayerAt( "NT", 1, true, false );
			Uses34Defence = DetermineDefenceAlignment();
			//         RosterLib.Utility.Announce( Uses34Defence ? " 3-4 Defence " : " 4-3 Defence " );
		}

		#region Points

		public void CountVictoryPoints()
		{
			//  process game list
			foreach ( NFLGame g in GameList )
			{
				var gamePoints = 0;
				if ( !g.Played() ) continue;

				var opp = ( g.HomeTeam == TeamCode ) ? g.AwayTeam : g.HomeTeam;
				var ha = ( g.HomeTeam == TeamCode ) ? "v" : "@";
				var res = g.ResultOut( TeamCode, false ).Substring( 0, 3 );
				if ( res == "Won" )
				{
					if ( g.AwayNflTeam == null ) g.GetAwayTeam();
					if ( g.HomeNflTeam == null ) g.GetHomeTeam();

					if ( ( g.AwayNflTeam != null ) && ( g.HomeNflTeam != null ) )
						gamePoints = ( g.HomeTeam == TeamCode ) ? g.AwayNflTeam.Wins : g.HomeNflTeam.Wins;
					else
						gamePoints = 0;
				}
				VictoryPoints += gamePoints;

				Utility.Announce( string.Format( "    Tm:{5} Wk {0} res {1} {3}{4} pts {2}",
											   g.Week, res, gamePoints, ha, opp, TeamCode ) );
			}
		}

		public void CountFaPoints( string seasonIn )
		{
			// Load the moves for the team from the start of the year to the beginning of the season

			const int nMoves = 0;

			var ds = Utility.TflWs.MovesDs( TeamCode, DateTime.Parse( "01/01/" + seasonIn ), DateTime.Parse( "01/09/" + seasonIn ) );
			// Process each move
			var dt = ds.Tables[ "SERVE" ];
			foreach ( DataRow dr in dt.Rows )
			{
				if ( dr.RowState != DataRowState.Deleted )
				{
					//nMoves++;
					var playerId = dr[ "PLAYERID" ].ToString();

					var plyr = new NFLPlayer( playerId );
					if ( plyr.PlayerCat != null )
					{
						string moveType;
						if ( MoveOut( dr[ "TO" ].ToString() ) )
						{
							moveType = "out";
							FaPoints -= plyr.Value();
							PlayersLost += FormatPlayer( plyr ) + "<br>";
							PlayersOut++;
						}
						else
						{
							moveType = "in ";
							FaPoints += plyr.Value();
							PlayersGot += FormatPlayer( plyr ) + "<br>";
							PlayersIn++;
						}
						Utility.Announce(
						   string.Format( "Player {0,-20} {7,-6} {6} From {1} To {2} {3,-15} POINTS = {4,3} TOTAL = {5,4}",
										 plyr.PlayerName, dr[ "FROM" ].ToString().Substring( 0, 10 ),
										 dr[ "TO" ].ToString().Substring( 0, 10 ),
										 dr[ "HOW" ].ToString().Trim(), plyr.Value(), FaPoints, moveType, plyr.PlayerPos ) );
					}
				}
			}
			Utility.Announce( string.Format( " {0} moves processed for {1}  IN:{2,3}  OUT:{3,3}={4,4}",
									  nMoves, Name, PlayersIn, PlayersOut, PlayersIn - PlayersOut ) );
		}

		#endregion Points

		private static string FormatPlayer( NFLPlayer plyr )
		{
			return string.Format( "{0},{1}{2}", plyr.PlayerName, plyr.PlayerPos.Trim(), string.Format( "{0,4}", plyr.Value() ) );
		}

		private static bool MoveOut( string dateTo )
		{
			if ( dateTo.Length > 0 )
			{
				if ( dateTo.Substring( 0, 10 ) == "30/12/1899" )
					return false;

				return true;
			}
			return false;
		}

		public void LoadTeamCard()
		{
			TeamCard = new TeamCard( this );
		}

		#region Projections

		private void InitialiseProjections()
		{
			//  Load Schedule
			var lastSeason = Int32.Parse( Season ) - 1;
#if DEBUG
			Utility.Announce( "NFLTeam.InitialiseProjections: Loading schedule for " + lastSeason.ToString() );
#endif
			_sched = new NFLSchedule( Season, this );
			_prevSched = new NFLSchedule( lastSeason.ToString(), this );
			ProjectionList = new ArrayList();
		}

		public void ProjectNextWeek()
		{
			if ( _myCat == null )
			{
				_myCat = new DefensiveTeamCategory();
			}
			_myCat.ProjectNextWeek( this );
		}

		/// <summary>
		///   Will find out who the starting RB or QB is.
		///   Called by Projections to show you who is going to
		///   score the most TDs in the upcoming season.
		/// </summary>
		/// <param name="positionIn"></param>
		/// <returns>A player</returns>
		public NFLPlayer GetCurrentStarter( string positionIn )
		{
			StarterList = null;
			StarterList = new ArrayList();
			LoadStarters( positionIn.Equals( "QB" ) ? "1" : "2", positionIn );

			NFLPlayer player = null;
			var myEnumerator = StarterList.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var p = ( NFLPlayer ) myEnumerator.Current;
				if ( p.PlayerPos.Trim().Equals( positionIn ) )
				{
					player = p;
					break;
				}
			}
			return player;
		}

		#endregion Projections

		public UnitMatrix GetMatrix()
		{
			// generate it if it is not there, but first see if you have it in xml
			//RosterLib.Utility.Announce( "NFLTeam.GetMatrix" );
			if ( Matrix == null )
			{
				Matrix = LoadMatrixFromXml();

				if ( Matrix == null )
				{
					if ( Utility.Wz == null )
						Utility.ExperiencePoints( Utility.CurrentWeek() == "00"
													? Utility.LastSeason()
													: Utility.CurrentSeason() );
					if ( Utility.Wz != null )
						Matrix = Utility.Wz.GetMatrix( TeamCode );
				}
			}
			return Matrix;
		}

		private UnitMatrix LoadMatrixFromXml()
		{
			return Masters.Tepm.GetMatrixFor( TeamCode );
		}

		private void LoadDivisionalOpponents()
		{
			//RosterLib.Utility.Announce( "     Loading Divisional Opponents" );
			_divOpponentList = new ArrayList();

			if ( String.IsNullOrEmpty( Div ) )
				Div = Utility.TflWs.GetDivFor( TeamCode, Season );

			var ds = Utility.TflWs.GetTeams( Season, Div );
			var dt = ds.Tables[ 0 ];
			foreach ( DataRow dr in dt.Rows )
				_divOpponentList.Add( dr[ "TEAMID" ].ToString() );
		}

		public void SetRecord( string seasonInFocus, bool skipPostseason )
		{
			var ds = Utility.TflWs.TeamRecord( TeamCode, seasonInFocus );
			var dt = ds.Tables[ "team" ];
			if ( dt.Rows.Count == 1 )
			{
				var dr = dt.Rows[ 0 ];
				PtsFor = Int32.Parse( dr[ "PTSFOR" ].ToString() );
				PtsAgin = Int32.Parse( dr[ "AGAINST" ].ToString() );
				Wins = Int32.Parse( dr[ "WINS" ].ToString() );
				Losses = Int32.Parse( dr[ "LOSSES" ].ToString() );
				Ties = Int32.Parse( dr[ "TIES" ].ToString() );
				_clip = Decimal.Parse( dr[ "CLIP" ].ToString() );
				OffRate = Int32.Parse( dr[ "OFFENSE" ].ToString() );
				DefRate = Int32.Parse( dr[ "DEFENSE" ].ToString() );
				Ratings = dr[ "RATE" ].ToString();
				//spreadRange = Int32.Parse( dr[ "SRANGE" ].ToString() );
			}
			//  Total Scores?
			if ( Tdp == 0 ) Tdp = CalculateScoreType( Constants.K_SCORE_TD_PASS, seasonInFocus, skipPostseason );
			if ( Tdr == 0 ) Tdr = CalculateScoreType( Constants.K_SCORE_TD_RUN, seasonInFocus, skipPostseason );
			if ( Fg == 0 ) Fg = CalculateScoreType( Constants.K_SCORE_FIELD_GOAL, seasonInFocus, skipPostseason );
			if ( string.IsNullOrEmpty( Ratings ) ) Ratings = "CCCCCC";
		}

		private int CalculateScoreType( string scoretype, string seasonIn, bool skipPostseason )
		{
			var scores = skipPostseason ?
			   Utility.TflWs.GetTeamRegularSeasonScoresFor( scoretype, TeamCode, seasonIn )
			   : Utility.TflWs.GetTeamScoresFor( scoretype, TeamCode, seasonIn );
			return scores.Tables[ 0 ].Rows.Count;
		}

		public string RecordOut()
		{
			SetRecord( Utility.CurrentSeason(), skipPostseason: false );
			return HtmlLib.HTMLPadL( PtsFor.ToString(), 3 ) + "-" +
				   HtmlLib.HTMLPadL( PtsAgin.ToString(), 3 ) + "  " +
				   "(" + HtmlLib.HTMLPadL( Wins.ToString(), 2 ) + "-" +
				   HtmlLib.HTMLPadL( Losses.ToString(), 2 ) + "-" +
				   HtmlLib.HTMLPadL( Ties.ToString(), 2 ) + ") " +
				   string.Format( "{0:0.##0}", _clip ) + " " +
				   string.Format( "{0}-{1}", _ratings, RatingPts() ) + " [" +
				   HtmlLib.HTMLPadL( OffRate.ToString(), 3 ) +
				   HtmlLib.HTMLPadL( DefRate.ToString(), 4 ) + "]  " +
				   HtmlLib.HTMLPadL( Tdp.ToString(), 3 ) + "  " +
				   HtmlLib.HTMLPadL( Tdr.ToString(), 3 ) + "  " +
				   HtmlLib.HTMLPadL( Fg.ToString(), 3 )
			   ;
		}

		public string RatingPtsOut()
		{
			return string.Format( "{0}-{1}", Ratings, RatingPts() );
		}

		public string RatingsOut()
		{
			//if (string.IsNullOrEmpty(_ratings)
			var spreadoutRatings = SpreadoutRatings();
			return string.Format( "{0} : {1}", spreadoutRatings, RatingPts() );
		}

		public string ScheduleHeaderOut()
		{
			var sb = new StringBuilder();
			for ( var i = 0; i < 17; i++ )
			{
				sb.Append( string.Format( "|   Week {0:0#}   ", i + 1 ) );
			}
			return sb.ToString();
		}

		public string ScheduleOut()
		{
			if ( GameList == null ) LoadGames( TeamCode, Season );

			var sb = new StringBuilder();
			for ( var i = 1; i < 18; i++ )
			{
				var g = GameForWeek( i );
				if ( !g.Played() ) g.LoadPrediction();
				sb.Append( g.GameLink( g.ResultFor( TeamCode, abbreviate: true ) ) );
			}
			return sb.ToString();
		}

		public string ScoreCountsOut()
		{
			if ( GameList == null ) LoadGames( TeamCode, Season );

			var sb = new StringBuilder();
			for ( var i = 1; i < 18; i++ )
			{
				var g = GameForWeek( i );
				if ( !g.Played() ) g.LoadPrediction();
				sb.Append( g.ScoreCountsFor( TeamCode ) );
			}
			return sb.ToString();
		}

		public string SeasonProjectionOut()
		{
			if ( GameList == null ) LoadGames( TeamCode, Season );
			var w = 0;
			var l = 0;
			var t = 0;
			var tdp = 0;
			var tdr = 0;
			var tdd = 0;
			var tds = 0;
			var fg = 0;
			var ptsfor = 0;
			var ptsagin = 0;

			for ( var i = 1; i < 18; i++ )
			{
				var g = GameForWeek( i );
				if ( !g.Played() ) g.LoadPrediction();
				if ( g.IsBye() ) continue;
				var res = g.ResultOut( TeamCode, true );
				switch ( res )
				{
					case "W":
						w++;
						break;

					case "L":
						l++;
						break;

					default:
						t++;
						break;
				}
				if ( g.HomeTeam.Equals( TeamCode ) )
				{
					tdp += g.ProjectedHomeTdp;
					tdr += g.ProjectedHomeTdr;
					tdd += g.ProjectedHomeTdd;
					tds += g.ProjectedHomeTds;
					fg += g.ProjectedHomeFg;
					ptsfor += g.HomeScore;
					ptsagin += g.AwayScore;
				}
				else
				{
					tdp += g.ProjectedAwayTdp;
					tdr += g.ProjectedAwayTdr;
					tdd += g.ProjectedAwayTdd;
					tds += g.ProjectedAwayTds;
					fg += g.ProjectedAwayFg;
					ptsfor += g.AwayScore;
					ptsagin += g.HomeScore;
				}
			}
			return string.Format( "Projection season {3}  ({0,2}-{1,2}-{2,1})  ({4}-{5})", w, l, t, Season, ptsfor, ptsagin ) + "    " +
				string.Format( "({0,2}-{1,2}-{2,2}-{3,2}-{4,2})  Avg res: {5,2}-{6,2}", tdp, tdr, tdd, tds, fg,
				AverageInt( ptsfor, 16 ), AverageInt( ptsagin, 16 ) );
		}

		private NFLGame GameForWeek( int w )
		{
			var gameLocated = new NFLGame();
			foreach ( var t in from NFLGame t in GameList where t.WeekNo == w select t )
			{
				gameLocated = t;
				break;
			}
			return gameLocated;
		}

		public string SpreadoutRatings()
		{
			//  C  B  E  -  A  A  B
			if ( string.IsNullOrEmpty( _ratings ) || _ratings.Length < 6 ) return "??????";
			return string.Format( "{0} {1} {2} - {3} {4} {5}",
			   POUnitLink( PoRating() ),
			   ROUnitLink(RoRating()),
			   PPUnitLink(PpRating()),
			   PRUnitLink(PrRating()),
			   RDUnitLink(RdRating()),
			   PDUnitLink( PdRating() )
			   );
		}

		/// <summary>
		///   What is the teams spread range as of a particular week
		///   Spread range tracks how teams are doing against the spread.
		///
		/// </summary>
		/// <param name="seasonIn"></param>
		/// <param name="week"></param>
		/// <returns></returns>
		public decimal SpreadRange( string seasonIn, string week )
		{
			var totSpreadRange = 0.0M;
			// load the schedle
			_sched = new NFLSchedule( seasonIn, this );
			// for each game tally the spreadRange
			foreach ( NFLGame g in _sched.GameList )
			{
				var myMargin = g.MarginFor( this );
				var expMargin = g.ExpMarginFor( this );

				var sr = DanGordan.SpreadRangeScore( myMargin, expMargin );
				totSpreadRange += sr;
				if ( g.Week.Equals( week ) ) break; //  thats enough
			}
			return totSpreadRange;
		}

		public string RatingPts()
		{
			if ( string.IsNullOrEmpty( Ratings ) ) SetRecord( Utility.LastSeason(), skipPostseason: false );
			int i;
			int nPts = 0;
			for ( i = 0; i < 6; i++ )
			{
				string c = _ratings.Substring( i, 1 );
				switch ( c )
				{
					case "A":
						nPts += 10;
						break;

					case "B":
						nPts += 7;
						break;

					case "C":
						nPts += 5;
						break;

					case "D":
						nPts += 3;
						break;

					case "E":
						nPts += 0;
						break;
				}
			}
			return string.Format( "{0}", nPts );
		}

		public bool IsDivisionalOpponent( string opponentCode )
		{
			bool bIsDivOpponent = false;
			if ( _divOpponentList == null ) LoadDivisionalOpponents();

			if ( _divOpponentList != null )
			{
				// LINQ query replaced this
				//for ( int i = 0; i < _divOpponentList.Count; i++ )
				//{
				//   string oppCode = (string) _divOpponentList[ i ];
				//   if ( oppCode == opponentCode )
				//   {
				//      bIsDivOpponent = true;
				//      break;
				//   }
				//}
				bIsDivOpponent = _divOpponentList.Cast<string>().Any( oppCode => oppCode == opponentCode );
			}
			return bIsDivOpponent;
		}

		public string TeamUrl()
		{
			return string.Format( "<a href =\"./TeamCards/{1}/TeamCard_{0}.htm\">{0}</a>", TeamCode, Season );
		}

		#region Schedule stuff

		public NflTeam OpponentFor( string seasonIn, int week )
		{
			if ( _sched == null ) LoadSchedule();

			if ( _sched != null && _sched.GameList == null ) _sched.Load();

			if ( _sched == null ) return null;

			var opponent = seasonIn == Utility.CurrentSeason() ? _sched.Opponent( week ) : _prevSched.Opponent( week );
			return opponent;
		}

		public NFLGame GameFor( string seasonIn, int week )
		{
			if ( _sched == null ) LoadSchedule();
			var g = seasonIn == Utility.CurrentSeason() ? _sched.Game( week ) : _prevSched.Game( week );
			return g;
		}

		public void LoadSchedule()
		{
			if ( Season == null ) Season = Utility.CurrentSeason();
			var lastSeason = Int32.Parse( Season ) - 1;
			//RosterLib.Utility.Announce(string.Format("NFLTeam.LoadSchedule:Instantiating Schedule for {0}", season));
			_sched = new NFLSchedule( Season, this );
			//RosterLib.Utility.Announce(string.Format("NFLTeam.LoadSchedule:Instantiating Schedule for {0}", lastSeason));
			_prevSched = new NFLSchedule( lastSeason.ToString(), this );
		}

		/// <summary>
		///   Returns a number representing how strong the teams opponents
		///   are based on the oppnents results last year.
		/// </summary>
		/// <returns>a clip number W-L clip of opponents</returns>
		public double StrengthOfSchedule()
		{
			var oppWins = 0;
			var oppLosses = 0;

			if ( SoS != 0.000D ) return SoS;

			//  set my record last year

			SetRecord( Utility.LastSeason(), skipPostseason: false );
			var myClip = Utility.Clip( Wins, Losses, Ties );
#if DEBUG
			Utility.Announce( string.Format( "---{0}----------------------------------------------------------------------------------", TeamCode ) );
			Utility.Announce( string.Format( "set {1} record last year {0} {2}-{3}-{4} {5:0.##0}",
			   Utility.LastSeason(), this, Wins, Losses, Ties, Utility.Clip( Wins, Losses, Ties ) ) );
#endif
			//  Calculate it
			foreach ( NFLGame g in GameList )
			{
#if DEBUG
				string res;
#endif
				var opp = ( g.HomeTeam == TeamCode ) ? g.AwayTeam : g.HomeTeam;
				var ha = ( g.HomeTeam == TeamCode ) ? "v" : "@";
				var opponent = new NflTeam( opp );
				opponent.SetRecord( Utility.LastSeason(), skipPostseason: false );
				oppWins += opponent.Wins;
				oppLosses += opponent.Losses;
				var oppClip = Utility.Clip( opponent.Wins, opponent.Losses, opponent.Ties );

				//  Calculate expected result based on this teams clip
				if ( myClip > oppClip )
				{
					ExpWins++;
#if DEBUG
					res = "WIN";
#endif
				}
				else
				{
					if ( myClip == oppClip )
					{
						if ( ha == "v" )
						{
							ExpWins++;
#if DEBUG
							res = "WIN";
#endif
						}
						else
						{
							ExpLosses++;
#if DEBUG
							res = "LOSS";
#endif
						}
					}
					else
					{
						ExpLosses++;
#if DEBUG
						res = "LOSS";
#endif
					}
				}

#if DEBUG
				Utility.Announce( string.Format( "    Wk {0}  {7}{1} {2,2}-{3,2} {4,3}-{5,3} {6}",
				   g.Week, opponent.Name, opponent.Wins, opponent.Losses, oppWins,
				   oppLosses, res,
				   ha ) );
#endif
			}
			SoS = ( double ) oppWins / ( oppWins + oppLosses );
#if DEBUG
			Utility.Announce(
			   string.Format( "{0} ({5,2}-{6,2} {4:0.##0}) Strength of schedule {1:0.##0} exp record {2,2}-{3,2}",
				  Name.Trim(), SoS, ExpWins, ExpLosses, myClip, Wins, Losses ) );
#endif

			return SoS;
		}

		public void CalculateDefensiveScoring( ICalculate myCalculator, [Optional] bool doOpponent )
		{
			GameList = LoadGamesFrom( myCalculator.StartWeek.Season, myCalculator.StartWeek.Week, myCalculator.Offset );

			PtsFor = 0;
			PtsAgin = 0;
			decimal totFantasyPoints = 0;
			var totDefensiveScores = 0;
			decimal totTotSacks = 0;
			var totTotInterceptions = 0;
			var totGames = 0;
			var totPointsAgin = 0;

			foreach ( NFLGame g in GameList )
			{
#if DEBUG
				Utility.Announce( string.Format( "  {0} Opponent in week {1} of {3} is {2}",
				   TeamCode, g.Week, g.OpponentTeam( TeamCode ).Name, g.Season ) );
#endif
				var theTeam = ( doOpponent ? g.OpponentTeam( TeamCode ) : g.Team( TeamCode ) );

				myCalculator.Calculate( theTeam, g );

				totFantasyPoints += theTeam.FantasyPoints;
				totDefensiveScores += theTeam.DefensiveScores;
				totTotSacks += theTeam.TotSacks;
				totTotInterceptions += theTeam.TotInterceptions;
				totPointsAgin += theTeam.PtsAgin;
				totGames++;

#if DEBUG
				if ( doOpponent )
					Utility.Announce(
					   string.Format(
						  "    {5} defense against {4} racks up {0,3:##0} Fpts and {1} Defensive Scores {2} Sacks {3} intercepts, scoring {6} points",
						  theTeam.FantasyPoints,
						  theTeam.DefensiveScores,
						  theTeam.TotSacks,
						  theTeam.TotInterceptions,
						  TeamCode,
						  g.OpponentTeam( TeamCode ).TeamCode,
						  theTeam.PtsAgin ) );
				else
					Utility.Announce(
					   string.Format(
						  "    {4} defense against {5} (score {7}) racks up {0,3:##0} Fpts and {1} Defensive Scores {2} Sacks {3} intercepts, allowing {6} points",
						  theTeam.FantasyPoints,
						  theTeam.DefensiveScores,
						  theTeam.TotSacks,
						  theTeam.TotInterceptions,
						  TeamCode,
						  g.OpponentTeam( TeamCode ).TeamCode,
						  theTeam.PtsAgin,
						  g.ScoreOut( TeamCode ) ) );
#endif
			}

			PtsAgin = totPointsAgin;
			FantasyPoints = totFantasyPoints;
			DefensiveScores = totDefensiveScores;
			TotSacks = totTotSacks;
			TotInterceptions = totTotInterceptions;
			Games = totGames;

#if DEBUG
			if ( doOpponent )
				Utility.Announce(
				   string.Format(
					  "{0} has allowed defenses to get {1,3:##0} Fpts on {2} Defensive Scores, {3} Sacks and {4} intercepts, scoring {5} points",
					   TeamCode, FantasyPoints, DefensiveScores, TotSacks, TotInterceptions, PtsAgin ) );
			else
				Utility.Announce(
				   string.Format(
					  "{0} defense got {1,3:##0} Fpts on {2} Defensive Scores, {3} Sacks and {4} intercepts, allowing {5} points",
					   TeamCode, FantasyPoints, DefensiveScores, TotSacks, TotInterceptions, PtsAgin ) );
#endif
		}

		#endregion Schedule stuff

		#region Accessors

		public decimal CurrentClip
		{
			get { return _clip; }
			set { _clip = value; }
		}

		public int Wins { get; set; }

		public int Losses { get; set; }

		public string OffensiveRating()
		{
			return Ratings.Substring( 0, 3 );
		}

		public string DefensiveRating()
		{
			return Ratings.Substring( 3, 3 );
		}

		public string DefensiveRating( string catCode )
		{
			return catCode == Constants.K_RUNNINGBACK_CAT ? RdRating() : PdRating();
		}

		public string OffensiveRating( string catCode )
		{
			var offRating = PoRating();

			switch ( catCode )
			{
				case Constants.K_QUARTERBACK_CAT:
					offRating = PoRating();
					break;

				case Constants.K_RUNNINGBACK_CAT:
					offRating = RoRating();
					break;

				case Constants.K_RECEIVER_CAT:
					offRating = PoRating();
					break;

				case Constants.K_KICKER_CAT:
					offRating = PoRating();
					break;
			}

			return offRating;
		}

		public string DefensiveUnit( string catCode )
		{
			return catCode == Constants.K_RUNNINGBACK_CAT ? RDUnitLink( RdRating() ) : PDUnitLink( PdRating() );
		}

		public string DefensiveUnitMatchUp( string catCode, string matchUp )
		{
			return catCode == Constants.K_RUNNINGBACK_CAT
			   ? RDUnitLink( matchUp ) : PDUnitLink( matchUp );
		}

		private string PDUnitLink( string pdRating )
		{
			return string.Format( "<a href='..\\Units\\PassDef\\PD-{0}.htm'>{1}</a>", TeamCode, pdRating );
		}

		private string RDUnitLink( string rdRating )
		{
			return string.Format( "<a href='..\\Units\\RunDef\\RD-{0}.htm'>{1}</a>", TeamCode, rdRating );
		}

		private string PRUnitLink( string rating )
		{
			return string.Format( "<a href='..\\Units\\PassRush\\PR-{0}.htm'>{1}</a>",
				TeamCode, rating );
		}

		private string PPUnitLink( string rating )
		{
			return string.Format( "<a href='..\\Units\\Prot\\PP-{0}.htm'>{1}</a>", 
				TeamCode, rating );
		}

		private string POUnitLink( string rating )
		{
			return string.Format( "<a href='..\\Units\\PassOff\\PO-{0}.htm'>{1}</a>",
				TeamCode, rating );
		}

		private string ROUnitLink( string rating )
		{
			return string.Format( "<a href='..\\Units\\RunOff\\RO-{0}.htm'>{1}</a>",
				TeamCode, rating );
		}

		public string PoRating()
		{
			return Ratings.Substring( 0, 1 );
		}

		public string RoRating()
		{
			return Ratings.Substring( 1, 1 );
		}

		public string RdRating()
		{
			return Ratings.Substring( 4, 1 );
		}

		public string PdRating()
		{
			return Ratings.Substring( 5, 1 );
		}

		public string PrRating()
		{
			return Ratings.Substring( 3, 1 );
		}

		public string PpRating()
		{
			return Ratings.Substring( 2, 1 );
		}

		public bool IsGoodOffence()
		{
			var isGood = false;

			if ( PoRating().Equals( "A" ) || PoRating().Equals( "B" ) )
				isGood = true;
			else if ( RoRating().Equals( "A" ) || RoRating().Equals( "B" ) )
				isGood = true;

			return isGood;
		}

		public bool IsGoodDefence()
		{
			var isGood = false;

			if ( PdRating().Equals( "A" ) || PdRating().Equals( "B" ) )
				isGood = true;
			else if ( RdRating().Equals( "A" ) || RdRating().Equals( "B" ) )
				isGood = true;

			return isGood;
		}

		public bool HasGoodProtection()
		{
			var isGood = PpRating().Equals( "A" ) || PpRating().Equals( "B" );
			return isGood;
		}

		public int VictoryPoints
		{
			get { return MVictoryPoints; }
			set { MVictoryPoints = value; }
		}

		public bool Uses34Defence { get; set; }

		public TeamCard TeamCard { get; set; }

		public string Division()
		{
			if ( String.IsNullOrEmpty( Div ) )
				Div = Utility.TflWs.GetDivFor( TeamCode, Season );
			return Div;
		}

		public string NickName { get; set; }

		public string Nick()
		{
			var nick = "";
			var nPoint = Name.IndexOf( ' ' );
			if ( nPoint > -1 )
				nick = Name.Substring( nPoint, Name.Length - nPoint ).TrimStart();

			var nPoint2 = nick.IndexOf( ' ' );
			if ( nPoint2 > -1 )
				nick = nick.Substring( nPoint2, nick.Length - nPoint2 ).TrimStart();

			return nick;
		}

		public string City()
		{
			const string nick = "";
			string city = "";
			int nPoint = Name.IndexOf( ' ' );
			if ( nPoint > -1 )
				city = Name.Substring( 0, nPoint );

			int nPoint2 = nick.IndexOf( ' ' );
			if ( nPoint2 > -1 )
				city = Name.Substring( 0, nPoint + nPoint2 + 1 );

			return city;
		}

		public decimal Clip()
		{
			return Utility.Clip( Wins, Losses, MTies );
		}

		public int GamesPlayed()
		{
			return Wins + Losses + MTies;
		}

		public NFLGame PreviousGame( DateTime when )
		{
			NFLGame g = null;
			var dr = Utility.TflWs.GetGamePriorTo( TeamCode, when );
			if ( dr != null ) g = new NFLGame( dr );
			return g;
		}

		public string NextOpponent( DateTime when )
		{
			var g = NextGame( when );
			var opp = g.OpponentOut( TeamCode );
			return opp;
		}

		public NFLGame NextGame( DateTime when )
		{
			NFLGame g = null;
			var dr = Utility.TflWs.GetGameAfter( TeamCode, when );
			if ( dr != null ) g = new NFLGame( dr );
			return g;
		}

		public NFLGame NextGame()
		{
			NFLGame g = null;
			var dr = Utility.TflWs.GetGameAfter( TeamCode, DateTime.Now );
			if ( dr != null ) g = new NFLGame( dr );
			return g;
		}

		public NFLGame NextGameOrBye()
		{
			NFLGame g = null;
			var dr = Utility.TflWs.GetGameAfter( TeamCode, DateTime.Now.Subtract( new TimeSpan( 0, 15, 0, 0 ) ) );  //  time difference
			if ( dr != null ) g = new NFLGame( dr );
			if ( g.GameDate > DateTime.Now.AddDays( 7 ) )
			{
				// bye situation return null
				g = new NFLGame( "BYE" );
			}
			return g;
		}

		public decimal RecordAfterWin( DateTime since )
		{
			var dt = Utility.TflWs.GetAllGamesDt( TeamCode );
			var winsAfterWin = 0.0M;
			var lossesAfterWin = 0.0M;
			var lastResultWasWin = false;
			foreach ( DataRow dr in dt.Rows )
			{
				if ( DateTime.Parse( dr[ "GameDate" ].ToString() ) > since )
				{
					var game = new NFLGame( dr );
					if ( lastResultWasWin )
					{
						if ( game.Won( this ) )
							winsAfterWin++;
						else
							lossesAfterWin++;
					}
					lastResultWasWin = game.Won( this );
				}
			}
			return ( winsAfterWin / ( winsAfterWin + lossesAfterWin ) );
		}

		public decimal SpreadRecordAfterWin( DateTime since )
		{
			var dt = Utility.TflWs.GetAllGamesDt( TeamCode );
			var winsAfterWin = 0.0M;
			var lossesAfterWin = 0.0M;
			var lastResultWasWin = false;
			foreach ( DataRow dr in dt.Rows )
			{
				if ( DateTime.Parse( dr[ "GameDate" ].ToString() ) <= since ) continue;
				var game = new NFLGame( dr );
				if ( lastResultWasWin )
				{
					if ( game.WonVsSpread( this ) )
						winsAfterWin++;
					else
						lossesAfterWin++;
				}
				lastResultWasWin = game.Won( this ); //  outright win
			}
			var totGames = winsAfterWin + lossesAfterWin;
			if ( totGames == 0.0M ) return 0.0M;
			return ( winsAfterWin / totGames );
		}

		public decimal SpreadRecordAfterLoss( DateTime since )
		{
			var dt = Utility.TflWs.GetAllGamesDt( TeamCode );
			var winsAfterLoss = 0.0M;
			var lossesAfterLoss = 0.0M;
			var spreadRecord = 0.0M;
			var lastResultWasLoss = false;
			foreach ( DataRow dr in dt.Rows )
			{
				if ( DateTime.Parse( dr[ "GameDate" ].ToString() ) <= since ) continue;
				NFLGame game = new NFLGame( dr );
				if ( lastResultWasLoss )
				{
					if ( game.WonVsSpread( this ) )
						winsAfterLoss++;
					else
						lossesAfterLoss++;
				}
				lastResultWasLoss = game.Lost( this ); //  outright loss
			}
			if ( winsAfterLoss + lossesAfterLoss > 0 )
				spreadRecord = winsAfterLoss / ( winsAfterLoss + lossesAfterLoss );
			return spreadRecord;
		}

		public string DueDownWeek()
		{
			string dueDownWeek = "00";
			decimal startingPowerNumber = NumberRating[ 0 ];
			for ( int index = 0; index < Constants.K_WEEKS_IN_REGULAR_SEASON; index++ )
			{
				if ( NumberRating[ index ] >= startingPowerNumber + 5.0M )
				{
					dueDownWeek = string.Format( "{0:0#}", index + 1 );
					break;
				}
			}
			return dueDownWeek;
		}

		public string StarDueDownWeek()
		{
			string starDueDownWeek = "00";
			int dueDownWeek = 0;
			decimal startingPowerNumber = NumberRating[ 0 ];
			for ( int index = 0; index < Constants.K_WEEKS_IN_REGULAR_SEASON; index++ )
			{
				if ( NumberRating[ index ] >= startingPowerNumber + 5.0M )
				{
					dueDownWeek = index + 1;
					break;
				}
			}
			if ( dueDownWeek > 0 )
			{
				if ( !dueDownWeek.Equals( Constants.K_WEEKS_IN_REGULAR_SEASON + 1 ) )
				{
					if ( NumberRating[ dueDownWeek ] >= startingPowerNumber + 5.0M )
					{
						starDueDownWeek = string.Format( "{0:0#}", dueDownWeek + 1 );
					}
				}
			}
			return starDueDownWeek;
		}

		public string DueUpWeek()
		{
			var dueUpWeek = "00";
			var startingPowerNumber = NumberRating[ 0 ];
			for ( var index = 0; index < Constants.K_WEEKS_IN_REGULAR_SEASON; index++ )
			{
				if ( NumberRating[ index ] > startingPowerNumber - 5.0M ) continue;
				dueUpWeek = string.Format( "{0:0#}", index + 1 );
				break;
			}
			return dueUpWeek;
		}

		public string StarDueUpWeek()
		{
			var starDueUpWeek = "00";
			int dueWeek = 0;
			decimal startingPowerNumber = NumberRating[ 0 ];
			for ( int index = 0; index < Constants.K_WEEKS_IN_REGULAR_SEASON; index++ )
			{
				if ( NumberRating[ index ] <= startingPowerNumber - 5.0M )
				{
					dueWeek = index + 1;

					break;
				}
			}
			if ( dueWeek > 0 )
			{
				if ( !dueWeek.Equals( Constants.K_WEEKS_IN_REGULAR_SEASON + 1 ) )
				{
					if ( NumberRating[ dueWeek ] <= startingPowerNumber - 5.0M )
					{
						starDueUpWeek = string.Format( "{0:0#}", dueWeek + 1 );
					}
				}
			}
			return starDueUpWeek;
		}

		public bool IsOutOfContention()
		{
			bool isOut = false;
			if ( GamesPlayed() > 8 )
				if ( Losses > ( GamesPlayed() / 2 ) )
					isOut = true;
			return isOut;
		}

		public bool IsRival( string opponentTeamCode )
		{
			bool isRival = false;
			switch ( TeamCode )
			{
				case "MV":
					if ( opponentTeamCode.Equals( "GB" ) )
						isRival = true;
					break;

				case "GB":
					if ( opponentTeamCode.Equals( "MV" ) )
						isRival = true;
					break;

				case "WR":
					if ( opponentTeamCode.Equals( "DC" ) )
						isRival = true;
					break;

				case "DC":
					if ( opponentTeamCode.Equals( "HT" ) )
						isRival = true;
					else if ( opponentTeamCode.Equals( "WR" ) )
						isRival = true;
					break;

				case "HT":
					if ( opponentTeamCode.Equals( "DC" ) )
					{
						isRival = true;
					}
					break;

				case "CL":
					if ( opponentTeamCode.Equals( "PS" ) )
						isRival = true;
					break;

				case "PS":
					if ( opponentTeamCode.Equals( "CI" ) )
						isRival = true;
					break;

				case "BB":
					if ( opponentTeamCode.Equals( "MD" ) )
						isRival = true;
					break;

				case "MD":
					if ( opponentTeamCode.Equals( "BB" ) )
						isRival = true;
					break;

				case "KC":
					if ( opponentTeamCode.Equals( "LC" ) )
						isRival = true;
					break;

				case "LC":
					if ( opponentTeamCode.Equals( "KC" ) )
						isRival = true;
					else if ( opponentTeamCode.Equals( "OR" ) )
						isRival = true;
					break;
			}
			return isRival;
		}

		public string ProperName()
		{
			var sbFullName = new StringBuilder();
			var sbWord = new StringBuilder();

			for ( int i = 0; i < Name.Length; i++ )
			{
				string theChar = Name.Substring( i, 1 );
				if ( theChar.Equals( " " ) )
				{
					sbFullName.Append( Proper( sbWord.ToString() ) );
					sbFullName.Append( " " );
					sbWord.Length = 0; //  clear the word out
				}
				else
					sbWord.Append( theChar );
			}
			sbFullName.Append( Proper( sbWord.ToString() ) );
			return sbFullName.ToString();
		}

		public string ProperNickName()
		{
			return Proper( NickName );
		}

		private static string Proper( string theName )
		{
			var sb = new StringBuilder();
			sb.Append( theName.Substring( 0, 1 ).ToUpper() );
			sb.Append( theName.Substring( 1, theName.Length - 1 ).ToLower() );
			return sb.ToString();
		}

		#endregion Accessors

		#region EP Metrics

		public void SetMetrics( Hashtable ht )
		{
			MetricsHt = ht;
		}

		public decimal AvgOffTDp( int weekSeed )
		{
			IDictionaryEnumerator myEnumerator = MetricsHt.GetEnumerator();
			int tot = 0, nGames = 0;
			while ( myEnumerator.MoveNext() )
			{
				EpMetric ep = ( EpMetric ) myEnumerator.Value;
				if ( ep.WeekSeed <= weekSeed )
				{
					tot += ep.OffTDp;
					nGames++;
				}
			}
			return nGames > 0 ? tot / nGames : 0.0M;
		}

		public decimal AvgOffTDr( int weekSeed )
		{
			IDictionaryEnumerator myEnumerator = MetricsHt.GetEnumerator();
			int tot = 0, nGames = 0;
			while ( myEnumerator.MoveNext() )
			{
				EpMetric ep = ( EpMetric ) myEnumerator.Value;
				if ( ep.WeekSeed <= weekSeed )
				{
					tot += ep.OffTDr;
					nGames++;
				}
			}
			return nGames > 0 ? tot / nGames : 0.0M;
		}

		public decimal AvgOffSaka( int weekSeed )
		{
			IDictionaryEnumerator myEnumerator = MetricsHt.GetEnumerator();
			decimal tot = 0.0M, nGames = 0.0M;
			while ( myEnumerator.MoveNext() )
			{
				EpMetric ep = ( EpMetric ) myEnumerator.Value;
				if ( ep.WeekSeed <= weekSeed )
				{
					tot += ep.OffSakAllowed;
					nGames++;
				}
			}
			return nGames > 0 ? tot / nGames : 0.0M;
		}

		public decimal AvgDefTDp( int weekSeed )
		{
			IDictionaryEnumerator myEnumerator = MetricsHt.GetEnumerator();
			int tot = 0, nGames = 0;
			while ( myEnumerator.MoveNext() )
			{
				EpMetric ep = ( EpMetric ) myEnumerator.Value;
				if ( ep.WeekSeed <= weekSeed )
				{
					tot += ep.DefTDp;
					nGames++;
				}
			}
			return nGames > 0 ? tot / nGames : 0.0M;
		}

		public decimal AvgDefTDr( int weekSeed )
		{
			IDictionaryEnumerator myEnumerator = MetricsHt.GetEnumerator();
			int tot = 0, nGames = 0;
			while ( myEnumerator.MoveNext() )
			{
				EpMetric ep = ( EpMetric ) myEnumerator.Value;
				if ( ep.WeekSeed <= weekSeed )
				{
					tot += ep.DefTDr;
					nGames++;
				}
			}
			return nGames > 0 ? tot / nGames : 0.0M;
		}

		public decimal AvgDefSak( int weekSeed )
		{
			IDictionaryEnumerator myEnumerator = MetricsHt.GetEnumerator();
			decimal tot = 0.0M, nGames = 0.0M;
			while ( myEnumerator.MoveNext() )
			{
				EpMetric ep = ( EpMetric ) myEnumerator.Value;
				if ( ep.WeekSeed <= weekSeed )
				{
					tot += ep.DefSak;
					nGames++;
				}
			}
			return nGames > 0 ? tot / nGames : 0.0M;
		}

		/// <summary>
		///   Rates the teams PO unit compared with League average
		///   at a particular point in time (represented by the weekseed).
		/// </summary>
		/// <param name="weekSeed">The week seed.</param>
		/// <returns>A decimal value over 1 if above average or under 1</returns>
		public decimal PoMultiplierAt( int weekSeed )
		{
			return ( AvgOffTDp( weekSeed ) / Config.LeagueAvgTDp() );
		}

		public decimal RoMultiplierAt( int weekSeed )
		{
			return ( AvgOffTDr( weekSeed ) / Config.LeagueAvgTDr() );
		}

		public decimal PpMultiplierAt( int weekSeed )
		{
			return AvgOffSaka( weekSeed ) == 0.0M
					 ? 1.0M
					 : Config.LeagueAvgSak() / AvgOffSaka( weekSeed );
		}

		public decimal PdMultiplierAt( int weekSeed )
		{
			return AvgDefTDp( weekSeed ) == 0
					 ? 1.0M
					 : Config.LeagueAvgTDp() / AvgDefTDp( weekSeed );
		}

		public decimal RdMultiplierAt( int weekSeed )
		{
			return AvgDefTDr( weekSeed ) == 0
					 ? 1.0M
					 : Config.LeagueAvgTDr() / AvgDefTDr( weekSeed );
		}

		public decimal PrMultiplierAt( int weekSeed )
		{
			return AvgDefSak( weekSeed ) == 0
					 ? 1.0M
					 : AvgDefSak( weekSeed ) / Config.LeagueAvgSak();
		}

		#endregion EP Metrics

		#region Play metrics

		public void TallyPlays( string seasonIn, bool skipPostseason )
		{
			var myEnumerator = MetricsHt.GetEnumerator();
			Passes = 0;
			while ( myEnumerator.MoveNext() )
			{
				var ep = ( EpMetric ) myEnumerator.Value;
				if ( Utility.SeasonSeed( ep.WeekSeed ) != seasonIn ) continue;
				if ( TeamCode == ep.HomeTeam )
				{
					Passes += ep.HomePasses;
					Runs += ep.HomeRuns;
				}
				else
				{
					Passes += ep.AwayPasses;
					Runs += ep.AwayRuns;
				}
			}
		}

		#endregion Play metrics

		#region Get Metrix

		public decimal GetMetric( string seasonIn, string week, string metric )
		{
			decimal ep = 0.0M;
			//  looking at the current unit Matrix
			UnitMatrix curr = GetMatrix();
			if ( curr != null ) ep = curr.PoMetricsWeek( Int32.Parse( week ) );
			return ep;
		}

		#endregion Get Metrix

		#region Team Card stuff

		public string RenderTeamCard()
		{
			var fileOut = string.Empty;
			if ( TeamCard == null ) LoadTeamCard();
			SetDefence();
			if ( TeamCard != null )
				fileOut = TeamCard.Render();
			return fileOut;
		}

		public bool DetermineDefenceAlignment()
		{
			var uses34 = false;

			if ( _spotList == null ) _spotList = new ArrayList();
			if ( _spotList.Count == 0 ) LoadPlayerUnits(); //  a lot of action here

			//  Count the linemen if 3 then we have the 3-4
			var nLinemen = 0;
			var nLineBackers = 0;
			foreach ( Spot item in _spotList )
			{
				if ( item.SpotName.Equals( "DT" ) ||
					item.SpotName.Equals( "DE" ) ||
					item.SpotName.Equals( "RDT" ) ||
					item.SpotName.Equals( "LDT" ) ||
					item.SpotName.Equals( "NT" ) ||
					item.SpotName.Equals( "RDE" ) ||
					item.SpotName.Equals( "LDE" )
				   )
					nLinemen++;
				if ( item.SpotName.Equals( "OLB" ) ||
					item.SpotName.Equals( "ILB" ) ||
					item.SpotName.Equals( "LB" ) ||
					item.SpotName.Equals( "ROLB" ) ||
					item.SpotName.Equals( "LOLB" ) ||
					item.SpotName.Equals( "RILB" ) ||
					item.SpotName.Equals( "LILB" ) ||
					item.SpotName.Equals( "SSLB" ) ||
					item.SpotName.Equals( "WSLB" ) ||
					item.SpotName.Equals( "SLB" ) ||
					item.SpotName.Equals( "WLB" ) ||
					item.SpotName.Equals( "MLB" )
				   )
					nLineBackers++;
			}
			if ( nLinemen == 3 )
				uses34 = true;
			if ( nLineBackers == 4 )
				uses34 = true;

			return uses34;
		}

		public NFLPlayer GetPlayerAt( string lineupSpot, int occurence, bool bDef, bool usedVal )
		{
			//  Examine the Starting Spot list for a particular position
			//RosterLib.Utility.Announce(string.Format("NFLTeam.GetPlayerAt at {0} for {1}", lineupSpot, NameOut()));

			NFLPlayer player = null;

			if ( _spotList == null ) _spotList = new ArrayList();
			if ( _spotList.Count == 0 ) LoadPlayerUnits(); //  a lot of action here

			foreach ( var t in _spotList )
			{
				var item = ( Spot ) t;

				if ( IsMatch( lineupSpot, item.SpotName, item.IsDef ) )
				{
					if ( !item.IsUsed )
					{
						item.IsUsed = usedVal;
						player = item.Player;
						break;
					}
				}
			}
			//if (player == null)
			//   RosterLib.Utility.Announce(string.Format("NFLTeam.GetPlayerAt - no Player at {0} for {1}", lineupSpot, NameOut()));
			//else
			//   RosterLib.Utility.Announce(string.Format("NFLPlayer.GetPlayerAt {0} for {2} is {1}", lineupSpot, player.PlayerName, NameOut()));
			return player;
		}

		public void DumpStarters()
		{
			var posCountHt = new Hashtable();
			var ds = Utility.TflWs.GetPlayer( TeamCode, "*", "S", "*" );
			var dt = ds.Tables[ 0 ];
			TraceIt( String.Format( "{0} - has {1} Starters ", Name, dt.Rows.Count ) );
			foreach ( DataRow dr in dt.Rows )
			{
				var cat = dr[ "CATEGORY" ].ToString();
				TraceIt(
				   $"{dr[ "PLAYERID" ]} - {dr[ "JERSEY" ],2} {dr[ "FIRSTNAME" ]}{dr[ "SURNAME" ]} {cat} {dr[ "POSDESC" ]} {dr[ "STAR" ]}" );
				if ( posCountHt.ContainsKey( cat ) )
					posCountHt[ cat ] = ( int ) posCountHt[ cat ] + 1;
				else
					posCountHt[ cat ] = 1;
			}

			var QbOk = TestNumberOfStarters( posCountHt, Constants.K_QUARTERBACK_CAT, 1 );
			var RbOk = TestNumberOfStarters( posCountHt, Constants.K_RUNNINGBACK_CAT, 2 );
			var RecOk = TestNumberOfStarters( posCountHt, Constants.K_RECEIVER_CAT, 3, 4 );  // 2 WR and 2 WR
			var PkOk = TestNumberOfStarters( posCountHt, Constants.K_KICKER_CAT, 1 );
			var LbOk = TestNumberOfStarters( posCountHt, Constants.K_LINEMAN_CAT, 7 );
			var DbOk = TestNumberOfStarters( posCountHt, Constants.K_DEFENSIVEBACK_CAT, 4 );
			var OlOk = TestNumberOfStarters( posCountHt, Constants.K_OFFENSIVELINE_CAT, 5 );

			foreach ( DataRow dr in dt.Rows )
			{
				var cat = dr[ "CATEGORY" ].ToString();
				TraceIt(
				   $"{dr[ "PLAYERID" ]} - {dr[ "JERSEY" ],2} {dr[ "FIRSTNAME" ]}{dr[ "SURNAME" ]} {cat} {dr[ "POSDESC" ]} {dr[ "STAR" ]}" );
				if ( posCountHt.ContainsKey( cat ) )
					posCountHt[ cat ] = ( int ) posCountHt[ cat ] + 1;
				else
					posCountHt[ cat ] = 1;
			}

			TraceIt( "-------------------------------------------------------" );
		}

		public int DumpMissingRunningBacks()
		{
			var posCountHt = new Hashtable();
			var ds = Utility.TflWs.GetPlayer( TeamCode, Constants.K_RUNNINGBACK_CAT, "S", "*" );
			var dt = ds.Tables[ 0 ];
			foreach ( DataRow dr in dt.Rows )
			{
				var cat = dr[ "CATEGORY" ].ToString();
				var posDesc = dr[ "POSDESC" ].ToString().Trim();
				if ( !cat.Equals( Constants.K_RUNNINGBACK_CAT ) || posDesc.Equals( "FB" ) ) continue;
				Announce(
				   String.Format( "{7} - {0} - {6,2} {1}{2} {5} {3} {4}",
								 dr[ "PLAYERID" ], dr[ "FIRSTNAME" ], dr[ "SURNAME" ], posDesc,
								 dr[ "STAR" ], cat, dr[ "JERSEY" ], TeamCode ) );
				if ( posCountHt.ContainsKey( cat ) )
					posCountHt[ cat ] = ( int ) posCountHt[ cat ] + 1;
				else
					posCountHt[ cat ] = 1;
			}
			TestNumberOfStarters( posCountHt, Constants.K_RUNNINGBACK_CAT, 1 );
			Announce( "-------------------------------------------------------" );
			return posCountHt.ContainsKey( Constants.K_RUNNINGBACK_CAT ) ? ( int ) posCountHt[ Constants.K_RUNNINGBACK_CAT ] : 0;
		}

		public int DumpMissingQuarterBacks()
		{
			var posCountHt = new Hashtable();
			var ds = Utility.TflWs.GetPlayer( TeamCode, Constants.K_QUARTERBACK_CAT, "S", "*" );
			var dt = ds.Tables[ 0 ];
			foreach ( DataRow dr in dt.Rows )
			{
				var cat = dr[ "CATEGORY" ].ToString();
				var posDesc = dr[ "POSDESC" ].ToString().Trim();
				if ( cat.Equals( Constants.K_QUARTERBACK_CAT ) && posDesc.Equals( "QB" ) )
				{
					Announce(
					   $"{TeamCode} - {dr[ "PLAYERID" ]} - {dr[ "JERSEY" ],2} {dr[ "FIRSTNAME" ]}{dr[ "SURNAME" ]} {cat} {posDesc} {dr[ "STAR" ]}" );
					if ( posCountHt.ContainsKey( cat ) )
						posCountHt[ cat ] = ( int ) posCountHt[ cat ] + 1;
					else
						posCountHt[ cat ] = 1;
				}
			}
			TestNumberOfStarters( posCountHt, Constants.K_QUARTERBACK_CAT, 1 );
			Announce( "-------------------------------------------------------" );
			return posCountHt.ContainsKey( Constants.K_QUARTERBACK_CAT ) ? ( int ) posCountHt[ Constants.K_QUARTERBACK_CAT ] : 0;
		}

		public int DumpMissingKickers()
		{
			var posCountHt = new Hashtable();
			var ds = Utility.TflWs.GetPlayer( TeamCode, Constants.K_KICKER_CAT, "S", "*" );
			var dt = ds.Tables[ 0 ];
			foreach ( DataRow dr in dt.Rows )
			{
				var cat = dr[ "CATEGORY" ].ToString();
				var posDesc = dr[ "POSDESC" ].ToString().Trim();
				if ( cat.Equals( Constants.K_KICKER_CAT ) )
				{
					Announce(
					   String.Format( "{7} - {0} - {6,2} {1}{2} {5} {3} {4}",
					   dr[ "PLAYERID" ], dr[ "FIRSTNAME" ], dr[ "SURNAME" ], posDesc,
									 dr[ "STAR" ], cat, dr[ "JERSEY" ], TeamCode ) );
					if ( posCountHt.ContainsKey( cat ) )
						posCountHt[ cat ] = ( int ) posCountHt[ cat ] + 1;
					else
						posCountHt[ cat ] = 1;
				}
			}
			TestNumberOfStarters( posCountHt, Constants.K_KICKER_CAT, 1 );
			Utility.Announce( "-------------------------------------------------------" );
			return posCountHt.ContainsKey( Constants.K_KICKER_CAT ) ? ( int ) posCountHt[ Constants.K_KICKER_CAT ] : 0;
		}

		public int DumpMissingTightEnds()
		{
			var posCountHt = new Hashtable();
			var ds = Utility.TflWs.GetPlayer( TeamCode, Constants.K_RECEIVER_CAT, "S", "TE" );
			var dt = ds.Tables[ 0 ];
			foreach ( DataRow dr in dt.Rows )
			{
				var cat = dr[ "CATEGORY" ].ToString();
				var posDesc = dr[ "POSDESC" ].ToString().Trim();
				Announce(
				   String.Format( "{7} - {0} - {6,2} {1}{2} {5} {3} {4}",
				   dr[ "PLAYERID" ], dr[ "FIRSTNAME" ], dr[ "SURNAME" ], posDesc,
								  dr[ "STAR" ], cat, dr[ "JERSEY" ], TeamCode ) );
				if ( posCountHt.ContainsKey( cat ) )
					posCountHt[ cat ] = ( int ) posCountHt[ cat ] + 1;
				else
					posCountHt[ cat ] = 1;
			}
			TestNumberOfStarters( posCountHt, Constants.K_RECEIVER_CAT, 1 );
			Announce( "-------------------------------------------------------" );
			return posCountHt.ContainsKey( Constants.K_RECEIVER_CAT ) ? ( int ) posCountHt[ Constants.K_RECEIVER_CAT ] : 0;
		}

		private bool TestNumberOfStarters( IDictionary posCountHt, string cat, int expectedStarters )
		{
			var isOk = true;
			if ( posCountHt[ cat ] != null )
			{
				var theCount = ( int ) posCountHt[ cat ];
				if ( theCount < expectedStarters )
				{
					Announce( string.Format( "{2} - Too few starting {0} ({1})",
												   Utility.MainPos( cat ), theCount, TeamCode ) );
					isOk = false;
				}
				else
				{
					if ( theCount > expectedStarters )
					{
						Announce( string.Format( "{2} - Too many starting {0} ({1})",
													   Utility.MainPos( cat ), theCount, TeamCode ) );
						isOk = false;
					}
				}
			}
			else
			{
				Announce( $"{TeamCode} - has no {Utility.MainPos( cat )}" );
				isOk = false;
			}
			return isOk;
		}

		private bool TestNumberOfStarters( IDictionary posCountHt, string cat, int expectedLo, int expectedHi )
		{
			var isOk = true;
			if ( posCountHt[ cat ] != null )
			{
				var theCount = ( int ) posCountHt[ cat ];
				if ( theCount < expectedLo )
				{
					Announce( $"{TeamCode} - Too few starting {Utility.MainPos( cat )} ({theCount})" );
					isOk = false;
				}
				else
				{
					if ( theCount > expectedHi )
					{
						Announce( $"{TeamCode} - Too many starting {Utility.MainPos( cat )} ({theCount})" );
						isOk = false;
					}
				}
			}
			else
			{
				Announce( $"{TeamCode} - has no {Utility.MainPos( cat )}" );
				isOk = false;
			}
			return isOk;
		}

		private void DumpSpots()
		{
			if ( _spotList == null ) return;
			TraceIt( String.Format( "{0} - has {1} spots filled ", Name, _spotList.Count ) );
			foreach ( var t in _spotList )
			{
				var item = ( Spot ) t;
				TraceIt(
				   String.Format( "{0,-3} - {1,-20} - Used={2} Def={3}", item.SpotName, item.Player.PlayerName, item.IsUsed,
								 item.IsDef ) );
			}
			TraceIt( "------------------------------------------------------" );
		}

		private static bool IsMatch( string lineupSlot, string starterPos, bool bSpotIsDef )
		{
			//RosterLib.Utility.Announce( string.Format( "IsMatch: {0} to {1} isDef={2}", lineupSlot, starterPos, bSpotIsDef ) );

			var bMatch = false;
			if ( lineupSlot == starterPos ) return true;
			//  Now the Alternative
			switch ( lineupSlot ) //  Standard spots as used by the Team Card
			{
				case "MLB":
					if ( ( starterPos == "MIKE" ) ) bMatch = true;
					break;

				case "RLB":
					if ( ( starterPos == "ROLB" ) || ( starterPos == "OLB" ) ) bMatch = true;
					break;

				case "ROLB":
					if ( ( starterPos == "ROLB" ) || ( starterPos == "OLB" ) ) bMatch = true;
					break;

				case "LLB":
					if ( ( starterPos == "LOLB" ) || ( starterPos == "OLB" ) ) bMatch = true;
					break;

				case "LOLB":
					if ( ( starterPos == "LOLB" ) || ( starterPos == "OLB" ) ) bMatch = true;
					break;

				case "LILB":
					if ( ( starterPos == "LILB" ) || ( starterPos == "ILB" ) ) bMatch = true;
					break;

				case "RILB":
					if ( ( starterPos == "RILB" ) || ( starterPos == "ILB" ) ) bMatch = true;
					break;

				case "RT":
					if ( ( starterPos == "LDT" ) && ( bSpotIsDef ) ) bMatch = true;
					break;

				case "LDT":
					if ( ( starterPos == "LT" ) && ( bSpotIsDef ) ) bMatch = true;
					break;

				case "RDT":
					if ( ( starterPos == "RT" ) && ( bSpotIsDef ) ) bMatch = true;
					break;

				case "RCB":
					if ( ( starterPos == "RC" ) && ( bSpotIsDef ) ) bMatch = true;
					break;

				case "LCB":
					if ( ( starterPos == "LC" ) && ( bSpotIsDef ) ) bMatch = true;
					break;

				case "LDE":
					if ( ( starterPos == "LE" ) && ( bSpotIsDef ) ) bMatch = true;
					if ( ( starterPos == "DE" ) && ( bSpotIsDef ) ) bMatch = true;
					break;

				case "RDE":
					if ( ( starterPos == "RE" ) && ( bSpotIsDef ) ) bMatch = true;
					if ( ( starterPos == "DE" ) && ( bSpotIsDef ) ) bMatch = true;
					break;

				case "PK":
					if ( starterPos == "K" ) bMatch = true;
					break;

				case "RB":
					if ( starterPos == "HB" ) bMatch = true;
					break;

				case "WR":
					if ( starterPos == "FL" ) bMatch = true;
					if ( starterPos == "SE" ) bMatch = true;
					break;
			}

			return bMatch;
		}

		#endregion Team Card stuff

		#region Lineups

		public string SpitLineups( bool bPersist )
		{
			var usedDs = Utility.TflWs.PositionsUsed( TeamCode, Season );
			var ds = CreatePositions();

			var posTable = ds.Tables[ 0 ];

			var str = new SimpleTableReport
			{
				ReportHeader = "Lineups :" + TeamCode + " Season " + Season,
				ColumnHeadings = true
			};
			str.AddColumn( new ReportColumn( "Pos", "POS", "{0,-10}" ) );

			// Only add the columns for the last 12 weeks
			for ( int i = 1; i <= 10; i++ )
			{
				var colName = "Wk" + String.Format( "{0:0#}", Week( i ) );
				var colHead = "Wk " + String.Format( "{0:0#}", Week( i ) );
				str.AddColumn( new ReportColumn( colHead, colName, "{0,-20}" ) );
			}
			str.LoadBody( LineupTable( posTable, usedDs ) );
			var outputfile = LineupFile();
			var html = str.RenderAsHtml( outputfile, bPersist );
			return html;
		}

		public string LineupFile()
		{
			return string.Format( "{0}LineupCards\\{1}\\LU-{2}.htm", Utility.OutputDirectory(), Season, TeamCode );
		}

		private static int Week( int offset )
		{
			int w = StartWeek() + offset;
			if ( w > Constants.K_WEEKS_IN_REGULAR_SEASON )
				w -= Constants.K_WEEKS_IN_REGULAR_SEASON;
			return w;
		}

		private static int StartWeek()
		{
			int startWeek = Int32.Parse( Utility.CurrentWeek() ) - 10;
			if ( startWeek < 0 )
				startWeek = Constants.K_WEEKS_IN_REGULAR_SEASON + startWeek;

			return startWeek;
		}

		private static DataSet CreatePositions()
		{
			//  A table that has all the positions in the right order
			var ds = new DataSet();
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "Pos", typeof( String ) );
			cols.Add( "slot", typeof( Int32 ) );

			AddRow( dt, "LCB" );
			AddRow( dt, "CB", 1 );
			AddRow( dt, "RCB" );
			AddRow( dt, "CB", 2 );
			AddRow( dt, "SS" );
			AddRow( dt, "FS" );

			AddRow( dt, "LLB" );
			AddRow( dt, "SLB" );
			AddRow( dt, "SAM" );
			AddRow( dt, "OLB", 1 );

			AddRow( dt, "JLB" );
			AddRow( dt, "MLB" );
			AddRow( dt, "MIKE" );
			AddRow( dt, "ILB", 1 );
			AddRow( dt, "ILB", 2 );
			AddRow( dt, "LB", 1 );
			AddRow( dt, "LB", 2 );
			AddRow( dt, "BLB" );
			AddRow( dt, "RLB" );
			AddRow( dt, "WLB" );
			AddRow( dt, "WILL" );
			AddRow( dt, "OLB", 2 );

			AddRow( dt, "LDE" );
			AddRow( dt, "LE" );
			AddRow( dt, "LDT" );
			AddRow( dt, "DT", 1 );
			AddRow( dt, "DT", 2 );
			AddRow( dt, "LT" );
			AddRow( dt, "NT" );
			AddRow( dt, "UT" );
			AddRow( dt, "RDE" );
			AddRow( dt, "RE" );
			AddRow( dt, "RDT" );
			AddRow( dt, "RT" );

			AddRow( dt, "LT" );
			AddRow( dt, "LG" );
			AddRow( dt, "C" );
			AddRow( dt, "RG" );
			AddRow( dt, "RT" );

			AddRow( dt, "TE" );
			AddRow( dt, "TE", 2 );
			AddRow( dt, "TE", 3 );

			AddRow( dt, "WR", 1 );
			AddRow( dt, "WR", 2 );
			AddRow( dt, "WR", 3 );
			AddRow( dt, "WR", 4 );
			AddRow( dt, "RB" );
			AddRow( dt, "FB" );
			AddRow( dt, "HB" );
			AddRow( dt, "QB" );
			AddRow( dt, "K" );

			ds.Tables.Add( dt );
			return ds;
		}

		private static void AddRow( DataTable dt, string strPos )
		{
			DataRow dr = dt.NewRow();
			dr[ "Pos" ] = strPos;
			dr[ "slot" ] = 1;
			dt.Rows.Add( dr );
		}

		private static void AddRow( DataTable dt, string strPos, int slot )
		{
			DataRow dr = dt.NewRow();
			dr[ "Pos" ] = strPos;
			dr[ "slot" ] = slot;
			dt.Rows.Add( dr );
		}

		private DataTable LineupTable( DataTable posTable, DataSet usedDs )
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "POS", typeof( String ) );
			for ( var i = 1; i <= 10; i++ )
			{
				var colName = "Wk" + String.Format( "{0:0#}", Week( i ) );
				cols.Add( colName, typeof( String ) );
			}

			//  one row for each position
			foreach ( DataRow dr in posTable.Rows )
			{
				var pos = dr[ "Pos" ].ToString();
				var slot = Int32.Parse( dr[ "slot" ].ToString() );
				var strSpot = pos.Trim() + ( slot > 1 ? slot.ToString() : "" );

				if ( Used( pos, usedDs ) )
				{
					var posrow = dt.NewRow();
					posrow[ "POS" ] = strSpot;
					for ( var i = 1; i <= 10; i++ )
					{
						var colName = "Wk" + String.Format( "{0:0#}", Week( i ) );
						posrow[ colName ] = Starter( TheSeason( i ).ToString(), Week( i ), pos, slot );
					}
					dt.Rows.Add( posrow );
				}
			}
			return dt;
		}

		private int TheSeason( int offset )
		{
			int wo = ( Int32.Parse( Utility.CurrentWeek() ) - 10 );
			wo += offset;
			int theSeason = Int32.Parse( Season );
			if ( wo <= 0 ) theSeason--;
			return theSeason;
		}

		private static bool Used( string pos, DataSet usedDs )
		{
			//  did the team use this type of position?
			var dt = usedDs.Tables[ 0 ];
			return dt.Rows.Cast<DataRow>().Any( dr => dr[ "POS" ].ToString().Trim() == pos );
		}

		public string Starter( string seasonIn, int weekNo, string pos, int slot )
		{
			string starter = "";
			string playerCode = Utility.TflWs.GetStarter( TeamCode, seasonIn, weekNo, pos, slot );
			if ( playerCode != "???" )
			{
				NFLPlayer p = Masters.Pm.GetPlayer( playerCode );
				starter = p.PlayerOut();
			}
			return starter;
		}

		#endregion Lineups

		#region Grid Loads

		private void LoadInjuries( string catIn, string strPos )
		{
			if ( InjuryList == null ) InjuryList = new ArrayList();
			InjuryList.Clear();

			var ds = Utility.TflWs.GetPlayer( TeamCode, catIn, "I", strPos );
			var dt = ds.Tables[ "player" ];
			if ( dt.Rows.Count != 0 )
			{
				foreach ( DataRow dr in dt.Rows )
				{
					var strPlayerName = dr[ "firstname" ].ToString().Trim() + " " + dr[ "surname" ].ToString().Trim();
					var strPlayerCode = dr[ "playerid" ].ToString();
					var strRookieYr = dr[ "rookieyr" ].ToString();
					var strPlayerPos = dr[ "posdesc" ].ToString();
					var strPlayerRole = dr[ "ROLE" ].ToString();
					var strPlayerCat = dr[ "CATEGORY" ].ToString();

					InjuryList.Add( new NFLPlayer( strPlayerName, strPlayerCode, strPlayerRole,
												 strRookieYr, strPlayerPos, strPlayerCat, this ) );
				}
			}
			return;
		}

		private void LoadFreeAgents( string catIn, string strPos, bool includeStarters )
		{
			var ds = Utility.TflWs.GetPlayer( TeamCode, catIn, "*", strPos );

			var dt = ds.Tables[ "player" ];
			if ( dt.Rows.Count != 0 )
			{
				foreach ( DataRow dr in dt.Rows )
				{
					var strPlayerCode = dr[ "playerid" ].ToString();
					var strPlayerPos = dr[ "posdesc" ].ToString().Trim();
					strPlayerPos = Purify( strPlayerPos );
					var ndx = NFLPlayer.K_FANTASY_POSITIONS.IndexOf( strPlayerPos );
					if ( ndx != -1 )
					{
						NFLPlayer p = Masters.Pm.GetPlayer( strPlayerCode );
						if ( ( !p.IsStarter() ) || includeStarters )
						{
							if ( FaList == null ) FaList = new ArrayList();
							FaList.Add( p );
						}
					}
				}
			}
			return;
		}

		private static string Purify( string pos )
		{
			if ( pos.IndexOf( ',' ) > -1 )
				//  we have a comma, take the first part
				pos = pos.Substring( 0, pos.IndexOf( ',' ) );

			return pos;
		}

		public string FreeAgents( bool isOffence, bool showHeader, bool includeStarters )
		{
			//  used by Matchup report
			var freeAgents = string.Empty;
			LoadFreeAgents( "*", "*", includeStarters );
			if ( FaList != null )
				freeAgents = HtmlList( FaList, isOffence, showHeader );
			return freeAgents;
		}

		public string UnitReport()
		{
			var sb = new StringBuilder();
			sb.Append( PoReport() );
#if !DEBUG
         sb.Append(PdReport());
         sb.Append(RoReport());
         sb.Append(RdReport());
         sb.Append(PpReport());
         sb.Append(PrReport());
#endif
			return sb.ToString();
		}

		#region Report methods

		public string PrReport()
		{
			return PrUnit( Season ) + PrList();
		}

		public string PpReport()
		{
			return PpUnit( Season ) + PpList();
		}

		public string PoReport()
		{
			return PoUnit( Season ) + PoList();
		}

		public string RoReport()
		{
			return RoUnit( Season ) + RoList();
		}

		public string RdReport()
		{
			return RdUnit( Season ) + RdList();
		}

		public string PdReport()
		{
			return PdUnit( Season ) + PdList();
		}

		#endregion Report methods

		#region Unit methods

		/// <summary>
		///   Prints out unit stats for the last 8 games
		/// </summary>
		/// <param name="seasonIn"></param>
		/// <returns></returns>
		public string PoUnit( string seasonIn )
		{
			TotSacksAllowed = 0.0M;
			TotInterceptions = 0;
			_totYdp = 0;
			TotTDp = 0;
			var nGames = 0;

			var s = "";
			s += HtmlLib.TableOpen( "BORDER='1'" );
			s += UnitReportName( "Passing Offense", KUnitcodePo );
			s += UnitHeader( "YDp", "Tdp", KUnitcodePd );

			LoadPreviousGames( TeamCode, seasonIn ); //  get the last 17 games played into the Game List

			var gameCount = 0;
			foreach ( NFLGame g in GameList )
			{
				if ( g.Played() )
				{
					gameCount++;
					if ( gameCount > 8 ) break;
					g.MetricsCalculated = false;
					g.TallyMetrics( metric: String.Empty );
					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( g.Season.Substring( 2, 2 ), "ALIGN='CENTER'" ); // 0
					s += HtmlLib.TableDataAttr( g.Week, "ALIGN='CENTER'" ); // 1
					s += HtmlLib.TableDataAttr( g.OpponentOut( TeamCode ), "ALIGN='CENTER'" ); //2
					s +=
					   HtmlLib.TableDataAttr(
						  string.Format( "{0} {1}", g.ResultOut( TeamCode, true ), g.ScoreOut( TeamCode ) ), "ALIGN='CENTER'" ); // 3
					s += HtmlLib.TableData( g.UnitStar( KUnitcodePo, TeamCode ) ); //  4.1 Leader
					var opp = g.Opponent( TeamCode );
					var oppStar = g.UnitStar( KUnitcodePd, opp );
					s += HtmlLib.TableData( oppStar ); //  4.2 Key
					s +=
					   HtmlLib.TableDataAttr( g.CurrRating( KUnitcodePd, g.Opponent( TeamCode ) ).ToString(),
											 "ALIGN='CENTER'" ); //  5. Opp RD Rating
					s += HtmlLib.TableDataAttr( g.YDp( TeamCode ).ToString(), "ALIGN='RIGHT'" ); // 6
					s += HtmlLib.TableDataAttr( g.Tdp( TeamCode ).ToString(), "ALIGN='RIGHT'" ); // 7
					s += HtmlLib.TableDataAttr( g.IntsAllowed( TeamCode ).ToString(), "ALIGN='RIGHT'" ); // 8
					s += HtmlLib.TableDataAttr( g.SacksAllowed( TeamCode ).ToString(), "ALIGN='RIGHT'" ); // 9
					s += HtmlLib.TableDataAttr( g.GameRating( KUnitcodePo, TeamCode ).ToString(), "ALIGN='CENTER'" ); //1 10
					s += HtmlLib.TableRowClose();
					TotSacksAllowed += g.SacksAllowed( TeamCode );
					TotInterceptions += g.IntsAllowed( TeamCode );
					_totYdp += g.YDp( TeamCode );
					TotTDp += g.Tdp( TeamCode );
					nGames++;
				}
			}
			s += HtmlLib.TableRowOpen();
			s += HtmlLib.TableData( "" ); // 0
			s += HtmlLib.TableData( "" ); // 1
			s += HtmlLib.TableData( "Totals" ); // 2
			s += HtmlLib.TableData( "" ); // 3
			s += HtmlLib.TableData( "" ); //  4. star
			s += HtmlLib.TableData( "" ); // 5
			s += HtmlLib.TableData( "" ); // 6
			s += HtmlLib.TableDataAttr( AverageIntOut( _totYdp, nGames ), "ALIGN='RIGHT'" ); //  6
			s += HtmlLib.TableDataAttr( AverageIntOut( TotTDp, nGames ), "ALIGN='RIGHT'" ); //  7
			s += HtmlLib.TableDataAttr( AverageIntOut( TotInterceptions, nGames ), "ALIGN='RIGHT'" ); //  8
			s += HtmlLib.TableDataAttr( AverageDecOut( TotSacksAllowed, nGames ), "ALIGN='RIGHT'" ); //  9
			s += HtmlLib.TableRowClose();
			s += HtmlLib.TableClose();
			return s;
		}

		public string RoUnit( string seasonIn )
		{
			int nGames = 0;
			TotYdr = 0;
			TotTDr = 0;

			string s = HtmlLib.TableOpen( "BORDER='1'" );

			s += UnitReportName( "Rushing Offense", KUnitcodeRo );
			s += UnitHeader( "YDr", "Tdr", KUnitcodeRd );

			LoadPreviousGames( TeamCode, seasonIn ); //  get the last 17 games played into the Game List

			var gameCount = 0;
			foreach ( NFLGame g in GameList )
			{
				if ( g.Played() )
				{
					gameCount++;
					if ( gameCount > 8 ) break;
					g.MetricsCalculated = false; //  force a recalculation
					g.TallyMetrics( String.Empty );
					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( g.Season.Substring( 2, 2 ), "ALIGN='CENTER'" ); // 0
					s += HtmlLib.TableDataAttr( g.Week, "ALIGN='CENTER'" ); // 1
					s += HtmlLib.TableDataAttr( g.OpponentOut( TeamCode ), "ALIGN='CENTER'" ); // 2
					s +=
					   HtmlLib.TableDataAttr(
						  string.Format( "{0} {1}", g.ResultOut( TeamCode, true ), g.ScoreOut( TeamCode ) ), "ALIGN='CENTER'" ); //3
					s += HtmlLib.TableData( g.UnitStar( KUnitcodeRo, TeamCode ) ); //  4 star
					s += HtmlLib.TableData( g.UnitStar( KUnitcodeRd, g.Opponent( TeamCode ) ) ); //  4 star
					s +=
					   HtmlLib.TableDataAttr( g.CurrRating( KUnitcodeRd, g.Opponent( TeamCode ) ).ToString(),
											 "ALIGN='CENTER'" ); //  5 Opp RD Rating
					s += HtmlLib.TableDataAttr( g.YDr( TeamCode ).ToString(), "ALIGN='RIGHT'" ); //  6 YDr
					s += HtmlLib.TableDataAttr( g.Tdr( TeamCode ).ToString(), "ALIGN='RIGHT'" ); //  7 Tdr
					s += HtmlLib.TableDataAttr( g.GameRating( KUnitcodeRo, TeamCode ).ToString(), "ALIGN='CENTER'" ); // 8
					s += HtmlLib.TableRowClose();
					TotYdr += g.YDr( TeamCode );
					TotTDr += g.Tdr( TeamCode );
					nGames++;
				}
			}
			s += HtmlLib.TableRowOpen();
			s += HtmlLib.TableData( "" ); // 0
			s += HtmlLib.TableData( "" ); // 1
			s += HtmlLib.TableData( "Totals" ); // 2
			s += HtmlLib.TableData( "" ); // 3
			s += HtmlLib.TableData( "" ); //  4. star
			s += HtmlLib.TableData( "" ); // 5
			s += HtmlLib.TableData( "" ); // 6
			s += HtmlLib.TableDataAttr( AverageIntOut( TotYdr, nGames ), "ALIGN='RIGHT'" ); //  6
			s += HtmlLib.TableDataAttr( AverageIntOut( TotTDr, nGames ), "ALIGN='RIGHT'" ); //  7
			s += HtmlLib.TableData( "" ); // 5
			s += HtmlLib.TableRowClose();
			s += HtmlLib.TableClose();
			return s;
		}

		public string RdUnit( string seasonIn )
		{
			int nGames = 0;
			TotYdrAllowed = 0;
			TotTDrAllowed = 0;

			string s = HtmlLib.TableOpen( "BORDER='1'" );
			s += UnitReportName( "Rushing Defence", KUnitcodeRd );
			s += UnitHeader( "YDr Alwd", "Tdr Alwd", KUnitcodeRo );

			LoadPreviousGames( TeamCode, seasonIn ); //  get the last 17 games played into the Game List
			var gameCount = 0;
			foreach ( NFLGame g in GameList )
			{
				if ( g.Played() )
				{
					gameCount++;
					if ( gameCount > 8 ) break;
					g.MetricsCalculated = false;
					g.TallyMetrics( String.Empty );
					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( g.Season.Substring( 2, 2 ), "ALIGN='CENTER'" ); // 0
					s += HtmlLib.TableDataAttr( g.Week, "ALIGN='CENTER'" ); // 1
					s += HtmlLib.TableDataAttr( g.OpponentOut( TeamCode ), "ALIGN='CENTER'" ); //  2
					s +=
					   HtmlLib.TableDataAttr(
						  string.Format( "{0} {1}", g.ResultOut( TeamCode, true ), g.ScoreOut( TeamCode ) ), "ALIGN='CENTER'" ); // 3
					s += HtmlLib.TableData( g.UnitStar( KUnitcodeRd, TeamCode ) ); //  4 star
					s += HtmlLib.TableData( g.UnitStar( KUnitcodeRo, g.Opponent( TeamCode ) ) ); //  4 star
					s +=
					   HtmlLib.TableDataAttr( g.CurrRating( KUnitcodeRd, g.Opponent( TeamCode ) ).ToString(),
											 "ALIGN='CENTER'" ); //  5 Opp RD Rating
					s += HtmlLib.TableDataAttr( g.YDrAllowed( TeamCode ).ToString(), "ALIGN='RIGHT'" ); //  6 YDr
					s += HtmlLib.TableDataAttr( g.TdrAllowed( TeamCode ).ToString(), "ALIGN='RIGHT'" ); //  7 Tdr
					s += HtmlLib.TableDataAttr( g.GameRating( KUnitcodeRd, TeamCode ).ToString(), "ALIGN='CENTER'" ); // 8
					s += HtmlLib.TableRowClose();
					nGames++;
					TotYdrAllowed += g.YDrAllowed( TeamCode );
					TotTDrAllowed += g.TdrAllowed( TeamCode );
				}
			}
			s += HtmlLib.TableRowOpen();
			s += HtmlLib.TableData( "" ); // 1
			s += HtmlLib.TableData( "Totals" ); // 2
			s += HtmlLib.TableData( "" ); // 3
			s += HtmlLib.TableData( "" ); //  4. star
			s += HtmlLib.TableData( "" ); // 5
			s += HtmlLib.TableData( "" ); // 6
			s += HtmlLib.TableData( "" ); // 7
			s += HtmlLib.TableDataAttr( AverageIntOut( TotYdrAllowed, nGames ), "ALIGN='RIGHT'" ); //  6
			s += HtmlLib.TableDataAttr( AverageIntOut( TotTDrAllowed, nGames ), "ALIGN='RIGHT'" ); //  7
			s += HtmlLib.TableData( "" ); // 8
			s += HtmlLib.TableRowClose();
			s += HtmlLib.TableClose();
			return s;
		}

		public string PdUnit( string seasonIn )
		{
			int nGames = 0;
			TotYDpAllowed = 0;
			TotTDpAllowed = 0;
			TotIntercepts = 0;
			decimal totSacks = 0.0M;

			string s = HtmlLib.TableOpen( "BORDER='1'" );
			s += UnitReportName( "Pass Defence", KUnitcodePd );
			s += UnitHeader( "YDp Alwd", "Tdp Alwd", KUnitcodePo );

			LoadPreviousGames( TeamCode, seasonIn ); //  get the last 17 games played into the Game List
			var gameCount = 0;
			foreach ( NFLGame g in GameList )
			{
				if ( g.Played() )
				{
					gameCount++;
					if ( gameCount > 8 ) break;
					g.MetricsCalculated = false;
					g.TallyMetrics( String.Empty );
					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( g.Season.Substring( 2, 2 ), "ALIGN='CENTER'" ); // 0
					s += HtmlLib.TableDataAttr( g.Week, "ALIGN='CENTER'" ); // 1
					s += HtmlLib.TableDataAttr( g.OpponentOut( TeamCode ), "ALIGN='CENTER'" ); // 2
					s +=
					   HtmlLib.TableDataAttr(
						  string.Format( "{0} {1}", g.ResultOut( TeamCode, true ), g.ScoreOut( TeamCode ) ), "ALIGN='CENTER'" ); // 3
					s += HtmlLib.TableData( g.UnitStar( KUnitcodePd, TeamCode ) ); //  4 star
					s += HtmlLib.TableData( g.UnitStar( KUnitcodePo, g.Opponent( TeamCode ) ) ); //  4 star
					s +=
					   HtmlLib.TableDataAttr( g.CurrRating( KUnitcodePd, g.Opponent( TeamCode ) ).ToString(),
											 "ALIGN='CENTER'" ); //  5
					s += HtmlLib.TableDataAttr( g.YDpAllowed( TeamCode ).ToString(), "ALIGN='RIGHT'" ); // 6
					s += HtmlLib.TableDataAttr( g.TdpAllowed( TeamCode ).ToString(), "ALIGN='RIGHT'" ); // 7 Tdp
					s += HtmlLib.TableDataAttr( g.Sacks( TeamCode ).ToString(), "ALIGN='RIGHT'" ); // 8
					s += HtmlLib.TableDataAttr( g.Interceptions( TeamCode ).ToString(), "ALIGN='RIGHT'" ); // 9
					s += HtmlLib.TableDataAttr( g.GameRating( KUnitcodePd, TeamCode ).ToString(), "ALIGN='CENTER'" ); // 10
					s += HtmlLib.TableRowClose();
					nGames++;
					TotYDpAllowed += g.YDpAllowed( TeamCode );
					TotTDpAllowed += g.TdpAllowed( TeamCode );
					TotIntercepts += g.Interceptions( TeamCode );
					totSacks += g.Sacks( TeamCode );
				}
			}
			s += HtmlLib.TableRowOpen();
			s += HtmlLib.TableData( "" ); // 1
			s += HtmlLib.TableData( "Totals" ); // 2
			s += HtmlLib.TableData( "" ); // 3
			s += HtmlLib.TableData( "" ); //  4. star
			s += HtmlLib.TableData( "" ); // 5
			s += HtmlLib.TableData( "" ); // 6
			s += HtmlLib.TableData( "" ); // 7
			s += HtmlLib.TableDataAttr( AverageIntOut( TotYDpAllowed, nGames ), "ALIGN='RIGHT'" ); //  6
			s += HtmlLib.TableDataAttr( AverageIntOut( TotTDpAllowed, nGames ), "ALIGN='RIGHT'" ); //  7
			s += HtmlLib.TableDataAttr( AverageDecOut( totSacks, nGames ), "ALIGN='RIGHT'" ); //  9
			s += HtmlLib.TableDataAttr( AverageIntOut( TotIntercepts, nGames ), "ALIGN='RIGHT'" ); //  8
			s += HtmlLib.TableRowClose();
			s += HtmlLib.TableClose();
			return s;
		}

		public string PrUnit( string seasonIn )
		{
			TotSacks = 0.0M;
			var s = "";
			s += HtmlLib.TableOpen( "BORDER='1'" );
			s += UnitReportName( "Pass Rush", KUnitcodePp );
			s += UnitHeader( "SAK", "SAK", KUnitcodePp );

			LoadPreviousGames( TeamCode, seasonIn ); //  get the last 17 games played into the Game List
			var gameCount = 0;
			foreach ( NFLGame g in GameList )
			{
				if ( g.Played() )
				{
					gameCount++;
					if ( gameCount > 8 ) break;
					g.MetricsCalculated = false;
					g.TallyMetrics( String.Empty );
					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( g.Season.Substring( 2, 2 ), "ALIGN='CENTER'" ); // 0
					s += HtmlLib.TableDataAttr( g.Week, "ALIGN='CENTER'" );
					s += HtmlLib.TableDataAttr( g.OpponentOut( TeamCode ), "ALIGN='CENTER'" );
					s +=
					   HtmlLib.TableDataAttr(
						  string.Format( "{0} {1}", g.ResultOut( TeamCode, true ), g.ScoreOut( TeamCode ) ), "ALIGN='CENTER'" );
					s += HtmlLib.TableData( g.UnitStar( KUnitcodePr, TeamCode ) ); //  star
					s += HtmlLib.TableData( g.UnitStar( KUnitcodePp, g.Opponent( TeamCode ) ) ); //  star
					s +=
					   HtmlLib.TableDataAttr( g.CurrRating( KUnitcodePp, g.Opponent( TeamCode ) ).ToString(),
											 "ALIGN='CENTER'" ); //  Opp PP Rating
					s += HtmlLib.TableDataAttr( g.Sacks( TeamCode ).ToString(), "ALIGN='RIGHT'" );
					s += HtmlLib.TableDataAttr( g.GameRating( KUnitcodePr, TeamCode ).ToString(), "ALIGN='CENTER'" );
					s += HtmlLib.TableRowClose();
					TotSacks += g.Sacks( TeamCode );
				}
			}
			s += HtmlLib.TableRowOpen();
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableData( "Totals" );
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableData( "" ); //  star
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableData( "" ); 
			s += HtmlLib.TableData( "" ); 
			s += HtmlLib.TableDataAttr( TotSacks.ToString(), "ALIGN='RIGHT'" );
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableRowClose();

			s += HtmlLib.TableClose();
			return s;
		}

		public string PpUnit( string seasonIn )
		{
			TotSacksAllowed = 0.0M;
			var s = "";
			s += HtmlLib.TableOpen( "BORDER='1'" );
			s += UnitReportName( "Pass Protection", KUnitcodePp );
			s += UnitHeader( "SAKa", "SAK", KUnitcodePr );

			LoadPreviousGames( TeamCode, seasonIn ); //  get the last 17 games played into the Game List
			var gameCount = 0;
			foreach ( NFLGame g in GameList )
			{
				if ( g.Played() )
				{
					gameCount++;
					if ( gameCount > 8 ) break;
					g.MetricsCalculated = false;
					g.TallyMetrics( String.Empty );
					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( g.Season.Substring( 2, 2 ), "ALIGN='CENTER'" ); // 0
					s += HtmlLib.TableDataAttr( g.Week, "ALIGN='CENTER'" );
					s += HtmlLib.TableDataAttr( g.OpponentOut( TeamCode ), "ALIGN='CENTER'" );
					s +=
					   HtmlLib.TableDataAttr(
						  string.Format( "{0} {1}", g.ResultOut( TeamCode, true ), g.ScoreOut( TeamCode ) ), "ALIGN='CENTER'" );
					s += HtmlLib.TableData( g.UnitStar( KUnitcodePp, TeamCode ) ); //  star
					s += HtmlLib.TableData( g.UnitStar( KUnitcodePr, g.Opponent( TeamCode ) ) ); //  star
					s +=
					   HtmlLib.TableDataAttr( g.CurrRating( KUnitcodePr, g.Opponent( TeamCode ) ).ToString(),
											 "ALIGN='CENTER'" ); //  Opp PR Rating
					s += HtmlLib.TableDataAttr( g.SacksAllowed( TeamCode ).ToString(), "ALIGN='RIGHT'" );
					s += HtmlLib.TableDataAttr( g.GameRating( KUnitcodePp, TeamCode ).ToString(), "ALIGN='CENTER'" );
					s += HtmlLib.TableRowClose();
					TotSacksAllowed += g.SacksAllowed( TeamCode );
				}
			}
			s += HtmlLib.TableRowOpen();
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableData( "Totals" );
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableData( "" ); //  star
			s += HtmlLib.TableData( "" ); //TODO:  Avg Opp PR Rating
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableDataAttr( TotSacksAllowed.ToString(), "ALIGN='RIGHT'" );
			s += HtmlLib.TableData( "" );
			s += HtmlLib.TableRowClose();

			s += HtmlLib.TableClose();
			return s;
		}

		#endregion Unit methods

		#region List methods

		public string PoList()
		{
			var unitPlayers = new ArrayList();
			LoadQuarterbacks( NFLPlayer.K_ROLE_STARTER );
			unitPlayers.AddRange( PassingUnit );
			LoadQuarterbacks( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( PassingUnit );
			LoadQuarterbacks( NFLPlayer.K_ROLE_RESERVE );
			unitPlayers.AddRange( PassingUnit );
			LoadQuarterbacks( " " );
			unitPlayers.AddRange( PassingUnit );
			LoadQuarterbacks( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( PassingUnit );

			LoadPassReceivers( NFLPlayer.K_ROLE_STARTER );
			unitPlayers.AddRange( PassingUnit );
			LoadPassReceivers( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( PassingUnit );
			LoadPassReceivers( NFLPlayer.K_ROLE_RESERVE );
			unitPlayers.AddRange( PassingUnit );
			LoadPassReceivers( " " );
			unitPlayers.AddRange( PassingUnit );
			LoadPassReceivers( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( PassingUnit );

			LoadProtectionUnit( NFLPlayer.K_ROLE_STARTER ); //  the offensive line
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( NFLPlayer.K_ROLE_RESERVE );
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( " " );
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( ProtectionUnit );

			return FormatUnit( unitPlayers, "Passing Unit" );
		}

		public string RoList()
		{
			var unitPlayers = new ArrayList();
			LoadRunningUnit( NFLPlayer.K_ROLE_STARTER );
			unitPlayers.AddRange( RunningUnit );
			LoadRunningUnit( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( RunningUnit );
			LoadRunningUnit( NFLPlayer.K_ROLE_RESERVE );
			unitPlayers.AddRange( RunningUnit );
			LoadRunningUnit( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( RunningUnit );
			LoadRunningUnit( " " );
			unitPlayers.AddRange( RunningUnit );
			LoadProtectionUnit( NFLPlayer.K_ROLE_STARTER ); //  the offensive line
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( " " );
			unitPlayers.AddRange( ProtectionUnit );
			return FormatUnit( unitPlayers, "Rushing Unit" );
		}

		public string RdList()
		{
			var unitPlayers = new ArrayList();
			LoadRunDefenceUnit( NFLPlayer.K_ROLE_STARTER );
			unitPlayers.AddRange( RunDefenceUnit );
			LoadRunDefenceUnit( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( RunDefenceUnit );
			LoadRunDefenceUnit( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( RunDefenceUnit );
			LoadRunDefenceUnit( " " );
			unitPlayers.AddRange( RunDefenceUnit );
			return FormatUnit( unitPlayers, "Run Defence Unit" );
		}

		public string PdList()
		{
			var unitPlayers = new ArrayList();
			LoadPassDefenceUnit( NFLPlayer.K_ROLE_STARTER );
			unitPlayers.AddRange( PassDefenceUnit );
			LoadPassRushUnit( NFLPlayer.K_ROLE_STARTER );
			unitPlayers.AddRange( PassRushUnit );
			LoadPassDefenceUnit( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( PassDefenceUnit );
			LoadPassRushUnit( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( PassRushUnit );
			LoadRunDefenceUnit( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( PassDefenceUnit );
			LoadRunDefenceUnit( " " );
			unitPlayers.AddRange( RunDefenceUnit );
			LoadPassDefenceUnit( " " );
			unitPlayers.AddRange( PassDefenceUnit );
			return FormatUnit( unitPlayers, "Pass Defence Unit" );
		}

		private string PrList()
		{
			var unitPlayers = new ArrayList();
			LoadPassRushUnit( NFLPlayer.K_ROLE_STARTER );
			unitPlayers.AddRange( PassRushUnit );
			LoadPassRushUnit( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( PassRushUnit );
			LoadPassRushUnit( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( PassRushUnit );
			LoadPassRushUnit( " " );
			unitPlayers.AddRange( PassRushUnit );
			return FormatUnit( unitPlayers, "Pass Rush Unit" );
		}

		private string PpList()
		{
			var unitPlayers = new ArrayList();
			LoadProtectionUnit( NFLPlayer.K_ROLE_STARTER );
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( NFLPlayer.K_ROLE_BACKUP );
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( NFLPlayer.K_ROLE_INJURED );
			unitPlayers.AddRange( ProtectionUnit );
			LoadProtectionUnit( " " );
			unitPlayers.AddRange( ProtectionUnit );
			return FormatUnit( unitPlayers, "Protection Unit" );
		}

		#endregion List methods

		private static string AverageIntOut( int total, int nGames )
		{
			return string.Format( "{0} <br> {1:0.0}", total, nGames == 0 ?
			   0 : Convert.ToDecimal( total / nGames ) );
		}

		private static string AverageInt( int total, int nGames )
		{
			return string.Format( "{0:0}", nGames == 0 ? 0 : Convert.ToDecimal( total / nGames ) );
		}

		private static string AverageDecOut( decimal total, int nGames )
		{
			return nGames == 0 ? string.Format( "{0} <br> {1:0.0}", total, 0 )
			   : string.Format( "{0} <br> {1:0.0}", total, total / nGames );
		}

		private string UnitReportName( string name, string unitCode )
		{
			var s = HtmlLib.TableRowOpen();
			s += HtmlLib.TableDataAttr( string.Format( "{0} {2} - {1}", Name, UnitRating( unitCode ), name ), "COLSPAN=11" );
			s += HtmlLib.TableRowClose();
			return s;
		}

		private static string UnitHeader( string stat1, string stat2, string opposingUnit )
		{
			var s = HtmlLib.TableRowOpen();
			s += HtmlLib.TableData( "Yr" );
			s += HtmlLib.TableData( "Wk" );
			s += HtmlLib.TableData( "Opp" );
			s += HtmlLib.TableData( "Res" );
			s += HtmlLib.TableData( " Leader" );
			s += HtmlLib.TableData( "vs Key" );
			s += HtmlLib.TableData( "Opp" + opposingUnit );
			s += HtmlLib.TableData( stat1 );
			if ( !stat2.Equals( "SAK" ) )
				s += HtmlLib.TableData( stat2 );
			if ( opposingUnit == KUnitcodePd )
			{
				s += HtmlLib.TableData( "Inta" );
				s += HtmlLib.TableData( "SAKa" );
			}
			if ( opposingUnit == KUnitcodePo )
			{
				s += HtmlLib.TableData( "SAK" );
				s += HtmlLib.TableData( "Int" );
			}
			s += HtmlLib.TableData( "G Rating" );
			s += HtmlLib.TableRowClose();
			return s;
		}

		public string UnitRating( string unitCode )
		{
			SetRecord( Season, skipPostseason: false );
			char unitRating;
			switch ( unitCode )
			{
				case "PO":
					unitRating = Ratings[ 0 ];
					break;

				case "RO":
					unitRating = Ratings[ 1 ];
					break;

				case "PP":
					unitRating = Ratings[ 2 ];
					break;

				case "PR":
					unitRating = Ratings[ 3 ];
					break;

				case "RD":
					unitRating = Ratings[ 4 ];
					break;

				case "PD":
					unitRating = Ratings[ 5 ];
					break;

				default:
					unitRating = '?';
					break;
			}
			return unitRating.ToString();
		}

		private static string FormatUnit( ArrayList unitPlayers, string unitName )
		{
			var s = HtmlLib.H3( unitName ) + HtmlLib.TableOpen( "BORDER='1'" );
			var plyrCount = 0;
			var lastCat = "X";

			foreach ( NFLPlayer p in unitPlayers )
			{
				if ( p.PlayerCat != lastCat )
				{
					s += p.CatRow( p.PlayerCat );
					s += p.PlayerHeaderRow( "YTD1", "YTD2" );
					lastCat = p.PlayerCat;
				}
				s += p.PlayerRow( true );
				plyrCount++;
			}
			if ( plyrCount == 0 )
				s += HtmlLib.TableRowOpen() + HtmlLib.TableData( "none" ) + HtmlLib.TableRowClose();
			s += HtmlLib.TableClose();
			return s;
		}

		public string KeyPlayer( string pos, string seasonIn, string week )
		{
			var star = "";
			LineupDs = Utility.TflWs.GetLineup( TeamCode, seasonIn, Int32.Parse( week ) );
			var dt = LineupDs.Tables[ "lineup" ];
			var lu = new Lineup( LineupDs );
			var player = lu.GetPlayerAt( pos );
			if ( player != null )
				star = player.PlayerNameShort;

			return star;
		}

		public void DumpLineup( string season, string week )
		{
			var lineup = new Lineup( TeamCode, season, week );
			lineup.DumpLineup();
		}

		public Lineup Lineup( string season, string week )
		{
			var lineup = new Lineup( TeamCode, season, week );
			lineup.DumpLineup();
			return lineup;
		}

		private static bool IsPos( string posType, string actPos )
		{
			string allPositions;
			switch ( posType )
			{
				case "RB":
					allPositions = "RB,HB,TB";
					break;

				case "LB":
					allPositions = "NT,LB,JACK,ILB,OLB,RILB,LILB,SAM,MIKE,SLB,MLB,WLB,RLB,LLB,WILL";
					break;

				case "DL":
					allPositions = "LE,DLE,LDE,RE,RDE,DRE,DT,LDT,RDT,DFT,DLT";
					break;

				case "OL":
					allPositions = "C,LG,RG,LT,RT,OL";
					break;

				default:
					allPositions = "";
					break;
			}
			var isPos = !( allPositions.IndexOf( actPos ) < 0 );
			return isPos;
		}

		public string InjuredList( bool isOffence )
		{
			//  used by Matchup report
			LoadInjuries( "*", "*" );
			return HtmlList( InjuryList, isOffence, true );
		}

		/// <summary>
		///  Puts an arraylist into an HTML table.
		/// </summary>
		/// <param name="plyrList">The plyr list.</param>
		/// <param name="isOffence">if set to <c>true</c> [is offence].</param>
		/// <param name="showHeader">if set to <c>true</c> [show header].</param>
		/// <returns></returns>
		public string HtmlList( ArrayList plyrList, bool isOffence, bool showHeader )
		{
			//  used by Matchup and FA report
			string s = HtmlLib.TableOpen( "BORDER='1'" );
			int plyrCount = 0;
			string lastCat = "X";

			foreach ( NFLPlayer p in plyrList )
			{
				if ( p.PlayerCat != lastCat )
				{
					s += p.CatRow( p.PlayerCat );
					if ( showHeader ) s += p.PlayerHeaderRow( "Career1", "Career2" );
					lastCat = p.PlayerCat;
				}

				if ( ( p.IsOffence() && isOffence ) || ( p.IsDefence() && !isOffence ) )
				{
					s += p.PlayerRow( false );
					plyrCount++;
				}
			}
			if ( plyrCount == 0 )
				s += HtmlLib.TableRowOpen() + HtmlLib.TableData( "none" ) + HtmlLib.TableRowClose();
			s += HtmlLib.TableClose();
			return s;
		}

		private void LoadOthers( string catIn, string strPos )
		{
			const string strPlayerRole = " ";
			string strPlayerCat = String.Empty;

			DataSet ds = Utility.TflWs.GetPlayer( TeamCode, catIn, strPlayerRole, strPos );
			DataTable dt = ds.Tables[ "player" ];
			if ( dt.Rows.Count != 0 )
			{
				foreach ( DataRow dr in dt.Rows )
				{
					string strPlayerName = dr[ "firstname" ].ToString().Trim() + " " + dr[ "surname" ].ToString().Trim();
					string strPlayerCode = dr[ "playerid" ].ToString();
					string strRookieYr = dr[ "rookieyr" ].ToString();
					string strPlayerPos = dr[ "posdesc" ].ToString();
					if ( strPlayerPos.IndexOf( Filters.DropPositions() ) == -1 )
						OtherList.Add( new NFLPlayer( strPlayerName, strPlayerCode, strPlayerRole,
													strRookieYr, strPlayerPos, strPlayerCat, null ) );
				}
			}
			return;
		}

		public void LoadCurrentStarters()
		{
			if ( StarterList == null ) StarterList = new ArrayList();
			LoadStarters( Constants.K_QUARTERBACK_CAT, strPos: string.Empty );
			LoadStarters( Constants.K_RUNNINGBACK_CAT, strPos: string.Empty );
			LoadStarters( Constants.K_RECEIVER_CAT, strPos: string.Empty );
			LoadStarters( Constants.K_KICKER_CAT, strPos: string.Empty );
			LoadStarters( Constants.K_LINEMAN_CAT, strPos: string.Empty );
			LoadStarters( Constants.K_DEFENSIVEBACK_CAT, strPos: string.Empty );
			LoadStarters( Constants.K_OFFENSIVELINE_CAT, strPos: string.Empty );
		}

		public void LoadTeam()
		{
			var ds = Utility.TflWs.GetCurrentPlayers( TeamCode );
			var dt = ds.Tables[ "player" ];
			if ( PlayerList == null ) PlayerList = new ArrayList();
			if ( dt.Rows.Count > 0 )
			{
				foreach ( DataRow dr in dt.Rows )
				{
					var strPlayerName = dr[ "firstname" ].ToString().Trim() + " " + dr[ "surname" ].ToString().Trim();
					var strPlayerCode = dr[ "playerid" ].ToString();
					var strRookieYr = dr[ "rookieyr" ].ToString();
					var strPlayerPos = dr[ "posdesc" ].ToString().Trim();
					var strPlayerRole = dr[ "role" ].ToString().Trim();
					PlayerList.Add( new NFLPlayer( strPlayerName, strPlayerCode, strPlayerRole,
													   strRookieYr, strPlayerPos, "", this ) );
				}
			}
			return;
		}

		public void LoadStarters( string catIn, string strPos )
		{
			var strPlayerName = "*Unknown*";
			var strPlayerCode = "XXXX01";
			const string strPlayerRole = "S";
			var strRookieYr = String.Empty;
			var strPlayerPos = String.Empty;

			var ds = Utility.TflWs.GetPlayer( TeamCode, catIn, strPlayerRole, strPos );
			var dt = ds.Tables[ "player" ];
			if ( dt.Rows.Count == 0 )
				StarterList.Add( new NFLPlayer( strPlayerName, strPlayerCode, strPlayerRole,
											  strRookieYr, strPlayerPos, "", this ) );
			else
			{
				foreach ( DataRow dr in dt.Rows )
				{
					strPlayerName = dr[ "firstname" ].ToString().Trim() + " " + dr[ "surname" ].ToString().Trim();
					strPlayerCode = dr[ "playerid" ].ToString();
					strRookieYr = dr[ "rookieyr" ].ToString();
					strPlayerPos = dr[ "posdesc" ].ToString().Trim();
					var dropPositions = Filters.DropPositions();
					if ( dropPositions.IndexOf( strPlayerPos ) == -1 )
						StarterList.Add( new NFLPlayer( strPlayerName, strPlayerCode, strPlayerRole,
														strRookieYr, strPlayerPos, "", this ) );

					else
						TraceIt( strPlayerName + " filtered out" );
				}
			}
			return;
		}

		private void LoadBackups( string catIn, string strPos )
		{
			string strPlayerName = "*Unknown*";
			string strPlayerCode = "XXXX01";
			const string strPlayerRole = "B";
			string strRookieYr = "";
			string strPlayerPos = String.Empty;

			DataSet ds = Utility.TflWs.GetPlayer( TeamCode, catIn, strPlayerRole, strPos );
			DataTable dt = ds.Tables[ "player" ];
			if ( dt.Rows.Count == 0 )
			{
				BackupList.Add( new NFLPlayer( strPlayerName, strPlayerCode, strPlayerRole,
											 strRookieYr, strPlayerPos, "", this ) );
			}
			else
			{
				foreach ( DataRow dr in dt.Rows )
				{
					strPlayerName = dr[ "firstname" ].ToString();
					strPlayerName += dr[ "surname" ];
					strPlayerCode = dr[ "playerid" ].ToString();
					strRookieYr = dr[ "rookieyr" ].ToString();
					strPlayerPos = dr[ "posdesc" ].ToString();
					if ( strPlayerPos.IndexOf( Filters.DropPositions() ) == -1 )
						BackupList.Add( new NFLPlayer( strPlayerName, strPlayerCode, strPlayerRole,
													 strRookieYr, strPlayerPos, "", this ) );
				}
			}
			return;
		}

		#endregion Grid Loads

		#region Old style output

		public string BoxHtml( bool showBackups, string catIn, string strPos )
		{
			ClearLists();

			LoadOldGrid( catIn, strPos );

			var s = HtmlLib.TableDataOpen( "VALIGN=TOP" ) + "\n";
			const string sColour = "WHITE";

			s += HtmlLib.TableOpen( "BORDER=0 WIDTH=150 CELLSPACING=0 CELLPADDING=0" ) +
				 CellLineBold( TeamBox( true ), sColour ) +
				 CellLine( StartersBox(), sColour );
			if ( showBackups )
			{
				s += CellLine( "&nbsp;", sColour ) +
					 CellLine( BackupsBox(), sColour );
				s += CellLine( "&nbsp;", sColour ) +
					 CellLine( OthersBox(), sColour );
				s += CellLine( "&nbsp;", sColour ) +
					 CellLine( InjuryBox(), sColour );
			}
			s += HtmlLib.TableClose() + "\n" + HtmlLib.TableDataClose() + "\n";

			return s;
		}

		private void ClearLists()
		{
			StarterList = new ArrayList();
			BackupList = new ArrayList();
			OtherList = new ArrayList();
			InjuryList = new ArrayList();
		}

		public string InjuryHtml()
		{
			var s = HtmlLib.TableDataOpen( "VALIGN=TOP" ) + "\n";
			const string sColour = "WHITE";

			s += HtmlLib.TableOpen( "BORDER=0 WIDTH=150 CELLSPACING=0 CELLPADDING=0" ) +
				 CellLineBold( TeamBox( true ), sColour ) +
				 CellLine( InjuryBox(), sColour );
			s += HtmlLib.TableClose() + "\n" + HtmlLib.TableDataClose() + "\n";

			return s;
		}

		private string InjuryBox()
		{
			var s = HtmlLib.TableOpen( "BORDER=1 WIDTH=146 CELLSPACING=0 CELLPADDING=0" )
					   + HtmlLib.TableRowOpen() + HtmlLib.TableDataOpen() + "\n";
			var myEnumerator = InjuryList.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var p = ( NFLPlayer ) myEnumerator.Current;
				s += p.PlayerBox( isBold: true );
			}
			return s + HtmlLib.TableDataClose() + HtmlLib.TableRowClose() + HtmlLib.TableClose() + "\n";
		}

		private string StartersBox()
		{
			var s = HtmlLib.TableOpen( "BORDER=1 WIDTH=146 CELLSPACING=0 CELLPADDING=0" ) + HtmlLib.TableRowOpen() +
					   HtmlLib.TableDataOpen() + "\n";
			var myEnumerator = StarterList.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var p = ( NFLPlayer ) myEnumerator.Current;
				var showPlayer = true;
				if ( p.PlayerPos.Trim() == "FB" && !Filters.ShowFBs() )
					showPlayer = false;
				if ( ( p.PlayerPos.Trim() == "TE" ) && ( !Filters.ShowTEs() ) ) showPlayer = false;

				if ( showPlayer ) s += p.PlayerBox( true );
			}
			return s + HtmlLib.TableDataClose() + HtmlLib.TableRowClose() + HtmlLib.TableClose() + "\n";
		}

		private string BackupsBox()
		{
			var s = HtmlLib.TableOpen( "BORDER=1 WIDTH=146 CELLSPACING=0 CELLPADDING=0" ) + HtmlLib.TableRowOpen() +
					   HtmlLib.TableDataOpen() + "\n";
			var myEnumerator = BackupList.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var p = ( NFLPlayer ) myEnumerator.Current;
				s += p.PlayerBox( false );
			}
			return s + HtmlLib.TableDataClose() + HtmlLib.TableRowClose() + HtmlLib.TableClose() + "\n";
		}

		private string OthersBox()
		{
			var s = HtmlLib.TableOpen( "BORDER=1 WIDTH=146 CELLSPACING=0 CELLPADDING=0" ) +
					   HtmlLib.TableRowOpen() + HtmlLib.TableDataOpen() + "\n";
			var myEnumerator = OtherList.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var p = ( NFLPlayer ) myEnumerator.Current;
				s += p.PlayerBox( false );
			}
			return s + HtmlLib.TableDataClose() + HtmlLib.TableRowClose() + HtmlLib.TableClose() + "\n";
		}

		private string TeamBox( bool bLong )
		{
			var s = "\n";
			var nameOut = bLong
							  ? HtmlLib.Font( "Verdana", ProperName(), "-1" )
							  : HtmlLib.Font( "Verdana", ProperNickName(), "-1" );

			s += HtmlLib.TableOpen( "BORDER=0 WIDTH=148 HEIGHT=50 CELLSPACING=0 CELLPADDING=0" ) +
				 HtmlLib.TableRowOpen() + "\n" +
				 HtmlLib.TableData( string.Format( "<a href='..\\..\\DepthCharts\\{1}.htm'>{0}</a>", nameOut, TeamCode ), "WHITE", "ALIGN=CENTER" ) + "\n" +
				 HtmlLib.TableRowClose() + "\n" +
				 HtmlLib.TableClose() + "\n";
			return s;
		}

		private static string CellLine( string lineVal, string sColour )
		{
			var s = HtmlLib.TableRowOpen() + "\n";
			var lv = HtmlLib.Font( "Verdana", lineVal, "-1" );
			s += HtmlLib.TableData( lv,
								   sColour,
								   "ALIGN=CENTER" ) + "\n" +
				 HtmlLib.TableRowClose() + "\n";

			return s;
		}

		private static string CellLineBold( string lineVal, string sColour )
		{
			var s = HtmlLib.TableRowOpen() + "\n";
			var lv = HtmlLib.Font( "Verdana", lineVal, "-1" );
			s += HtmlLib.TableData( HtmlLib.Bold( lv ),
								   sColour,
								   "ALIGN=CENTER" ) + "\n" +
				 HtmlLib.TableRowClose() + "\n";

			return s;
		}

		#endregion Old style output

		#region Div Stuff

		public string TeamDiv()
		{
			var d = new DivBlock( HtmlLib.HtmlPad( Name, 25 ) + "&nbsp;&nbsp;" + RecordOut(), 2, "he2" );
			d.AddContainer( TeamDivContents() );
			return d.Html();
		}

		private string TeamDivContents()
		{
			TraceIt( "NFlTeam.TeamDivContents for " + TeamCode);

			string s = String.Empty;

			if ( Config.ShowGameLog() ) s += GameLogDiv();

			LoadPlayerUnits(); //  Dont want empty units - expensive as it takes a lot of time

			s += UnitTypeDiv();
			TraceIt( $"NFlTeam.TeamDivContents finished for {TeamCode}" );

			return s;
		}

		#endregion Div Stuff

		#region Game Log

		public ArrayList LoadGamesFrom( string sStartSeason, string sStartWeek, int offset )
		{
			if ( GameList == null ) GameList = new ArrayList();
			GameList.Clear();

			var processWeek = new NFLWeek( Int32.Parse( sStartSeason ), Int32.Parse( sStartWeek ), true );

			if ( offset > 0 )
			{
				for ( var i = 0; i < offset; i++ )
				{
					foreach ( var game in processWeek.GameList() )
						GameList.Add( game );
					processWeek = processWeek.NextWeek( processWeek );
				}
			}
			else
			{
				//  going backwards
				for ( var i = 0; i > offset; i-- )
				{
					//  process week
#if DEBUG
					Utility.Announce( string.Format( "  getting {0} game for {1} wk {2} ",
					  Name, processWeek.Season, processWeek.Week ) );
#endif
					var dr = Utility.TflWs.GetGame( processWeek.Season,
								  string.Format( "{0:00}", Int32.Parse( processWeek.Week ) ), TeamCode );
					if ( dr != null )
					{
						var myGame = new NFLGame( dr );
						GameList.Add( myGame );
					}
					processWeek = processWeek.PreviousWeek( processWeek, false, regularSeasonGamesOnly: true );
				}
			}
			return GameList;
		}

		public void LoadGames( string sTeam, string sSeason )
		{
#if DEBUG
			//Utility.Announce( $"  loading season {sSeason} games only for {sTeam}" );
#endif
			if ( GameList == null ) GameList = new ArrayList();
			GameList.Clear();
			var ds = Utility.TflWs.GetSeason( sTeam, sSeason );
			var dt = ds.Tables[ "sched" ];
			foreach ( DataRow dr in dt.Rows )
			{
				var gameCode = $"{sSeason}:{dr[ "WEEK" ]}-{dr[ "GAMENO" ]}";
				var g = Masters.Gm.GetGame( gameCode );
				if ( g == null )
				{
					g = new NFLGame( dr );
					Masters.Gm.AddGame( g );
				}
				GameList.Add( g );
			}
		}

		public void LoadPreviousGames( string sTeam, string sSeason )
		{
			//TODO:   fails single responsibility
#if DEBUG
			//Utility.Announce( string.Format( "NFLTeam.LoadPreviousGames:-loading last {0} games for {1}",
			//   Constants.K_GAMES_IN_REGULAR_SEASON, sTeam ) );
#endif
			if ( GameList == null ) GameList = new ArrayList();
			GameList.Clear();
			// get last 16 games
			var ds = Utility.TflWs.GetLastGames(
			   sTeam, Constants.K_GAMES_IN_REGULAR_SEASON, Int32.Parse( sSeason ) );
			AddGamesFromDataSet( ds, sTeam );
		}

		public void LoadPreviousRegularSeasonGames( string sTeam, string sSeason, DateTime focusDate )
		{
			//TODO:   fails single responsibility
			if ( sTeam == "LC" && sSeason == "2017" )
				sTeam = "SD";  // code change!!
			if ( sTeam == "LR" && sSeason == "2017" )
				sTeam = "SL";  // code change!!
#if DEBUG
			Announce( $"NFLTeam.LoadPreviousRegularSeasonGames:-loading last {Constants.K_GAMES_IN_REGULAR_SEASON} games for {sTeam}" );
#endif
			if ( GameList == null ) GameList = new ArrayList();
			GameList.Clear();
			// get last 16 games
			var ds = Utility.TflWs.GetLastRegularSeasonGames(
			   sTeam, Constants.K_GAMES_IN_REGULAR_SEASON, focusDate );
			AddGamesFromDataSet( ds, sTeam );
		}

		private void AddGamesFromDataSet( DataSet ds, string sTeam )
		{
			var dt = ds.Tables[ "sched" ];
			foreach ( var g in from DataRow dr in dt.Rows
							   where dr.RowState != DataRowState.Deleted
							   select new NFLGame( dr ) )
			{
				GameList.Add( g );
#if DEBUG
				Announce( string.Format( "{1} included game {2} -{0}",
				   g.Result.LogResult(), sTeam, g.GameCodeOut() ) );
#endif
			}
		}

		public void LoadPreviousGames( int nGames, DateTime theDate )
		{
			if ( GameList == null ) GameList = new ArrayList();
			GameList.Clear();

			var ds = Utility.TflWs.GetLastGames( teamCode: TeamCode, nGames: 4, theDate: theDate );
			AddGamesFromDataSet( ds, TeamCode );
		}

		public void LoadPreviousRegularSeasonGames( int nGames, DateTime theDate )
		{
			if ( GameList == null ) GameList = new ArrayList();
			GameList.Clear();

			var ds = Utility.TflWs.GetLastRegularSeasonGames( TeamCode, 4, theDate );
			AddGamesFromDataSet( ds, TeamCode );
		}

		private string GameLogDiv()
		{
			var gl = new DivBlock( "Games", 3, "he3" );
			gl.AddContainer( GameLog() );
			return gl.Html() + "\n";
		}

		private string GameLog()
		{
			var s = HtmlLib.DivOpen( "class='he4i'" ) + "\n" +
					   HtmlLib.TableOpen( "class='info' cellpadding='0' cellspacing='0'" ) +
					   "\n" + GameHeaders();
			if ( GameList == null ) LoadGames( TeamCode, Season );
			if ( GameList != null )
			{
				var myEnumerator = GameList.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					var g = ( NFLGame ) myEnumerator.Current;
					s += g.GameRow( TeamCode );
				}
			}
			s += HtmlLib.TableClose() + "\n" + HtmlLib.DivClose();
			return s;
		}

		private static string GameHeaders()
		{
			return HtmlLib.TableRowOpen() + "\n" + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Week" + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Date" + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Result" + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Away" + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Home" + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Score" + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + HtmlLib.TableRowClose() + "\n";
		}

		#endregion Game Log

		#region Unit routines

		public string UnitTypeDiv()
		{
			TraceIt( "NFlTeam.UnitTypeDiv for " + TeamCode);

			string s = String.Empty;

			if ( Filters.DoPassingUnit() || Filters.DoRunningUnit() || Filters.DoProtectionUnit() ||
				Filters.DoKickingUnit() )
			{
				var o =
				   new DivBlock( $"Offence  {_ratings.Substring( 0, 3 )} Exp {OffExp()}", 3, "he3" );
				o.AddContainer( OffUnits() );
				s += o.Html() + "\n";
			}
			if ( Filters.DoPassDefenceUnit() || Filters.DoPassRushUnit() || Filters.DoRunDefenceUnit() )
			{
				var d =
				   new DivBlock( string.Format( "Defence  {0} Exp {1}", _ratings.Substring( 3, 3 ), DefExp() ), 3, "he3" );
				d.AddContainer( DefUnits() );
				s += d.Html() + "\n";
			}
			TraceIt( "NFlTeam.UnitTypeDiv finished for " + TeamCode);

			return s;
		}

		public void LoadPlayerUnits()
		{

			TraceIt( "NFlTeam.LoadLineupPlayers:Loading units for " + TeamCode );
			DumpStarters();

			if ( Filters.DoPassingUnit() ) LoadPassingUnit( Filters.QbRoleFilter() );
			if ( Filters.DoRunningUnit() ) LoadRunningUnit( Filters.RbRoleFilter() );
			if ( Filters.DoProtectionUnit() ) LoadProtectionUnit( Filters.OlRoleFilter() );
			if ( Filters.DoPassRushUnit() ) LoadPassRushUnit( Filters.DefRoleFilter() );
			if ( Filters.DoPassDefenceUnit() ) LoadPassDefenceUnit( Filters.DefRoleFilter() );
			if ( Filters.DoRunDefenceUnit() ) LoadRunDefenceUnit( Filters.DefRoleFilter() );
			if ( Filters.DoKickingUnit() ) LoadKickingUnit();

			DumpSpots();
			TraceIt( "NFlTeam.LoadLineupPlayers:Finished loading units for " + TeamCode);

		}

		private int OffExp()
		{
			var ep = 0;
			if ( Filters.CalculateUnitExperience() )
			{
				TraceIt( "NFlTeam.OffExp: for " + TeamCode );
				if ( PassingUnit != null ) ep += UnitExperience( PassingUnit );
				if ( RunningUnit != null ) ep += UnitExperience( RunningUnit );
				if ( ProtectionUnit != null ) ep += UnitExperience( ProtectionUnit );
				if ( KickingUnit != null ) ep += UnitExperience( KickingUnit );
			}
			return ep;
		}

		private int DefExp()
		{
			var ep = 0;
			if ( Filters.CalculateUnitExperience() )
			{
				TraceIt( "NFlTeam.DefExp: for " + TeamCode );
				if ( PassRushUnit != null ) ep += UnitExperience( PassRushUnit );
				if ( RunDefenceUnit != null ) ep += UnitExperience( RunDefenceUnit );
				if ( PassDefenceUnit != null ) ep += UnitExperience( PassDefenceUnit );
			}
			return ep;
		}

		private int UnitExperience( ArrayList unit )
		{
			TraceIt( "NFlTeam.UnitExperience:Calculating Unit Experience" );
			var tot = 0;
			if ( unit != null )
				tot += ( from NFLPlayer p in unit where p != null select ( int ) p.ExperiencePoints ).Sum();

			return tot;
		}

		public string OffUnits()
		{
			var s = String.Empty;
			if ( Filters.DoPassingUnit() )
			{
				const string fmt = "Passing Unit Grade: {0} Exp {1}";
				var po =
				   new DivBlock(
					  string.Format( fmt, _ratings.Substring( 0, 1 ),
									UnitExperience( PassingUnit ) ), 4, "he4h" );
				po.AddContainer( PlayerUnit( PassingUnit ) );
				s += po.Html() + "\n";
			}
			if ( Filters.DoRunningUnit() )
			{
				TraceIt( string.Format( "NFlTeam.OffUnits:Doing Running Unit for {0}", TeamCode ) );
				var ro =
				   new DivBlock(
					  $"Rushing UnitGrade: {_ratings.Substring( 1, 1 )} Exp {UnitExperience( RunningUnit )}", 4, "he4h" );
				ro.AddContainer( PlayerUnit( RunningUnit ) );
				s += ro.Html() + "\n";
			}
			if ( Filters.DoProtectionUnit() )
			{
				var pp =
				   new DivBlock(
					  string.Format( "Pass Protection UnitGrade: {0} Exp {1}", _ratings.Substring( 2, 1 ),
									UnitExperience( ProtectionUnit ) ), 4, "he4h" );
				pp.AddContainer( Unit( ProtectionUnit ) );
				s += pp.Html() + "\n";
			}
			if ( Filters.DoKickingUnit() )
			{
				var kk = new DivBlock( string.Format( "Kicking Exp {0} ", UnitExperience( KickingUnit ) ), 4, "he4h" );
				kk.AddContainer( PlayerUnit( KickingUnit ) );
				s += kk.Html() + "\n";
			}
			return s;
		}

		public string DefUnits()
		{
			var s = String.Empty;
			if ( Filters.DoPassRushUnit() )
			{
				var pr =
				   new DivBlock(
					  string.Format( "Pass Rush Unit Grade: {0} Exp {1}", _ratings.Substring( 3, 1 ),
									UnitExperience( PassRushUnit ) ), 4, "he4h" );
				pr.AddContainer( PlayerUnit( PassRushUnit ) );
				s += pr.Html() + "\n";
			}
			if ( Filters.DoRunDefenceUnit() )
			{
				var rd =
				   new DivBlock(
					  string.Format( "Run Defence Unit Grade: {0} Exp {1}", _ratings.Substring( 4, 1 ),
									UnitExperience( RunDefenceUnit ) ), 4, "he4h" );
				rd.AddContainer( PlayerUnit( RunDefenceUnit ) );
				s += rd.Html() + "\n";
			}
			if ( Filters.DoPassDefenceUnit() )
			{
				var pd =
				   new DivBlock(
					  string.Format( "Pass Defence Unit Grade: {0} Exp {1}", _ratings.Substring( 5, 1 ),
									UnitExperience( PassDefenceUnit ) ), 4, "he4h" );
				pd.AddContainer( PlayerUnit( PassDefenceUnit ) );
				s += pd.Html() + "\n";
			}
			return s;
		}

		private static string Unit( ICollection unitList )
		{
			string s = String.Empty;

			if ( unitList != null )
			{
				if ( unitList.Count > 0 )
				{
					//  for each player
					s = HtmlLib.DivOpen( "class='he4i'" ) + "\n"
						+ HtmlLib.TableOpen( "class='info' cellpadding='0' cellspacing='0'" )
						+ "\n" + PlayerHeaders();
					var myEnumerator = unitList.GetEnumerator();
					while ( myEnumerator.MoveNext() )
					{
						var p = ( NFLPlayer ) myEnumerator.Current;
						s += p.PlayerRow( false );
					}
					s += HtmlLib.TableClose() + "\n" + HtmlLib.DivClose();
				}
			}
			return s;
		}

		private string PlayerUnit( ArrayList unitList )
		{
			Announce( "NFlTeam.PlayerUnit" );

			var s = String.Empty;

			if ( unitList != null )
			{
				if ( unitList.Count > 0 )
				{
					foreach ( NFLPlayer p in unitList )
					{
						var pd = new DivBlock( p.PlayerHeader(), 5, "he5h" );
						pd.AddContainer( p.PlayerDiv() );
						s += pd.Html();
					}
				}
			}
			return s;
		}

		private static string PlayerHeaders()
		{
			return HtmlLib.TableRowOpen() + "\n" + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Player" +
				   HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Pos" +
				   HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Role" +
				   HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "FT" +
				   HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Age" +
				   HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + "Scores" +
				   HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowClose() + "\n";
		}

		#endregion Unit routines

		#region Tipping Stuff

		public string SeasonProjection( IPrognosticate strategy, string metricName, DateTime projectionDate )
		{
			if ( strategy == null ) return String.Empty;

			strategy.AuditTrail = true;

			NFLResult result;
			NFLGame game;
			int metric;

			//  Lazy intitialisation of the sched
			if ( _sched == null ) InitialiseProjections();

			var om = new NFLOutputMetric( metricName, null, this );

			//  Go through the schedule and predict the games
			//  Clear win totals
			var projWins = 0;
			var projLosses = 0;
			var projTies = 0;

			if ( ( _sched != null ) && ( _sched.GameList != null ) )
			{
				//  load the projections
				for ( var i = 0; i < _sched.GameList.Count; i++ )
				{
					if ( i > 20 ) continue;
					game = ( NFLGame ) _sched.GameList[ i ];
					result = strategy.PredictGame( game, new DbfPredictionStorer(), projectionDate );
					if ( i > 15 ) continue;
#if DEBUG
					Announce( result.LogResult() );
#endif
					metric = metricName == "Spread"
								? Convert.ToInt32( ( ( game.IsHome( TeamCode ) ) ? result.Spread : 0.0M - ( result.Spread ) ) )
								: ( metricName == "Tdp"
									  ? ( ( game.IsHome( TeamCode ) ) ? result.HomeTDp : result.AwayTDp )
									  : ( ( game.IsHome( TeamCode ) ) ? result.HomeTDr : result.AwayTDr ) );

					if ( metric > 0 ) projWins++;
					if ( metric == 0 ) projTies++;
					if ( metric < 0 ) projLosses++;

					om.AddWeeklyOutput(
					   Int32.Parse( game.Week ), metric,
					   game.Opponent( TeamCode ), game.IsHome( TeamCode ) );
				}
			}
			if ( metricName == "Spread" )
			{
				//  Load up previous results
				if ( _prevSched.GameList != null )
				{
					foreach ( NFLGame g in _prevSched.GameList )
					{
						if ( Int32.Parse( g.Week ) < 18 )
						{
							result = g.Result;
							metric = Convert.ToInt32( ( g.IsHome( TeamCode ) ) ? result.Spread : 0.0M - ( result.Spread ) );
							ApplyPrevResult( metric );
							var so =
							   new SeasonOpposition( g.Opponent( TeamCode ), g.IsHome( TeamCode ), metric );
							om.AddPrevWeeklyOutput( Int32.Parse( g.Week ), so );
						}
					}
				}
			}

			if ( ProjectionList == null ) ProjectionList = new ArrayList();

			ProjectionList.Add( om ); //  store it for later

			Wins = projWins;
			Losses = projLosses;
			Ties = projTies;

			return om.RenderAsHtml();
		}

		private void ApplyPrevResult( int metric )
		{
			if ( metric > 0 ) PrevWins++;
			if ( metric < 0 ) PrevLosses++;
		}

		#endregion Tipping Stuff

		#region Kicker Stuff

		public string KickerProjection()
		{
			//projFGList = new ArrayList();

			//  Load the kickers career results into fgResultList
			LoadKicker();

			if ( GameList == null )
			{
				LoadGames( TeamCode, Season );
			}

			if ( GameList != null )
			{
				//  For each game in the game list
				for ( int i = 0; i < GameList.Count; i++ )
				{
					NFLGame game = ( NFLGame ) GameList[ i ];

					//  pick a random result for the game
					int projectedFg = GuessFGs( game );
					FieldGoals += projectedFg;
					if ( TeamCode == game.HomeTeam )
						game.ProjectedHomeFg = projectedFg;
					else
						game.ProjectedAwayFg = projectedFg;
				}
			}

			// render some HTML table rows
			return RenderKicker();
		}

		/// <summary>
		///   Puts the number of FGs allowed per game into the fgResultlist.
		///   This balances off the kickers results.
		///   Need to include 0 games.
		///   Only go back to last season.
		/// </summary>
		private static ArrayList GetFGallowed( string oppteamCode )
		{
			//  Get the schedule for last season for the opponent

			ArrayList fgAllowed = new ArrayList();

			DataSet ds = Utility.TflWs.TeamSchedDs( Utility.LastSeason(), oppteamCode );
			DataTable dt = ds.Tables[ "sched" ];
			//  For each game on their schedule
			foreach ( DataRow dr in dt.Rows )
			{
				string week = dr[ "WEEK" ].ToString();
				string gameNo = dr[ "GAMENO" ].ToString();
				string gameCode = Utility.LastSeason() + week + gameNo;
				string opponentsOpponent = dr[ "HOMETEAM" ].ToString() == oppteamCode
											  ? dr[ "AWAYTEAM" ].ToString()
											  : dr[ "HOMETEAM" ].ToString();
				//  How many FGs did their opponents score?
				DataSet scores =
				   Utility.TflWs.GetTeamScoresFor( "3", opponentsOpponent,
												  Utility.LastSeason(), week, gameNo );
				int fgsAllowed = scores.Tables[ 0 ].Rows.Count;
				SeasonOpposition so = new SeasonOpposition( opponentsOpponent, false, fgsAllowed );

				fgAllowed.Add( new FieldGoalResult( gameCode, so ) );
			}
			return fgAllowed;
		}

		private string RenderKicker()
		{
			var om = new NFLOutputMetric( "Fg", _kicker, this );
			if ( GameList != null )
			{
				//  load the projections
				for ( int i = 0; i < GameList.Count; i++ )
				{
					NFLGame game = ( NFLGame ) GameList[ i ];
					int nFg = TeamCode == game.HomeTeam ? game.ProjectedHomeFg : game.ProjectedAwayFg;
					om.AddWeeklyOutput( Int32.Parse( game.Week ), nFg, game.Opponent( TeamCode ), game.IsHome( TeamCode ) );
				}
				//  load the last season
				if ( _fgResultList != null )
				{
					for ( int j = 0; j < _fgResultList.Count; j++ )
					{
						FieldGoalResult fgr = ( FieldGoalResult ) _fgResultList[ j ];
						if ( fgr.IsLastYear() )
							om.AddPrevWeeklyOutput( Int32.Parse( fgr.Week() ), fgr.So );
					}
				}
			}

			if ( ProjectionList == null ) ProjectionList = new ArrayList();

			ProjectionList.Add( om );

			return om.RenderAsHtml();
		}

		private int GuessFGs( NFLGame game )
		{
			int nFg = 1;
			string opponent = ( TeamCode == game.HomeTeam ) ? game.AwayTeam : game.HomeTeam;
			//  Also load in the FGs allowed by the defensive team
			ArrayList fgAllowedList = GetFGallowed( opponent );
			ArrayList totalFGlist = JoinLists( fgAllowedList, _fgResultList );

			int nStats = totalFGlist.Count;

			if ( nStats > 0 )
			{
				RandomNumber myRandom = new RandomNumber(); //  Generate a random number

				nFg = GuessFg( _fgResultList, nStats, myRandom );

				if ( nFg > 0 )
					if ( game.IsBadWeather() ) nFg--;

				if ( game.IsDomeGame() ) nFg++;

				//if ( game.HomeTeam == this.TeamCode ) nFG++;
			}

			return nFg;
		}

		private static ArrayList JoinLists( IList list1, ArrayList list2 )
		{
			for ( var i = 0; i < list1.Count; i++ )
			{
				var fgr = ( FieldGoalResult ) list1[ i ];
				list2.Add( fgr );
			}
			return list2;
		}

		private static int GuessFg( IList fgResList, int nStats, RandomNumber myRandom )
		{
			int nRandom = myRandom.GetNumberInRange( 0, nStats - 1 );
			FieldGoalResult res = ( FieldGoalResult ) fgResList[ nRandom ];
			return res.FgCount;
		}

		public void LoadKicker()
		{
			_fgResultList = new ArrayList();
			var fgCount = 0;
			_kicker = GetPlayerAt( "PK", 1, false, false ); //  depends on the Kicking unit being loaded
			string gameCode = String.Empty;
			string loWeek = "999999";
			string hiWeek = "000000";

			if ( _kicker != null )
			{
				//  Get all the Scores for this kicker
				var ds = Utility.TflWs.GetScoresFor( "3", _kicker.PlayerCode );
				var dt = ds.Tables[ "score" ];
				if ( dt.Rows.Count > 0 )
				{
					string lastGameCode = string.Format( "{0}{1}{2}", dt.Rows[ 0 ][ "SEASON" ],
														dt.Rows[ 0 ][ "WEEK" ], dt.Rows[ 0 ][ "GAMENO" ] );

					foreach ( DataRow dr in dt.Rows )
					{
						//  for each score in the game

						string gameWeek = string.Format( "{0}{1}", dr[ "SEASON" ], dr[ "WEEK" ] );

						gameCode = gameWeek + dr[ "GAMENO" ];

						if ( gameCode != lastGameCode )
						{
							//  Add the result to the kickers list
							SeasonOpposition so = GetLastOpp( lastGameCode, fgCount );
							_fgResultList.Add( new FieldGoalResult( gameCode, so ) );
							fgCount = 0;
							lastGameCode = gameCode;
						}
						fgCount++;
						if ( string.Compare( gameWeek, loWeek ) < 0 ) loWeek = gameWeek;
						if ( string.Compare( gameWeek, hiWeek ) > 0 ) hiWeek = gameWeek;
					}
					_fgResultList.Add( new FieldGoalResult( gameCode, GetLastOpp( lastGameCode, fgCount ) ) );
					//  How many weeks in the range
					_kicker.SetHiWeek( hiWeek );
					_kicker.SetLoWeek( loWeek );

					int nRange = WeekRange( loWeek, hiWeek );

					//  Add goose eggs for the difference
					int diff = nRange - _fgResultList.Count;
					var soDud = new SeasonOpposition( "   ", false, 0 );
					for ( int i = 1; i < diff; i++ )
						_fgResultList.Add( new FieldGoalResult( loWeek, soDud ) );
				}
			}
		}

		private SeasonOpposition GetLastOpp( string gameCode, int fgCount )
		{
			var gameSeason = gameCode.Substring( 0, 4 );
			var gameWeek = gameCode.Substring( 4, 2 );
			var gameNo = gameCode.Substring( 6, 1 );
			var bHome = false;
			var strOpp = "";

			//  Need to Know the game data here
			NFLGame game = GetGame( gameSeason, gameWeek, gameNo );

			if ( game != null )
			{
				if ( game.HomeTeam == TeamCode )
				{
					bHome = true;
					strOpp = game.AwayTeam;
				}
				else
					strOpp = game.HomeTeam;
			}
			return ( new SeasonOpposition( strOpp, bHome, fgCount ) );
		}

		public NFLGame GetGame( string gameSeason, string gameWeek, string gameNo )
		{
			var gameKey = string.Format( "{0}:{1}-{2}", gameSeason, gameWeek, gameNo );
			Utility.Announce( string.Format( "NFLTeam:GetGame: for {0}", gameKey ) );
			var game = Masters.Gm.GetGame( gameKey );
			if ( game == null )
			{
				var ds = Utility.TflWs.GameFor( gameSeason, gameWeek, gameNo );
				var dt = ds.Tables[ "sched" ];
				foreach ( DataRow dr in dt.Rows )
				{
					game = new NFLGame( dr );
					Masters.Gm.AddGame( game );
					break; //  should only be one!
				}
			}
			return game;
		}

		private static int WeekRange( string loWeek, string hiWeek )
		{
			return Base17( hiWeek ) - Base17( loWeek );
		}

		private static int Base17( string week )
		{
			//  Convert to base 17
			//  multiply yr by 17 and add week
			return ( Int32.Parse( week.Substring( 0, 4 ) ) * 17 ) + Int32.Parse( week.Substring( 4, 2 ) );
		}

		/// <summary>
		///   Team for output display.
		/// </summary>
		/// <returns></returns>
		public string NameOut()
		{
			if ( NickName == null )
			{
				//  load it up
				Name = Utility.TflWs.TeamFor( TeamCode, Season );
				NickName = "";
			}
			return string.Format( "{0} {1}", Name.Trim(), NickName.Trim() ).Trim();
		}

		public string DepthChartLink()
		{
			var url = string.Format( "<a href='../DepthCharts/{0}.htm'>{1}</a>",
				TeamCode, NameOut() );
			return url;
		}

		public override string ToString()
		{
			return NameOut();
		}

		public int FieldGoals { get; set; }

		public int FaPoints { get; set; }

		public int PlayersIn { get; set; }

		public int PlayersOut { get; set; }

		public string PlayersGot { get; set; }

		public string PlayersLost { get; set; }

		public double SoS { get; set; }

		public int ExpWins { get; set; }

		public int ExpLosses { get; set; }

		public ArrayList GameList { get; set; }

		public Hashtable MetricsHt { get; set; }

		public int Passes { get; set; }

		public int Runs { get; set; }

		public decimal ExperiencePoints { get; set; }

		public DataSet LineupDs { get; set; }

		public string Ratings
		{
			get { return AdjustedRatings( _ratings ); }
			set { _ratings = value; }
		}

		public int ProjectedSacks { get; set; }

		public int ProjectedSteals { get; set; }

		public int Points { get; set; }

		public int MTies
		{
			get { return Ties; }
			set { Ties = value; }
		}

		public int[,] NibbleRating
		{
			get { return _nibbleRating; }
			set { _nibbleRating = value; }
		}

		public int Ties { get; set; }

		#endregion Kicker Stuff

		public string TdrBreakdownLink()
		{
			return string.Format( ".//breakdowns//{0}-TDR.htm", TeamCode );
		}

		public string TdpBreakdownLink()
		{
			return string.Format( ".//breakdowns//{0}-TDP.htm", TeamCode );
		}

		//TODO:  Needs to be updated each pre season with adjustments

		#region Adjustments

		public string AdjustedRatings( string ratings )
		{
			if ( string.IsNullOrEmpty( ratings ) ) return ratings;

			if ( Utility.CurrentSeason().Equals( "2010" ) )
				ratings = ConvertTeamRatings2010( ratings );
			if ( Utility.CurrentSeason().Equals( "2011" ) )
				ratings = ConvertTeamRatings2011( ratings );
			if ( Utility.CurrentSeason().Equals( "2015" ) )
				ratings = ConvertTeamRatings2015( ratings );
			return ratings;
		}

		private string ConvertTeamRatings2010( string ratings )
		{
			if ( TeamCode.Equals( "BB" ) )
			{
				//  Lose Terrel Owens
				ratings = PassOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "AC" ) )
			{
				//  Lost Kurt Warner and Boldin
				ratings = PassOffence( -2, ratings );
				//  Lost Antone Rolle
				ratings = PassDefence( -1, ratings );
			}

			if ( TeamCode.Equals( "AF" ) )
			{
				//  Gained Dunta Robinson RCB
				ratings = PassDefence( 1, ratings );
			}

			if ( TeamCode.Equals( "BR" ) )
			{
				//  Got Boldin
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "CI" ) )
			{
				//  Get Terrel Owens
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "CH" ) )
			{
				//  Got Julius Peppers
				ratings = PassRush( 1, ratings );
				//  Get back Urbackker
				ratings = RunDefence( 1, ratings );
			}

			if ( TeamCode.Equals( "CP" ) )
			{
				//  Lost Julius Peppers
				ratings = PassRush( -1, ratings );
				//  Lost Jake Delhomme
				ratings = PassOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "CL" ) )
			{
				//  Get Jake Delhomme
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "DB" ) )
			{
				//  Lose Brandon Marshall
				ratings = PassOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "DL" ) )
			{
				//  Got Van den Bosch
				ratings = PassRush( 1, ratings );
			}

			if ( TeamCode.Equals( "HT" ) )
			{
				//  Lost Dunta Robinson RCB
				ratings = PassDefence( -1, ratings );
			}

			if ( TeamCode.Equals( "IC" ) )
			{
				//  Lost Ryan Ilja OL
				ratings = OffensiveLine( -1, ratings );
			}

			if ( TeamCode.Equals( "KC" ) )
			{
				//  Got Ryan Ilja OL
				ratings = OffensiveLine( 1, ratings );
			}

			if ( TeamCode.Equals( "MD" ) )
			{
				//  Gain Brandon Marshall
				ratings = PassOffence( 1, ratings );
				// lost J Porter
				ratings = RunDefence( -1, ratings );
			}

			if ( TeamCode.Equals( "NG" ) )
			{
				//  Got Antone Rolle
				ratings = PassDefence( 1, ratings );
			}

			if ( TeamCode.Equals( "NO" ) )
			{
				//  Lost Fujita
				ratings = RunDefence( -1, ratings );
			}

			if ( TeamCode.Equals( "PE" ) )
			{
				//  Lost Donovan McNabb
				ratings = PassOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "OR" ) )
			{
				//  Replace Jamarcus with Campbell and get Ken Wimbley
				ratings = PassOffence( 1, ratings );
				ratings = RunDefence( 1, ratings );
			}

			if ( TeamCode.Equals( "PS" ) )
			{
				//  Get Troy Polamalu back
				ratings = PassDefence( 2, ratings );
			}

			if ( TeamCode.Equals( "SD" ) )
			{
				//  Got some new corners
				ratings = PassDefence( 1, ratings );
			}

			if ( TeamCode.Equals( "SF" ) )
			{
				//  Get 2 rookie Offensive linemen
				ratings = OffensiveLine( 1, ratings );
			}

			if ( TeamCode.Equals( "TT" ) )
			{
				//  Lost Van den Bosch
				ratings = PassRush( -1, ratings );
			}

			if ( TeamCode.Equals( "WR" ) )
			{
				//  Get Donovan McNabb
				ratings = PassOffence( 1, ratings );
			}
			return ratings;
		}

		private string ConvertTeamRatings2011( string ratings )
		{
			if ( TeamCode.Equals( "TT" ) )
			{
				//  CJ2K hold out
				ratings = RunOffence( -2, ratings );
			}

			if ( TeamCode.Equals( "AC" ) )
			{
				//  Got Kolb
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "AF" ) )
			{
				//  Got JuJones
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "BR" ) )
			{
				//  Got Evans
				ratings = PassOffence( 1, ratings );
				//  Vonta Leach
				ratings = RunOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "CI" ) )
			{
				//  Palmer retiring
				ratings = PassOffence( -2, ratings );
			}

			if ( TeamCode.Equals( "HT" ) )
			{
				//  Lost Vonta Leach
				ratings = RunOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "IC" ) )
			{
				//  Lost Bob Sanders
				ratings = RunDefence( -1, ratings );
			}

			if ( TeamCode.Equals( "SD" ) )
			{
				//  Got Bob Sanders
				ratings = RunDefence( 1, ratings );
				//  Vincent jackson not holding out
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "NJ" ) )
			{
				// Lost Braylon Edwards
				ratings = PassOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "SF" ) )
			{
				// Got Braylon Edwards
				ratings = PassOffence( 1, ratings );
				//  Get 2 rookie Offensive linemen
				ratings = OffensiveLine( 1, ratings );
			}

			if ( TeamCode.Equals( "OR" ) )
			{
				//  Lost Nnamdi
				ratings = PassDefence( -1, ratings );
			}

			if ( TeamCode.Equals( "PE" ) )
			{
				//  Got Nnamdi
				ratings = PassDefence( 2, ratings );
			}

			if ( TeamCode.Equals( "BB" ) )
			{
				//  Lost Evans
				ratings = PassOffence( -1, ratings );
			}

			return ratings;
		}

		private string ConvertTeamRatings2015( string ratings )
		{
			if ( TeamCode == null ) return "null team";

			if ( TeamCode.Equals( "SS" ) )
			{
				//  + Jimmy Graham
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "GB" ) )
			{
				//  odds
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "DC" ) )
			{
				//  Lost Murray
				ratings = RunOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "BR" ) )
			{
				//  odds
				ratings = RunOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "NE" ) )
			{
				//  odds
				ratings = RunOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "KC" ) )
			{
				//  odds
				ratings = RunOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "DL" ) )
			{
				//  Lost Bush
				ratings = RunOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "NO" ) )
			{
				//  lost Graham
				ratings = PassOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "AF" ) )
			{
				// odds
				ratings = PassRush( 1, ratings );
			}

			if ( TeamCode.Equals( "CP" ) )
			{
				// Odds
				ratings = PassRush( -1, ratings );
			}

			if ( TeamCode.Equals( "BB" ) )
			{
				//  odds
				ratings = PassDefence( -1, ratings );
				//  Got McCoy
				ratings = RunOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "MV" ) )
			{
				//  Got AP is back
				ratings = RunOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "NJ" ) )
			{
				//  odds
				ratings = RunOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "SD" ) )
			{
				//  got Rookie
				ratings = RunOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "OR" ) )
			{
				//  got Rookie
				ratings = PassOffence( 1, ratings );
			}

			if ( TeamCode.Equals( "WR" ) )
			{
				//  RG3 bombs
				ratings = PassOffence( -1, ratings );
			}

			if ( TeamCode.Equals( "JJ" ) )
			{
				//  odds
				ratings = PassRush( -1, ratings );
			}

			return ratings;
		}

		private string PassOffence( int diff, string ratings )
		{
			return ChangeRating( 0, diff, ratings );
		}

		private string RunOffence( int diff, string ratings )
		{
			return ChangeRating( 1, diff, ratings );
		}

		private string PassRush( int diff, string ratings )
		{
			return ChangeRating( 3, diff, ratings );
		}

		private string RunDefence( int diff, string ratings )
		{
			return ChangeRating( 4, diff, ratings );
		}

		private string PassDefence( int diff, string ratings )
		{
			return ChangeRating( 5, diff, ratings );
		}

		private string OffensiveLine( int diff, string ratings )
		{
			return ChangeRating( 2, diff, ratings );
		}

		public string ChangeRating( int startPos, int diff, string ratings )
		{
			var unit = Convert.ToChar( ratings.Substring( startPos, 1 ) );
			var ascii = Convert.ToInt32( unit );
			ascii -= diff;
			if ( ascii > 69 ) ascii = 69;  // E
			if ( ascii < 65 ) ascii = 65;  // A
			var newUnit = Convert.ToChar( ascii );
			string newRatings;
			switch ( startPos )
			{
				case 0:
					newRatings = string.Format( "{0}{1}", newUnit, ratings.Substring( 1, 5 ) );
					break;

				case 5:
					newRatings = string.Format( "{0}{1}", ratings.Substring( 0, 5 ), newUnit );
					break;

				default:
					newRatings = string.Format( "{0}{1}{2}", ratings.Substring( 0, startPos ), newUnit,
											   ratings.Substring( startPos + 1, 6 - startPos - 1 ) );
					break;
			}

			Announce( string.Format( "Adjusted Ratings for {2} changed from {0} to {1}", ratings, newRatings, TeamCode ) );

			return newRatings;
		}

		#endregion Adjustments

		#region Player loads

		private void LoadPassingUnit( string roleFilter )
		{
			TraceIt( string.Format( "NFLTeam.LoadPassingUnit {0}", roleFilter ) );

			if ( PassingUnit == null ) PassingUnit = new ArrayList();
			PassingUnit.Clear();
			if ( Filters.ShowQBs() ) AddPlayers( "1", "QB", roleFilter, PassingUnit );
			if ( Filters.ShowWRs() ) AddPlayers( "3", "WR", roleFilter, PassingUnit );
			if ( Filters.ShowWRs() ) AddPlayers( "3", "FL", roleFilter, PassingUnit );
			if ( Filters.ShowWRs() ) AddPlayers( "3", "SE", roleFilter, PassingUnit );
			if ( Filters.ShowTEs() ) AddPlayers( "3", "TE", roleFilter, PassingUnit );

			TraceIt( string.Format( "NFLTeam.LoadPassingUnit {0} - finished", roleFilter ) );
		}

		public void LoadQuarterbacks( string roleFilter )
		{
			TraceIt( string.Format( "NFLTeam.LoadQuarterbacks {0}", roleFilter ) );
			if ( PassingUnit == null ) PassingUnit = new ArrayList();
			PassingUnit.Clear();
			if ( Filters.ShowQBs() ) AddPlayers( "1", "QB", roleFilter, PassingUnit );
		}

		public void LoadPassReceivers( string roleFilter )
		{
			if ( PassingUnit == null ) PassingUnit = new ArrayList();
			PassingUnit.Clear();
			if ( Filters.ShowWRs() ) AddPlayers( "3", "WR", roleFilter, PassingUnit );
			if ( Filters.ShowWRs() ) AddPlayers( "3", "FL", roleFilter, PassingUnit );
			if ( Filters.ShowWRs() ) AddPlayers( "3", "SE", roleFilter, PassingUnit );
			if ( Filters.ShowTEs() ) AddPlayers( "3", "TE", roleFilter, PassingUnit );
		}

		private void LoadRunningUnit( string roleFilter )
		{
			TraceIt( string.Format( "NFLTeam.LoadRunningUnit {0}", roleFilter ) );

			if ( RunningUnit == null ) RunningUnit = new ArrayList();
			RunningUnit.Clear();
			if ( Filters.ShowRBs() ) AddPlayers( "2", "RB", roleFilter, RunningUnit );
			if ( Filters.ShowHBs() ) AddPlayers( "2", "HB", roleFilter, RunningUnit );
			if ( Filters.ShowFBs() ) AddPlayers( "2", "FB", roleFilter, RunningUnit );

			//DumpUnit( RunningUnit, "Running backs" );
		}

		public List<string> LoadKickUnit()
		{
			if ( KickUnit == null ) KickUnit = new KickUnit();
			return KickUnit.Load( TeamCode );
		}

		public List<string> LoadRushUnit()
		{
			TraceIt( $"NFLTeam.LoadRushUnit for {TeamCode}" );

			if ( RunUnit == null ) RunUnit = new RushUnit();
			return RunUnit.Load( TeamCode );
		}

		public List<string> LoadPassUnit()
		{
			TraceIt( "NFLTeam.LoadPasshUnit" );

			if ( PassUnit == null ) PassUnit = new PassUnit();
			return PassUnit.Load( TeamCode );
		}



		private void DumpUnit( ArrayList unit, string unitName )
		{
			Announce( unitName );
			foreach ( NFLPlayer p in unit )
				Announce( string.Format( "  {0}", p.PlayerOut() ) );
		}



		/// <summary>
		/// Loads the protection unit.
		/// </summary>
		/// <param name="roleFilter">The role filter eg RosterGrid.OLRoleFilter().</param>
		private void LoadProtectionUnit( string roleFilter )
		{
			TraceIt( $"NFLTeam.LoadProtectionUnit {roleFilter}");
			if ( ProtectionUnit == null ) ProtectionUnit = new ArrayList();
			ProtectionUnit.Clear();
			AddPlayers( "7", "OL", roleFilter, ProtectionUnit );
			AddPlayers( "7", "LT", roleFilter, ProtectionUnit );
			AddPlayers( "7", "RT", roleFilter, ProtectionUnit );
			AddPlayers( "7", "C", roleFilter, ProtectionUnit );
			AddPlayers( "7", "G", roleFilter, ProtectionUnit );
		}

		private void LoadKickingUnit()
		{
			TraceIt( "NFLTeam.LoadKickingUnit");
			if ( KickingUnit == null ) KickingUnit = new ArrayList();
			KickingUnit.Clear();
			AddPlayers( "4", "PK", "*", KickingUnit );
			AddPlayers( "4", "K ", "*", KickingUnit );
		}

		private void LoadPassRushUnit( string roleFilter )
		{
			TraceIt( $"NFLTeam.LoadPassRushUnit {roleFilter}");
			if ( PassRushUnit == null ) PassRushUnit = new ArrayList();
			PassRushUnit.Clear();
			AddPlayers( "5", "DE", roleFilter, PassRushUnit );
			AddPlayers( "5", "RDE", roleFilter, PassRushUnit );
			AddPlayers( "5", "LDE", roleFilter, PassRushUnit );
			AddPlayers( "5", "DT", roleFilter, PassRushUnit );
			AddPlayers( "5", "DL", roleFilter, PassRushUnit );
			AddPlayers( "5", "LE", roleFilter, PassRushUnit );
			AddPlayers( "5", "RE", roleFilter, PassRushUnit );
		}

		private void LoadPassDefenceUnit( string roleFilter )
		{
			TraceIt( $"NFLTeam.LoadPassDefenceUnit {roleFilter}");
			if ( PassDefenceUnit == null ) PassDefenceUnit = new ArrayList();
			PassDefenceUnit.Clear();
			AddPlayers( "6", "CB", roleFilter, PassDefenceUnit );
			AddPlayers( "6", "LCB", roleFilter, PassDefenceUnit );
			AddPlayers( "6", "RCB", roleFilter, PassDefenceUnit );
			AddPlayers( "6", "LC", roleFilter, PassDefenceUnit );
			AddPlayers( "6", "RC", roleFilter, PassDefenceUnit );
			AddPlayers( "6", "SS", roleFilter, PassDefenceUnit );
			AddPlayers( "6", "FS", roleFilter, PassDefenceUnit );
			AddPlayers( "6", "DB", roleFilter, PassDefenceUnit );
		}

		private void LoadRunDefenceUnit( string roleFilter )
		{
			TraceIt( $"NFLTeam.LoadRunDefenceUnit {roleFilter}");
			if ( RunDefenceUnit == null ) RunDefenceUnit = new ArrayList();
			RunDefenceUnit.Clear();
			AddPlayers( "5", "LDT", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "RDT", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "LT", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "RT", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "NT", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "OE", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "LB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "OLB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "RLB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "LLB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "LOLB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "LILB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "ROLB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "RILB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "SLB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "WLB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "MLB", roleFilter, RunDefenceUnit );
			AddPlayers( "5", "JLB", roleFilter, RunDefenceUnit );
		}

		private void AddPlayers( string strCat, string strPos, string strRole, ArrayList unitList )
		{
			TraceIt( "NFLTeam.AddPlayers: Adding players in Cat " + strCat + " " + strPos );

			if ( ( Config.HideBackups() ) && ( strRole == NFLPlayer.K_ROLE_BACKUP ) ) return;
			if ( ( Config.HideReserves() ) && ( strRole == NFLPlayer.K_ROLE_RESERVE ) ) return;
			if ( ( Config.HideInjuries() ) && ( strRole == NFLPlayer.K_ROLE_INJURED ) ) return;

			AddPlayerData( strCat, strPos, strRole, unitList );

			TraceIt( "NFLTeam.AddPlayers: Finished adding players in Cat " + strCat + " " + strPos);

			return;
		}

		private void AddPlayerData( string strCat, string strPos, string strRole, ArrayList unitList )
		{
			var roleFilter = String.Empty;

			if ( strRole != "*" ) roleFilter = strRole;

			var ds = Utility.TflWs.GetPlayer( TeamCode, strCat, roleFilter, strPos );
			var dt = ds.Tables[ "player" ];

			TraceIt( 
                string.Format( 
                    "NFLTeam.AddPlayerData: adding {0} players in Cat {1} role {2} ",
			        dt.Rows.Count, 
                    strCat, 
                    strPos ) );

			if ( dt.Rows.Count != 0 )
				foreach ( DataRow dr in dt.Rows )
					AddPlayer( dr, unitList );

			return;
		}

		private static bool AlreadyHave( IEnumerable list, string playerId )
		{
			return list.Cast<NFLPlayer>().Any( p => p.PlayerCode == playerId );
		}

		private void AddPlayer( DataRow dr, ArrayList list )
		{
			var strPlayerCode = dr[ "playerid" ].ToString();
			var strPlayerPos = dr[ "posdesc" ].ToString().Trim();
			var strPlayerName = dr[ "firstname" ].ToString().Trim() + " " + dr[ "surname" ].ToString().Trim();

			//Announce( string.Format( "   NFLTeam.AddPlayer:adding {0} {1}  ", strPlayerName, strPlayerPos ) );

			if ( AlreadyHave( list, strPlayerCode ) ) return;

			var nDrop = strPlayerPos.IndexOf( Filters.DropPositions() );
			if ( nDrop > 0 ) return;

			var player = Masters.Pm.GetPlayer( strPlayerCode ); //  first time will load
			player.LoadOwner();
			player.JerseyNo = dr[ "jersey" ].ToString();
			if ( player.JerseyNo == "0" ) player.JerseyNo = "";

			//player.LoadPerformances( RosterGrid.AllGames, true ); //  current season only

			//if ( RosterGrid.reportType == "New" )
			//{
			//   if ( RosterGrid.ShowEp() ) player.CalculateEp();
			//}
			//else
			//   //  Get the EP from the EPMaster
			//   player.ExperiencePoints = Masters.Epm.GetEP( player.PlayerCode );

			//if ( player.totStats != null)
			//	RosterLib.Utility.Announce( string.Format( "NFLTeam.AddPlayer EP {0}", player.totStats.Stat1( player.PlayerCat, false ) ) );

			//  Fill a slot with the player if he qualifys
			FillSpot( player );

			list.Add( player );

			if ( PlayerList == null ) PlayerList = new ArrayList();
			PlayerList.Add( player );
			//Announce( string.Format( "   NFLTeam.AddPlayer:finished adding {0} {1}  ", strPlayerName, strPlayerPos ) );
		}

#endregion Player loads

#region Player Experience

		/// <summary>
		/// Distributes the experience points to each member of a particular unit.
		/// for a particular game.
		///
		/// </summary>
		/// <param name="gameCode">The game code</param>///
		/// <param name="unitCode">The unit..</param>
		/// <param name="expPoints">The exp points.</param>
		public void DistributeExperience( string gameCode, string unitCode, decimal expPoints )
		{
			ArrayList unit = new ArrayList();

			if ( unitCode == "PO" ) unit = PassingUnit;

			foreach ( NFLPlayer p in unit )
				p.ExperiencePoints += expPoints;
		}

#endregion Player Experience

#region Power Ratings

		public decimal GetPowerRating( string week )
		{
			if ( _hillenMaster == null ) _hillenMaster = new HillenMaster( "Hillen", "Hillen.xml" );
			var theKey = string.Format( "{0}:{1}:{2}", Season, week, TeamCode );
			var teamsPowerRating = _hillenMaster.GetStat( theKey );

			if ( teamsPowerRating == 0 )
				Announce( string.Format( "Unable to get Power Rating for {0} for week {1}",
				TeamCode, week ) );

			return teamsPowerRating;
		}

		private decimal AdjustForResult( decimal teamsPowerRating, decimal opponentsPowerRating,
		   NFLGame game, string teamCode )
		{
			//Utility.Announce( string.Format( "Adjusting initial rating of {0} for {2} based on the game {1}",
			//   teamsPowerRating, game.GameCodeOut(), TeamCode ) );

			//  calculate difference to prediction
			var predictor = new HillinPredictor();
			var predictedResult = predictor.PredictGame( game, null, game.GameDate );
			var predictedMarginForTeam = predictedResult.MarginForTeam( TeamCode );
			//Utility.Announce( string.Format( "Predicted Margin for {0} is {1}", TeamCode, predictedMarginForTeam ) );
			var actualMarginForTeam = game.Result.MarginForTeam( TeamCode );
			//Utility.Announce( string.Format( "   Result of {2} means Actual Margin for {0} is {1}",
			//   TeamCode, actualMarginForTeam, game.Result.LogResult() ) );
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

#endregion Power Ratings

#region Spots

		private void FillSpot( NFLPlayer plyr )
		{
			//  starters only
#if DEBUG
			Utility.Announce( "NFLTeam.FillSpot: for " + plyr.PlayerName );
			if ( plyr.PlayerCode.Equals( "GOREFR01" ) )
				DumpSpots();
#endif
			if ( plyr.PlayerRole == "S" )
			{
				//  Q: Why these other spots? Ans: there are two spots for these
				if ( IsVacant( plyr.PlayerPos, _spotList ) ||
					( plyr.PlayerPos.IndexOf( "WR" ) > -1 ) ||
					( plyr.PlayerPos == "DE" ) || ( plyr.PlayerPos == "ILB" ) ||
					( plyr.PlayerPos == "RT" ) || ( plyr.PlayerPos == "LT" ) ||
					( plyr.PlayerPos == "OLB" ) )
				{
					var spot = new Spot( ConvertSpot( plyr.PlayerPos, _spotList ), plyr );
#if DEBUG
					Utility.Announce( string.Format( "NFLTeam.FillSpot {0} with {1}", spot.SpotName, plyr.PlayerName ) );
#endif
					if ( _spotList == null ) _spotList = new ArrayList();
					_spotList.Add( spot );
				}
			}
		}

		private static string ConvertSpot( string aSpot, ArrayList spotList )
		{
			var convSpot = aSpot;
			convSpot = ConvertCb( aSpot, spotList, convSpot );
			convSpot = ConvertDt( aSpot, spotList, convSpot );
			if ( aSpot.IndexOf( "OE" ) > -1 ) convSpot = "RDE";
			if ( aSpot.IndexOf( "SLB" ) > -1 ) convSpot = "RLB";
			if ( aSpot.IndexOf( "WLB" ) > -1 ) convSpot = "LLB";
			if ( aSpot.IndexOf( "RCB" ) > -1 ) convSpot = "RCB";
			if ( aSpot.IndexOf( "LCB" ) > -1 ) convSpot = "LCB";
			if ( aSpot.IndexOf( "FS" ) > -1 ) convSpot = "FS";
			if ( aSpot.IndexOf( "SS" ) > -1 ) convSpot = "SS";
			if ( aSpot.IndexOf( "WR" ) > -1 ) convSpot = "WR";
			if ( aSpot.IndexOf( "FL" ) > -1 ) convSpot = "WR";
			if ( aSpot.IndexOf( "SE" ) > -1 ) convSpot = "WR";
			if ( aSpot.IndexOf( "HB" ) > -1 ) convSpot = "RB";
			if ( aSpot.IndexOf( "RB" ) > -1 ) convSpot = "RB";
			return convSpot;
		}

		private static string ConvertDt( string aSpot, ArrayList spotList, string convSpot )
		{
			if ( aSpot.Equals( "DT" ) )
			{
				//  if NT is vacant use that spot
				if ( IsVacant( "NT", spotList ) )
					convSpot = "NT";
			}
			return convSpot;
		}

		private static string ConvertCb( string aSpot, ArrayList spotList, string convSpot )
		{
			if ( aSpot.Equals( "CB" ) )
			{
				//  the spot he takes could be LCB or RCB
				foreach ( Spot spot in spotList )
				{
					if ( spot.SpotName.Equals( "LCB" ) )
					{
						convSpot = "RCB";
						break;
					}
					if ( spot.SpotName.Equals( "RCB" ) )
					{
						convSpot = "LCB";
						break;
					}
				}
			}
			return convSpot;
		}

		/// <summary>
		///   Do we have a vacancy for a pos
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="spotList"></param>///
		/// <returns></returns>
		private static bool IsVacant( string pos, ArrayList spotList )
		{
			bool isVacant = true;
			pos = ConvertSpot( pos, spotList );

			if ( spotList != null )
			{
				//  is the spot is a generic one like CB, all slots must be taken to stop it being added to the list
				if ( pos.Equals( "CB" ) )
				{
					//  count corner backs
					int nCorners = 0;
					//  if count is 2 then we are full
					foreach ( Spot spot in spotList )
					{
						if ( spot.SpotName.Equals( "LCB" ) || spot.SpotName.Equals( "RCB" ) )
							nCorners++;
						if ( nCorners > 1 ) break;
					}
					if ( nCorners > 1 ) isVacant = false;
				}
				else
				{
					foreach ( Spot spot in spotList )
					{
						if ( spot.SpotName.Equals( pos ) )
						{
							//  we already got one
							isVacant = false;
							break;
						}
					}
				}
			}
			return isVacant;
		}

#endregion Spots

#region IComparable Members

		public int CompareTo( object obj )
		{
			// 1. Cast the obj int a team
			var team = ( NflTeam ) obj;
			// 2. Use the inherited behaviour of a decimal
			return ( team._clip.CompareTo( _clip ) );
		}

#endregion IComparable Members

#region HelperClass  Spot

		public class Spot
		{
			public Spot( string spotNameIn, NFLPlayer playerIn )
			{
				//  constructor
				SpotName = spotNameIn.Trim();
				Player = playerIn;
				IsDef = !Player.IsOffence();
			}

			public string SpotName { get; set; }

			public NFLPlayer Player { get; set; }

			public bool IsDef { get; set; }

			public bool IsUsed { get; set; }

			public bool CanFit( string pos )
			{
				if ( Player != null )
					return false; //  spot taken

				if ( SpotName.Equals( pos ) )
					return true;

				if ( pos.Equals( "CB" ) && ( SpotName.Equals( "LCB" ) || SpotName.Equals( "RCB" ) ) )
					return true;

				return false;
			}
		}

#endregion HelperClass  Spot

		public void TallyStats(IBreakdown breakdowns)
		{
			foreach ( NFLGame game in GameList )
			{
				TraceIt( $"Tallying stats for {game}" );

				game.MetricsCalculated = false;  //  force the calcs
				game.TallyMetrics( metric: string.Empty, breakdowns: breakdowns );
				game.TallyStatsFor( this );
			}
		}

		public bool WinningRecord()
		{
			if ( Clip() > .499M )
				return true;
			else
				return false;
		}

		public bool Above500()
		{
			if ( Clip() > 0.5M )
				return true;
			else
				return false;
		}

		public void Announce( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Trace( "   " + message );
		}

		public void TraceIt( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Trace( "   " + message );
		}
	}
}