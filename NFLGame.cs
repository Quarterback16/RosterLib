using NLog;
using RosterLib.Interfaces;
using RosterLib.Models;
using RosterLib.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using TFLLib;

namespace RosterLib
{
	/// <summary>
	/// Class representing an NFL Game.
	/// </summary>
	///

	public class NFLGame
	{
		#region Privates

		//private int m_weekNo;

		private string _myTip;

		//private int m_TotalPoints = 0;

		private decimal _homeSaKa;
		private decimal _awaySaKa;

		public NFLWeek GameWeek { get; set; }

		#endregion Privates

		private const int KLastAfternoonTimeslot = 6;

		public string Id { get; set; }

		public Logger Logger { get; set; }

		public NflTeam HomeNflTeam;
		public NflTeam AwayNflTeam;

		public List<PlayerGameMetrics> Pgms { get; set; }

		public List<NflStat> StatList { get; set; }

		public List<YahooOutput> YahooList { get; set; }

		public List<GridStatsOutput> GridStatsList { get; set; }

		public List<NFLPlayer> HomePlayers { get; set; }

		public List<NFLPlayer> AwayPlayers { get; set; }

		public Lineup HomeLineup { get; set; }

		public Lineup AwayLineup { get; set; }

		public NFLPlayer AwayQb1 { get; set; }
		public NFLPlayer HomeQb1 { get; set; }

		public NFLPlayer AwayRb1 { get; set; }
		public NFLPlayer HomeRb1 { get; set; }

		public bool IsRegularSeasonGame()
		{
			return GameType().Equals("regular");
		}

		public NFLResult BookieTip { get; set; }

		public List<NFLPlayer> FantasyPlayers { get; set; }

		public IGetPredictions PredictionGetter { get; set; }

		public List<PlayerGameMetrics> PlayerGameMetrics { get; set; }

		public IYahooStatService YahooStatsService { get; set; }

		private IReason Checker { get; set; }


		public enum Stat
		{
			Sack = 0
		}

		//  EP metrics

		public IPlayerGameMetricsDao PgmDao { get; set; }

		#region Constructors

		public NFLGame()
		{
			Season = "XXXX";
			Week = "XX";
			GameCode = "X";
		}

		internal List<NFLPlayer> LoadAllFantasyHomePlayers()
		{
			return LoadAllFantasyHomePlayers( GameDate, string.Empty );
		}

		internal List<NFLPlayer> LoadAllFantasyAwayPlayers()
		{
			return LoadAllFantasyAwayPlayers( GameDate, string.Empty );
		}

		public NFLGame( string weekIn, DateTime dateIn, string homeCodeIn, string awayCodeIn,
						int homeScoreIn, int awayScoreIn, decimal spreadIn, decimal totalIn, string season, string gameCode )
		{
			// constructor
			//RosterLib.Utility.Announce( string.Format( "  Constructing game in week {0} {1} @ {2}", weekIn, awayCodeIn, homeCodeIn ) );
			YahooList = new List<YahooOutput>();
			GridStatsList = new List<GridStatsOutput>();
			Season = season;
			HomeScore = homeScoreIn;
			AwayScore = awayScoreIn;
			Spread = spreadIn;
			Total = totalIn;
			Week = weekIn;
			GameDate = dateIn;
			GameCode = gameCode;
			HomeTeam = homeCodeIn;
			AwayTeam = awayCodeIn;

			Result = new NFLResult( homeCodeIn, homeScoreIn, awayCodeIn, awayScoreIn );
		}

		public NFLGame( string season, string weekIn )
		{
			Season = season;
			Week = weekIn;
			YahooList = new List<YahooOutput>();
			GridStatsList = new List<GridStatsOutput>();
		}

		/// <summary>
		///   Game key is YYYY:0W-X
		/// </summary>
		/// <param name="gameKey"></param>
		public NFLGame( string gameKey )
		{
			if ( gameKey.Equals( "BYE" ) )
			{
				Season = "XXXX";
				Week = "XX";
				GameCode = "X";
			}
			else
			{
				YahooList = new List<YahooOutput>();
				GridStatsList = new List<GridStatsOutput>();
				if ( gameKey.Length < 4 )
					throw new Exception( "Invalid Game Key :" + gameKey );
				Season = gameKey.Substring( 0, 4 );
				Week = gameKey.Substring( 5, 2 );
				GameCode = gameKey.Substring( 8, 1 );
				var ds = Utility.TflWs.GameFor( Season, Week, GameCode );
				var dr = ds.Tables[ 0 ].Rows[ 0 ];
				LoadGameFromDr( dr );
				if ( Played() )
					Result = new NFLResult( HomeTeam, HomeScore, AwayTeam, AwayScore );
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NFLGame"/> class.
		///   Experience points are calculated values.
		/// </summary>
		/// <param name="gameNode">The game node in xml.</param>
		public NFLGame( XmlNode gameNode )
		{
			YahooList = new List<YahooOutput>();
			GridStatsList = new List<GridStatsOutput>();
			foreach ( XmlNode n in gameNode.ChildNodes )
			{
				switch ( n.Name )
				{
					case "season":
						Season = n.InnerText;
						break;

					case "week":
						Week = n.InnerText;
						break;

					case "gamecode":
						GameCode = n.InnerText;
						break;

					case "date":
						GameDate = Convert.ToDateTime( n.InnerText );
						break;

					case "hometeamcode":
						HomeTeam = n.InnerText;
						break;

					case "awayteamcode":
						AwayTeam = n.InnerText;
						break;

					case "home-metrics":
						ProcessMetrics( "home", n );
						break;

					case "away-metrics":
						ProcessMetrics( "away", n );
						break;
				}
			}
			MetricsCalculated = true;
		}

		public NFLGame( DataRow dr )
		{
			LoadGameFromDr( dr );
			YahooList = new List<YahooOutput>();
			GridStatsList = new List<GridStatsOutput>();
		}

		private void LoadGameFromDr( DataRow dr )
		{
			if ( dr == null ) return;

			Id = dr[ "id" ].ToString();
			Season = dr[ "SEASON" ].ToString();
			Week = dr[ "WEEK" ].ToString();
			Spread = Decimal.Parse( dr[ "SPREAD" ].ToString() );
			Total = Decimal.Parse( dr[ "TOTAL" ].ToString() );
			GameDate = DateTime.Parse( dr[ "GAMEDATE" ].ToString() );
			GameCode = dr[ "GAMENO" ].ToString();
			HomeTeam = dr[ "HOMETEAM" ].ToString();
			AwayTeam = dr[ "AWAYTEAM" ].ToString();
			HomeScore = Int32.Parse( dr[ "HOMESCORE" ].ToString() );
			AwayScore = Int32.Parse( dr[ "AWAYSCORE" ].ToString() );
			Hour = dr[ "GAMEHOUR" ].ToString();
			IsOnTv = Boolean.Parse( dr[ "GAMELIVE" ].ToString() );
			MyTip = dr[ "MYTIP" ].ToString();

			ProjectedAwayFg = Int32.Parse( dr[ "AWAY_FG" ].ToString() );
			ProjectedAwayTdp = Int32.Parse( dr[ "AWAY_TDP" ].ToString() );
			ProjectedAwayTdr = Int32.Parse( dr[ "AWAY_TDR" ].ToString() );
			ProjectedHomeFg = Int32.Parse( dr[ "HOME_FG" ].ToString() );
			ProjectedHomeTdp = Int32.Parse( dr[ "HOME_TDP" ].ToString() );
			ProjectedHomeTdr = Int32.Parse( dr[ "HOME_TDR" ].ToString() );

			AwayFg = Int32.Parse( dr[ "AWAY_FG" ].ToString() );
			AwayTDp = Int32.Parse( dr[ "AWAY_TDP" ].ToString() );
			AwayTDr = Int32.Parse( dr[ "AWAY_TDR" ].ToString() );
			AwayTDd = Int32.Parse( dr[ "AWAY_TDD" ].ToString() );
			AwayTDd = Int32.Parse( dr[ "AWAY_TDS" ].ToString() );

			HomeFg = Int32.Parse( dr[ "HOME_FG" ].ToString() );
			HomeTDp = Int32.Parse( dr[ "HOME_TDP" ].ToString() );
			HomeTDr = Int32.Parse( dr[ "HOME_TDR" ].ToString() );
			HomeTDd = Int32.Parse( dr[ "HOME_TDD" ].ToString() );
			HomeTDs = Int32.Parse( dr[ "HOME_TDS" ].ToString() );

			HomeTeamName = Masters.Tm.TeamFor( HomeTeam, Season );
			AwayTeamName = Masters.Tm.TeamFor( AwayTeam, Season );

			GetHomeTeam();
			GetAwayTeam();

			Result = new NFLResult( HomeTeam, HomeScore, AwayTeam, AwayScore );
		}

		public decimal MyLine()
		{
			decimal margin = 0.0M;
			if ( _myTip != null )
			{
				if ( _myTip.Trim().Length > 0 )
				{
					//  convert A4 to -4, H8 to 8
					var prediction = MyTip.Substring( 0, 1 );
					margin = Decimal.Parse( MyTip.Substring( 1, MyTip.Length - 1 ) );
					if ( prediction.Equals( "A" ) ) margin *= -1.0M;
				}
			}
			return margin;
		}

		public NflTeam GetHomeTeam()
		{
			HomeNflTeam = Masters.Tm.GetTeam( Season, HomeTeam );
			HomeNflTeam.SetRecord( Season, skipPostseason: false );
			return HomeNflTeam;
		}

		public NflTeam GetAwayTeam()
		{
			AwayNflTeam = Masters.Tm.GetTeam( Season, AwayTeam );
			AwayNflTeam.SetRecord( Season, skipPostseason: false );
			return AwayNflTeam;
		}

		#endregion Constructors


		internal string DumpPgmsAsHtml( string header, string teamCodeInFocus )
		{
			var sb = new StringBuilder();
			sb.Append( HtmlLib.H3( header ) );
			sb.Append( HtmlLib.TableOpen( "border='0'" ) );
			sb.Append( HtmlLib.TableRowOpen() );
			sb.Append( HtmlLib.TableData( DumpOffenceHtml( teamCodeInFocus ) ) );
			sb.Append( HtmlLib.TableData( DumpDefenceHtml( teamCodeInFocus ) ) );
			sb.Append( HtmlLib.TableRowClose() );
			sb.Append( HtmlLib.TableClose() );
			return sb.ToString();
		}

		internal string DumpFantasyPlayersAsHtml( string header, string teamCodeInFocus )
		{
			var sb = new StringBuilder();
			sb.Append( HtmlLib.H3( header ) );
			sb.Append( HtmlLib.TableOpen( "border='0'" ) );
			sb.Append( HtmlLib.TableRowOpen() );
			sb.Append( HtmlLib.TableData( DumpFantasyHtml( teamCodeInFocus ) ) );
			sb.Append( HtmlLib.TableRowClose() );
			sb.Append( HtmlLib.TableClose() );
			return sb.ToString();
		}

		private string DumpFantasyHtml( string teamCodeInFocus )
		{
			if ( YahooStatsService == null ) YahooStatsService = new YahooStatService();
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableOpen() );
			foreach ( var plyr in FantasyPlayers )
			{
				if ( plyr.CurrTeam == null ) continue;
				if ( plyr.CurrTeam.TeamCode == null ) continue;
				if ( plyr.CurrTeam.TeamCode.Equals( teamCodeInFocus ) )
				{
					var pts = YahooStatsService.GetStat( plyr.PlayerCode, Season, Week );
					sb.Append( HtmlLib.TableRowOpen() );
					sb.Append( HtmlLib.TableDataOpen() );
					sb.Append(
					   $"{plyr.PlayerName}" );
					sb.Append( HtmlLib.TableDataClose() );
					sb.Append( HtmlLib.TableDataOpen() );
					sb.Append(
					   $"{pts}" );
					sb.Append( HtmlLib.TableDataClose() );
					sb.Append( HtmlLib.TableRowClose() );
				}
			}
			sb.Append( HtmlLib.TableClose() );
			return sb.ToString();
		}

		private string DumpOffenceHtml( string teamCodeInFocus )
		{
			var pgmCount = 0;
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableOpen() );
			foreach ( var pgm in Pgms )
			{
				pgmCount++;
				if ( pgmCount == 1 )
					sb.Append( pgm.PgmHeaderVerticalRow() );

				var plyr = new NFLPlayer( pgm.PlayerId );
				pgm.ProjectedFantasyPoints = pgm.CalculateProjectedFantasyPoints( plyr );
				pgm.FantasyPoints = pgm.CalculateActualFantasyPoints( plyr );
				if ( plyr.CurrTeam.TeamCode == null ) continue;
				if ( plyr.CurrTeam.TeamCode.Equals( teamCodeInFocus ) )
					if ( plyr.IsOffence() )
					{
						sb.Append(
							$"{pgm.FormatProjectionsAsTableRow( plyr )}" );
						sb.Append(
							$"{pgm.FormatActualsAsTableRow( plyr )}" );
					}
			}
			sb.Append( HtmlLib.TableClose() );
			return sb.ToString();
		}

		private string DumpDefenceHtml( string teamCodeInFocus )
		{
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableOpen() );
			foreach ( var pgm in Pgms )
			{
				var plyr = new NFLPlayer( pgm.PlayerId );
				if ( plyr.CurrTeam.TeamCode == null ) continue;
				if ( plyr.CurrTeam.TeamCode.Equals( teamCodeInFocus ) )
					if ( plyr.IsDefence() )
						sb.Append(
						   $"{pgm.FormatAsTableRow( plyr.PlayerName, plyr.RoleOut(), 0 )}" );
			}
			sb.Append( HtmlLib.TableClose() );
			return sb.ToString();
		}

		/// <summary>
		/// Tallies the metrics for the game if it has been played.
		/// Need to know # of Tdp, Tdr, SAK
		/// Heavy 10 IO
		/// </summary>
		public void TallyMetrics( string metric, IBreakdown breakdowns = null)
		{
			if ( metric == String.Empty )
			{
				if ( !MetricsCalculated )
				{
					HomeFg = ( int ) Utility.TflWs.TeamScores( "3", Season, Week, GameCode, HomeTeam );
					AwayFg = ( int ) Utility.TflWs.TeamScores( "3", Season, Week, GameCode, AwayTeam );
					HomeTDp = ( int ) Utility.TflWs.TeamScores( "P", Season, Week, GameCode, HomeTeam );
					HomeTDr = ( int ) Utility.TflWs.TeamScores( "R", Season, Week, GameCode, HomeTeam );
					AwayTDp = ( int ) Utility.TflWs.TeamScores( "P", Season, Week, GameCode, AwayTeam );
					AwayTDr = ( int ) Utility.TflWs.TeamScores( "R", Season, Week, GameCode, AwayTeam );
					HomeTDs = ( int ) Utility.TflWs.TeamScores( Constants.K_SCORE_KICK_RETURN, Season, Week, GameCode, HomeTeam );
					HomeTDs += ( int ) Utility.TflWs.TeamScores( Constants.K_SCORE_PUNT_RETURN, Season, Week, GameCode, HomeTeam );
					AwayTDs = ( int ) Utility.TflWs.TeamScores( Constants.K_SCORE_KICK_RETURN, Season, Week, GameCode, AwayTeam );
					AwayTDs += ( int ) Utility.TflWs.TeamScores( Constants.K_SCORE_PUNT_RETURN, Season, Week, GameCode, AwayTeam );
					HomeTDd = ( int ) Utility.TflWs.TeamScores( Constants.K_SCORE_FUMBLE_RETURN, Season, Week, GameCode, HomeTeam );
					HomeTDd += ( int ) Utility.TflWs.TeamScores( Constants.K_SCORE_INTERCEPT_RETURN, Season, Week, GameCode, HomeTeam );
					AwayTDd = ( int ) Utility.TflWs.TeamScores( Constants.K_SCORE_FUMBLE_RETURN, Season, Week, GameCode, AwayTeam );
					AwayTDd += ( int ) Utility.TflWs.TeamScores( Constants.K_SCORE_INTERCEPT_RETURN, Season, Week, GameCode, AwayTeam );

					HomeSaKa = GetTeamStats( Constants.K_STATCODE_SACK, 
						Season, Week, GameCode, AwayTeam, breakdowns );
					AwaySaKa = GetTeamStats( Constants.K_STATCODE_SACK, 
						Season, Week, GameCode, HomeTeam, breakdowns );

					HomeInt = ( int ) Utility.TflWs.TeamStats( "M", Season, Week, GameCode, AwayTeam );
					AwayInt = ( int ) Utility.TflWs.TeamStats( "M", Season, Week, GameCode, HomeTeam );
					HomePasses = ( int ) Utility.TflWs.TeamStats( "A", Season, Week, GameCode, HomeTeam );
					AwayPasses = ( int ) Utility.TflWs.TeamStats( "A", Season, Week, GameCode, AwayTeam );
					HomeRuns = ( int ) Utility.TflWs.TeamStats( "R", Season, Week, GameCode, HomeTeam );
					AwayRuns = ( int ) Utility.TflWs.TeamStats( "R", Season, Week, GameCode, AwayTeam );
					HomeYDr = ( int ) Utility.TflWs.TeamStats( "Y", Season, Week, GameCode, HomeTeam );
					AwayYDr = ( int ) Utility.TflWs.TeamStats( "Y", Season, Week, GameCode, AwayTeam );
					HomeYDp = ( int ) Utility.TflWs.TeamStats( "S", Season, Week, GameCode, HomeTeam );
					AwayYDp = ( int ) Utility.TflWs.TeamStats( "S", Season, Week, GameCode, AwayTeam );
					MetricsCalculated = true;
				}
#if DEBUG
				//else
				//   Utility.Announce(string.Format("Metrics already tallied for game {0}", GameName()));
#endif
			}
			else
			{
				switch ( metric )
				{
					case "Y":
						HomeYDr = ( int ) Utility.TflWs.TeamStats( "Y", Season, Week, GameCode, HomeTeam );
						AwayYDr = ( int ) Utility.TflWs.TeamStats( "Y", Season, Week, GameCode, AwayTeam );
						break;

					case "S":
						HomeYDp = ( int ) Utility.TflWs.TeamStats( "S", Season, Week, GameCode, HomeTeam );
						AwayYDp = ( int ) Utility.TflWs.TeamStats( "S", Season, Week, GameCode, AwayTeam );
						break;
				}
			}
		}

		public decimal GetTeamStats( 
			string statCode, 
			string season, 
			string week, 
			string gameCode, 
			string teamCode,
			IBreakdown breakdowns = null)
		{
			var stats = Utility.TflWs.TeamStats( statCode, season, week, gameCode, teamCode );
			if ( breakdowns != null )
				breakdowns.AddLine( $"{teamCode}-{statCode}", $"{week} {statCode} {stats}");
			ReasonablenessCheck( teamCode, season, week, statCode, stats );  //  for a single game
			return stats;
		}

		private void ReasonablenessCheck( 
			string teamCode, string season, string week, string statCode, decimal stats )
		{
			if ( Checker == null ) Checker = new ReasonablenessChecker();
			if ( Checker.IsNotReasonable( statCode, stats ) )
				LogInfo( $"!! {season}:{week} {teamCode} value of {stats} for {statCode} is out of range" );
		}

		/// <summary>
		/// Processes the metrics by picking out the attributes.
		/// </summary>
		/// <param name="homeOrAway">The home or away.</param>
		/// <param name="n">The metrics node.</param>
		private void ProcessMetrics( string homeOrAway, XmlNode n )
		{
			if ( Equals( homeOrAway, "home" ) )
			{
				if ( n.Attributes != null )
				{
					HomeScore = Int32.Parse( n.Attributes[ "score" ].Value );
					HomeTDp = Int32.Parse( n.Attributes[ "TDp" ].Value );
					HomeTDr = Int32.Parse( n.Attributes[ "TDr" ].Value );
					HomeSaKa = Decimal.Parse( n.Attributes[ "SAKa" ].Value );
				}
			}
			else
			{
				if ( n.Attributes != null )
				{
					AwayScore = Int32.Parse( n.Attributes[ "score" ].Value );
					AwayTDp = Int32.Parse( n.Attributes[ "TDp" ].Value );
					AwayTDr = Int32.Parse( n.Attributes[ "TDr" ].Value );
					AwaySaKa = Decimal.Parse( n.Attributes[ "SAKa" ].Value );
				}
			}
		}

		public string Index()
		{
			var index = ( int ) GameDate.DayOfWeek;
			if ( index < 2 )
				index += 7;
			return string.Format( "{0}{1}{2}", index, Hour, HomeNflTeam.ApCode );
		}

		public bool IsPlayoff()
		{
			return ( WeekNo > 17 );
		}

		//  game is in the last two seasons
		public bool IsRecent()
		{
			var now = Int32.Parse( Utility.CurrentSeason() );
			var offset = now - Int32.Parse( Season );
			return !( offset > 2 );
		}

		public bool IsHome( string teamIn )
		{
			return teamIn == HomeTeam;
		}

		public bool IsAway( string teamCodeIn )
		{
			return teamCodeIn == AwayTeam;
		}

		public bool HomeDog()
		{
			return Int32.Parse( Week ) != Constants.K_WEEKS_IN_A_SEASON && ( Dog() == HomeTeam );
		}

		/// <summary>
		///   A game is a revenge situation if either team wants revenge
		/// </summary>
		/// <returns></returns>
		public bool IsRevengeSituation()
		{
			return DanGordan.WantsRevenge( HomeNflTeam, AwayNflTeam, GameDate ) ||
				   DanGordan.WantsRevenge( AwayNflTeam, HomeNflTeam, GameDate );
		}

		public bool IsRevengeSituationFor( NflTeam t )
		{
			return
			   IsHome( t.TeamCode )
				  ? DanGordan.WantsRevenge( HomeNflTeam, AwayNflTeam, GameDate )
				  : DanGordan.WantsRevenge( AwayNflTeam, HomeNflTeam, GameDate );
		}

		public bool Played(bool addDay = true)
		{
            if ( HomeScore + AwayScore < 3) return false;
			if ( addDay )
			{
				return ( GameDate.AddDays( 1 ).Date < DateTime.Now.Date ) && ( HomeScore + AwayScore > 0 );
			}
			else
				return ( GameDate.Date < DateTime.Now.Date ) && ( HomeScore + AwayScore > 0 );
		}

		public bool WasRout()
		{
			return MarginOfVictory() >= 21;
		}

		public bool WasPhonyWin()
		{
			var isPhony = false;
			decimal winnersAvgGainPerpass, losersAvgGainPerPass;
			if ( WinningTeam().Equals( HomeTeam ) )
			{
				winnersAvgGainPerpass = HomeAverageGainPerPass();
				losersAvgGainPerPass = AwayAverageGainPerPass();
			}
			else
			{
				winnersAvgGainPerpass = AwayAverageGainPerPass();
				losersAvgGainPerPass = HomeAverageGainPerPass();
			}
			if ( losersAvgGainPerPass > winnersAvgGainPerpass )
				isPhony = true;
			//TODO: add turnover criteria and Sacks
			return isPhony;
		}

		public decimal HomeAverageGainPerPass()
		{
			var avgGain = 0.0M;
			if ( HomePasses > 0 )
				avgGain = ( ( decimal ) HomeYDp / HomePasses );
			return avgGain;
		}

		public decimal AwayAverageGainPerPass()
		{
			var avgGain = 0.0M;
			if ( AwayPasses > 0 )
				avgGain = ( ( decimal ) AwayYDp / AwayPasses );
			return avgGain;
		}

		public string Dog()
		{
			return Spread == 0.0M ? "??" : SpreadDog();
		}

		public bool TippedByMe()
		{
			var tippedIt = false;

			if ( HomeWin() )
			{
				if ( MyTip.Substring( 0, 1 ).Equals( "H" ) )
					tippedIt = true;
			}
			else
			{
				if ( AwayWin() )
				{
					if ( MyTip.Substring( 0, 1 ).Equals( "A" ) )
						tippedIt = true;
				}
			}
			return tippedIt;
		}

		public bool MeAts()
		{
			//  my team to wager on
			//int mySpread = Int32.Parse( MyTip.Substring( 1, 2 ) );
			//if ( MyTip.Substring(0, 1).Equals( "A" ) )
			//   mySpread = 0 - mySpread;
			//decimal diff = mySpread - Spread;
			//string myTeam;
			//if ( diff < 0.0M )
			//   myTeam = AwayTeam;
			//else
			//   myTeam = HomeTeam;

			return false;
		}

		public bool Upset()
		{
			return !SpreadFavourite().Equals( WinningTeamCode() );
		}

		public bool IsDog( NflTeam t )
		{
			return IsHome( t.TeamCode ) ? ( Spread < 0 ) : ( !( Spread < 0 ) );
		}

		public bool IsMondayNight()
		{
			return GameDate.DayOfWeek.Equals( DayOfWeek.Monday );
		}

		public string GameDay()
		{
			return GameDate.DayOfWeek.ToString().Substring( 0, 2 );
		}

		public bool IsSundayNight()
		{
			return ( GameDate.DayOfWeek.Equals( DayOfWeek.Sunday ) && Int32.Parse( Hour ) > KLastAfternoonTimeslot );
		}

		public bool IsFavourite( NflTeam t )
		{
			return !IsDog( t );
		}

		public string SpreadDog()
		{
			if ( Spread == 0 )
				return "  ";
			return Decimal.Compare( Spread, 0 ) > 0 ? AwayTeam : HomeTeam;
		}

		public bool Involves( string teamCode )
		{
			return HomeTeam.Equals( teamCode ) || AwayTeam.Equals( teamCode );
		}

		public string SpreadFavourite()
		{
			if ( Spread == 0 )
				return "  ";
			return Decimal.Compare( Spread, 0 ) > 0 ? HomeTeam : AwayTeam;
		}

		public string Favourite()
		{
			return MarginOfVictory() > 0 ? HomeTeam : AwayTeam;
		}

		public NflTeam FavouriteTeam()
		{
			return MarginOfVictory() > 0 ? HomeNflTeam : AwayNflTeam;
		}

		public NflTeam DogTeam()
		{
			return MarginOfVictory() > 0 ? AwayNflTeam : HomeNflTeam;
		}

		public string Opponent( string teamIn )
		{
			return teamIn == HomeTeam ? AwayTeam : HomeTeam;
		}

		public NflTeam OpponentTeam( string teamIn )
		{
			if ( AwayNflTeam == null ) AwayNflTeam = GetAwayTeam();
			if ( HomeNflTeam == null ) HomeNflTeam = GetHomeTeam();

			return teamIn == HomeTeam ? AwayNflTeam : HomeNflTeam;
		}

		public NflTeam Team( string teamIn )
		{
			if ( AwayNflTeam == null ) AwayNflTeam = GetAwayTeam();
			if ( HomeNflTeam == null ) HomeNflTeam = GetHomeTeam();

			return teamIn == HomeTeam ? HomeNflTeam : AwayNflTeam;
		}

		public bool IsDivisionalGame()
		{
			//RosterLib.Utility.Announce( "IsDivisionalGame:");
			return HomeNflTeam.IsDivisionalOpponent( AwayNflTeam.TeamCode );
		}

		public int MarginOfVictory()
		{
			return Math.Abs( HomeScore - AwayScore );
		}

		public int MarginFor( NflTeam t )
		{
			var margin = MarginOfVictory();

			if ( Lost( t ) ) margin *= -1;

			return margin;
		}

		public decimal ExpMarginFor( NflTeam t )
		{
			return IsHome( t.TeamCode ) ? Spread : Spread * -1;
		}

		public string WinningTeam()
		{
			return HomeWin() ? HomeTeamName : AwayTeamName;
		}

		public string LosingTeam()
		{
			if ( HomeWin() )
				return AwayTeamName;
			return HomeTeamName;
		}

		public string WinningTeamCode()
		{
			return HomeWin() ? HomeTeam : AwayTeam;
		}

		public string GameType()
		{
			var gameType = "regular";

			if ( WeekNo > 17 )
				gameType = "playoff";

			if ( WeekNo == 21 )
				gameType = "superbowl";

			return gameType;
		}

		public bool HomeWin()
		{
			return ( HomeScore > AwayScore );
		}

		public bool AwayWin()
		{
			return ( HomeScore < AwayScore );
		}

		public bool Won( NflTeam t )
		{
			return ( WinningTeamCode().Equals( t.TeamCode ) );
		}

		public int AgainstFor( string teamCode )
		{
			return teamCode.Equals( HomeTeam ) ? AwayScore : HomeScore;
		}

		public int ScoreFor( string teamCode )
		{
			return teamCode.Equals( HomeTeam ) ? HomeScore : AwayScore;
		}

		public int ScoreFor( NflTeam team )
		{
			return team.TeamCode.Equals( HomeTeam ) ? HomeScore : AwayScore;
		}

		public bool WonVsSpread( NflTeam t )
		{
			var wonIt = IsHome( t.TeamCode )
					   ? ( HomeScore - Spread - AwayScore ) > 0.0M
					   : ( AwayScore + Spread - HomeScore ) > 0.0M;

			return wonIt;
		}

		public bool Lost( NflTeam t )
		{
			return !Won( t );
		}

		public bool Tie()
		{
			return ( HomeScore == AwayScore );
		}

		public string ScoreOut( string teamInFocus )
		{
			return teamInFocus == HomeTeam ? $"{HomeScore:#0}-{AwayScore:#0}" : String.Format( "{0:#0}-{1:#0}", AwayScore, HomeScore );
		}

		public string ProjectedScoreOut( string teamInFocus )
		{
			return teamInFocus == HomeTeam ? $"{ProjectedHomeScore:#0}-{ProjectedAwayScore:#0}" : String.Format( "{0:#0}-{1:#0}", ProjectedAwayScore, ProjectedHomeScore );
		}

		public string GameCodeOut()
		{
			//  used by NFLGambler
			return $"{Season}:{WeekOut()}";
		}

		public string WeekCodeOut()
		{
			return $"{Season}:{Int32.Parse( Week ):0#}";
		}

		public string GameKey()
		{
			//  used by NFLGambler
			return $"{Season}:{Week}-{GameCode}";
		}

		public string GameRow( string teamInFocus )
		{
			string row = HtmlLib.TableRowOpen( " BGCOLOR=" + SetColour( "LIME", teamInFocus ) ) + "\n";
			row += HtmlLib.TableData( WeekOut() ) + "\n";
			row += HtmlLib.TableData( GameDate.ToString( "dd MMM yy" ) ) + "\n";
			row += HtmlLib.TableData( ResultOut( teamInFocus, false ) ) + "\n";
			row += HtmlLib.TableData( TeamOut( AwayTeam, teamInFocus ) ) + "\n";
			row += HtmlLib.TableData( TeamOut( HomeTeam, teamInFocus ) ) + "\n";
			row += HtmlLib.TableData( AwayScore + "-" + HomeScore ) + "\n";
			row += HtmlLib.TableRowClose() + "\n";
			return row;
		}

		public string GameName( bool linkToDepthChart = false)
		{
			if (linkToDepthChart)
				return $"{Season}:{Week} {DepthChartLink(AwayTeam)} @ {DepthChartLink(HomeTeam)}";
			return $"{Season}:{Week} {AwayTeam} @ {HomeTeam}";
		}

		public string DepthChartLink( string teamCode )
		{
			return $"<a href='..\\..\\..\\DepthCharts\\{teamCode}.htm'>{teamCode}</a>";
		}

		public override string ToString()
		{
			return GameName();
		}

		public bool WentIntoOvertime()
		{
			DataSet ds = Utility.TflWs.GetOvertimeScoresFor( Season, Week, GameCode );
			bool isOvertime = ds.Tables[ 0 ].Rows.Count > 0;
			return isOvertime;
		}

		public string ScoreOut()
		{
			return $"{Season}:{Week} {AwayTeam} {AwayScore} @ {HomeTeam} {HomeScore}";
		}

		public string WordyScoreOut()
		{
			return $"{Season}:{Week} {AwayNflTeam.NameOut()} {AwayScore} @ {HomeNflTeam.NameOut()} {HomeScore}";
		}

		private string SetColour( string tieColour, string teamInFocus )
		{
			var theColour = tieColour;
			switch ( ResultOut( teamInFocus, false ) )
			{
				case "Won":
					theColour = "LIME";
					break;

				case "Lost":
					theColour = "DARKPINK";
					break;
			}
			return theColour;
		}

		public string ResultRow()
		{
#if DEBUG
			Utility.Announce( "ResultRow: Winner is " + WinnerOut( AwayTeamName ) );
#endif
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableRowOpen() );
			sb.Append( HtmlLib.TableData( Season ) );
			sb.Append( HtmlLib.TableData( Week ) );
			sb.Append( HtmlLib.TableData( WinnerOut( AwayTeamName ) ) );
			sb.Append( HtmlLib.TableData( AwayScore.ToString( CultureInfo.InvariantCulture ) ) );
			sb.Append( HtmlLib.TableData( WinnerOut( HomeTeamName ) ) );
			sb.Append( HtmlLib.TableData( HomeScore.ToString( CultureInfo.InvariantCulture ) ) );
			sb.Append( HtmlLib.TableData( Spread.ToString( CultureInfo.InvariantCulture ) ) );
			sb.Append( HtmlLib.TableData( Total.ToString( CultureInfo.InvariantCulture ) ) );
			sb.Append( HtmlLib.TableRowClose() );
			return sb.ToString();
		}

		private string WinnerOut( string team )
		{
			return team == WinningTeam() ? HtmlLib.Bold( team ) : team;
		}

		public string ResultOut( string teamInFocus, bool abbreviate )
		{
			var theResult = "tied";
			if ( teamInFocus == HomeTeam )
			{
				if ( HomeScore > AwayScore ) theResult = abbreviate ? "W" : "Won ";
				else if ( HomeScore < AwayScore ) theResult = abbreviate ? "L" : "Lost";
				else if ( AwayScore > HomeScore )
					theResult = abbreviate ? "W" : "Won";
				else if ( AwayScore < HomeScore ) theResult = abbreviate ? "L" : "Lost";
			}
			else
			{
				if ( AwayScore > HomeScore ) theResult = abbreviate ? "W" : "Won ";
				else if ( AwayScore < HomeScore ) theResult = abbreviate ? "L" : "Lost";
				else if ( HomeScore > AwayScore )
					theResult = abbreviate ? "W" : "Won";
				else if ( HomeScore < AwayScore ) theResult = abbreviate ? "L" : "Lost";
			}
			return theResult;
		}

		public string ResultFor( string teamInFocus, bool abbreviate, bool barIt = true )
		{
			string dividerChar;
			if ( barIt )
				dividerChar = "|";
			else
				dividerChar = "";

			var theResult = "     BYE     ";

			if ( IsBye() ) return string.Format( "{1}{0}", theResult, dividerChar );
			if ( teamInFocus == HomeTeam )
			{
				if ( HomeScore > AwayScore )
				{
					theResult = abbreviate ? "W" : "Won ";
					theResult = $"{theResult} v {AwayTeam} {HomeScore,2}-{AwayScore,2} ";
				}
				else if ( HomeScore < AwayScore )
				{
					theResult = abbreviate ? "L" : "Lost";
					theResult = $"{theResult} v {AwayTeam} {HomeScore,2}-{AwayScore,2} ";
				}
				else if ( AwayScore > HomeScore )
				{
					theResult = abbreviate ? "W" : "Won";
					theResult = $"{theResult} v {AwayTeam} {HomeScore,2}-{AwayScore,2} ";
				}
				else if ( AwayScore < HomeScore )
				{
					theResult = abbreviate ? "L" : "Lost";
					theResult = $"{theResult} v {AwayTeam} {HomeScore,2}-{AwayScore,2} ";
				}
				else if ( AwayScore == 0 && HomeScore == 0 )
				{
					if ( barIt )
					{
						theResult = $"   {HomeTeam} v {AwayTeam}   ";
					}
					else
					{
						theResult = $"{HomeTeam} v {AwayTeam}";
					}
				}
			}
			else
			{
				if ( AwayScore > HomeScore )
				{
					theResult = abbreviate ? "W" : "Won ";
					theResult = $"{theResult} @ {HomeTeam} {AwayScore,2}-{HomeScore,2} ";
				}
				else if ( AwayScore < HomeScore )
				{
					theResult = abbreviate ? "L" : "Lost";
					theResult = $"{theResult} @ {HomeTeam} {AwayScore,2}-{HomeScore,2} ";
				}
				else if ( HomeScore > AwayScore )
				{
					theResult = abbreviate ? "W" : "Won";
					theResult = $"{theResult} @ {HomeTeam} {AwayScore,2}-{HomeScore,2} ";
				}
				else if ( HomeScore < AwayScore )
				{
					theResult = abbreviate ? "L" : "Lost";
					theResult = $"{theResult} @ {HomeTeam} {AwayScore,2}-{HomeScore,2} ";
				}
				else if ( AwayScore == 0 && HomeScore == 0 )
				{
					theResult = string.Format( "{1} @ {0}", HomeTeam, AwayTeam );
					if ( barIt )
					{
						theResult = $"   {AwayTeam} @ {HomeTeam}   ";
					}
					else
					{
						theResult = $"{AwayTeam} @ {HomeTeam}";
					}
				}
			}
			return string.Format( "{1}{0}", theResult, dividerChar );
		}

		public string ScoreCountsFor( string teamInFocus )
		{
			var theResult = " (0-0-0-0-0) ";
			if ( IsBye() ) return string.Format( "|{0}", theResult );
			if ( Played() )
			{
				//TODO actuals
				if ( teamInFocus == HomeTeam )
				{
					theResult = string.Format( " ({0}-{1}-{2}-{3}-{4}) ",
					   HomeTDp, HomeTDr, HomeTDd, HomeTDs, HomeFg );
				}
				else
				{
					theResult = string.Format( " ({0}-{1}-{2}-{3}-{4}) ",
					  AwayTDp, AwayTDr, AwayTDd, AwayTDs, AwayFg );
				}
			}
			else
			{
				if ( teamInFocus == HomeTeam )
				{
					theResult = string.Format( " ({0}-{1}-{2}-{3}-{4}) ",
					   ProjectedHomeTdp, ProjectedHomeTdr, ProjectedHomeTdd, ProjectedHomeTds, ProjectedHomeFg );
				}
				else
				{
					theResult = string.Format( " ({0}-{1}-{2}-{3}-{4}) ",
					   ProjectedAwayTdp, ProjectedAwayTdr, ProjectedAwayTdd, ProjectedAwayTds, ProjectedAwayFg );
				}
			}

			return string.Format( "|{0}", theResult );
		}

		public decimal ExperiencePoints( NFLPlayer p, string teamCode )
		{
			var ep = ( p.Unit() == NflTeam.KUnitcodePo ) || ( p.Unit() == NflTeam.KUnitcodeRo )
				  ? ( FeaturesPlayer( p.PlayerCode ) ? ExperiencePoints( p.Unit(), teamCode ) : 0.0M )
				  : ExperiencePoints( p.Unit(), teamCode );
			return ep;
		}

		public decimal ExperiencePoints( string unitCode, string teamCode )
		{
			if ( !MetricsCalculated ) TallyMetrics( String.Empty );
			decimal ep;
			var ures = UnitResult( unitCode, teamCode );
			switch ( ures )
			{
				case "W":
					ep = 4.0M;
					break;

				case "D":
					ep = 2.5M;
					break;

				default:
					ep = 1.0M;
					break;
			}
			return ep;
		}

		/// <summary>
		///   Prints the main leader of the unit that played in the game
		///   Needs to nknow the lineups for the game.
		/// </summary>
		/// <param name="unitCode"></param>
		/// <param name="teamCode"></param>
		/// <returns></returns>
		public string UnitHorse( string unitCode, string teamCode )
		{
			const string horse = "to be determined";
			return horse;
		}

		public string UnitStar( string unitCode, string teamCode )
		{
			string star;
			switch ( unitCode )
			{
				case NflTeam.KUnitcodePo:
					star = StarQuarterBack( teamCode );
					break;

				case NflTeam.KUnitcodePd:
					star = StarDefensiveBack( teamCode );
					break;

				case NflTeam.KUnitcodeRd:
					star = StarLinebacker( teamCode );
					break;

				case NflTeam.KUnitcodeRo:
					star = StarRunningBack( teamCode );
					break;

				case NflTeam.KUnitcodePr:
					star = StarPassRusher( teamCode );
					break;

				case NflTeam.KUnitcodePp:
					star = StarOffensiveLineman( teamCode );
					break;

				default:
					star = "unitCode unknown";
					break;
			}
			return star;
		}

		public char GameRating( string unitCode, string teamCode )
		{
			var nYDp = teamCode == HomeTeam ? HomeYDp : AwayYDp;
			var nYDr = teamCode == HomeTeam ? HomeYDr : AwayYDr;
			var nYDpDef = teamCode == HomeTeam ? AwayYDp : HomeYDp;
			var nYDrDef = teamCode == HomeTeam ? AwayYDr : HomeYDr;
			var nSacksAllowed = teamCode == AwayTeam ? _awaySaKa : _homeSaKa;

			var gameRating = FullGameRating( nYDp, nYDr, nYDrDef, nYDpDef, nSacksAllowed );
			char unitRating;
			switch ( unitCode )
			{
				case NflTeam.KUnitcodePo:
					unitRating = gameRating[ 0 ];
					break;

				case NflTeam.KUnitcodeRo:
					unitRating = gameRating[ 1 ];
					break;

				case NflTeam.KUnitcodePp:
					unitRating = gameRating[ 2 ];
					break;

				case NflTeam.KUnitcodePr:
					unitRating = gameRating[ 3 ];
					break;

				case NflTeam.KUnitcodeRd:
					unitRating = gameRating[ 4 ];
					break;

				case NflTeam.KUnitcodePd:
					unitRating = gameRating[ 5 ];
					break;

				default:
					unitRating = '?';
					break;
			}
			return unitRating;
		}

		private static string FullGameRating( int nYDp, int nYDr, int nYDrDef, int nYDpDef, decimal nSaksAllowed )
		{
			char cPo, cRo, cPp, cPr, cRd, cPd;

			if ( nYDr > 199 )
				cRo = 'A';
			else if ( nYDr > 149 )
				cRo = 'B';
			else if ( nYDr > 99 )
				cRo = 'C';
			else if ( nYDr > 39 )
				cRo = 'D';
			else
				cRo = 'E';

			if ( nYDp > 319 )
				cPo = 'A';
			else if ( nYDp > 219 )
				cPo = 'B';
			else if ( nYDp > 169 )
				cPo = 'C';
			else if ( nYDp > 109 )
				cPo = 'D';
			else
				cPo = 'E';

			if ( nYDrDef > 199 )
				cRd = 'E';
			else if ( nYDrDef > 149 )
				cRd = 'D';
			else if ( nYDrDef > 99 )
				cRd = 'C';
			else if ( nYDrDef > 39 )
				cRd = 'B';
			else
				cRd = 'A';

			if ( nYDpDef > 319 )
				cPd = 'E';
			else if ( nYDpDef > 219 )
				cPd = 'D';
			else if ( nYDpDef > 169 )
				cPd = 'C';
			else cPd = nYDpDef > 119 ? 'B' : 'A';

			if ( nSaksAllowed < 1.0M )
			{
				cPp = 'A';
				cPr = 'E';
			}
			else if ( nSaksAllowed < 3.0M )
			{
				cPp = 'B';
				cPr = 'D';
			}
			else if ( nSaksAllowed < 5.0M )
			{
				cPp = 'C';
				cPr = 'C';
			}
			else if ( nSaksAllowed < 7.0M )
			{
				cPp = 'D';
				cPr = 'B';
			}
			else
			{
				cPp = 'E';
				cPr = 'A';
			}
			var sb = new StringBuilder();
			sb.Append( cPo );
			sb.Append( cRo );
			sb.Append( cPp );
			sb.Append( cPr );
			sb.Append( cRd );
			sb.Append( cPd );

			return sb.ToString();
		}

		public char CurrRating( string unitCode, string teamCode )
		{
			var focusTeam = teamCode == HomeTeam ? GetHomeTeam() : GetAwayTeam();
			focusTeam.SetRecord( Season, skipPostseason: false );
			char unitRating;
			switch ( unitCode )
			{
				case NflTeam.KUnitcodePo:
					unitRating = focusTeam.Ratings[ 0 ];
					break;

				case NflTeam.KUnitcodeRo:
					unitRating = focusTeam.Ratings[ 1 ];
					break;

				case NflTeam.KUnitcodePp:
					unitRating = focusTeam.Ratings[ 2 ];
					break;

				case NflTeam.KUnitcodePr:
					unitRating = focusTeam.Ratings[ 3 ];
					break;

				case NflTeam.KUnitcodeRd:
					unitRating = focusTeam.Ratings[ 4 ];
					break;

				case NflTeam.KUnitcodePd:
					unitRating = focusTeam.Ratings[ 5 ];
					break;

				default:
					unitRating = '?';
					break;
			}
			return unitRating;
		}

		public string StarDefensiveBack( string teamCode )
		{
			return StarBack( "FS", teamCode );
		}

		private string StarPassRusher( string teamCode )
		{
			return StarBack( "DE", teamCode );
		}

		private string StarOffensiveLineman( string teamCode )
		{
			return StarBack( "C", teamCode );
		}

		private string StarLinebacker( string teamCode )
		{
			return StarBack( "MLB", teamCode );
		}

		private string StarRunningBack( string teamCode )
		{
			return StarBack( "RB", teamCode );
		}

		private string StarQuarterBack( string teamCode )
		{
			return StarBack( "QB", teamCode );
		}

		private string StarBack( string backCode, string teamCode )
		{
			var focusTeam = teamCode == HomeTeam ? GetHomeTeam() : GetAwayTeam();
			return focusTeam.KeyPlayer( backCode, Season, Week );
		}

		public string UnitResult( string unitCode, string teamCode )
		{
			var ures = "";
			decimal metric;
			if ( unitCode == NflTeam.KUnitcodePo )
			{
				metric = teamCode == HomeTeam ? HomeTDp : AwayTDp;
				ures = ResultOf( metric, 0.9M, 1.3M );   //  0=L, 1=D, 2=W, 3+=W
			}
			if ( unitCode == NflTeam.KUnitcodeRo )
			{
				metric = teamCode == HomeTeam ? HomeTDr : AwayTDr;
				ures = ResultOf( metric, 0.0M, 1.0M );   //  0=L, 1=D, 2+W
														 //Utility.Announce( string.Format( "    Tdr = {0} so Unit Result is {1} ", metric, ures ) );
			}
			if ( unitCode == NflTeam.KUnitcodePp )
			{
				metric = teamCode == HomeTeam ? HomeSaKa : AwaySaKa;
				ures = ReverseResultOf( metric, 2.0M, 4.0M ); //  0=W, 1=W, 2-3=D, 4+=L
			}
			if ( unitCode == NflTeam.KUnitcodePr )
			{
				metric = teamCode == HomeTeam ? AwaySaKa : HomeSaKa;
				ures = ResultOf( metric, 2.0M, 4.0M ); //  0=L, 1=L, 2-3=D, 4+=W
			}
			if ( unitCode == NflTeam.KUnitcodePd )
			{
				metric = teamCode == HomeTeam ? AwayTDp : HomeTDp;
				ures = ReverseResultOf( metric, 0.9M, 1.3M );   //  0=W, 1=D, 2=L, 3+=W
			}
			if ( unitCode == NflTeam.KUnitcodeRd )
			{
				metric = teamCode == HomeTeam ? AwayTDr : HomeTDr;
				ures = ReverseResultOf( metric, 0.0M, 1.0M );   //  0=W, 1=D, 2+=L
			}
			return ures;
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

		private static string ReverseResultOf( decimal metric, decimal lossQuota, decimal drawQuota )
		{
			var res = ResultOf( metric, lossQuota, drawQuota );

			if ( res == "W" )
				return "L";

			return ( res == "L" ) ? "W" : "D";
		}

		/// <summary>
		///   Tells you if a player appeared in a game.
		///   If he did, he will have stats or Scores or if post 2004 he will appear in a lineup.
		///   Even though a player may be rostered he may be a bench player or more likely Injured.
		/// </summary>
		/// <param name="playerId">The player id.</param>
		/// <returns>true is the player appears in the game</returns>
		public bool FeaturesPlayer( string playerId )
		{
			var isFeatured = false;
			if ( Utility.TflWs.AnyStatsForGame( playerId, Season, Int32.Parse( Week ), GameCode ) )
				isFeatured = true;
			else
			{
				if ( Utility.TflWs.AnyScoresForGame( playerId, Season, Int32.Parse( Week ), GameCode ) )
					isFeatured = true;
			}
			//			if ( isFeatured )
			//				RosterLib.Utility.Announce( string.Format( "{0} is featured in {1}", playerId, GameName() ) );
			//			else
			//				RosterLib.Utility.Announce( string.Format( "{0} is not featured in {1}", playerId, GameName() ) );

			return isFeatured;
		}

		public string ScoreOut3()
		{
			return Played() ? string.Format( "{0} {1} @ {2} {3}",
			   AwayTeam, AwayScore, HomeTeam, HomeScore ) : "Not Played";
		}

		public string ScoreOut2( string teamInFocus )
		{
			var theResult = "tied";
			if ( teamInFocus == HomeTeam )
			{
				if ( HomeScore > AwayScore )
					theResult = string.Format( "Won {0}-{1}", HomeScore, AwayScore );
				else
				{
					if ( HomeScore < AwayScore ) theResult = string.Format( "Lost {1}-{0}", HomeScore, AwayScore );
				}
			}
			else
			{
				if ( AwayScore > HomeScore )
					theResult = string.Format( "Won {1}-{0}", HomeScore, AwayScore );
				else
				{
					if ( AwayScore < HomeScore ) theResult = string.Format( "Lost {0}-{1}", HomeScore, AwayScore );
				}
			}
			return theResult;
		}

		public string ResultvsSpread( string teamInFocus )
		{
			var theResult = "???";

			if ( ( HomeScore + AwayScore ) == 0 )
				theResult = "Push";
			else
			{
				if ( GameDate < DateTime.Now )
				{
					if ( teamInFocus == HomeTeam )
					{
						if ( ( HomeScore - Spread ) > AwayScore )
							theResult = "Win";
						else
							theResult = ( HomeScore - Spread ) == AwayScore ? "Push" : "Loss";
					}
					else if ( AwayScore > ( HomeScore - Spread ) )
						theResult = "Win";
					else
						theResult = AwayScore == ( HomeScore - Spread ) ? "Push" : "Loss";
				}
			}
			return theResult;
		}

		public string ResultvsTotal()
		{
			var theResult = "Push";

			if ( Played() )
			{
				if ( HomeScore + AwayScore > Total )
					theResult = "Over";
				else
				{
					if ( HomeScore + AwayScore < Total )
						theResult = "Under";
				}
			}
			return theResult;
		}

		public string OpponentOut( string teamInFocus )
		{
			var ha = ( HomeTeam == teamInFocus ) ? "v" : "@";
			var opp = ( HomeTeam == teamInFocus ) ? AwayTeam : HomeTeam;
			return ha + opp;
		}

		private static string TeamOut( string teamCode, string teamInFocus )
		{
			return teamCode == teamInFocus ? HtmlLib.Bold( teamCode ) : teamCode;
		}

		public decimal DefensiveFantasyPtsFor( string teamCode )
		{
			if ( !Played() ) return 0.0M;

			NflTeam team;
			if ( IsAway( teamCode ) )
				team = AwayNflTeam;
			else
				team = HomeNflTeam;

			if ( GameWeek == null ) GameWeek = new NFLWeek( Season, Week );
			var calculator = new DefensiveScoringCalculator( GameWeek, offset: 0 );
			calculator.Calculate( team: team, game: this );
			return team.FantasyPoints;
		}

		private string WeekOut()
		{
			return Int32.Parse( Week ) > 17 ? "PO" : Week;
		}

		public bool IsBadWeather()
		{
			var badWeather = false;
			if ( Int32.Parse( Week ) > 8 )
				if ( !IsDomeGame() )
					if ( IsGameInNe() ) badWeather = true;
			return badWeather;
		}

		public bool IsDomeGame()
		{
			bool isDome;

			switch ( HomeTeam )
			{
				case "NO":
					isDome = true;
					break;

				case "MV":
					isDome = true;
					break;

				case "IC":
					isDome = true;
					break;

				case "DC":
					isDome = true;
					break;

				case "DL":
					isDome = true;
					break;

				case "AF":
					isDome = true;
					break;

				case "AC":
					isDome = true;
					break;

				default:
					isDome = false;
					break;
			}

			return isDome;
		}

		public bool IsPrimeTime()
		{
			var isPrimeTime = IsMondayNight() || IsSundayNight();
			return isPrimeTime;
		}

		/// <summary>
		/// Determines whether the game is in the North East.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if [is game in NE]; otherwise, <c>false</c>.
		/// </returns>
		public bool IsGameInNe()
		{
			bool isNe;

			switch ( HomeTeam )
			{
				case "NE":
					isNe = true;
					break;

				case "NG":
					isNe = true;
					break;

				case "NJ":
					isNe = true;
					break;

				case "PE":
					isNe = true;
					break;

				case "PS":
					isNe = true;
					break;

				case "WR":
					isNe = true;
					break;

				case "BR":
					isNe = true;
					break;

				case "BB":
					isNe = true;
					break;

				case "GB":
					isNe = true;
					break;

				case "DL":
					isNe = true;
					break;

				case "CL":
					isNe = true;
					break;

				case "CH":
					isNe = true;
					break;

				default:
					isNe = false;
					break;
			}

			return isNe;
		}

		public string AussieDate()
		{
			//  We are a day ahead
			return GameDate.AddDays( 1D ).ToLongDateString();
		}

		public DateTime AussieDateTime()
		{
			//  We are a day ahead
			var aus = GameDate.AddDays( 1D );
			return aus;
		}

		public string AussieHour( bool showTv )
		{
			var aussieHr = "??:??";

			if ( Hour == null ) return aussieHr;

			//  Time changes at begining of october
			if ( ( GameDate.Month < 10 ) && ( GameDate.Month > 3 ) )
			{
				switch ( Hour.Trim() )
				{
					case "1":
						aussieHr = "03:00";
						break;

					case "2":
						aussieHr = "04:00";
						break;

					case "3":
						aussieHr = "05:00";
						break;

					case "4":
						aussieHr = "06:00";
						break;

					case "7":
						aussieHr = "09:00";
						break;

					case "8":
						aussieHr = "10:00";
						break;

					case "9":
						aussieHr = "11:00";
						break;

					case "0":
						aussieHr = "12:00";
						break;

					default:
						aussieHr = "??:??";
						break;
				}
			}
			else
			{
				switch ( Hour.Trim() )
				{
					case "1":
						aussieHr = "04:00";
						break;

					case "4":
						aussieHr = "07:00";
						break;

					case "8":
						aussieHr = "11:00";
						break;

					case "9":
						aussieHr = "12:00";
						break;

					default:
						aussieHr = "??:??";
						break;
				}
			}

			if ( showTv && IsOnTv ) aussieHr += " TV";

			return aussieHr;
		}

		#region Stats

		public int YDp( string teamCode )
		{
			return HomeTeam == teamCode ? HomeYDp : AwayYDp;
		}

		public int YDr( string teamCode )
		{
			return HomeTeam == teamCode ? HomeYDr : AwayYDr;
		}

		public int YDrAllowed( string teamCode )
		{
			return HomeTeam == teamCode ? AwayYDr : HomeYDr;
		}

		public int YDpAllowed( string teamCode )
		{
			return HomeTeam == teamCode ? AwayYDp : HomeYDp;
		}

		public int Tdr( string teamCode )
		{
			return HomeTeam == teamCode ? HomeTDr : AwayTDr;
		}

		public int Fg( string teamCode )
		{
			return HomeTeam == teamCode ? HomeFg : AwayFg;
		}

		public int Tdp( string teamCode )
		{
			return HomeTeam == teamCode ? HomeTDp : AwayTDp;
		}

		public int IntsAllowed( string teamCode )
		{
			return HomeTeam == teamCode ? AwayInt : HomeInt;
		}

		public int Interceptions( string teamCode )
		{
			return HomeTeam == teamCode ? HomeInt : AwayInt;
		}

		public decimal SacksAllowed( string teamCode )
		{
			return HomeTeam == teamCode ? HomeSaKa : AwaySaKa;
		}

		public decimal Sacks( string teamCode )
		{
			return HomeTeam == teamCode ? AwaySaKa : HomeSaKa;
		}

		public int TdrAllowed( string teamCode )
		{
			return HomeTeam == teamCode ? AwayTDr : HomeTDr;
		}

		public int TdpAllowed( string teamCode )
		{
			return HomeTeam == teamCode ? AwayTDp : HomeTDp;
		}

		#endregion Stats

		#region Accessors

		public int TotalPoints
		{
			get { return HomeScore + AwayScore; }
		}

		public int WeekNo
		{
			get { return Int32.Parse( Week ); }
		}

		public string Week { get; set; }

		public string Season { get; set; }

		public string Hour { get; set; }

		public string GameCode { get; set; }

		public string HomeTeam { get; set; }

		public string AwayTeam { get; set; }

		public string HomeTeamName { get; set; }

		public string AwayTeamName { get; set; }

		public DateTime GameDate { get; set; }

		public int HomeScore { get; set; }

		public int AwayScore { get; set; }

		#region Projections

		public int ProjectedHomeTdr { get; set; }

		public int ProjectedHomeTdp { get; set; }

		public int ProjectedHomeTdd { get; set; }

		public int ProjectedHomeTds { get; set; }

		public int ProjectedHomeYdr { get; set; }

		public int ProjectedHomeYdp { get; set; }

		public int ProjectedHomeFg { get; set; }

		public int ProjectedHomeScore { get; set; }

		public int ProjectedAwayTdr { get; set; }

		public int ProjectedAwayTdp { get; set; }

		public int ProjectedAwayTdd { get; set; }

		public int ProjectedAwayTds { get; set; }

		public int ProjectedAwayFg { get; set; }

		public int ProjectedAwayScore { get; set; }

		public int ProjectedAwayYdr { get; set; }

		public int ProjectedAwayYdp { get; set; }

		#endregion Projections

		#region Actuals

		public int HomeYdr { get; set; }

		public int HomeYdp { get; set; }

		public int HomeFg { get; set; }

		public int AwayFg { get; set; }

		public int AwayYdr { get; set; }

		public int AwayYdp { get; set; }

		#endregion Actuals

		public bool IsOnTv { get; set; }

		public decimal Spread { get; set; }

		public decimal Total { get; set; }

		public NFLResult Result { get; set; }

		public Hashtable StatsHt { get; set; }

		public Hashtable PlayersHt { get; set; }

		public int HomeTDp { get; set; }

		public int HomeTDr { get; set; }

		public int HomeTDd { get; set; }

		public int HomeTDs { get; set; }

		public decimal HomeSaKa
		{
			get { return _homeSaKa; }
			set { _homeSaKa = value; }
		}

		public int AwayTDp { get; set; }

		public int AwayTDr { get; set; }

		public int AwayTDd { get; set; }

		public int AwayTDs { get; set; }

		public decimal AwaySaKa
		{
			get { return _awaySaKa; }
			set { _awaySaKa = value; }
		}

		public int HomePasses { get; set; }

		public int AwayPasses { get; set; }

		public int HomeRuns { get; set; }

		public int AwayRuns { get; set; }

		public bool MetricsCalculated { get; set; }

		public int Rating { get; set; }

		public string MyTip
		{
			get { return _myTip; }
			set { _myTip = value; }
		}

		public int HomeYDr { get; set; }

		public int HomeYDp { get; set; }

		public int AwayYDr { get; set; }

		public int AwayYDp { get; set; }

		public int HomeInt { get; set; }

		public int AwayInt { get; set; }

		/// <summary>
		///  Loads the stats for a game into a hash table.
		/// </summary>
		public void LoadStats( DataLibrarian tflWs )
		{
			LoadPlayersForTeam( HomeTeam, tflWs );
			LoadPlayersForTeam( AwayTeam, tflWs );
		}

		private void LoadPlayersForTeam( string teamCode, DataLibrarian tflWs )
		{
			GameCode = tflWs.GetGameCode( Season, Week, teamCode );
			var s = tflWs.PlayerStats( Constants.K_STATCODE_SACK, Season, Week, GameCode, teamCode );
			var hashCode = string.Format( "{0}{1}", teamCode, Constants.K_STATCODE_SACK );
			var count = tflWs.TeamStats( Constants.K_STATCODE_SACK, Season, Week, GameCode, teamCode );
			if ( PlayersHt == null ) PlayersHt = new Hashtable();
			PlayersHt.Add( hashCode, s );
			if ( StatsHt == null ) StatsHt = new Hashtable();
			StatsHt.Add( hashCode, count );
		}

		public decimal StatFor( string teamCode, string statIn )
		{
			return ( decimal ) StatsHt[ HashCode( teamCode, statIn ) ];
		}

		public string PlayersFor( string teamCode, string statIn )
		{
			return PlayersHt[ HashCode( teamCode, statIn ) ].ToString();
		}

		private static string HashCode( string teamCode, string statIn )
		{
			return string.Format( "{0}{1}", teamCode, statIn );
		}

		public decimal HomeDecEquivalent { get; set; }

		public decimal AwayDecEquivalent { get; set; }

		public decimal GordanLine { get; set; }

		public bool BetHome { get; set; }

		public bool BetAway { get; set; }

		public decimal SpreadResult { get; set; }

		#endregion Accessors

		public decimal SpreadScore()
		{
			SpreadResult = HomeScore - AwayScore - Spread; //  Spread is positive for HT
			return SpreadResult;
		}

		public void RefreshTotals()
		{
#if DEBUG
			Utility.Announce( string.Format( "Refreshing TD totals {0}", GameCodeOut() ) );
#endif
			var scores = Utility.TflWs.ScoresDs( Season, Week, GameCode );
			var nHomeTDp = 0;
			var nAwayTDp = 0;
			var nHomeTDr = 0;
			var nAwayTDr = 0;
			var nHomeFg = 0;
			var nAwayFg = 0;
			var nHomeTDd = 0;
			var nAwayTDd = 0;
			var nHomeTDs = 0;
			var nAwayTDs = 0;

			foreach ( DataRow dr in scores.Tables[ 0 ].Rows )
			{
				var isHome = IsHome( dr[ "TEAM" ].ToString() );
				var scoreType = dr[ "SCORE" ].ToString();
				switch ( scoreType )
				{
					case Constants.K_SCORE_TD_PASS:
						if ( isHome )
							nHomeTDp++;
						else
							nAwayTDp++;
						break;

					case Constants.K_SCORE_TD_RUN:
						if ( isHome )
							nHomeTDr++;
						else
							nAwayTDr++;
						break;

					case Constants.K_SCORE_INTERCEPT_RETURN:
						if ( isHome )
							nHomeTDd++;
						else
							nAwayTDd++;
						break;

					case Constants.K_SCORE_FUMBLE_RETURN:
						if ( isHome )
							nHomeTDd++;
						else
							nAwayTDd++;
						break;

					case Constants.K_SCORE_KICK_RETURN:
						if ( isHome )
							nHomeTDs++;
						else
							nAwayTDs++;
						break;

					case Constants.K_SCORE_PUNT_RETURN:
						if ( isHome )
							nHomeTDs++;
						else
							nAwayTDs++;
						break;

					case Constants.K_SCORE_FIELD_GOAL:
						if ( isHome )
							nHomeFg++;
						else
							nAwayFg++;
						break;
				}
			}
			var res = new NFLResult( HomeTeam, HomeScore, AwayTeam, AwayScore )
			{
				HomeTDp = nHomeTDp,
				HomeTDr = nHomeTDr,
				HomeTDd = nHomeTDd,
				HomeTDs = nHomeTDs,
				AwayTDr = nAwayTDr,
				AwayTDp = nAwayTDp,
				AwayTDd = nAwayTDd,
				AwayTDs = nAwayTDs,
				HomeFg = nHomeFg,
				AwayFg = nAwayFg
			};
			StoreResult( res );
		}

		public void StoreResult( NFLResult res )
		{
			Utility.TflWs.StoreResult( Season, Week, GameCode, res.AwayScore, res.HomeScore,
			   res.HomeTDp, res.AwayTDp, res.HomeTDr, res.AwayTDr, res.HomeFg, res.AwayFg, res.AwayTDd, res.HomeTDd,
			   res.AwayTDs, res.HomeTDs );
		}

		public void WriteProjection()
		{
			var r = new GameProjection( this );
			r.Render();
		}

		/// <summary>
		///   returns the spread or the projection
		/// </summary>
		/// <returns></returns>
		public decimal GetSpread()
		{
			return Spread != 0 ? Spread : ProjectedSpread();
		}

		public string GetFormattedSpread()
		{
			var spread = GetSpread();
			if ( spread == 0.0M ) return "OTB";
			if ( spread == 0.5M ) return "PKM";
			var spreadOut = string.Format( "{0:#}", spread );
			if ( spread > 0 ) spreadOut = "+" + spreadOut;
			return spreadOut;
		}

		public string GetFormattedSpread( string teamcodeInFocus )
		{
			var spread = GetSpread();
			if ( spread == 0.0M ) return "OTB";
			if ( spread == 0.5M ) return "PKM";
			var spreadOut = string.Format( "{0:#}", spread );
			if ( IsHome( teamcodeInFocus ) )
			{
				if ( spread > 0 ) spreadOut = "+" + spreadOut;
			}
			else
			{
				if ( spread > 0 ) spreadOut = "-" + spreadOut;
			}
			return spreadOut;
		}

		public decimal ProjectedSpread()
		{
			return ( ProjectedHomeScore - ProjectedAwayScore );
		}

		internal string ScorersOff( string scoreType, string teamCode )
		{
			var scorers = new StringBuilder();

			var ds = Utility.TflWs.ScoresDs( scoreType, teamCode, Season, Week, GameCode );
			foreach ( DataRow score in ds.Tables[ 0 ].Rows )
			{
				var playerId = score[ "PLAYERID1" ].ToString();
				var player = new NFLPlayer( playerId, Utility.CurrentLeague );

				scorers.Append( player.PlayerNameShort + ", " );
			}
			var scorersOut = scorers.ToString();
			if ( scorersOut.Length > 2 ) scorersOut = scorersOut.Remove( scorersOut.Length - 2, 2 );
			return scorersOut;
		}

		public void TallyStatsFor( NflTeam team )
		{
#if DEBUG
			Announce( "Tallying Stats for " + team.NameOut() );
#endif
			var gameYDr = YDr( team.TeamCode );
			var gameYDp = YDp( team.TeamCode );
			var gameTDp = Tdp( team.TeamCode );
			var gameTdr = Tdr( team.TeamCode );
			var gameSacks = Sacks( team.TeamCode );
			var gameSacksAllowed = SacksAllowed( team.TeamCode );
			var gameYDrAllowed = YDrAllowed( team.TeamCode );
			var gameYDpAllowed = YDpAllowed( team.TeamCode );
			var gameTdpAllowed = TdpAllowed( team.TeamCode );
			var gameTdrAllowed = TdrAllowed( team.TeamCode );
			var gameInterceptions = IntsAllowed( team.TeamCode );
			var gameInterceptionsThrown = Interceptions( team.TeamCode );
			var gameFg = Fg( team.TeamCode );

			team.TotYdr += gameYDr;
			team.TotYdp += gameYDp;
			team.TotSacksAllowed += gameSacksAllowed;
			team.TotSacks += gameSacks;
			team.TotYdrAllowed += gameYDrAllowed;
			team.TotTDpAllowed += gameTdpAllowed;
			team.TotIntercepts += gameInterceptions;

			//  NflStats are defined here
			if ( StatList == null ) StatList = new List<NflStat>();
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "YDr", gameYDr, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "YDp", gameYDp, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "TDp", gameTDp, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "TDr", gameTdr, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "TDrAllowed", gameTdrAllowed, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "Sacks", gameSacks, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "SacksAllowed", gameSacksAllowed, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "YDrAllowed", gameYDrAllowed, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "YDpAllowed", gameYDpAllowed, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "TDpAllowed", gameTdpAllowed, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "INTs", gameInterceptions, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "INTsThrown", gameInterceptionsThrown, Opponent( team.TeamCode ) ) );
			StatList.Add( new NflStat( Season, Week, team.TeamCode, "FG", gameFg, Opponent( team.TeamCode ) ) );
		}

		public List<NflStat> GenerateStats()
		{
			TallyMetrics( String.Empty );
			TallyStatsFor( HomeNflTeam );
			TallyStatsFor( AwayNflTeam );
			return StatList;
		}

		public List<GridStatsOutput> GenerateGridStatsOutput()
		{
			TallyGridStatsFor( HomeNflTeam, announceIt: false );
			TallyGridStatsFor( AwayNflTeam, announceIt: false );
			return GridStatsList;
		}

		public void TallyGridStatsFor( NflTeam nflTeam, bool announceIt )
		{
			var theWeek = new NFLWeek( Season, WeekNo );

			var scorer = new GS4Scorer( theWeek ) { AnnounceIt = announceIt, ScoresOnly = true };
			var playerList = LoadLineupPlayers( nflTeam.TeamCode == HomeTeam ? HomeTeam : AwayTeam );
			foreach ( var nflPlayer in playerList )
			{
				if ( !nflPlayer.IsFantasyPlayer() ) continue;
				var qty = scorer.RatePlayer( nflPlayer, theWeek );
				if ( qty > 0.0M )
				{
					GridStatsList.Add( new GridStatsOutput(
									  Season,
									  Week,
									  nflPlayer.PlayerCode,
									  qty,
									  Opponent( nflTeam.TeamCode ) ) );
				}
			}
		}

		public List<YahooOutput> GenerateYahooOutput()
		{
			TallyYahooFor( HomeNflTeam, announceIt: true );
			TallyYahooFor( AwayNflTeam, announceIt: true );
			return YahooList;
		}

		public void TallyYahooFor( NflTeam nflTeam, bool announceIt )
		{
			var theWeek = new NFLWeek( Season, WeekNo );

			var scorer = new YahooScorer( theWeek );
			//  Lineup could be missing some people, manually entered games will have NO LINEUP
			var playerList = LoadLineupPlayers( 
				nflTeam.TeamCode == HomeTeam ? HomeTeam : AwayTeam );
			foreach ( var nflPlayer in playerList )
			{
				if ( !nflPlayer.IsFantasyPlayer() ) continue;
				var qty = scorer.RatePlayer( nflPlayer, theWeek, false );
				if ( qty > 0.0M )
				{
					Announce( $"{qty} for {nflPlayer.PlayerName} in {Season + ":" + Week}" );

					var yo = new YahooOutput(
									  Season,
									  Week,
									  nflPlayer.PlayerCode,
									  qty,
									  Opponent( nflTeam.TeamCode ) );

					//Announce( $"adding {yo.StatOut()}" );

					YahooList.Add( yo );
				}
			}
		}

		public List<NFLPlayer> LoadHomePlayers()
		{
			HomePlayers = LoadLineupPlayers( HomeTeam );
			return HomePlayers;
		}

		public List<NFLPlayer> LoadAwayPlayers()
		{
			AwayPlayers = LoadLineupPlayers( AwayTeam );
			return AwayPlayers;
		}

		public List<NFLPlayer> LoadAllFantasyAwayPlayers(
			DateTime? date,
			string catFilter = ""
			)
		{
			if ( date == null )
			{
				date = GameDate;
			}
			AwayPlayers = new List<NFLPlayer>();

			AwayPlayers.AddRange( 
				LoadTeamPlayersForCat( 
					AwayTeam, 
					catFilter, 
					date ) );

			return AwayPlayers;
		}

		public List<NFLPlayer> LoadAllFantasyHomePlayers( 
			DateTime? date,
			string catFilter = ""
			)
		{
			if ( date == null )
			{
				date = GameDate;
			}
			HomePlayers = new List<NFLPlayer>();

			HomePlayers.AddRange( 
				LoadTeamPlayersForCat( 
					HomeTeam, 
					catFilter,
					date ) );

			return HomePlayers;
		}

		private static IEnumerable<NFLPlayer> LoadTeamPlayersForCat( 
			string teamCode, 
			string playerCat,
			DateTime? date	)
		{
			var players = new List<NFLPlayer>();
			var ds = Utility.TflWs.GetTeamPlayersByDate( 
				teamCode, 
				playerCat, 
				(DateTime) date );
			var dt = ds.Tables[ "player" ];
			if ( dt.Rows.Count == 0 ) return players;
			players.AddRange( from DataRow dr in dt.Rows
							  select new NFLPlayer( dr[ "PLAYERID" ].ToString() ) );
			return players;
		}

		public void LoadLineups()
		{
			if ( PgmDao == null ) PgmDao = new DbfPlayerGameMetricsDao();

			LoadHomeLineup();
			LoadAwayLineup();
		}

		public void LoadPgms()
		{
			if ( PgmDao == null ) PgmDao = new DbfPlayerGameMetricsDao();

			if ( Pgms == null ) Pgms = new List<PlayerGameMetrics>();
			Pgms = PgmDao.GetGame( GameKey() );

			FantasyPlayers = new List<NFLPlayer>();
			foreach ( var pgm in Pgms )
			{
				FantasyPlayers.Add( new NFLPlayer( pgm.PlayerId ) );
			}
		}

		public void LoadHomeLineup()
		{
			//TODO Lineups may be missing people
			//if ( HomeTeam == null || Season == null || WeekNo == 0 ) return;
			HomeLineup = new Lineup( Utility.TflWs.GetLineup( HomeTeam, Season, WeekNo ) );
			HomeQb1 = DetermineHomePlayerAt( "QB" );  //  first one you find!!
			if ( HomeQb1 != null )
			{
				HomeQb1.CurrentGameMetrics = PgmDao.GetPlayerWeek( GameKey(), HomeQb1.PlayerCode );
				HomeRb1 = DetermineHomePlayerAt( "RB" );  //TODO:  get this from stats  first one
				HomeRb1.CurrentGameMetrics = PgmDao.GetPlayerWeek( GameKey(), HomeRb1.PlayerCode );
			}
		}

		public void LoadAwayLineup()
		{
			AwayLineup = new Lineup( Utility.TflWs.GetLineup( AwayTeam, Season, WeekNo ) );
			AwayQb1 = DetermineAwayPlayerAt( "QB" );
			if ( AwayQb1 != null )
			{
				AwayQb1.CurrentGameMetrics = PgmDao.GetPlayerWeek( GameCode, AwayQb1.PlayerCode );
				AwayRb1 = DetermineAwayPlayerAt( "RB" );
				AwayRb1.CurrentGameMetrics = PgmDao.GetPlayerWeek( GameCode, AwayRb1.PlayerCode );
			}
		}

		private NFLPlayer DetermineAwayPlayerAt( string pos )
		{
			return AwayLineup.GetPlayerAt( pos );
		}

		private NFLPlayer DetermineHomePlayerAt( string pos )
		{
			return HomeLineup.GetPlayerAt( pos );
		}

		public List<NFLPlayer> LoadLineupPlayers( string teamCode )
		{
			Announce( string.Format( "NFLGame.LoadLineupPlayers for {0}:{1}",
				teamCode, GameCodeOut() ) );

			var LineupDs = Utility.TflWs.GetLineup( teamCode, Season, WeekNo );
			var lineup = new Lineup( LineupDs );

			Announce( string.Format( "NFLGame.LoadLineupPlayers {0} players in lineup", lineup.PlayerList.Count ) );

			return lineup.PlayerList;
		}

		private void Announce( string msg )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Trace( msg );
			Console.WriteLine( msg );
		}

		private void LogInfo( string msg )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Info( msg );
		}

		public decimal OpponentPowerRating( string teamCode, string week )
		{
			var opp = OpponentTeam( teamCode );
			var powerRating = opp.GetPowerRating( week );
#if DEBUG
			Utility.Announce( string.Format( "   {0} opponent is {1} with PowerRating of {2} ",
			  teamCode, opp.TeamCode, powerRating ) );
#endif
			return powerRating;
		}

		internal string EvaluatePrediction( NFLResult predictedResult )
		{
			if ( Played() )
				return predictedResult.WinningTeam().Equals( Result.WinningTeam() ) ? "SU:WIN" : "SU:LOSS";
			return "Not Played";
		}

		public string EvaluatePredictionAts( NFLResult predictedResult, decimal spread )
		{
			if ( Played() )
			{
				var predictedMargin = ( Decimal ) predictedResult.Margin();
				if ( predictedMargin.Equals( spread ) ) return "ATS:PUSH";

				var selection = predictedResult.WinningTeamAts( spread );

				Utility.Announce( string.Format( "Prediction chooses {0} to win when spread is {1}",
												 selection, spread ) );

				var actualWinner = Result.WinningTeamAts( spread );
				if ( actualWinner.Equals( "TIE" ) ) return "ATS:PUSH";

				return selection.Equals( actualWinner ) ? "ATS:WIN" : "ATS:LOSS";
			}
			return "Not Played";
		}

		public void CalculateSpreadResult()
		{
            //  dont use the actual score properties
            var homescore = 0;
            var awayscore = 0;
			if ( Spread == 0 )
			{
                // OTB, give it to the home team 21-20
                homescore = 21;
                awayscore = 20;
			}
			else
			{
				//  what is the bookies tip
				var winningScore = 0.0M;
				var losingScore = 0.0M;

				if ( ( Total > 0 ) )
				{
					var splitScore = Total - Math.Abs( Spread );
					losingScore = Math.Round( splitScore / 2, MidpointRounding.AwayFromZero );
					winningScore = Math.Round( Total - losingScore, MidpointRounding.AwayFromZero );
				}

				if ( ( winningScore - losingScore ) < Math.Abs( Spread ) )
					winningScore++;

				if ( Spread > 0 )
				{
					homescore = ( int ) winningScore;
                    awayscore = ( int ) losingScore;
				}
				else
				{
					homescore = ( int ) losingScore;
					awayscore = ( int ) winningScore;
				}
			}
			BookieTip = new NFLResult( HomeTeam, homescore, AwayTeam, awayscore );
		}

		public virtual NFLResult GetPrediction( string predMethod )
		{
			if ( PredictionGetter == null )
				PredictionGetter = new DbfPredictionQueryMaster();

			var prediction = PredictionGetter.Get( predMethod, Season, Week, GameCode );
			return prediction.NflResult;
		}

		public void LoadPrediction( string method )
		{
			if ( ProjectedHomeScore + ProjectedAwayScore != 0 ) return;
			var result = GetPrediction( method );
			ProjectedHomeTdp = result.HomeTDp;
			ProjectedHomeFg = result.HomeFg;
			ProjectedHomeTdr = result.HomeTDr;
			ProjectedHomeTds = result.HomeTDs;
			ProjectedHomeTdd = result.HomeTDd;
			ProjectedAwayTdp = result.AwayTDp;
			ProjectedAwayFg = result.AwayFg;
			ProjectedAwayTdr = result.AwayTDr;
			ProjectedHomeYdp = result.HomeYDp;
			ProjectedHomeYdr = result.HomeYDr;
			ProjectedAwayYdp = result.AwayYDp;
			ProjectedAwayYdr = result.AwayYDr;
			ProjectedAwayTds = result.AwayTDs;
			ProjectedAwayTdd = result.AwayTDd;
			ProjectedHomeScore = result.HomeScore;
			ProjectedAwayScore = result.AwayScore;
			HomeScore = result.HomeScore;
			AwayScore = result.AwayScore;
			Result = result;
		}

		/// <summary>
		///   The default prediction method is "unit"
		/// </summary>
		public void LoadPrediction()
		{
			LoadPrediction( "unit" );
		}

		internal int TotalTds()
		{
			return ( HomeTDd + HomeTDs + HomeTDr + HomeTDp + AwayTDd + AwayTDs + AwayTDr + AwayTDp );
		}

		internal string ProjectionLink()
		{
			var urlOut = ProjectionUrl();
			return string.Format( "<a href='{0}'>{1}</a>", urlOut, GetPrediction( "unit" ).PredictedScore() );
		}

		public string ProjectionUrl()
		{
			var urlOut = string.Format( ".//gameprojections//{0}-{1}@{2}.htm", Week, AwayTeam, HomeTeam );
			return urlOut;
		}

		public string GameProjectionUrl()
		{
			var urlOut = string.Format( ".//gameprojections//Week {0:0#}/{1}@{2}.htm", Int32.Parse( Week ), AwayTeam, HomeTeam );
			return urlOut;
		}

		public string SummaryFile()
		{
			var summaryFile = string.Format( "{0}{1}//GameSummaries//Week {4}//{2}@{3}.htm",
				Utility.OutputDirectory(), Season, AwayTeam, HomeTeam, Week );
			return summaryFile;
		}

		public string SummaryUrl( string textOut )
		{
			var summaryFile = string.Format( "<a href='..//{1}//GameSummaries//Week {0}//{2}@{3}.htm'>|{4} {5,2} @ {6} {7,2}</a>",
				Week, Season, AwayTeam, HomeTeam, AwayTeam, AwayScore, HomeTeam, HomeScore );
			return summaryFile;
		}

		internal string ProjectionLink( string textOut )
		{
			return string.Format( "<a href='..//projections//gameprojections//Week {0}//{1}@{2}.htm'>{3}</a>",
			   Week, AwayTeam, HomeTeam, textOut );
		}

		internal string GameLink( string textOut )
		{
			if ( IsBye() ) return textOut;
			return Played() ? SummaryUrl( textOut ) : ProjectionLink( textOut );
		}

		internal string PlayerProjectionsHtml()
		{
			var html = HomeTeamPlayerProjections();
			html += AwayTeamPlayerProjections();
			return html;
		}

		private string AwayTeamPlayerProjections()
		{
			if ( AwayNflTeam == null ) AwayNflTeam = GetAwayTeam();
			return PlayerProjectionsHtml( AwayNflTeam );
		}

		private string HomeTeamPlayerProjections()
		{
			if ( HomeNflTeam == null ) HomeNflTeam = GetHomeTeam();
			return PlayerProjectionsHtml( HomeNflTeam );
		}

		private string PlayerProjectionsHtml( NflTeam nflTeam )
		{
			var html = HtmlLib.H4( nflTeam.NameOut() ) + Environment.NewLine;
			html += HtmlLib.TableWithBorderOpen();
			if ( nflTeam.PlayerList.Count == 0 ) nflTeam.LoadPlayerUnits();
			if ( PgmDao == null ) PgmDao = new DbfPlayerGameMetricsDao();
			if ( GameWeek == null ) GameWeek = new NFLWeek( Season, Week );
			var scorer = new YahooProjectionScorer();
			var nPlayers = 0;
			var nTotPts = 0.0M;
			var totPgm = new PlayerGameMetrics();
			foreach ( NFLPlayer p in nflTeam.PlayerList )
			{
				if ( !p.IsFantasyOffence() ) continue;

				nPlayers++;
				var pgm = PgmDao.Get( p.PlayerCode, GameKey() );
				if ( nPlayers == 1 ) html += pgm.PgmHeaderRow();
				if ( !pgm.HasNumbers() ) continue;
				SetProjectedStats( p, pgm );
				var fpts = scorer.RatePlayer( p, GameWeek );
				nTotPts += fpts;
				html += HtmlLib.Para( pgm.FormatAsTableRow( p.PlayerName, p.PlayerRole, fpts ) ) + Environment.NewLine;
				totPgm.ProjYDp += pgm.ProjYDp;
				totPgm.ProjTDp += pgm.ProjTDp;
				totPgm.ProjYDr += pgm.ProjYDr;
				totPgm.ProjTDr += pgm.ProjTDr;
				totPgm.ProjYDc += pgm.ProjYDc;
				totPgm.ProjTDc += pgm.ProjTDc;
				totPgm.ProjFG += pgm.ProjFG;
				totPgm.ProjPat += pgm.ProjPat;
			}
			html += HtmlLib.Para( totPgm.FormatAsTableRow( "Totals", "", nTotPts ) ) + Environment.NewLine;
			html += HtmlLib.TableClose();
			return html;
		}

		private void SetProjectedStats( NFLPlayer p, PlayerGameMetrics pgm )
		{
			p.ProjectedFg = pgm.ProjFG;
			p.ProjectedPat = pgm.ProjPat;
			p.ProjectedReceptions = pgm.ProjRec;
			p.ProjectedTDc = pgm.ProjTDc;
			p.ProjectedTDp = pgm.ProjTDp;
			p.ProjectedTDr = pgm.ProjTDr;
			p.ProjectedYDp = pgm.ProjYDp;
			p.ProjectedYDr = pgm.ProjYDr;
			p.ProjectedYDc = pgm.ProjYDc;
		}

		public PlayerGameMetrics GetPgmFor( string playerCode )
		{
			if ( PgmDao == null ) PgmDao = new DbfPlayerGameMetricsDao();
			return PgmDao.Get( playerCode, GameKey() );
		}

		internal bool IsBye()
		{
			return ( Season.Equals( "XXXX" ) );
		}

		public string PredictedResult()
		{
			return IsBye() ? new string( ' ', 11 ) : GetPrediction( "unit" ).ToString();
		}

		public string TheLine( string teamCodeInFocus )
		{
			return IsBye() ? "BYE" : GetFormattedSpread( teamCodeInFocus );
		}

		public string GameApKey()
		{
			return HomeNflTeam.ApCode;
		}

		public string GamebookUrl()
		{
			var rootUrl = "http://www.nfl.com/liveupdate/gamecenter/";
			return string.Format( "{0}{1}/{2}_Gamebook.pdf", rootUrl, Id, HomeNflTeam.ApCode.Trim() );
		}
	}
}