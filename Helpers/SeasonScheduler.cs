using System;
using RosterLib.Interfaces;

namespace RosterLib.Helpers
{
   public class SeasonScheduler : ISeasonScheduler
   {
      public bool ScheduleAvailable( string season )
      {
         var ds = Utility.TflWs.GetAllGames( Int32.Parse( season ) );
         if (ds.Tables["SCHED"].Rows.Count > 0)
            return true;
         return false;
      }


      public DateTime SeasonStarts(string season)
      {
         return WeekStarts( season, "01" );
      }

      public DateTime RegularSeasonEnds(string season)
      {
         return WeekEnds( season, "17" );
      }

      public DateTime SeasonEnds(string season)
      {
         return WeekEnds(season, "21");
      }

      public DateTime WeekStarts(string season, string week )
      {
         var ds = Utility.TflWs.GameFor(season, week, "A");
         return GetGameDate(ds);
      }

      private static DateTime GetGameDate(System.Data.DataSet ds)
      {
         if (ds.Tables[0].Rows.Count == 0) return new DateTime(1, 1, 1);
         var gameDate = DateTime.Parse(ds.Tables["SCHED"].Rows[0]["GAMEDATE"].ToString());
         return gameDate;
      }

      public DateTime WeekEnds(string season, string week )
      {
         var gameCode = "P";
         if (week == "21")
            gameCode = "A";
         if (week == "20")
            gameCode = "B";
         if (week == "19" || week == "18" )
            gameCode = "D";

         var ds = Utility.TflWs.GameFor(season, week, gameCode );
         return GetGameDate(ds);
      }

      /// <summary>
      ///   Return the WeekKey SSSS:WW for a particular date
      /// </summary>
      /// <param name="theDate">the query date</param>
      /// <returns>WeekKey</returns>
      public string WeekKey(DateTime theDate)
      {
         while (!theDate.DayOfWeek.Equals(DayOfWeek.Sunday))
            theDate = theDate.AddDays(1);

         var dr = Utility.TflWs.GetWeekRecord(theDate);

         if ( dr == null )
         {
            var noWeek = "00";
            var theSeason = theDate.Year.ToString();
            return string.Format("{0}:{1}", theSeason, noWeek);
         }
         else
         {
            var season = dr["SEASON"].ToString();
            var week = dr["WEEK"].ToString();
            return string.Format("{0}:{1}", season, week);
         }
      }
   }
}
