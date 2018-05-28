using System;

namespace RosterLib
{

   /// <summary>
   /// I predict the Tdp in a week.
   /// </summary>
   public interface IPredictTDc
   {
      Int32 PredictTDc(NFLPlayer plyr, string season, int week);

   }


}

