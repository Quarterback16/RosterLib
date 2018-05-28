
using System;

namespace RosterLib
{

   /// <summary>
   /// I predict the YPc in a week.
   /// </summary>
   public interface IPredictYDc
   {
      Int32 PredictYDc(NFLPlayer plyr, string season, int week);

   }


}
