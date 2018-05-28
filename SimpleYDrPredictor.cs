using System;

namespace RosterLib
{
   /// <summary>
   ///   Predicts the YDr primarily for a RB
   /// </summary>
   public class SimpleYDrPredictor : IPredictYDr
   {

      public Int32 PredictYDr(NFLPlayer plyr, string season, int week)
      {
         int YDr = 0;
         //  starters only
         if (plyr.IsStarter() && (plyr.PlayerCat == RosterLib.Constants.K_RUNNINGBACK_CAT) && plyr.IsRusher())
         {
            int gamesOppPlayed = 0;
            NflTeam opponent = plyr.CurrTeam.OpponentFor(season, week);
            if (opponent != null)  //  not on a bye
            {
               //  Workout Team YDr average
               int teamTotYDr = 0;
               int gamesPlayed = 0;
               plyr.CurrTeam.LoadGames(plyr.CurrTeam.TeamCode, season);
               foreach (NFLGame g in plyr.CurrTeam.GameList)
               {
                  if (g.Played())
                  {
                     g.TallyMetrics("Y");
                     teamTotYDr += g.IsHome(plyr.CurrTeam.TeamCode) ? g.HomeYDr : g.AwayYDr;
                     gamesPlayed++;
                  }
               }

               //  work out the average that the oppenent gives up
               int teamTotYDrAllowed = 0;

               opponent.LoadGames(opponent.TeamCode, season);
               foreach (NFLGame g in opponent.GameList)
               {
                  if (g.Played())
                  {
                     g.TallyMetrics("Y");
                     teamTotYDrAllowed += g.IsHome(plyr.CurrTeam.TeamCode) ? g.AwayYDr : g.HomeYDr;  //  switch it around
                     gamesOppPlayed++;
                  }
               }

               Decimal predictedYDr;
               if ((gamesPlayed > 0) && (gamesOppPlayed > 0))
               {
                  //  Average the averages

                  predictedYDr = ((teamTotYDr / gamesPlayed) +
                                           (teamTotYDrAllowed / gamesOppPlayed)) / 2;
               }
               else
                  predictedYDr = 0.0M;

               //  find out how many carries the starter usually has
               //  YDr is the proportion of the whole
               YDr = Convert.ToInt32(predictedYDr * .8M);
            }
         }
         return YDr;
      }

      

   }
}



