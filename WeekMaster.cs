using System;

namespace RosterLib
{
   public class WeekMaster : CacheMaster, IWeekMaster
   {

      public NFLWeek GetWeek(string season, int week)
      {
         NFLWeek w;
         var weekKey = string.Format("{0}:{1:0#}", season, week);

         if (TheHt.ContainsKey(weekKey))
         {
            w = (NFLWeek)TheHt[weekKey];
            CacheHits++;
         }
         else
         {
            w = new NFLWeek( int.Parse(season), week, loadGames: true);
            PutWeek(w);
            CacheMisses++;
         }
         return w;
      }

      public NFLWeek PreviousWeek( NFLWeek theWeek  )
      {
         var previousWeekNo = theWeek.WeekNo - 1;
         var previousSeasonNo = theWeek.SeasonNo;
         if (previousWeekNo < 1)
         {
            previousWeekNo = Constants.K_WEEKS_IN_A_SEASON;
            previousSeasonNo--;
         }

         var previousWeek = GetWeek(previousSeasonNo.ToString(), previousWeekNo );

         return previousWeek;
      }

      private void PutWeek(NFLWeek w)
      {
         var weekKey = string.Format("{0}:{1:0#}", w.Season, w.WeekNo);
         TheHt.Add(weekKey, w);
         IsDirty = true;
      }
   }
}