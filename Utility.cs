using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using TFLLib;
using RosterLib.Helpers;

namespace RosterLib
{
   public static class Utility
   {
      public static WizSeason Wz;

      public static NFLWeek MCurrentWeek;

      private static DataLibrarian _tflWs;

      public static DataLibrarian TflWs
      {
         get
         {
			return _tflWs 
				?? (_tflWs = new DataLibrarian(
									NflConnectionString(),
									TflConnectionString(),
									CtlConnectionString(), 
									new NLogAdaptor()));
         }
         set { _tflWs = value; }
      }

      /// <summary>
      ///   Different machines will need to output to different places due to LoadUnitscal policies
      ///   not a good idea to have the durectory as a konstant
      /// </summary>
      /// <returns></returns>
      public static string OutputDirectory()
      {
         var outputDir = Config.OutputDirectory();
         return outputDir;
      }

      public static string XmlDirectory()
      {
         var outputDir = Config.XmlDirectory();
         return outputDir;
      }

      public static string PrimaryDrive()
      {
         return Config.PrimaryDrive();
      }

      /// <summary>
      ///   Use this to turn back time
      /// </summary>
      /// <returns></returns>
      public static DateTime CurrentDate()
      {
         //			return new DateTime( 2011, 1, 1 );   // should equate to Week 17 of 2010 season
         //			return new DateTime( 2010, 9, 11 );  // should equate to Week  1 of 2010 season
         return DateTime.Now;
      }

      public static string HostName()
      {
         return Environment.MachineName;
      }

      public static string RatingPts(string ratings)
      {
         int i;
         int nPts = 0;
         for (i = 0; i < 6; i++)
         {
            string c = ratings.Substring(i, 1);
            switch (c)
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
         return string.Format("{0}", nPts);
      }

      public static decimal Percent(int quotient, int divisor)
      {
         return 100 * Average(quotient, divisor);
      }

      public static decimal Average(int quotient, int divisor)
      {
         //  need to do decimal other wise INT() will occur
         if (divisor == 0) return 0.0M;
         return (Decimal.Parse(quotient.ToString()) /
                 Decimal.Parse(divisor.ToString()));
      }

      public static string DotNetVersion()
      {
         return IsNet45OrNewer() ? "4.5" : Environment.Version.ToString();
      }

      public static bool IsNet45OrNewer()
      {
         // Class "ReflectionContext" exists in .NET 4.5 .
         return Type.GetType("System.Reflection.ReflectionContext", false) != null;
      }

      public static void Announce(string rpt, int indent = 3)
      {
         var theLength = rpt.Length + indent;
         rpt = rpt.PadLeft(theLength, ' ');
         //Console.WriteLine(rpt);  //  not viewable when in Unit Test mode
         //WriteLog(rpt);
#if DEBUG
         Debug.WriteLine(rpt);
#endif
      }

      //public static void ExecuteStep( Action stepMethod, Func<bool> goNogoMethod )
      //{
      //    if ( goNogoMethod() )
      //    {
      //        var runStorer = new DbfRunStorer();
      //        ExecuteStep( stepMethod, runStorer );
      //    }
      //    else
      //        Announce( string.Format( "{0} skipped.", stepMethod.Method.Name ), 0 );
      //}

      public static void ExecuteStep(Action stepMethod, IRunStorer runStorer)
      {
         Announce(string.Format("---- {0} --------------------------------", stepMethod.Method.Name), 0);
         var stopwatch = new Stopwatch();
         stopwatch.Start();
         Announce(string.Format("{0} ...", stepMethod.Method.Name), 0);
         stepMethod.Invoke();
         var ts = StopTheWatch(stopwatch, string.Format("Finished: {0}", stepMethod.Method.Name));
         Announce("============================================", 0);
         if (runStorer != null)
            runStorer.StoreRun(stepMethod.Method.Name, ts);
      }

      public static bool JustDoIt()
      {
         return true;
      }

      public static string Dtos(DateTime theDate)
      {
         return string.Format("{0}{1:00}{2:00}", theDate.Year, theDate.Month, theDate.Day);
      }

      public static void WriteLog(string message)
      {
         Console.WriteLine(
             string.Format("{0}:{1}",
             DateTime.Now.ToString("dd MMM yyyy - hh:mm:ss  "), message));
      }

      public static int CurrSeason;
      public static int CurrWeek;

      static Utility()
      {
         CurrentLeague = Constants.K_LEAGUE_Yahoo;
      }

      public static string CurrentSeason()
      {
         if (CurrSeason == 0)
         {
            //GetCurrentSeasonFromDatabase();
            SetCurrentSeasonBasedOnDate();
         }
         return CurrSeason.ToString();
      }

      private static void SetCurrentSeasonBasedOnDate()
      {
         var theDate = CurrentDate();
         var nYear = theDate.Year;
         if (theDate.Month < 3)
            nYear--;
         CurrSeason = nYear;
      }

      public static string CurrentWeek()
      {
         if (CurrWeek == 0)
            SetCurrentWeekBasedOnDate();
         return CurrWeek.ToString( CultureInfo.InvariantCulture );
      }

      private static void SetCurrentWeekBasedOnDate()
      {
         var theDate = CurrentDate();
         theDate = theDate.Subtract(new TimeSpan(15, 0, 0));  //  rough time difference
         if ((theDate.Month < 9) && (theDate.Month > 2))
            CurrWeek = 0;
         else
            SetWeekFromSchedule(theDate);
      }

      public static string NextWeek()
      {
         if (CurrWeek == 0)
            SetCurrentWeekBasedOnDate();
         return (CurrWeek+1).ToString();
      }

      private static void SetWeekFromSchedule(DateTime theDate)
      {
         while (!theDate.DayOfWeek.Equals(DayOfWeek.Sunday))
            theDate = theDate.AddDays(1);
         CurrWeek = Int32.Parse(TflWs.GetWeekFor(theDate));
      }

      public static string LastSeason()
      {
         var lastSeason = Int32.Parse(CurrentSeason()) - 1;
         return lastSeason.ToString();
      }

      public static string SeasonInFocus()
      {
         return CurrentWeek() == "0" ? LastSeason() : CurrentSeason();
      }

      /// <summary>
      ///   The upcoming Week
      /// </summary>
      /// <returns></returns>
      public static NFLWeek CurrentNFLWeek()
      {
         return MCurrentWeek ?? (MCurrentWeek =
            new NFLWeek(Int32.Parse(CurrentSeason()), Int32.Parse(CurrentWeek())));
      }

      public static NFLWeek UpcomingWeek(DateTime theDate)
      {
         DataRow dr = null;
         while (dr == null)
         {
            while (!theDate.DayOfWeek.Equals(DayOfWeek.Sunday))
               theDate = theDate.AddDays(1);
            dr = TflWs.GetNflWeekFor(theDate);
            if (dr == null) theDate = theDate.AddDays(1);
         }
         var game = new NFLGame(dr);
         var week = new NFLWeek(game.Season, game.WeekNo);
         return week;
      }

      public static int PreviousSeason()
      {
         return (Int32.Parse(CurrentSeason()) - 1);
      }

      public static string PreviousSeason(string season)
      {
         return (Int32.Parse(season) - 1).ToString();
      }

      public static int PreviousWeek()
      {
         var currentWeek = CurrentNFLWeek();
         var previousWeek = currentWeek.PreviousWeek(currentWeek, false, false);
         return previousWeek.WeekNo;
      }

      public static string PreviousWeekAsString()
      {
         return string.Format( "{0:0#}", PreviousWeek() );
      }

      public static NFLGame GetGameFor(string season, string week, string teamCode)
      {
         var dr = TflWs.GetGame(season, week, teamCode);
         return new NFLGame(dr);
      }

      public static int UpcomingWeek()
      {
         return Int32.Parse(TflWs.GetCurrentWeek()) + 1;
      }

      public static int WeeksAgo(int howManyWeeks)
      {
         int wk = Int32.Parse(CurrentWeek());
         int ago = wk - howManyWeeks;
         if (ago < 1) ago = 1;
         return ago;
      }

      /// <summary>
      /// Gets the team, if we have it already return it otherwise create it.
      /// </summary>
      /// <param name="teamCode">The team code.</param>
      /// <returns>NFLTeam</returns>
      public static NflTeam GetTeam(string teamCode)
      {
         //  if we have it already return it otherwise create it
         var team = new NflTeam(teamCode);
         return team;
      }

      public static string DivisionOut(string divCode)
      {
         string sDivision;

         switch (divCode)
         {
            case "A":
               sDivision = "NFC-East";
               break;

            case "B":
               sDivision = "NFC-North";
               break;

            case "C":
               sDivision = "NFC-South";
               break;

            case "D":
               sDivision = "NFC-West";
               break;

            case "E":
               sDivision = "AFC-East";
               break;

            case "F":
               sDivision = "AFC-North";
               break;

            case "G":
               sDivision = "AFC-South";
               break;

            case "H":
               sDivision = "AFC-West";
               break;

            default:
               sDivision = "???";
               break;
         }
         return sDivision;
      }

      public static string CategoryFor(string posdesc)
      {
         var catOut = "?";

         posdesc = posdesc.Trim();

         if (String.IsNullOrEmpty(posdesc)) return "<blank>";

         const string qb = "QB,P,";
         const string rb = "RB,HB,FB,";
         const string wr = "WR,TE,FL,";
         const string pk = "PK,K,";
         const string dl = "DE,DT,LB,NT,ILB,OLB";
         const string db = "DB,CB,FS,SS";
         const string ol = "OT,G,T,C,OG,";

         if (qb.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
            catOut = "1";
         else if (rb.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
            catOut = "2";
         else if (wr.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
            catOut = "3";
         else if (pk.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
            catOut = "4";
         else if (dl.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
            catOut = "5";
         else if (db.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
            catOut = "6";
         else if (ol.IndexOf(posdesc, System.StringComparison.Ordinal) > -1)
            catOut = "7";
         return catOut;
      }

      public static string CategoryOut(string strCat)
      {
         string catOut;
         switch (strCat)
         {
            case "1":
               catOut = "Quarterback";
               break;

            case "2":
               catOut = "Runningback";
               break;

            case "3":
               catOut = "Receiver";
               break;

            case "4":
               catOut = "Kicker";
               break;

            case "5":
               catOut = "Linebacker";
               break;

            case "6":
               catOut = "Secondary";
               break;

            case "7":
               catOut = "Offensive Line";
               break;

            default:
               catOut = "???";
               break;
         }
         return catOut;
      }

      public static string ScoreTypeOut(string scoreType)
      {
         string scoreTypeOut;
         switch (scoreType)
         {
            case Constants.K_SCORE_TD_PASS:
               scoreTypeOut = "Touchdown Pass";
               break;

            case Constants.K_SCORE_TD_RUN:
               scoreTypeOut = "Touchdown Run";
               break;

            case Constants.K_SCORE_FIELD_GOAL:
               scoreTypeOut = "Field Goal";
               break;

            case Constants.K_SCORE_PAT:
               scoreTypeOut = "PAT";
               break;

            case Constants.K_SCORE_PAT_PASS:
               scoreTypeOut = "2 point pass Conversion";
               break;

            case Constants.K_SCORE_PAT_RUN:
               scoreTypeOut = "2 point run Conversion";
               break;

            case Constants.K_SCORE_KICK_RETURN:
               scoreTypeOut = "Kick Return";
               break;

            case Constants.K_SCORE_PUNT_RETURN:
               scoreTypeOut = "Punt Return";
               break;

            case Constants.K_SCORE_SAFETY:
               scoreTypeOut = "Safety";
               break;

            case Constants.K_SCORE_FUMBLE_RETURN:
               scoreTypeOut = "Fumble Return";
               break;

            case Constants.K_SCORE_INTERCEPT_RETURN:
               scoreTypeOut = "Intercept Return";
               break;

            default:
               scoreTypeOut = scoreType;
               break;
         }
         return scoreTypeOut;
      }

      public static string StatTypeOut(string statType)
      {
         string scoreTypeOut;
         switch (statType)
         {
            case Constants.K_STATCODE_SACK:
               scoreTypeOut = "Sacks";
               break;

            case Constants.K_STATCODE_RUSHING_YARDS:
               scoreTypeOut = "Rushing Yards";
               break;

            case Constants.K_STATCODE_RECEPTION_YARDS:
               scoreTypeOut = "Reception Yards";
               break;

            case Constants.K_STATCODE_PASSES_CAUGHT:
               scoreTypeOut = "Passes Caught";
               break;

            case Constants.K_STATCODE_PASSING_YARDS:
               scoreTypeOut = "Passing Yards";
               break;

            case Constants.K_STATCODE_FUMBLES_LOST:
               scoreTypeOut = "Fumbles Lost";
               break;

            case Constants.K_STATCODE_INTERCEPTIONS_MADE:
               scoreTypeOut = "Interceptions Made";
               break;

            case Constants.K_STATCODE_INTERCEPTIONS_THROWN:
               scoreTypeOut = "Interceptions Thrown";
               break;

            default:
               scoreTypeOut = statType;
               break;
         }
         return scoreTypeOut;
      }

      public static int WeekSeed(string season, string week)
      {
         var s = Int32.Parse(season);
         var seed = (s - 1980) * 22;
         seed += Int32.Parse(week);
         return seed;
      }

      public static string CurrentLeague { get; set; }

      public static ArrayList UnitList { get; set; }

      public static void LoadUnits()
      {
         //Announce( "RosterGrid.LoadUnits" );
         UnitList = new ArrayList();
         var po = new TeamUnit("PO", "Passing Offence");
         var ro = new TeamUnit("RO", "Rushing Offence");
         var pp = new TeamUnit("PP", "Pass Protection");
         var pr = new TeamUnit("PR", "Pass Rush");
         var rd = new TeamUnit("RD", "Run Defence");
         var pd = new TeamUnit("PD", "Pass Defence");
         po.AddPosition("QB");
         po.AddPosition("WR");
         po.AddPosition("FL");
         po.AddPosition("SE");
         po.AddPosition("TE");
         UnitList.Add(po);
         UnitList.Add(ro);
         UnitList.Add(pp);
         UnitList.Add(pr);
         UnitList.Add(rd);
         UnitList.Add(pd);
      }

      public static string SeedOut(int weekSeed)
      {
         int baseyr = (weekSeed / 22);
         int wk = weekSeed - (baseyr * 22);
         return String.Format("{0}:{1:0#}", baseyr + 1980, wk);
      }

      public static string SeasonSeed(int weekSeed)
      {
         int baseyr = (weekSeed / 22);
         return String.Format("{0}", baseyr + 1980);
      }

      public static void ExperiencePoints()
      {
         ExperiencePoints(SeasonInFocus());
      }

      /// <summary>
      ///   Determine team ratings based on experience points.
      /// </summary>
      /// <param name="season"></param>
      public static void ExperiencePoints(string season)
      {
         Announce("ExperiencePoints...");
         //  used for projections
         var toWeek = Constants.K_WEEKS_IN_REGULAR_SEASON.ToString();
         if (season == CurrentSeason()) toWeek = CurrentWeek();
         Announce("RosterGrid.ExperiencePoints for " + season);
         Wz = new WizSeason(season, "01", toWeek);

         if (CurrentWeek() != "0") return;

         //  only need this pre-season
         Wz.SummaryStats();
         Wz.PassingDifferentials();
         Wz.RunningDifferentials();
      }

      /// <summary>
      ///   copies a file under the Output directory as the from and too files both get the output dir prefix
      /// </summary>
      /// <param name="fromFile"></param>
      /// <param name="targetFile"></param>
      public static void CopySubFile(string fromFile, string targetFile)
      {
         var sourcePath = OutputDirectory();
         var targetPath = OutputDirectory();
         var sourceFile = Path.Combine(sourcePath, fromFile);
         var destFile = Path.Combine(targetPath, targetFile);

         EnsureDirectory(destFile);

         // To copy a file to another location and
         // overwrite the destination file if it already exists.
         File.Copy(sourceFile, destFile, overwrite: true);
      }

      public static void CopyFile( string fromFile, string targetFile )
      {
         EnsureDirectory( targetFile );

         // To copy a file to another location and
         // overwrite the destination file if it already exists.
         File.Copy( fromFile, targetFile, overwrite: true );
      }
      public static void MoveFile(string fromFile, string targetFile)
      {
         Announce(string.Format("Moving File: from {0} to {1}", fromFile, targetFile));
         CopySubFile(fromFile, targetFile);
         var sourcePath = OutputDirectory();
         var workFile = Path.Combine(sourcePath, fromFile);
         File.Delete(workFile);
      }

      internal static void EnsureDirectory(string destFile)
      {
         var directoryInfo = new FileInfo(destFile).Directory;
         if (directoryInfo != null)
         {
            if (!Directory.Exists(directoryInfo.ToString()))
            {
               string currentWorkingDirectory =
                  Path.GetDirectoryName(
                     System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
               Directory.CreateDirectory(directoryInfo.ToString());
            }
         }
      }

      public static string MainPos(string catCode)
      {
         var sPos = "??";

         switch (catCode)
         {
            case Constants.K_QUARTERBACK_CAT:
               sPos = "QB";
               break;

            case Constants.K_RUNNINGBACK_CAT:
               sPos = "RB";
               break;

            case Constants.K_RECEIVER_CAT:
               sPos = "WR";
               break;

            case Constants.K_KICKER_CAT:
               sPos = "PK";
               break;

            case Constants.K_LINEMAN_CAT:
               sPos = "DL";
               break;

            case Constants.K_DEFENSIVEBACK_CAT:
               sPos = "DB";
               break;

            case Constants.K_OFFENSIVELINE_CAT:
               sPos = "OL";
               break;
         }
         return sPos;
      }

      public static void PrintIndexAndKeysAndValues(Hashtable myList)
      {
         var myEnumerator = myList.GetEnumerator();
         var i = 0;
         Announce("\t-INDEX-\t-KEY-\t-VALUE-");
         while (myEnumerator.MoveNext())
            Announce(String.Format("\t[{0}]:\t{1}\t{2}", i++, myEnumerator.Key, myEnumerator.Value));
      }

      public static string SeasonFor(DateTime when)
      {
         //  season clicks over on the 1-Mar
         return when.Month < 3 ? (when.Year - 1).ToString() : when.Year.ToString();
      }

      public static decimal Clip(int w, int l, int t)
      {
         var clip = 0.0M;
         decimal tot = (w + l + t);
         if (tot > 0)
            clip = ((Convert.ToDecimal(w) * 2.0M) + Convert.ToDecimal(t)) / (tot * 2.0M);
         return clip;
      }

      public static string GameKey(string season, string week, string gameCode)
      {
         return string.Format("{0}:{1}-{2}", season, week, gameCode);
      }

      public static DateTime StartOfSeason(string season)
      {
         //  get the game date for the first game and skip to Sunday
         var dGame = GetGameOne(season);

         return NextSunday(dGame);
      }

      public static bool IsSunday(DateTime dGame)
      {
         return dGame.DayOfWeek == DayOfWeek.Sunday;
      }

      private static DateTime NextSunday(DateTime dGame)
      {
         var testDate = dGame;
         while (!IsSunday(testDate))
            testDate = testDate.AddDays(1);
         return testDate;
      }

      public static DateTime GetGameOne(string season)
      {
         var gameOneDr = TflWs.GetGameByCode(season, "01", "A");
         var gameOne = new NFLGame(gameOneDr);
         return gameOne.GameDate;
      }

      public static DateTime StartOfSeason()
      {
         return StartOfSeason(CurrentSeason());
      }

      public static TimeSpan StopTheWatch(Stopwatch stopwatch, string message)
      {
         stopwatch.Stop();
         var ts = stopwatch.Elapsed;
         var elapsedTime = FormatElapsedTime(ts);
         Announce(string.Format("{0} took {1}", message, elapsedTime));
         return ts;
      }

      public static string FormatElapsedTime(TimeSpan ts)
      {
         var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                         ts.Hours, ts.Minutes, ts.Seconds,
                                         ts.Milliseconds / 10);
         return elapsedTime;
      }

      public static int PickAScore(int score)
      {
         int nClosest;
         switch (score)
         {
            case 1:
               nClosest = 0;
               break;

            case 2:
               nClosest = 3;
               break;

            case 4:
               nClosest = 3;
               break;

            case 5:
               nClosest = 6;
               break;

            case 8:
               nClosest = 7;
               break;

            case 11:
               nClosest = 10;
               break;

            case 15:
               nClosest = 14;
               break;

            case 16:
               nClosest = 17;
               break;

            case 18:
               nClosest = 17;
               break;

            case 19:
               nClosest = 20;
               break;

            case 22:
               nClosest = 21;
               break;

            case 25:
               nClosest = 24;
               break;

            case 26:
               nClosest = 28;
               break;

            case 29:
               nClosest = 28;
               break;

            case 32:
               nClosest = 31;
               break;

            case 34:
               nClosest = 35;
               break;

            case 36:
               nClosest = 35;
               break;

            case 37:
               nClosest = 38;
               break;

            case 39:
               nClosest = 38;
               break;

            default:
               nClosest = score;
               break;
         }

         return nClosest;
      }

      public static bool DoIt()
      {
         return true;
      }

      public static int HiScore(ArrayList gameList, string teamCode)
      {
         int hiScore = 0;
         foreach (NFLGame game in gameList)
         {
            int gameScore = game.ScoreFor(teamCode);
            if (gameScore > hiScore)
               hiScore = gameScore;
         }
         return hiScore;
      }

      public static int LoScore(ArrayList gameList, string teamCode)
      {
         int loScore = 9999;
         foreach (NFLGame game in gameList)
         {
            int gameScore = game.ScoreFor(teamCode);
            if (gameScore < loScore)
               loScore = gameScore;
         }
         return loScore;
      }

      public static int HiAgainst(ArrayList gameList, string teamCode)
      {
         int hiScore = 0;
         foreach (NFLGame game in gameList)
         {
            int gameScore = game.AgainstFor(teamCode);
            if (gameScore > hiScore)
               hiScore = gameScore;
         }
         return hiScore;
      }

      public static int LoAgainst(ArrayList gameList, string teamCode)
      {
         int loScore = 9999;
         foreach (NFLGame game in gameList)
         {
            int gameScore = game.AgainstFor(teamCode);
            if (gameScore < loScore)
               loScore = gameScore;
         }
         return loScore;
      }

      public static int OffRating(NflTeam team, DateTime predictionDate)
      {
         team.LoadPreviousRegularSeasonGames(4, predictionDate);
         var offArray = new int[4];
         var i = 0;
         foreach (NFLGame game in team.GameList)
         {
            offArray[i] = game.ScoreFor(team.TeamCode);
            i++;
         }
         Array.Sort(offArray);
         var tot = offArray[1] + offArray[2];
         return tot / 2;
      }

      public static int DefRating(NflTeam team, DateTime predictionDate)
      {
         team.LoadPreviousRegularSeasonGames(4, predictionDate);
         var defArray = new int[4];
         var i = 0;

         foreach (NFLGame game in team.GameList)
         {
            defArray[i] = game.AgainstFor(team.TeamCode);
            i++;
         }
         Array.Sort(defArray);
         int tot = defArray[1] + defArray[2];
         return tot / 2;
      }

      public static bool Contains(string subString, string mainString)
      {
         var nSpot = mainString.IndexOf(subString.Trim());
         return nSpot != -1;
      }

      public static string NflConnectionString()
      {
         var connections = ConfigurationManager.ConnectionStrings;
         var connStr = connections["NflConnectionString"].ConnectionString;
         return connStr;
      }

      public static string TflConnectionString()
      {
         var connections = ConfigurationManager.ConnectionStrings;
         return connections["TflConnectionString"].ConnectionString;
      }

      public static string CtlConnectionString()
      {
         var connections = ConfigurationManager.ConnectionStrings;
         return connections["CtlConnectionString"].ConnectionString;
      }

      public static void DumpDataSet(DataSet ds)
      {
         var dt = ds.Tables[0];
         foreach (DataRow dr in dt.Rows)
         {
            var sb = new StringBuilder();
            foreach (DataColumn dc in dt.Columns)
            {
               var colVal = string.Format("{0}:{1},", dc.ColumnName, dr[dc]);
               sb.Append(colVal);
            }
            var rowData = sb.ToString();
            rowData = rowData.Remove(rowData.Length - 1, 1);
            Announce(rowData);
         }
      }

      public static void DumpLoggerInfo( NLog.LogManager logManager )
      {
         //foreach ( var item in logManager.Configuration.Rules )
         //{
            
         //}
      }
   }
}