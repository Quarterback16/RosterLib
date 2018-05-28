using System;

namespace RosterLib
{

   /// <summary>
   /// I evaluate a player using a certain scoring system.
   /// </summary>
   public interface IPredictFGs
   {
      Int32 PredictFGs(NFLPlayer plyr, string season, int week );

   }


}