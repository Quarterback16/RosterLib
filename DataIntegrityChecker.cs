using System;
using System.Data;

namespace RosterLib
{
   public class DataIntegrityChecker
   {
      public string Season { get; set; }

      public int Week { get; set; }

      public NFLWeek NflWeek { get; set; }

      public int ScoresChecked { get; set; }

      public int StatsChecked { get; set; }

      public int Errors { get; set; }

      public int Warnings { get; set; }

      public DataIntegrityChecker()
      {
         StatsChecked = 0;
         ScoresChecked = 0;
         Errors = 0;
         Warnings = 0;
      }

      public void CheckScores()
      {
         if (string.IsNullOrEmpty(Season))
            RosterLib.Utility.Announce("Please specify the Season and optionally the Week");
         else
         {
            if (Week > 0)
               CheckScoresForWeek( Week );
            {
               for (int w = 1; w < 18; w++)
               {
                  CheckScoresForWeek(w);
               }
            }
         }
         ReportResults(ScoresChecked, "Scores");
      }

      private void CheckScoresForWeek( int week )
      {
         NflWeek = new NFLWeek(Season, week);
         var scoreFactory = new ScoreFactory();

         var ds = Utility.TflWs.ScoresDs(Season, String.Format("{0:0#}", week));
         var dt = ds.Tables["score"];
         foreach (DataRow dr in dt.Rows)
         {
            ScoresChecked++;
            var score = scoreFactory.CreateScore(dr["SCORE"].ToString());
            if (score != null)
            {
               score.Load(dr);
#if DEBUG
               score.Dump();
#endif
               if (score.IsValid())
               {
               }
               else
                  Error(score);
            }
         }
      }

      public void CheckStats()
      {
         if (string.IsNullOrEmpty(Season))
            RosterLib.Utility.Announce("Please specify the Season and Week");
         else
         {
            if ( Week > 0)
               CheckStatsForWeek( Week );
            else
            {
               for (int w = 1; w < 18; w++)
               {
                  CheckStatsForWeek( w );                  
               }
            }
         }
         ReportResults( StatsChecked, "Stats" );
      }

      private void CheckStatsForWeek( int week )
      {
         NflWeek = new NFLWeek(Season, week);
         var statFactory = new StatFactory();

         var ds = Utility.TflWs.PlayerStatsDs(Season, String.Format("{0:0#}", week));
         var dt = ds.Tables["stat"];
         foreach (DataRow dr in dt.Rows)
         {
            StatsChecked++;
            var stat = statFactory.CreateStat(dr["STAT"].ToString());
            if (stat != null)
            {
               stat.Load(dr);
#if DEBUG
               stat.Dump();
#endif
               if (stat.IsValid())
               {
                  if (!stat.IsReasonable())
                     Warn(string.Format("{0} {1} not valid", stat.Name, stat.Quantity), stat);
               }
               else
                  Error(stat);
            }
         }
      }

      private void ReportResults(int thingCheckedCount, string thingChecked)
      {
         if ( Week > 0 )
            RosterLib.Utility.Announce(string.Format("Week {3}:{4} {5} Checked {0} Errors {1} Warnings {2}",
                thingCheckedCount, Errors, Warnings, Season, Week, thingChecked));
         else
            RosterLib.Utility.Announce(string.Format("Season {3} {4} Checked {0} Errors {1} Warnings {2}",
                thingCheckedCount, Errors, Warnings, Season, thingChecked));
      }

      private void Warn(string msg, IStat stat)
      {
         Warnings++;
         stat.Dump();
         RosterLib.Utility.Announce(string.Format("          Warning : {0} - {1} - {2}",
             NflWeek.WeekKey(":"), msg, stat.PlayerId));
      }

      private void Error(IStat stat)
      {
         Errors++;
         stat.Dump();
         RosterLib.Utility.Announce(string.Format("          Error : {0} - {1} - {2}",
             NflWeek.WeekKey(":"), stat.Error, stat.PlayerId));
      }

      private void Error(IScore score)
      {
         Errors++;
         score.Dump();
         RosterLib.Utility.Announce(string.Format("          Error : {0} - {1} - {2}",
             NflWeek.WeekKey(":"), score.Error, score.PlayerId1));
      }
   }
}