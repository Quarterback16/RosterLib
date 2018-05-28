using System;
using System.Data;
using System.Collections;
using System.Linq;
using TFLLib;
using NLog;
using System.Runtime.InteropServices; //  for [Optional]
using System.Collections.Generic;

namespace RosterLib
{
	/// <summary>
	///  Represents a week in the NFL.
	///  Used by :
	///      Roster Grid - To spit out Next Weeks matchups
	///                  - To spit out the best propositions for the week
	/// </summary>
	public class NFLWeek
	{
		protected readonly DataLibrarian TflWs;
		private DataSet _sched;
		public ArrayList _gameList;

      #region  Accessors

      public Logger Logger { get; set; }

      public string Week { get; set; }

		public int WeekNo
		{
			get { return Int32.Parse(Week); }
		}

		public string ZeroWeek()
		{
			return string.Format("{0:0#}", WeekNo);
		}

		public string Season { get; set; }

		public int SeasonNo
		{
			get { return Int32.Parse(Season); }
		}

		public NFLGambler Kenny { get; set; }

		/// <summary>
		///   A week has passed when all the games have been played
		/// </summary>
		/// <returns></returns>
		public bool HasPassed()
		{
			var gl = GameList();
			return gl.Count != 0 && gl.Cast<NFLGame>().All(g => g.Played());
		}

		public string WeekKey([Optional] string separator)
		{
			if (separator == null) separator = ":";
			return $"{Season}{separator}{WeekNo:00}";
		}

		public override string ToString()
		{
			return WeekKey("-");
		}

		#endregion

//		private Outlook.Application oApp;

		#region  Constructors

		public NFLWeek(string seasonIn, int weekIn)
		{
			Season = seasonIn;
			Week = string.Format("{0:0#}", weekIn);
			_sched = Utility.TflWs.GetGames( int.Parse(seasonIn), weekIn);
		}

      public NFLWeek( string seasonIn, int weekIn, bool loadGames )
      {
         Season = seasonIn;
         Week = string.Format( "{0:0#}", weekIn );
         if ( loadGames )
            _sched = Utility.TflWs.GetGames( int.Parse( seasonIn ), weekIn );
      }

      public NFLWeek(string seasonIn, string weekIn)
		{
			Season = seasonIn;
			Week = weekIn;
			_sched = Utility.TflWs.GetGames(Int32.Parse(seasonIn), Int32.Parse(weekIn));
#if DEBUG
			//Utility.Announce($"NFLWeek.Init {weekIn} has {_sched.Tables[ 0 ].Rows.Count} games" );
#endif
		}

		public NFLWeek(int seasonIn, int weekIn)
		{
			TflWs = Utility.TflWs;
			Season = seasonIn.ToString();
			_sched = LoadSchedule(seasonIn, weekIn);
#if DEBUG
			//Utility.Announce($"NFLWeek:Constructor Week {weekIn}:{Season} has {_sched.Tables[ 0 ].Rows.Count} games");
#endif
			Week = weekIn.ToString();
		}

		public NFLWeek(int seasonIn, int weekIn, bool loadGames)
		{
			Season = seasonIn.ToString();
			Week = weekIn.ToString();
			if (loadGames)
			{
				TflWs = Utility.TflWs;
				_sched = LoadSchedule(seasonIn, weekIn);
#if DEBUG
				//Utility.Announce( $"NFLWeek:Constructor Week {weekIn}:{Season} has {_sched.Tables[ 0 ].Rows.Count} games" );
#endif
			}
		}

		#endregion

		public DataSet LoadSchedule(int season, int week)
		{
			return Utility.TflWs.GetGames(season, week);
		}


		#region  Tipping Stuff

		public int MyTipsCorrect()
		{
			return GameList().Cast<NFLGame>().Count(g => g.TippedByMe());
		}

		public int MyAtsCorrect()
		{
			return GameList().Cast<NFLGame>().Count(g => g.MeAts());
		}

		public int SpreadTotalCorrect()
		{
			return GameList().Cast<NFLGame>().Count(g => ! g.Upset());
		}

		public int TotalHomeWins()
		{
			return GameList().Cast<NFLGame>().Count(g => g.HomeWin());
		}

		#endregion

		#region  Gambling stuff

		public void RenderBestBets()
		{
			Kenny = new NFLGambler(750.00D);
			var playList = Kenny.Consider(this);
			Kenny.RenderBets(playList, true, true);
		}

		public ArrayList GameList(int season, int week)
		{
			//  express the scedule as a collection of games
			if (_gameList == null) LoadGameList(season, week);
			return _gameList;
		}

		public ArrayList GameList()
		{
			//  express the schedule as a collection of games
			if (_gameList == null) LoadGameList();
			return _gameList;
		}

		public List<NFLGame> GamesList()
		{
			var gamesList = new List<NFLGame>();
			var aGames = GameList();
			foreach ( NFLGame game in aGames )
			{
				gamesList.Add( game );
			}
			return gamesList;
		}

		private void LoadGameList(int season, int week)
		{
			//RosterLib.Utility.Announce(string.Format("LoadGameList: Loading Teams"));

			if (_sched == null)
				_sched = LoadSchedule(season, week);

			if (_sched != null)
			{
				_gameList = new ArrayList();
				var dt = _sched.Tables[0];
				foreach (DataRow dr in dt.Rows)
				{
					//var gameCode = string.Format("{0}:{1}-{2}", Season, dr["WEEK"], dr["GAMENO"]);
					//RosterLib.Utility.Announce(string.Format("LoadGameList: getting Game:{0}", gameCode ));

					//NFLGame g = Masters.Gm.GetGame( gameCode ); 
					var g = new NFLGame(dr);
					//Masters.Gm.AddGame( g );
					_gameList.Add(g);
				}
			}
			else
				Utility.Announce(string.Format("LoadGameList: No Sched"));
		}

		public void LoadGameList()
		{
			Announce(string.Format("LoadGameList: Loading Teams Week {0}", Week ) );

			_sched = Utility.TflWs.GetGames(Int32.Parse(Season), WeekNo);
			if (_sched != null)
			{
				_gameList = new ArrayList();
				var dt = _sched.Tables[0];
				foreach (DataRow dr in dt.Rows)
				{
					var gameCode = $"{Season}:{dr[ "WEEK" ]}-{dr[ "GAMENO" ]}";
					Announce(string.Format("LoadGameList: getting Game:{0}", gameCode ));

					var g = new NFLGame(dr);

					_gameList.Add(g);
				}
			}
			else
				Announce(string.Format("LoadGameList: No Sched"));
		}

      private void Announce( string msg )
      {
         if (Logger==null)
            Logger = NLog.LogManager.GetCurrentClassLogger();

         Logger.Trace( msg );
      }

      #endregion

      public NFLWeek PreviousWeek(NFLWeek theWeek, bool loadgames, bool regularSeasonGamesOnly )
		{
			var previousWeekNo = theWeek.WeekNo - 1;
			var previousSeasonNo = theWeek.SeasonNo;
			if (previousWeekNo < 1)
			{
				previousWeekNo = Constants.K_WEEKS_IN_A_SEASON;
				previousSeasonNo--;
			}
			if ( regularSeasonGamesOnly )
				while (previousWeekNo > Constants.K_WEEKS_IN_REGULAR_SEASON)
					previousWeekNo--;

			var previousWeek = new NFLWeek(previousSeasonNo, previousWeekNo, loadgames);
#if DEBUG
			//if ( regularSeasonGamesOnly )
			//   Utility.Announce( string.Format( "Previous Regular Week to {1} was {0}", 
			//      previousWeek.WeekKey(":"), theWeek.WeekKey( ":" ) ) );
			//else
			//   Utility.Announce( string.Format( "Previous Week to {1} was {0}", 
			//      previousWeek.WeekKey(":"), theWeek.WeekKey(":") ) );
#endif
			return previousWeek;
		}

		public NFLWeek NextWeek(NFLWeek theWeek, bool loadgames = false)
		{
			var nextWeekNo = theWeek.WeekNo + 1;
			var nextSeasonNo = theWeek.SeasonNo;
			if (nextWeekNo > Constants.K_WEEKS_IN_A_SEASON)
			{
				nextWeekNo = 1;
				nextSeasonNo++;
			}
			return new NFLWeek(nextSeasonNo, nextWeekNo, loadgames);
		}

		public bool IsBefore(NFLWeek testWeek)
		{
			bool isBefore = false;

			if (Int32.Parse(testWeek.Season) < Int32.Parse(Season))
				isBefore = true;
			else
			{
				if (Int32.Parse(testWeek.Season) == Int32.Parse(Season))
				{
					if (Int32.Parse(testWeek.Week) <= Int32.Parse(Week))
						isBefore = true;
				}
			}

			return isBefore;
		}

		public void RenderMatchups()
		{
			Utility.Announce("Render Matchups:");
			var dt = _sched.Tables[0];
			foreach (DataRow dr in dt.Rows)
			{
				var gameCode = string.Format("{0}:{1}-{2} {3} @ {4}", Season, dr["WEEK"], dr["GAMENO"], dr["AWAYTEAM"],
														  dr["HOMETEAM"]);

				Utility.Announce("RenderMatchups:Getting " + gameCode);

				var g = Masters.Gm.GetGame(gameCode);
				if (g == null)
				{
					Utility.Announce("RenderMatchups:Instantiating " + gameCode);
					g = new NFLGame(dr);
					Masters.Gm.AddGame(g);
				}
				var mur = new MatchupReport(g);
				mur.Render( writeProjection:true);
			}
		}

		public void RenderMatchups(string gowHostTeam)
		{
			Utility.Announce("Render Matchups: Game of the Week");
			var dt = _sched.Tables[0];
			foreach (DataRow dr in dt.Rows)
			{
				var gameCode = string.Format("{0}:{1}-{2}", Season, dr["WEEK"], dr["GAMENO"]);
				Utility.Announce("RenderMatchups:Getting " + gameCode);
				var g = Masters.Gm.GetGame(gameCode);
				if (g == null)
				{
					Utility.Announce("RenderMatchups:Instantiating " + gameCode);
					g = new NFLGame(dr);
					Masters.Gm.AddGame(g);
				}
				if (g.HomeTeam == gowHostTeam)
				{
					if (g.HomeNflTeam == null) g = new NFLGame(dr);
					var mur = new MatchupReport(g);
					mur.Render(writeProjection:true);
				}
			}
		}

		#region  Appointment stuff

		//public void SaveAppointments()
		//{
		//    //  Sort the week by time slots
		//    int nGames = 0;
		//    DataTable dt = sched.Tables[0];
		//    dt.DefaultView.Sort = "GAMEDATE ASC, GAMEHOUR ASC";
		//    DataRow dr1 = dt.Rows[0];
		//    string lastTime = VFPToolkit.dates.DTOS( DateTime.Parse( dr1["GameDate"].ToString() ) ) + dr1["GameHour"]; 
		//    string matchList = "";
		//    foreach (DataRow dr in dt.Rows)
		//    {
		//        string thisTime = VFPToolkit.dates.DTOS( DateTime.Parse( dr["GameDate"].ToString() ) ) + dr["GameHour"]; 
		//        if ( thisTime != lastTime )
		//        {
		//            CreateAppointment( matchList, lastTime, nGames );
		//            matchList = String.Empty;
		//            lastTime = thisTime;
		//            nGames = 0;
		//        }
		//        matchList += dr["AWAYTEAM"] + " @ " + dr["HOMETEAM"] + "\n";
		//        nGames++;
		//    }
		//    CreateAppointment( matchList, lastTime, nGames );
		//}

		//private void CreateAppointment( string matchList, string timeSlot, int nGames )
		//{
		//    if ( oApp == null )
		//    {
		//        oApp = new Outlook.Application();
		//        NameSpace oNs = oApp.GetNamespace("mapi");

		//        //Log on by using a dialog box to choose the profile.
		//        oNs.Logon(Missing.Value, Missing.Value, true, true);
		//    }
		//    DateTime usDate = new DateTime( Int32.Parse( timeSlot.Substring( 0, 4 ) ),
		//        Int32.Parse( timeSlot.Substring( 4, 2 ) ),
		//        Int32.Parse( timeSlot.Substring( 6, 2 ) ),
		//        0, 0, 0
		//        );
		//    DateTime ausDate = usDate.AddDays( 1D ); 
		//    int year = ausDate.Year;
		//    int month = ausDate.Month;
		//    int day = ausDate.Day;
		//    int hour = 3;
		//    string subject = "NFL";
		//    switch ( timeSlot.Substring( 8, 2 ) )
		//    {
		//        case "1 ":
		//            subject += ( nGames > 1 ) ? string.Format( " - {0} Early Games", nGames ) : " - " + matchList ;
		//            hour = ( month > 3 && month < 11 ) ? 3 : 4;
		//            break;
		//        case "4 ":
		//            subject += ( nGames > 1 ) ? string.Format( " - {0} Late Games", nGames ) : " - " + matchList ;
		//            hour = ( month > 3 && month < 11 ) ? 6 : 7;
		//            break;
		//        case "7 ":
		//            subject += ( nGames > 1 ) ? string.Format( " - {0} Night Games", nGames ) : " - " + matchList;
		//            hour = ( month > 3 && month < 11 ) ? 9 : 10;
		//            break;
		//        case "8 ":
		//            subject += ( nGames > 1 ) ? string.Format( " - {0} Night Games", nGames ) : " - " + matchList;
		//            hour = ( month > 3 && month < 11 ) ? 10 : 11;
		//            break;
		//        case "9 ":
		//            subject += ( nGames > 1 ) ? string.Format( " - {0} Night Games", nGames )  : " - " + matchList;
		//            hour = ( month > 3 && month < 11 ) ? 11 : 12;
		//            break;

		//        default:
		//            break;
		//    }

		//    AppointmentItem objApt = (AppointmentItem) oApp.CreateItem( OlItemType.olAppointmentItem );
		//    objApt.Body = matchList;
		//    objApt.Start = new DateTime( year, month, day, hour,   0, 0 );
		//    objApt.End   = new DateTime( year, month, day, hour+3, 0, 0 );
		//    objApt.Subject = subject;
		//    objApt.ReminderSet = false;
		//    objApt.Sensitivity = OlSensitivity.olPersonal;
		//    objApt.BusyStatus = OlBusyStatus.olFree;
		//    objApt.Save();
		//}

		public int AusHour(string usHour, int month)
		{
			var hour = 0;

			switch (usHour)
			{
				case "1 ":
					hour = (month > 3 && month < 11) ? 3 : 5;
					break;
				case "4 ":
					hour = (month > 3 && month < 11) ? 6 : 8;
					break;
				case "7 ":
					hour = (month > 3 && month < 11) ? 9 : 11;
					break;
				case "8 ":
					hour = (month > 3 && month < 11) ? 10 : 12;
					break;
				case "9 ":
					hour = (month > 3 && month < 11) ? 11 : 13;
					break;
			}
			return hour;
		}

		#endregion

		public NFLGame GameFor(string teamCode)
		{
			//  go through the games and send back the one for the chosen team

			var myGameList = GameList(SeasonNo, WeekNo);

			if (myGameList == null) return null;

			return (from NFLGame g in myGameList 
					  let opp = g.OpponentTeam(teamCode) 
					  select g).FirstOrDefault(g => g.HomeNflTeam.TeamCode.Equals(
						  teamCode) || g.AwayNflTeam.TeamCode.Equals(teamCode));
		}

		public string GameCodeFor( string teamCode )
		{
			var gameKey = string.Empty;
			var game = GameFor(teamCode);
			if ( game != null ) gameKey = game.GameKey();
			return gameKey;
		}
	}
}