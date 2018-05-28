using System;

namespace RosterLib
{

   /// <summary>
   /// I predict the TDp in a week.
   /// </summary>
   public interface IPredictYDr
   {
      Int32 PredictYDr(NFLPlayer plyr, string season, int week);

   }


}