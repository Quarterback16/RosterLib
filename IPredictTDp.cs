using System;

namespace RosterLib
{

   /// <summary>
   /// I predict the TDp in a week.
   /// </summary>
   public interface IPredictTDp
   {
      Int32 PredictTDp(NFLPlayer plyr, string season, int week);

   }


}
