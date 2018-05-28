using System;

namespace RosterLib
{

   /// <summary>
   /// I predict defensive stats.
   /// </summary>
   public interface IPredictDefensiveTeams
   {
      Int32 PredictSacks( NflTeam t, string season, int week );
      Int32 PredictSteals( NflTeam t, string season, int week);

   }


}
