using System;

namespace RosterLib
{

   /// <summary>
   /// I predict the Tdp in a week.
   /// </summary>
   public interface IPredictTDr
   {
      Int32 PredictTDr(NFLPlayer plyr, string season, int week);

   }


}
