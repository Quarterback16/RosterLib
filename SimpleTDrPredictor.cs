using System;

namespace RosterLib
{
   /// <summary>
   ///   Predicts the TDrs primarily for a RB
   /// </summary>
   public class SimpleTDrPredictor : IPredictTDr
   {

      public Int32 PredictTDr(NFLPlayer plyr, string season, int week)
      {
         //  Predict the number of FGs this player will kick
         int tDr = 0;
         //  starters only
         if (plyr.IsStarter() && (plyr.PlayerCat == RosterLib.Constants.K_RUNNINGBACK_CAT) && plyr.IsRusher() )
         {
            if (plyr.CurrTeam.Ratings.Equals("CCCCCC")) plyr.CurrTeam.SetRecord(season, skipPostseason:false);

            //  who are the opponents
            NflTeam opponent = plyr.CurrTeam.OpponentFor(season, week);
            if (opponent != null)
            {
               if (opponent.Ratings.Equals("CCCCCC")) opponent.SetRecord(season, skipPostseason: false);  // Incase not initialised
               //  not on a bye
               tDr = 1;
               int diff = ConvertRating(plyr.CurrTeam.RoRating()) - ConvertRating(opponent.RdRating());
               if (diff > 1) tDr += 1;
               if (diff > 3) tDr += 1;
               if (diff < -1) tDr -= 1;
            }

            //  What is the Game
            NFLGame game = plyr.CurrTeam.GameFor(season, week);
            if (game != null)
            {
               if (game.IsHome(plyr.CurrTeam.TeamCode)) tDr += 1;
               if (game.IsBadWeather()) tDr -= 1;
            }

         }
         return tDr;
      }

      private int ConvertRating(string rating)
      {
         int val;
         switch (rating)
         {
            case "A":
               val = 5;
               break;
            case "B":
               val = 4;
               break;
            case "C":
               val = 4;
               break;
            case "D":
               val = 4;
               break;
            default:
               val = 1;
               break;
         }
         return val;
      }

   }
}


