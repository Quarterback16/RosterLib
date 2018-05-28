
using System;

namespace RosterLib
{
   /// <summary>
   ///   Predicts the YDr primarily for a RB
   /// </summary>
   public class SimpleYDcPredictor : IPredictYDc
   {

      public Int32 PredictYDc(NFLPlayer plyr, string season, int week)
      {
         int YDc = 0;
         //  starters only
         if (plyr.IsStarter() && (plyr.PlayerCat == RosterLib.Constants.K_RECEIVER_CAT))
         {
            int gamesOppPlayed = 0;
            NflTeam opponent = plyr.CurrTeam.OpponentFor(season, week);
            if (opponent != null)  //  not on a bye
            {
               //  Workout Team YDc average
               int teamTotYDc = 0;
               int gamesPlayed = 0;
               plyr.CurrTeam.LoadGames(plyr.CurrTeam.TeamCode, season);
               foreach (NFLGame g in plyr.CurrTeam.GameList)
               {
                  if (g.Played())
                  {
                     g.TallyMetrics("Y");
                     teamTotYDc += g.IsHome(plyr.CurrTeam.TeamCode) ? g.HomeYDp : g.AwayYDp;
                     gamesPlayed++;
                  }
               }

               //  work out the average that the oppenent gives up
               int teamTotYDcAllowed = 0;

               opponent.LoadGames(opponent.TeamCode, season);
               foreach (NFLGame g in opponent.GameList)
               {
                  if (g.Played())
                  {
                     g.TallyMetrics("Y");
                     teamTotYDcAllowed += g.IsHome(plyr.CurrTeam.TeamCode) ? g.AwayYDr : g.HomeYDr;  //  switch it around
                     gamesOppPlayed++;
                  }
               }

               Decimal predictedYDc;
               if ((gamesPlayed > 0) && (gamesOppPlayed > 0))
               {
                  //  Average the averages

                  predictedYDc = ((teamTotYDc / gamesPlayed) +
                                       (teamTotYDcAllowed / gamesOppPlayed)) / 2;
               }
               else
                  predictedYDc = 0.0M;

               //  find out how many carries the starter usually has
               //  YDr is the proportion of the whole
               YDc = plyr.IsTe() ? Convert.ToInt32( predictedYDc*.2M ) : Convert.ToInt32( predictedYDc*.4M );
            }
         }
         return YDc;
      }

      

   }
}



