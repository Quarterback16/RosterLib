using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace RosterLib
{
	/// <summary>
	/// Summary description for NFLPlayer.
	/// </summary>
	public class NFLPlayer : IComparable
	{
		public Logger Logger { get; private set; }

		#region Constants

		public const string K_ROLE_STARTER = "S";
		public const string K_ROLE_BACKUP = "B";
		public const string K_ROLE_RESERVE = "R";
		public const string K_ROLE_INJURED = "I";
		public const string K_ROLE_SUSPENDED = "X";

		public const String K_FANTASY_POSITIONS = "QB,RB,HB,FL,WR,TE,PK,K";
		public const String K_RUSHING_POSITIONS = "RB,HB";

		#endregion Constants

		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string JerseyNo { get; set; }
		public int HeightFeet { get; set; }
		public int HeightInches { get; set; }
		public int Weight { get; set; }
		public string DBirth { get; set; }
		public string College { get; set; }
		public DateTime StartDate { get; set; }
		public int Rating { get; set; }
		public int Adp { get; set; }

		public string PlayerName;
		public string PlayerNameShort;
		public string PlayerCode;
		public string PlayerRole;
		public string PlayerPos;
		public string PlayerCat;
		public string RookieYear;
		//public string howGot;    //  D1, FA  etc

		public string Owner;

		private string _drafted;
		public int Scores;
		public int CurrScores;
		private string _teamLastYear;
		private PlayerPos _myCat;

		#region Projections

		public List<NFLGame> CurrentGames { get; set; }

		#endregion Projections

		public string LoWeek;
		public string HiWeek;

		//  the players list of games played (potentially)
		//  each game is encapsulated as an NFLPerformance object
		public ArrayList PerformanceList;

		public PlayerStats TotStats;

		//  later attempt at capturing Player output
		public Dictionary<String, PlayerGameMetrics> GameMetrics { get; set; }

		public void DumpMetrics()
		{
			if ( GameMetrics != null )
			{
				foreach ( KeyValuePair<string, PlayerGameMetrics> pair in GameMetrics )
				{
					Announce( $"{pair.Key}, {pair.Value}");
					Console.WriteLine( "{0}, {1}", pair.Key, pair.Value );
				}
			}
		}

		public PlayerGameMetrics CurrentGameMetrics { get; set; }

		#region Constructors

		public NFLPlayer( string nameIn, string codeIn, string roleIn, string strRookieYrIn,
						 string strPlayerPos, string strPlayerCat, NflTeam teamIn )
		{
			LastSeason = "";
			TraceIt( "NFLPlayer: Instantiating new Player " + nameIn );
			PlayerName = nameIn;
			PlayerCode = codeIn;
			PlayerRole = roleIn;
			PlayerPos = strPlayerPos.Trim();
			PlayerCat = strPlayerCat;
			RookieYear = strRookieYrIn;

			FillPlayer();
			if ( RookieYear == Utility.CurrentSeason() ) PlayerName += "*";
			Owner = "**";
			LoadOwner();
			PerformanceList = new ArrayList();
			TotStats = new PlayerStats();
			Drafted = Utility.TflWs.Drafted( codeIn );
			_teamLastYear = Utility.TflWs.PlayedFor( PlayerCode, Int32.Parse( Utility.CurrentSeason() ) - 1, 17 );
			CurrTeam = teamIn;
		}

		public NFLPlayer( string nameIn, string codeIn, string roleIn, string strRookieYrIn,
						 string strPlayerPos, string dBirthIn, int scoresIn, int currScoresIn, string strCatIn,
						 NflTeam teamIn )
		{
			LastSeason = "";
			Announce( "2. Instantiating new Player " + nameIn );

			PlayerName = nameIn;
			PlayerCode = codeIn;
			PlayerRole = roleIn;
			PlayerPos = strPlayerPos.Trim();
			PlayerCat = strCatIn;
			RookieYear = strRookieYrIn;
			DBirth = dBirthIn;
			Scores = scoresIn;
			CurrScores = currScoresIn;

			FillPlayer();

			CurrTeam = teamIn;
			if ( RookieYear == Utility.CurrentSeason() ) PlayerName += "*";
			Owner = "**";
			LoadOwner();
			PerformanceList = new ArrayList();
			_teamLastYear = Utility.TflWs.PlayedFor( PlayerCode, Int32.Parse( Utility.CurrentSeason() ) - 1, 17 );
			TotStats = new PlayerStats();
			Drafted = Utility.TflWs.Drafted( codeIn ).Trim();
		}

		public NFLPlayer( DataRow dr, [Optional] string fantasyLeague )
		{
			LastSeason = "";
			LoadPlayer( dr );
			if ( !string.IsNullOrEmpty( fantasyLeague ) )
				LoadOwner( fantasyLeague );
		}

		public NFLPlayer( string playerId )
		{
			LastSeason = "";
			if ( playerId.Trim().Length > 0 )
			{
				PlayerCode = playerId;
				FillPlayer();
				TotStats = new PlayerStats();
				_teamLastYear = Utility.TflWs.PlayedFor( PlayerCode, Int32.Parse( Utility.CurrentSeason() ) - 1, 17 );
				CurrTeam = new NflTeam( TeamCode );
				LoadOwner( Constants.K_LEAGUE_Yahoo );
			}
			else
				Announce( "NFLPlayer: Unable to instantiate " + playerId );
		}

		public NFLPlayer( string playerId, [Optional] string fantasyLeague )
		{
			LastSeason = "";
			if ( playerId.Trim().Length > 0 )
			{
				PlayerCode = playerId;
				FillPlayer();
				TotStats = new PlayerStats();
				LoadOwner( fantasyLeague );
				_teamLastYear = Utility.TflWs.PlayedFor( PlayerCode, Int32.Parse( Utility.CurrentSeason() ) - 1, 17 );
				CurrTeam = new NflTeam( TeamCode );
			}
			else
				Announce( "NFLPlayer: Unable to instantiate " + playerId );
		}

		public NFLPlayer( XmlNode node )
		{
			LastSeason = "";
#if DEBUG
			Announce( "Instantiating new Player via xml" );
#endif
			if ( node.Attributes != null )
			{
				PlayerCode = node.Attributes[ "id" ].Value;
				PlayerName = node.Attributes[ "name" ].Value;
				RookieYear = node.Attributes[ "rookie-year" ].Value;
				PlayerPos = node.Attributes[ "pos" ].Value;
				PlayerRole = node.Attributes[ "role" ].Value;
				StarRating = node.Attributes[ "star" ].Value;
				Bio = node.Attributes[ "bio" ].Value;
				PlayerCat = node.Attributes[ "cat" ].Value;
				DBirth = node.Attributes[ "dob" ].Value;
				Owner = node.Attributes[ "owner" ].Value;
				JerseyNo = node.Attributes[ "jersey" ].Value;
				Injury = node.Attributes[ "INJURY" ].Value;
				var team = node.Attributes[ "currteam" ].Value;
				CurrTeam = Masters.Tm.GetTeam( Utility.CurrentSeason(), team );
			}
			if ( node.Attributes != null )
			{
				Scores = Int32.Parse( node.Attributes[ "Scores" ].Value );
				CurrScores = Int32.Parse( node.Attributes[ "currscores" ].Value );
			}
			if ( RookieYear == Utility.CurrentSeason() ) PlayerName += "*";
#if DEBUG
			Announce( string.Format( "Instantiated {0} via xml", PlayerName ) );
#endif
		}

		public NFLPlayer()
		{
			PlayerCode = string.Empty;
			PlayerName = string.Empty;
			LastSeason = string.Empty;
		}

		#endregion Constructors

		#region Accessors

		public string TeamCode
		{
			get
			{
				if ( CurrTeam == null )
				{
					Announce( string.Format( "NFLPlayer.TeamCode Player {0} {1} has a null CurrTeam", PlayerName,
															 PlayerCode ) );
					return "??";
				}
				return CurrTeam.TeamCode;
			}
		}

		public decimal Points { get; set; }

		public string Bio { get; set; }

		public string Opponent { get; set; }

		public string OppRate { get; set; }

		public string PlayerSpread { get; set; }

		public string LineupPos { get; set; }

		public bool IsRetired { get; set; }

		public string LastSeason { get; set; }

		public string Drafted
		{
			get { return _drafted ?? ( _drafted = Utility.TflWs.Drafted( PlayerCode ) ); }
			set { _drafted = value; }
		}

		public decimal ExperiencePoints { get; set; }

		public bool GamesLoaded { get; set; }

		public bool EpDone { get; set; }

		public NflTeam CurrTeam { get; set; }

		public string TeamLastYear
		{
			get
			{
				return _teamLastYear ??
					   ( _teamLastYear =
						 Utility.TflWs.PlayedFor( PlayerCode, Int32.Parse( Utility.CurrentSeason() ) - 1, 17 ) );
			}
			set { _teamLastYear = value; }
		}

		public string StarRating { get; set; }

		public decimal ProjectedTDp { get; set; }

		public decimal ProjectedTDr { get; set; }

		public decimal ProjectedTDc { get; set; }

		public int ProjectedYDp { get; set; }

		public int ProjectedYDr { get; set; }

		public int ProjectedYDc { get; set; }

		public int ProjectedFg { get; set; }

		public int ProjectedPat { get; set; }

		public int ProjectedReceptions { get; set; }

		private void SetLastSeason()
		{
			LastSeason = Utility.TflWs.Retired( PlayerCode );
			if ( !string.IsNullOrEmpty( LastSeason ) )
				IsRetired = true;
		}

		public string Status()
		{
			string statusOut;

			if ( TeamCode == "??" )
			{
				//  workout if the guy has retired yet
				SetLastSeason();
				IsRetired = ( LastSeason.Length > 0 );
				statusOut = IsRetired ? "Retired" : "Free Agent";
			}
			else
			{
				//  return the Name of the Team
				if ( CurrTeam == null ) CurrTeam = new NflTeam( TeamCode );
				statusOut = CurrTeam.NameOut();
			}
			return statusOut;
		}

		public string ActiveStatus()
		{
			SetLastSeason();
			IsRetired = ( LastSeason.Length > 0 );
			return IsRetired ? "Retired" : "Active";
		}

		public bool IsProbablyRetired( int season )
		{
			bool isProbablyRetired = false;

			// years since rookie yr > 4
			if ( NoOfSeasons() < 5 ) return false;

			int yearsAgo = 0;

			if ( IsKicker() )
			{
				// year of last score (the dont rack up stats) 2+ years ago
				var yearOfLastScore = Utility.TflWs.YearOfLastScore( PlayerCode );
				if ( string.IsNullOrEmpty( yearOfLastScore ) ) return true;
				yearsAgo = season - Int32.Parse( yearOfLastScore );
				if ( yearsAgo > 2 )
					isProbablyRetired = true;
				return isProbablyRetired;
			}
			// year of last stat 2+ years ago
			var yearOfLastStat = Utility.TflWs.YearOfLastStat( PlayerCode );
			if ( string.IsNullOrEmpty( yearOfLastStat ) ) return true;
			yearsAgo = season - Int32.Parse( yearOfLastStat );
			if ( yearsAgo > 2 )
				isProbablyRetired = true;

			return isProbablyRetired;
		}

		public bool Retire()
		{
			if ( IsRetired )
			{
				// do nothing
				return false;
			}

			//  Retire the player
			Announce( $"Retiring {PlayerName}" );
			Utility.TflWs.RetirePlayer( DateTime.Now.AddDays(-2), PlayerCode );
			Utility.TflWs.SetCurrentTeam( PlayerCode, "??" );
			IsRetired = true;

			return true;
		}

		public string Unit()
		{
			var unitOut = "";

			if ( PlayerCat != null )
			{
				if ( PlayerCat == "1" )
					unitOut = "PO";
				else if ( PlayerCat == "2" )
					unitOut = "RO";
				else if ( PlayerCat == "3" )
					unitOut = "PO";
				else if ( PlayerCat == "4" )
					unitOut = "KK";
				else if ( PlayerCat == "5" )
					unitOut = "RD";
				else if ( PlayerCat == "6" )
					unitOut = "PD";
				else if ( PlayerCat == "7" )
					unitOut = "PP";

				//  Pass Rushes are a subset of Category 5
				if ( PlayerPos.IndexOf( "DE", StringComparison.Ordinal ) >= 0 )
					unitOut = "PR";
				else if ( PlayerPos.IndexOf( "DT", StringComparison.Ordinal ) >= 0 )
					unitOut = "PR";
			}
			return unitOut;
		}

		public string UnitRating()
		{
			var myUnitRating = "?";

			switch ( Unit() )
			{
				case "PO":
					myUnitRating = CurrTeam.Ratings.Substring( 0, 1 );
					break;

				case "RO":
					myUnitRating = CurrTeam.Ratings.Substring( 1, 1 );
					break;

				case "PP":
					myUnitRating = CurrTeam.Ratings.Substring( 2, 1 );
					break;

				case "PR":
					myUnitRating = CurrTeam.Ratings.Substring( 3, 1 );
					break;

				case "RD":
					myUnitRating = CurrTeam.Ratings.Substring( 4, 1 );
					break;

				case "PD":
					myUnitRating = CurrTeam.Ratings.Substring( 5, 1 );
					break;
			}

			return myUnitRating;
		}

		public string OpponentRating( string ratings )
		{
			var myOpponentRating = "?";

			if ( ratings.Length.Equals( 6 ) )
			{
				switch ( Unit() )
				{
					case "PO":
						myOpponentRating = ratings.Substring( 5, 1 ); // PD
						break;

					case "RO":
						myOpponentRating = ratings.Substring( 4, 1 ); //  RD
						break;

					case "PP":
						myOpponentRating = ratings.Substring( 3, 1 ); //  PR
						break;

					case "PR":
						myOpponentRating = ratings.Substring( 2, 1 ); // PP
						break;

					case "RD":
						myOpponentRating = ratings.Substring( 1, 1 );
						break;

					case "PD":
						myOpponentRating = ratings.Substring( 0, 1 );
						break;
				}
			}
			else
				Announce( "Cant get opponent ratings for " + PlayerName + ": " + ratings );

			return myOpponentRating;
		}

		public decimal HealthRating()
		{
			//  Health rating is a percentage  Injuries / Seasons
			//  foored at 0.1
			var injuryRating = int.Parse( Injury );
			if ( injuryRating == 0 )
				return 1.0M;

			decimal seasons = ( decimal ) NoOfSeasons();
			decimal hr = injuryRating / seasons;
			hr = ( 1.0M - hr );
			if ( hr == 0.0M ) hr = 0.1M;
			var healthRating = string.Format( "{0:#.00}", hr );
			var hrShort = Decimal.Parse( healthRating );
			return hrShort;
		}

		#endregion Accessors

		public DataSet LastScores( string scoreType, int weekFrom, int weekTo, string season, string id )
		{
#if DEBUG
			//Utility.Announce( string.Format( "Getting last Scores {0} for {1}s season {2} from:{3} to:{4}",
			//   Utility.ScoreTypeOut( scoreType ), PlayerName, season, weekTo, weekFrom ) );
#endif
			var ds = Utility.TflWs.GetScoresForWeeks( scoreType, PlayerCode, season, weekTo, weekFrom, id );

#if DEBUG
			Utility.DumpDataSet( ds );
#endif
			return ds;
		}

		public DataSet LastStats( string statType, int weekFrom, int weekTo, string season )
		{
			//  Get the last 4 weeks of stats for this player
#if DEBUG
			//Utility.Announce( string.Format( "NFLPlayer.LastStats -Getting {2}:{3}-{4} {0} stats for {1}",
			//   Utility.StatTypeOut( statType ), PlayerName, season, weekFrom, weekTo ) );
#endif
			var ds = Utility.TflWs.GetStatsForWeeks( PlayerCode, season, weekTo, weekFrom, statType );
#if DEBUG
			//Utility.DumpDataSet( ds );
#endif
			return ds;
		}

		private void FillPlayer()
		{
			var ds = Utility.TflWs.GetPlayer( PlayerCode );
			var dt = ds.Tables[ "player" ];
            if (dt == null)
            {
                Logger.Error($"Player not found for {PlayerCode}");
                return;
            }
            if ( dt.Rows.Count == 1 )
				LoadPlayer( dt.Rows[ 0 ] );
			else
			{
				if (dt.Rows.Count > 1 )
				{
					Logger.Error( $"Duplicated ID in Player database {PlayerCode}" );
				}
			}
		}

		public override string ToString()
		{
			return PlayerName;
		}

		public string PlayerNameTo( int len )
		{
			var sb = new StringBuilder();
			sb.Append( PlayerNameShort );
			for ( int i = 0; i < len; i++ )
			{
				sb.Append( " " );
			}
			return sb.ToString().Substring( 0, len );
		}

		public bool IsStar()
		{
			if ( StarRating == null )
				return false;

			return StarRating.Trim().Length > 0;
		}

		public void LoadPlayer( DataRow dr )
		{
			if ( dr.RowState == DataRowState.Deleted ) return;

			PlayerCode = dr[ "PLAYERID" ].ToString();
			Surname = dr[ "surname" ].ToString().Trim();
			FirstName = dr[ "firstname" ].ToString().Trim();
			PlayerName = dr[ "firstname" ].ToString().Trim() + " " + dr[ "surname" ].ToString().Trim();
			var firstName = dr[ "firstname" ].ToString().Trim();
			if ( firstName.Length > 0 )
				PlayerNameShort = firstName.Substring( 0, 1 ) + dr[ "surname" ].ToString().Trim();
			else
				PlayerNameShort = dr[ "surname" ].ToString().Trim();

			College = dr[ "college" ].ToString();
			RookieYear = dr[ "rookieyr" ].ToString();
			PlayerPos = dr[ "posdesc" ].ToString().Trim();
			if ( PlayerPos == "K" ) PlayerPos = "PK";
			PlayerRole = dr[ "ROLE" ].ToString();
			PlayerCat = dr[ "CATEGORY" ].ToString();
			Injury = dr[ "INJURY" ].ToString();
			StarRating = dr[ "STAR" ].ToString();
			Bio = FixBio( dr[ "BIO" ].ToString() );
			JerseyNo = dr[ "JERSEY" ].ToString();
			var d = Convert.ToDateTime( dr[ "DOB" ].ToString() );
			DBirth = d.ToShortDateString();
			Scores = Int32.Parse( dr[ "Scores" ].ToString() );
			Rating = Int32.Parse( dr[ "CURRATING" ].ToString() );
			Adp = Int32.Parse( dr[ "PROJECTED" ].ToString() );
			CurrScores = Int32.Parse( dr[ "curscores" ].ToString() );
			CurrTeam = new NflTeam( dr[ "CURRTEAM" ].ToString() );
			if ( RookieYear == Utility.CurrentSeason() ) PlayerName += "*";
			TotStats = new PlayerStats();
			SetLastSeason();
		}

		/// <summary>
		/// Fixes the bio by removing the "ì" characters );.
		/// Replace() does not apear to work
		/// </summary>
		/// <param name="bioIn">The bio stored in the memo file.</param>
		/// <returns>fixed string</returns>
		private string FixBio( string bioIn )
		{
			if ( bioIn.Length > 0 )
			{
				var sb = new StringBuilder();
				foreach ( var c in bioIn )
				{
					int n = c;
					//RosterLib.Utility.Announce( string.Format("Character {0} is {1}", c, n ));
					if ( ( n != 236 ) && ( n != 10 ) )
					{
						sb.Append( c );
					}
				}
				Bio = sb.ToString();
			}
			return Bio;
		}

		public void SetLoWeek( string loWeekIn )
		{
			LoWeek = loWeekIn;
		}

		public void SetHiWeek( string hiWeekIn )
		{
			HiWeek = hiWeekIn;
		}

		public void LoadSeason( string season )
		{
			if ( season == null ) season = Utility.CurrentSeason();

			var ds = Utility.TflWs.GetSeason( CurrTeam.TeamCode, season );
			var dt = ds.Tables[ 0 ];
			if ( CurrentGames == null ) CurrentGames = new List<NFLGame>();
			CurrentGames.Clear();
			foreach ( var game in from DataRow dr in dt.Rows select new NFLGame( dr ) )
			{
				game.LoadPrediction( "unit" );
				CurrentGames.Add( game );
			}
		}

		public void LoadPerformances( bool allGames, bool currSeasonOnly, string whichSeason )
		{
			if ( String.IsNullOrEmpty( PlayerName ) ) return;
			TraceIt( $"NFLPlayer.LoadPerformances allGames={allGames}" );
			if ( GamesLoaded ) return;
			TraceIt( $"NFLPlayer.LoadPerformances GamesLoaded {GamesLoaded}" );
			var nYearToGoTo = allGames ? Int32.Parse( RookieYear ) : Int32.Parse( whichSeason );
			TraceIt( $"NFLPlayer.LoadPerformances Loading from {Utility.CurrentSeason()} to {nYearToGoTo}" );

			for ( var s = Int32.Parse( Utility.CurrentSeason() ); s >= nYearToGoTo; s-- )
			{
				TraceIt( string.Format( "NFLPlayer.LoadPerformances doing Season {0}", s ) );

				for ( var w = Constants.K_WEEKS_IN_A_SEASON; w > 0; w-- )
				{
					// What team did he play for?
					var teamCode = Utility.TflWs.PlayedFor( PlayerCode, s, w );
					if ( !string.IsNullOrEmpty( teamCode ) )
					{
						if ( ( teamCode.Length <= 0 ) || ( teamCode.Equals( "??" ) ) ) continue;

						TraceIt(
						   $"NFLPlayer.LoadPerformances: Season:{s}  Week:{w}  {PlayerName} Played for {teamCode}" );

						if ( PerformanceList == null ) PerformanceList = new ArrayList();
						var perf = new NflPerformance( s, w, teamCode, this );
						PerformanceList.Add( perf );
					}
				}
			}

			var nGames = ( PerformanceList == null ) ? 0 : PerformanceList.Count;

			TraceIt( $"NFLPlayer.LoadPerformances  {PlayerName} had {nGames} games added from {nYearToGoTo} to {Utility.CurrentSeason()}" );

			TallyPerformance( true, currSeasonOnly, whichSeason );

			GamesLoaded = true; // dont want to do this multiple times
		}

		#region Total Performance

		public void TallyPerformance( bool allGames, bool currSeasonOnly, string whichSeason )
		{
			if ( string.IsNullOrEmpty( PlayerName ) ) return;
			TraceIt( string.Format("NFLPlayer.TallyPerformance for {0}", PlayerName));
			if ( PerformanceList == null ) return;
			if ( TotStats == null ) TotStats = new PlayerStats();
			var tally = true;
			TotStats.Zeroise();
			foreach ( NflPerformance p in PerformanceList )
			{
				TraceIt( string.Format( "Doing {0}:{1}", p.Season, p.Week ) );
				if ( currSeasonOnly ) tally = p.Season.ToString( CultureInfo.InvariantCulture ) == whichSeason;
				if ( !tally ) continue;
				TraceIt( string.Format("   Tallying {0}:{1}", p.Season, p.Week));

				TotStats.Tdp += p.PerfStats.Tdp;
				TotStats.Tdr += p.PerfStats.Tdr;
				TotStats.Tdc += p.PerfStats.Tdc;
				TotStats.Fg += p.PerfStats.Fg;
				TotStats.Pat += p.PerfStats.Pat;
				TotStats.PatPass += p.PerfStats.PatPass;
				TotStats.PatCatch += p.PerfStats.PatCatch;
				TotStats.PatRun += p.PerfStats.PatRun;
				TotStats.KickRet += p.PerfStats.KickRet;
				TotStats.FumRet += p.PerfStats.FumRet;
				TotStats.IntRet += p.PerfStats.IntRet;
				TotStats.Safety += p.PerfStats.Safety;
				//  Stats
				TotStats.YDr += p.PerfStats.YDr;
				TotStats.Rushes += p.PerfStats.Rushes;
				TotStats.Completions += p.PerfStats.Completions;
				TotStats.PassAtts += p.PerfStats.PassAtts;
				TotStats.YDp += p.PerfStats.YDp;
				TotStats.PassInt += p.PerfStats.PassInt;
				TotStats.Catches += p.PerfStats.Catches;
				TotStats.YDc += p.PerfStats.YDc;
				TotStats.Ints += p.PerfStats.Ints;
				TotStats.Sacks += p.PerfStats.Sacks;
			}
			TraceIt( string.Format("NFLPlayer.TallyPerformance for {0} done", PlayerName));
		}

		#endregion Total Performance

		/// <summary>
		///  Works out how many seasons the player played for.
		///  Has to factor in the season the player retired.
		/// </summary>
		/// <returns></returns>
		public int NoOfSeasons()
		{
			var nSeasons = 1;
			SetLastSeason();
			var lastYr = IsRetired ? LastSeason : Utility.CurrentSeason();

			if ( RookieYear == null ) return 0;
			if ( string.IsNullOrEmpty( lastYr ) ) return 0;

			try
			{
				nSeasons = Int32.Parse( lastYr ) - Int32.Parse( RookieYear ) + ( IsRetired ? 1 : 0 );
			}
			catch ( FormatException ex )
			{
				Announce( string.Format( "{1} Cant format lastYr={0}", lastYr, ex.Message ) );
			}

			return nSeasons;
		}

		public int DraftRound()
		{
			SetDraftRound();
			var round = Drafted.Substring( 0, 1 ) == "D" ? Int32.Parse( Drafted.Substring( 1, 1 ) ) : 0;
			return round;
		}

		public bool IsInPrime()
		{
			return ( NoOfSeasons() > 3 ) && ( NoOfSeasons() < 9 );
		}

		public string Seasons()
		{
			return string.Format( "({0})", NoOfSeasons() );
		}

		public bool IsRookie()
		{
			//TODO:  Update this
			return ( RookieYear == Utility.CurrentSeason() );
		}

		public bool IsInjured()
		{
			return ( PlayerRole == "I" );
		}

		public bool IsSuspended()
		{
			return ( PlayerRole == "X" );
		}

		public bool IsStarter()
		{
			return ( PlayerRole == Constants.K_ROLE_STARTER );
		}

		public bool IsOneOrTwo()
		{
			return ( IsStarter() || IsBackup() );
		}

		public string RoleOut()
		{
			var roleOut = $"Unknown role {PlayerRole}";

			switch ( PlayerRole )
			{
				case Constants.K_ROLE_STARTER:
					roleOut = "Starter";
					break;

				case Constants.K_ROLE_BACKUP:
					roleOut = "BackUp";
					break;

				case Constants.K_ROLE_RESERVE:
					roleOut = "Reserve";
					break;

				case Constants.K_ROLE_DEEP_RESERVE:
					roleOut = "Deep Reserve";
					break;

				case Constants.K_ROLE_INJURED:
					roleOut = "Injured";
					break;

				case Constants.K_ROLE_SUSPENDED:
					roleOut = "Suspended";
					break;

				case Constants.K_ROLE_HOLDOUT:
					roleOut = "Holdout";
					break;
			}
			return roleOut;
		}

		public bool IsBackup()
		{
			return ( PlayerRole == "B" );
		}

		public bool IsFreeAgent()
		{
			return ( Owner == "**" );
		}

		public bool IsPlayoffBound()
		{
			var isPlayoffBound = CurrTeam.IsPlayoffBound;  //  set in TEAM.DBF
			return isPlayoffBound;
		}

		public bool IsNewbie()
		{
			return ( CurrTeam.TeamCode != TeamLastYear );
		}

		public bool IsActive()
		{
			return ( CurrTeam.TeamCode != "??" );
		}

		public void LoadOwner( [Optional] string fantasyLeague )
		{
			var theLeague = ( string.IsNullOrEmpty( fantasyLeague ) ) ? Utility.CurrentLeague : fantasyLeague;
			Owner = Utility.TflWs.GetStatus( PlayerCode, theLeague, Utility.CurrentSeason() );
		}

		public string LoadAllOwners()
		{
			const string l1 = Constants.K_LEAGUE_Gridstats_NFL1;
			const string l2 = Constants.K_LEAGUE_Yahoo;

			LoadOwner( l2 );
			var o2 = Owner;
			LoadOwner( l1 );
			var o1 = Owner;
			return $"{o1}  {o2}";
		}

		#region Old Grid report

		public string PlayerBox( bool isBold )
		{
			var s = "\n";
			var nameOut = Owner == "**"
							  ? HtmlLib.Centre( HtmlLib.Font( "Verdana", PlayerName, "-1" ) )
							  : HtmlLib.Font( "Verdana", PlayerName + " - " + Owner, "-1" );

			nameOut = $"<a href='..\\..\\..\\players\\{PlayerCode}.htm'>{nameOut}</a>";
			if ( PlayerRole.Equals( "I" ) || PlayerRole.Equals( "H" ) || PlayerRole.Equals( "X" ) )
			{
				nameOut = HtmlLib.Strikeout( nameOut );
				isBold = false;
			}
			if ( isBold ) nameOut = HtmlLib.Bold( nameOut );
			if ( IsItalic() ) nameOut = HtmlLib.Italics( nameOut );

			s += HtmlLib.TableOpen(
			   "BORDER=1 WIDTH=144 CELLSPACING=0 CELLPADDING=0" + " BGCOLOR=" + SetColour( "RED" ) +
			   " BORDERCOLOR=" + SetBorderColour() ) +
				 HtmlLib.TableRowOpen() + "\n" +
				 HtmlLib.TableData( nameOut, SetColour( "RED" ), "ALIGN=CENTER" ) + "\n" +
				 HtmlLib.TableRowClose() + "\n" +
				 HtmlLib.TableClose() + "\n";
			return s;
		}

		private string SetBorderColour()
		{
			string theColour;
			switch ( PlayerRole )
			{
				case "S":
					theColour = "BLACK";
					break;

				case "B":
					theColour = "BLUE";
					break;

				default:
					theColour = "GRAY";
					break;
			}
			return theColour;
		}

		public bool IsFantasyPlayer()
		{
			var isFantasyPlayer = false;
			if ( PlayerPos == null ) return isFantasyPlayer;

			var pos = PlayerPos.Trim().Split( ',' );
			if ( pos.Any( t => Contains( t, K_FANTASY_POSITIONS ) ) )
				isFantasyPlayer = true;
			return isFantasyPlayer;
		}

		public decimal ScoresPerYear()
		{
			if ( NoOfSeasons() <= 0 ) return 0.0M;

			var scoresPerYear = Decimal.Parse( Scores.ToString( CultureInfo.InvariantCulture ) ) / Decimal.Parse( NoOfSeasons().ToString( CultureInfo.InvariantCulture ) );
			return Decimal.Parse( scoresPerYear.ToString( "0.##" ) );
		}

		public string ScoresOut()
		{
			return string.Format( "{0:0.0}-{1}", ScoresPerYear(), Seasons() );
		}

		public string SetColour( string takenColour )
		{
			var theColour = "WHITE";
			if ( IsFantasyPlayer() )
			{
				if ( Owner == "CC" || Owner == "BB" )
					theColour = "YELLOW";
				else
					theColour = Owner == "**" ? "LIME" : takenColour;
			}
			return theColour;
		}

		public bool Contains( string subString, string mainString )
		{
			var nSpot = mainString.IndexOf( subString.Trim(), StringComparison.Ordinal );
			return nSpot != -1;
		}

		public bool IsItalic()
		{
			var bItalic = false;
			if ( PlayerPos != null )
			{
				if ( PlayerPos.IndexOf( "FB", StringComparison.Ordinal ) > -1 )
					bItalic = true;
				else
				{
					if ( PlayerPos.IndexOf( "TE", StringComparison.Ordinal ) > -1 )
						bItalic = true;
					else if ( PlayerPos.IndexOf( "P", StringComparison.Ordinal ) > -1 )
						bItalic = true;
				}
			}
			return bItalic;
		}

		#endregion Old Grid report

		#region Player Reports

		public void DumpHistory()
		{
			var pr = new PlayerReport( this );
			pr.Render();
		}

		public string PlayerReport( [Optional] bool forceIt )
		{
			if ( forceIt )
			{
				var pr = new PlayerReport( PlayerCode );
				pr.Render();
                Announce( $"   Player report for {pr.Player} force rendered." );
            }
            else
			{
				if ( IsPlayerReport() ) return Url( PlayerName );

				var pr = new PlayerReport( PlayerCode );
				pr.Render();
				Announce( $"   Player report for {pr.Player} rendered." );
			}
			return Url( PlayerName );
		}

		public bool IsPlayerReport()
		{
			var reportFileName = PlayerReportFileName();
			var exists = File.Exists( reportFileName );

			if ( exists )
				TraceIt( $"A player report for {PlayerName} already exists" );

			return exists;
		}

        public bool IsPlayerProjection(string season)
        {
            var reportFileName = PlayerProjection(season,render:false);
            var exists = File.Exists( reportFileName );
            return exists;
        }

        public string PlayerReportFileName()
		{
			return $@"{Utility.OutputDirectory()}\players\{PlayerCode}.htm";
		}

		public void TraceIt( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Trace( "   " + message );
		}

		public void Announce( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Info( "   " + message );
		}

		public bool DeletePlayerReport()
		{
			var deleted = false;
			if (IsPlayerReport())
			{
				var fileToDelete = PlayerReportFileName();
				File.Delete( fileToDelete );
				deleted = true;
			}
			return deleted;
		}

		#endregion Player Reports

		public string PlayerProjection( string season, bool render = true )
		{
			var r = new PlayerProjection( 
                PlayerCode, 
                season );
			if (render)
                r.Render();
			return r.FileName();
		}

		public string Url( string text, [Optional] bool forceReport )
		{
			var url = text;
			if ( forceReport ) PlayerReport( forceIt: true );
            if ( IsPlayerReport() )
            {
                var reportFileName = $@"{Utility.OutputDirectory()}\players\{PlayerCode}.htm";
                url = $"<a href =\"file:///{reportFileName}\">{text}</a>";
            }
            else
                url = url.PadRight( 40, ' ' ).Substring(0,40);
			return url;
		}

		public string PlayerRow( bool addAvg )
		{
			TraceIt( "NFLPlayer.PlayerRow() " );
			var nameOut = PlayerName;
			if ( PlayerRole == "S" ) nameOut = HtmlLib.Bold( nameOut );
			if ( IsItalic() ) nameOut = HtmlLib.Italics( nameOut );
			if ( Config.DoPlayerReports() )
				if ( !IsPlayerReport() ) PlayerReport();

			var s = $"{HtmlLib.TableRowOpen( " BGCOLOR=" + SetColour( "PINK" ) )}\n";
			if ( _teamLastYear == null )
				_teamLastYear = Utility.TflWs.PlayedFor( PlayerCode, Int32.Parse( Utility.CurrentSeason() ) - 1, 17 );

			//  Name
			s += HtmlLib.TableData( NewPlayerFlag() + JerseyNo + " " + Url( nameOut ) ) + "\n";
			//  *
			s += HtmlLib.TableData( StarOut() ) + "\n";
			//  Pos
			s += HtmlLib.TableData( PlayerPos + " - " + PlayerRole ) + "\n";
			//  Age
			s += HtmlLib.TableData( Age( DBirth ) + "&nbsp;" + Seasons() ) + "\n";
			if ( Config.ShowEp() )
			{
				ExperiencePoints = Masters.Epm.GetEp( PlayerCode );
				s += HtmlLib.TableData( string.Format( "{0}", Convert.ToInt32( ExperiencePoints ) ) ) + "\n";
			}
			//  Scores
			s += HtmlLib.TableData( string.Format( "{0}-{1}", CurrScores, Scores ) ) + "\n";

			//  Performance stats
			LoadPerformances( Config.AllGames, false, WhichSeason() );
			TallyPerformance( allGames: true, currSeasonOnly: true, whichSeason: WhichSeason() );
			if ( TotStats != null )
			{
				//  Career Stats
				s += HtmlLib.TableData( TotStats.Stat1( PlayerCat, addAvg ) ) + "\n";
				s += HtmlLib.TableData( TotStats.Stat2( PlayerCat ) ) + "\n";
			}
			s += HtmlLib.TableData( Owner ) + "\n";
			s += HtmlLib.TableRowClose() + "\n";
			return s;
		}

		private static string WhichSeason()
		{
			return Int32.Parse( Utility.CurrentWeek() ) < 2 ? Utility.LastSeason() : Utility.CurrentSeason();
		}

		private string StarOut()
		{
			var star = HtmlLib.Spaces( 1 );
			if ( IsStar() ) star = HtmlLib.FixedImage( Utility.OutputDirectory(), "star.jpg" );
			return star;
		}

		public string PlayerHeaderRow( string perfCol1, string perfCol2 )
		{
			if ( _myCat == null )
			{
				var cf = new CategoryFactory();
				_myCat = cf.CreatePos( PlayerCat, this );
			}

			var s = string.Format( "{0}\n", HtmlLib.TableRowOpen() );
			s += HtmlLib.TableHeader( "Name" ) + "\n";
			s += HtmlLib.TableHeader( "*" ) + "\n";
			s += HtmlLib.TableHeader( "Pos" ) + "\n";
			s += HtmlLib.TableHeader( "Age" ) + "\n";
			if ( Config.ShowEp() )
				s += HtmlLib.TableHeader( "EP" ) + "\n";
			s += HtmlLib.TableHeader( "Scores" ) + "\n";
			if ( _myCat.Category != null )
			{
				if ( _myCat.Category.Trim().Length > 0 )
				{
					s += HtmlLib.TableHeader( _myCat.PerfCol1() ) + "\n";
					s += HtmlLib.TableHeader( _myCat.PerfCol2() ) + "\n";
					s += HtmlLib.TableHeader( "FT" ) + "\n";
					s += HtmlLib.TableRowClose() + "\n";
				}
			}
			return s;
		}

		public string CatRow( string cat )
		{
			var s = string.Format( "{0}\n", HtmlLib.TableRowOpen( " BGCOLOR=WHITE" ) );
			s += HtmlLib.TableData( Utility.CategoryOut( cat ), "WHITE", "COLSPAN='8' ALIGN='CENTER'" );
			s += HtmlLib.TableRowClose() + "\n";
			return s;
		}

		public string PlayerOut()
		{
			return Url( JerseyNo + " " + PlayerName );
		}

		public string TeamRating( string season )
		{
			return Utility.TflWs.GetRatingsFor( TeamCode, season );
		}

		private string NewPlayerFlag()
		{
			return _teamLastYear == CurrTeam.TeamCode ? "&nbsp;" : ">";
		}

		public string PlayerAge()
		{
			var ageOut = Age( DBirth );
			if ( ageOut == "??" )
				ageOut = string.Format( "{0}", 22 + NoOfSeasons() );
			return ageOut;
		}

		private string Age( string dob )
		{
			var age = string.Empty;

			if ( !string.IsNullOrEmpty( PlayerName ) )
			{
				TraceIt( string.Format( "{0} was born on {1}", PlayerName, dob ) );
				if ( ( dob == "30/12/1899" ) || ( dob == null ) )
					age = string.Format( "{0}?", NoOfSeasons() + 23 );
				else
				{
					var ts = DateTime.Now - Convert.ToDateTime( dob );
					var nAge = ( ts.Days / 365 );
					age = string.Format( "{0} ", nAge );
				}
			}
			return age;
		}

		/// <summary>
		/// Calculates the EP for the player based on the unit performance.
		/// </summary>
		public void CalculateEp( string season )
		{
			if ( EpDone )
				Announce( $" EP for {PlayerName} calculated as {ExperiencePoints}" );
			else
			{
				TraceIt( $"Calculating EP for {PlayerName}" );

				if ( PerformanceList == null ) LoadPerformances( Config.AllGames, false, season );
				ExperiencePoints = 0;
				if ( PerformanceList != null )
					foreach ( NflPerformance g in PerformanceList )
						if ( g.Game != null )
						{
							var theGame = Masters.Gm.GetGame( g.Game.GameKey() );
							if ( theGame == null )
							{
								g.Game.TallyMetrics( String.Empty );
								Masters.Gm.AddGame( g.Game );
							}
							else
								g.Game = theGame;
							var ep = g.Game.ExperiencePoints( this, g.TeamCode );

							if ( ep > 0 )
								TraceIt( string.Format( "  {2} got {0} EP for {1}",
								   ep, g.Game.GameName(), PlayerNameShort ) );

							ExperiencePoints += ep;
							//  add to the teams count too
							CurrTeam.ExperiencePoints += ep;
							//  record on the performance too
							g.ExperiencePoints = ep;
						}
				EpDone = true;
			}
		}

		#region Player Questions?

		public bool IsOffence()
		{
			return ( PlayerCat == "1" ) || ( PlayerCat == "2" ) || ( PlayerCat == "3" ) || ( PlayerCat == "4" ) ||
				   ( PlayerCat == "7" );
		}

		public bool IsFantasyOffence()
		{
			return ( PlayerCat == "1" ) || ( PlayerCat == "2" ) || ( PlayerCat == "3" ) || ( PlayerCat == "4" );
		}

		public bool IsDefence()
		{
			return ( PlayerCat == Constants.K_LINEMAN_CAT ) || ( PlayerCat == Constants.K_DEFENSIVEBACK_CAT );
		}

		public bool IsKicker()
		{
			return PlayerCat == Constants.K_KICKER_CAT;
		}

		public bool IsTe()
		{
			return ( PlayerCat == Constants.K_RECEIVER_CAT ) && Contains( "TE", PlayerPos );
		}

		public bool IsQb()
		{
			return ( PlayerCat == Constants.K_QUARTERBACK_CAT ) && Contains( "QB", PlayerPos );
		}

		public bool IsRb()
		{
			return ( PlayerCat == Constants.K_RUNNINGBACK_CAT );
		}

		public bool IsReturner()
		{
			return Contains( "PR", PlayerPos ) || Contains( "KR", PlayerPos );
		}

		public bool HasSpecRole( string specRole )
		{
			return Contains( specRole, PlayerPos );
		}

		public bool IsRusher()
		{
			var pos = PlayerPos.Trim().Split( ',' );
			return pos.Any( t => Contains( t, K_RUSHING_POSITIONS ) );
		}

		public bool IsAce()
		{
			var isAce = false;
			if ( CurrTeam == null ) CurrTeam = new NflTeam( TeamCode );
			if ( CurrTeam.RunUnit == null ) CurrTeam.LoadRushUnit();
			if ( CurrTeam.RunUnit.AceBack == this )
				isAce = true;
			return isAce;
		}

		/// <summary>
		///  Player is part of a 2-headed monster rotation
		/// </summary>
		/// <returns></returns>
		public bool IsTandemBack()
		{
			var isTandemBack = false;
			if ( CurrTeam == null ) CurrTeam = new NflTeam( TeamCode );
			if ( CurrTeam.RunUnit == null ) CurrTeam.LoadRushUnit();
			if ( CurrTeam.RunUnit.TandemBack( this ) )
				isTandemBack = true;
			return isTandemBack;
		}

		public bool IsInUnit( string unitCode )
		{
			return ( Unit() == unitCode );
		}

		public bool IsAtHome( NFLWeek week )
		{
			var atHome = false;

			var game = week.GameFor( TeamCode );

			if ( game == null )
				Announce( string.Format( "  no game found for {0} in week {1}", TeamCode, week.WeekKey() ) );
			else
				atHome = game.HomeTeam.Equals( TeamCode );

			//if ( atHome )
			//   RosterLib.Utility.Announce( string.Format( "  {0} is at home vs {1}", PlayerNameShort, game.AwayNflTeam.NameOut() ) );

			return atHome;
		}

		#endregion Player Questions?

		/// <summary>
		/// Determines the value of the player depending on a few variables.
		/// Used by the FA Market Analysis.
		/// D1 * 3, D2 * 2
		/// </summary>
		/// <returns>integer no of points</returns>
		public int Value()
		{
			var pts = 1;
			if ( ( PlayerCat == null ) || ( IsRetired ) )
				pts = 0;
			else
			{
				if ( IsStarter() )
					pts = 4;
				else if ( IsBackup() )
					pts = 2;

				// modify for category
				if ( PlayerCat == "1" ) pts *= 4;
				if ( PlayerPos.Trim().Equals( "P" ) ) pts /= 2;
				if ( PlayerCat == "2" ) pts *= 3;
				if ( PlayerCat == "3" ) pts *= 2;
				if ( PlayerCat == "4" ) pts *= 1;
				if ( PlayerCat == "5" ) pts *= 3;
				if ( PlayerCat == "6" ) pts *= 3;
				if ( PlayerCat == "7" ) pts *= 3;

				//  bonus for being mature
				if ( IsInPrime() ) pts *= 2;

				//  Rookie values
				if ( IsRookie() )
				{
					//  What round?
					SetDraftRound();

					switch ( Drafted )
					{
						case "D1":
							pts *= 3;
							break;

						case "D2":
							pts *= 2;
							break;
					}
				}
			}

			return pts;
		}

		public void SetDraftRound()
		{
			if ( Drafted == null )
				Drafted = Utility.TflWs.Drafted( PlayerCode ).Substring( 0, 2 );
			else
			{
				if ( Drafted.Length == 0 )
					Drafted = Utility.TflWs.Drafted( PlayerCode ).Substring( 0, 2 );
			}
		}

		public string PlayerHeader()
		{
			return $"{HtmlLib.TableOpen( "cellpadding='0' cellspacing='0'" )}{PlayerRow( false )}{HtmlLib.TableClose()}\n";
		}

		public string PlayerDiv()
		{
			var s = "";
			if ( Config.ShowEp() ) return s; //  dont show games AND EP

			var pCount = 1;
			s = HtmlLib.DivOpen( "class='he5i'" ) + "\n"
				+ HtmlLib.TableOpen( "class='info' cellpadding='0' cellspacing='0' border='0'" )
				+ "\n";
			if ( Config.ShowPerformance )
			{
				if ( PerformanceList == null ) LoadPerformances( Config.AllGames, true, Utility.CurrentSeason() );
				if ( PerformanceList != null )
				{
					foreach ( NflPerformance p in PerformanceList )
					{
						if ( pCount == 1 )
						{
							s += p.PerfHeaders();
							pCount++;
						}
						s += p.PerfRow();
					}
				}
			}
			s += HtmlLib.TableClose() + "\n" + HtmlLib.DivClose();
			return s;
		}

		public void ProjectNextWeek()
		{
			if ( !IsStarter() ) return;
			if ( _myCat == null )
			{
				if ( !string.IsNullOrEmpty( PlayerCat ) )
				{
					var cf = new CategoryFactory();
					_myCat = cf.CreatePos( PlayerCat, this );
				}
			}
			if ( ( _myCat != null ) && _myCat.Category.Trim().Length > 1 )
				_myCat.ProjectNextWeek( this );
		}

		/// <summary>
		///    Stores a projected metric on the players record.
		/// </summary>
		/// <param name="nProjected"></param>
		public void StoreProjection( int nProjected )
		{
			Utility.TflWs.StoreProjection( nProjected, PlayerCode );
		}

		public void Save()
		{
			if ( Utility.TflWs.PlayerExists( FirstName, Surname, College ) )
				Utility.Announce(
				   string.Format( "No Save: Player {0}, {1}, out of {2} exists", FirstName, Surname, College ) );
			else
			{
				var nextId = Utility.TflWs.NextId( FirstName, Surname );

				if ( !string.IsNullOrEmpty( nextId ) )
				{
					PlayerPos = ValidatePosDesc( PlayerPos );
					PlayerCat = Utility.CategoryFor( PlayerPos );

					Utility.TflWs.StorePlayer( nextId, FirstName, Surname, TeamLastYear, Constants.K_ROLE_STARTER,
											  HeightFeet, HeightInches,
											  Weight, College, RookieYear, PlayerPos, PlayerCat, DBirth );
				}
			}
		}

		public string ValidatePosDesc( string candidatePos )
		{
			var pos = candidatePos.Trim();
			const string validPositions = "C,CD,S,DB,DE,DL,NT,DT,LB,P,WR,FB,HB,K,FS,PK,RB,G,OT,T,HB,ILB,OLB,LDE,RDE,LDT,RDT,LE,LG,LILB,RILB,LOLB,LT,MLB,MIKE,MILB,OG,OL,QB,RCB,LCB,RDT,RE,RG,RILB,RLB,ROLB,S,SS,TE,WILB,WILL,WLB,WR,";

			if ( validPositions.IndexOf( pos, StringComparison.Ordinal ) < 0 )
				pos = "  ";
			return pos;
		}

		public decimal PointsForWeek( NFLWeek week, IRatePlayers rater, bool savePoints = true )
		{
			return rater.RatePlayer( this, week );
		}

		public string Injuries()
		{
			var nInjuries = 0;
			if ( !string.IsNullOrEmpty( Injury ) )
			{
				nInjuries = Int32.Parse( Injury );
				if ( IsInjured() )
					nInjuries++;
			}
			return nInjuries.ToString( CultureInfo.InvariantCulture );
		}

		public string Injury { get; set; }

		public string GameFor( string season, int week )
		{
			return "v TT Mon";
		}

		public string GameKeyFor( string season, string week )
		{
			var dr = Utility.TflWs.GetGame( season, week, CurrTeam.TeamCode );
			var gameKey = string.Empty;
			if ( dr != null )
			{
				var g = new NFLGame( dr );
				gameKey = g.GameKey();
			}
			return gameKey;
		}

		public string ProjectionLink( string season )
		{
			var url = $"<a href =\"..//..//PlayerProjections/{PlayerCode}.htm\">{PlayerName}</a>";
			return url;
		}

		public string ProjectionLink( string season, int padding )
		{
			var url = $"<a href =\"..//..//PlayerProjections/{PlayerCode}.htm\">{PlayerName.PadRight( padding )}</a>";
			return url;
		}

		public string ProjectionLink( int padding )
		{
			var season = Utility.CurrSeason;
			return ProjectionLink( season.ToString( CultureInfo.InvariantCulture ), padding );
		}

		public string ProjectionLink()
		{
			var season = Utility.CurrSeason;
			return ProjectionLink( season.ToString( CultureInfo.InvariantCulture ) );
		}

		public bool IsShortYardageBack()
		{
			var npoint = PlayerPos.IndexOf( "SH", StringComparison.Ordinal );
			return ( npoint > -1 );
		}

		public bool IsThirdDownBack()
		{
			var npoint = PlayerPos.IndexOf( "3D", StringComparison.Ordinal );
			return ( npoint > -1 );
		}

		internal bool IsFullback()
		{
			var npoint = PlayerPos.IndexOf( "FB", StringComparison.Ordinal );
			return ( npoint > -1 );
		}

		internal bool IsQuarterback()
		{
			var npoint = PlayerPos.IndexOf( "QB", StringComparison.Ordinal );
			return ( npoint > -1 );
		}

		internal bool HasPos( string pos )
		{
			var npoint = PlayerPos.IndexOf( pos, StringComparison.Ordinal );
			return ( npoint > -1 );
		}

		public void TallyProjections( bool allGames, bool currSeasonOnly, string whichSeason )
		{
			if ( !string.IsNullOrEmpty( PlayerName ) )
			{
				TraceIt( $"NFLPlayer.TallyProjections for {PlayerName}" );

				//TODO: implement
			}

			TraceIt( string.Format( "NFLPlayer.TallyPerformance for {0} done", PlayerName ) );
		}

		public void LoadProjections( PlayerGameMetrics pgm )
		{
			ProjectedFg = pgm.ProjFG;
			ProjectedPat = pgm.ProjPat;
			ProjectedTDc = pgm.ProjTDc;
			ProjectedTDp = pgm.ProjTDp;
			ProjectedTDr = pgm.ProjTDr;
			ProjectedYDc = pgm.ProjYDc;
			ProjectedYDp = pgm.ProjYDp;
			ProjectedYDr = pgm.ProjYDr;
		}

		public int Compare( object x, object y )
		{
			var player1 = ( NFLPlayer ) x;
			var player2 = ( NFLPlayer ) y;
			return player1.TotStats.Rushes > player2.TotStats.Rushes ? 1 : 0;
		}

		public int CompareTo( object obj )
		{
			var player2 = ( NFLPlayer ) obj;
			return TotStats.Rushes > player2.TotStats.Rushes ? 1 : 0;
		}

		internal bool IsWideout()
		{
			return PlayerPos.Contains( "WR" );
		}

		internal bool IsTightEnd()
		{
			return PlayerPos.Contains( "TE" );
		}

		internal object FantasyPoints( YahooCalculator scorer )
		{
			throw new NotImplementedException();
		}

		public string DetailLine()
		{
			var nextGame = CurrTeam.NextGameOrBye();

			//  This line of code will drop bye players off - actually would like to keep
			//if (nextGame == null)
			//   return string.Empty;

			var nextOpponentTeam = NextOpponentTeam( nextGame );
			var defensiveRating = nextOpponentTeam.DefensiveRating( PlayerCat );
			var c = new YahooCalculator();
			c.Calculate( this, nextGame );

			const string formatStr =
			   "{0,2} {1,-25} {8} {9,5:0.00} {2} {3} {4,-12} {5}   {6,2:#0}  {7,5:##0.0}%  {18} {10} {11}  {12}  {13} {14,3}  {15}  ({16,2}) {17}";
			if ( nextGame == null ) return string.Empty;

			var detailLine = string.Format( formatStr,
				JerseyNo,
				ProjectionLink( nextGame.Season, 25 ).PadRight( 25 ),
				CurrTeam.TeamCode,
				PlayerRole,
				PlayerPos.PadRight( 12 ),
				LoadAllOwners(),
				TotStats.Touches,
				TotStats.TouchLoad,
				PlayerAge(),
				ScoresPerYear(),
				AussieDate( nextGame ),
				AussieHour( nextGame ),
				NextOpponentOut( nextGame ),
				defensiveRating,
				NextGameSpread( nextGame ),
				NextResult( nextGame ),
				Points,
				FantasyAdvice( defensiveRating, nextGame ),
				nextGame.Week
				);
			return detailLine;
		}

		public NflTeam NextOpponentTeam( NFLGame nextGame )
		{
			var nextOpponentTeam = ( nextGame == null ) ? new NflTeam( "BYE" ) : nextGame.OpponentTeam( CurrTeam.TeamCode );
			return nextOpponentTeam;
		}

		public bool NextGameIsBye()
		{
			var nextGame = CurrTeam.NextGameOrBye();
			return nextGame.IsBye();
		}

		public string NextResult( NFLGame nextGame )
		{
			return nextGame.IsBye() ? new string( ' ', 11 ) : nextGame.GetPrediction( "unit" ).ToString();
		}

		public string NextGameSpread( NFLGame nextGame )
		{
			return nextGame.IsBye() ? "BYE" : nextGame.GetFormattedSpread();
		}

		public string NextOpponentOut( NFLGame nextGame )
		{
			return nextGame.IsBye() ? "XXX" : nextGame.OpponentOut( CurrTeam.TeamCode );
		}

		private static string AussieHour( NFLGame nextGame )
		{
			return nextGame.IsBye() ? "XX:XX" : nextGame.AussieHour( showTv: false );
		}

		private static string AussieDate( NFLGame nextGame )
		{
			return nextGame.IsBye() ? new string( ' ', 10 ) : string.Format( "{0:ddd dd MMM}", nextGame.AussieDateTime() );
		}

		private string FantasyAdvice( string defensiveRating, NFLGame nextGame )
		{
			if ( nextGame.IsBye() ) return "     ";
			var predictedResult = nextGame.GetPrediction( "unit" );
			var advice = "     ";
			if ( defensiveRating == "A" || defensiveRating == "B" )
				return advice;
			if ( PlayerCat.Equals( Constants.K_RUNNINGBACK_CAT ) )
			{
				if ( predictedResult.WinningTeam() != CurrTeam.TeamCode )
					return advice;
			}
			advice = "START";
			return advice;
		}

		public void AddMetric( string metricType, string gameKey, decimal nScores )
		{
			var qty = decimal.Parse( nScores.ToString( CultureInfo.InvariantCulture ) );
			AddMetric( metricType, gameKey, qty, ( int ) Points );
		}

		public void AddMetric( string metricType, string gameKey, decimal nScores, int fp )
		{
			if ( GameMetrics == null )
				GameMetrics = new Dictionary<string, PlayerGameMetrics>();
			var pgm = GameMetrics.ContainsKey( gameKey )
									   ? GameMetrics[ gameKey ] :
									   new PlayerGameMetrics
									   {
										   GameKey = gameKey,
										   PlayerId = PlayerCode
									   };

			if ( nScores == 0 ) return;

			if ( metricType.Equals( Constants.K_SCORE_FIELD_GOAL ) )
				pgm.FG += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_SCORE_PAT ) )
				pgm.Pat += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_SCORE_TD_PASS ) )
				pgm.TDp += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_SCORE_PAT_PASS ) )
				pgm.Pat += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_STATCODE_PASSING_YARDS ) )
				pgm.YDp += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_STATCODE_INTERCEPTIONS_THROWN ) )
				pgm.Pat += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_STATCODE_RECEPTION_YARDS ) )
				pgm.YDc += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_SCORE_TD_RUN ) )
				pgm.TDr += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_SCORE_TD_CATCH ) )
				pgm.TDc += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_SCORE_PAT_RUN ) )
				pgm.Pat += ( int ) nScores;
			else if ( metricType.Equals( Constants.K_STATCODE_RUSHING_YARDS ) )
				pgm.YDr = ( int ) nScores;

			pgm.FantasyPoints = fp;
			GameMetrics[ gameKey ] = pgm;
		}

		public string MetricsOut( string gameKey )
		{
			if ( GameMetrics == null ) return string.Empty;

			return GameMetrics.ContainsKey( gameKey )
			   ? GameMetrics[ gameKey ].ToString()
			   : string.Format( "GameKey: {0} not found", gameKey );
		}

		public void ConsoleOut()
		{
			Console.WriteLine( "PlayerId:     {0}", PlayerCode );
			Console.WriteLine( "Name:         {0}", PlayerName );
			Console.WriteLine( "Age:          {0}", PlayerAge() );
			Console.WriteLine( "Current:      {0}", CurrTeam );
			Console.WriteLine( "Pos:          {0}", PlayerPos );
			Console.WriteLine( "College:      {0}", College );
			Console.WriteLine( "Seasons:      {0}", NoOfSeasons() );
			Console.WriteLine( "Status:       {0}", ActiveStatus() );
			Console.WriteLine( "Role:         {0}", RoleOut() );
		}

		public void TallyScores( string season, int weekNo )
		{
			if ( CurrentGameMetrics == null ) CurrentGameMetrics = new PlayerGameMetrics();

			var scoreDs = Utility.TflWs.GetTDpForWeek( PlayerCode, season, weekNo );
			CurrentGameMetrics.TDp = ScoreCount( scoreDs, Constants.K_SCORE_TD_PASS );

			scoreDs = Utility.TflWs.GetTDcForWeek( PlayerCode, season, weekNo );
			CurrentGameMetrics.TDc = ScoreCount( scoreDs, Constants.K_SCORE_TD_PASS );

			scoreDs = Utility.TflWs.GetScoresForWeek( Constants.K_SCORE_TD_RUN, PlayerCode, season, weekNo );
			CurrentGameMetrics.TDr = ScoreCount( scoreDs, Constants.K_SCORE_TD_RUN );

			scoreDs = Utility.TflWs.GetScoresForWeek( Constants.K_SCORE_FIELD_GOAL, PlayerCode, season, weekNo );
			CurrentGameMetrics.FG = ScoreCount( scoreDs, Constants.K_SCORE_FIELD_GOAL );

			scoreDs = Utility.TflWs.GetScoresForWeek( Constants.K_SCORE_PAT, PlayerCode, season, weekNo );
			CurrentGameMetrics.Pat = ScoreCount( scoreDs, Constants.K_SCORE_PAT );
		}

		private static int ScoreCount( DataSet ds, string scoreType )
		{
			var scoreCount = 0;
			var dt = ds.Tables[ 0 ];
			foreach ( DataRow dr in dt.Rows )
			{
				var st = dr[ "SCORE" ].ToString();
				if ( st.Equals( scoreType ) ) scoreCount++;
			}
			return scoreCount;
		}

		public void TallyStats( string season, int weekNo )
		{
			if ( CurrentGameMetrics == null ) CurrentGameMetrics = new PlayerGameMetrics();

			var statDs = Utility.TflWs.GetStatsForWeeks( PlayerCode, season, weekNo, weekNo, Constants.K_STATCODE_PASSING_YARDS );
			CurrentGameMetrics.YDp = StatCount( statDs );

			statDs = Utility.TflWs.GetStatsForWeeks( PlayerCode, season, weekNo, weekNo, Constants.K_STATCODE_RUSHING_YARDS );
			CurrentGameMetrics.YDr = StatCount( statDs );

			statDs = Utility.TflWs.GetStatsForWeeks( PlayerCode, season, weekNo, weekNo, Constants.K_STATCODE_RECEPTION_YARDS );
			CurrentGameMetrics.YDc = StatCount( statDs );
		}

		private static int StatCount( DataSet ds )
		{
			var statCount = 0.0M;
			var dt = ds.Tables[ 0 ];
			foreach ( DataRow dr in dt.Rows )
			{
				var qty = Decimal.Parse( dr[ "QTY" ].ToString() );
				statCount += qty;
			}
			return Convert.ToInt32( statCount );
		}

		public string ActualStats()
		{
			if ( CurrentGameMetrics == null ) SetCurrentGame();
			return CurrentGameMetrics == null ? string.Empty : CurrentGameMetrics.ActualStatsOut( PlayerCat );
		}

		public void SetCurrentGame()
		{
			if ( GameMetrics == null ) return;
			foreach ( KeyValuePair<string, PlayerGameMetrics> pair in GameMetrics )
			{
				CurrentGameMetrics = pair.Value;
				break;
			}
		}

		/// <summary>
		///   Get the stats from the PGM record
		/// </summary>
		/// <param name="g"></param>
		/// <returns></returns>
		public string ActualStatsFor( NFLGame g )
		{
			var dao = new DbfPlayerGameMetricsDao();
			var pgm = dao.Get( PlayerCode, g.GameKey() );
			var stats = string.Empty;
			switch ( PlayerCat )
			{
				case Constants.K_QUARTERBACK_CAT:
					stats = string.Format( "{0} ({1})", pgm.YDp, pgm.TDp );
					break;

				case Constants.K_RUNNINGBACK_CAT:
					stats = string.Format( "{0}({1})", pgm.YDr, pgm.TDr );
					break;

				case Constants.K_RECEIVER_CAT:
					stats = string.Format( "{0}({1})", pgm.YDc, pgm.TDc );
					break;

				case Constants.K_KICKER_CAT:
					stats = string.Format( "{0}({1})", pgm.FG, pgm.Pat );
					break;
			}
			return stats;
		}

		public decimal ActualFpts( NFLGame g )
		{
			var dao = new DbfPlayerGameMetricsDao();
			var pgm = dao.Get( PlayerCode, g.GameKey() );
			CurrentGameMetrics = pgm;
			return pgm.FantasyPoints;
		}

		public void UpdateActuals( IPlayerGameMetricsDao dao )
		{
			if ( CurrentGameMetrics == null ) return;
			CurrentGameMetrics.FantasyPoints = Points;
			CurrentGameMetrics.UpdateAcuals( dao );
		}

		public decimal HealthFactor()
		{
			var effectiveness = 1.0M;
			Int32.TryParse( Injuries(), out int nInjury );
			if ( nInjury > 0 )
			{
				var injChance = ( ( nInjury * 10.0M ) / 100.0M );
				effectiveness = 1 - injChance;
			}
			return effectiveness;
		}
	}
}