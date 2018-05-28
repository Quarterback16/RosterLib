
using System;

namespace RosterLib
{
   /// <summary>
   ///   Predicts the TDcs primarily for a WR
   /// </summary>
   public class SimpleTDcPredictor : IPredictTDc
   {

      public Int32 PredictTDc(NFLPlayer plyr, string season, int week)
      {
         //  Predict the number of FGs this player will kick
         int tdc = 0;
         //  starters only
         if (plyr.IsStarter() && (plyr.PlayerCat == RosterLib.Constants.K_RECEIVER_CAT))
         {
            if (plyr.CurrTeam.Ratings.Equals("CCCCCC")) plyr.CurrTeam.SetRecord(season, skipPostseason: false);
            
            //  who are the opponents
            NflTeam opponent = plyr.CurrTeam.OpponentFor(season, week);
            if (opponent != null)
            {
               if (opponent.Ratings.Equals("CCCCCC")) opponent.SetRecord(season, skipPostseason: false);  // Incase not initialised
               //  not on a bye
               tdc = 1;
               int diff = ConvertRating(plyr.CurrTeam.PoRating()) - ConvertRating(opponent.PdRating());
               if (diff > 0) tdc += 1;
               if (diff > 2) tdc += 1;
               if (diff < -2) tdc -= 1;
            }

            //  What is the Game
            NFLGame game = plyr.CurrTeam.GameFor(season, week);
            if (game != null)
            {
               if (game.IsHome(plyr.CurrTeam.TeamCode)) tdc += 1;
               if (game.IsBadWeather()) tdc -= 1;
            }
            tdc = plyr.IsTe() ? Convert.ToInt32( tdc*.25M ) : Convert.ToInt32( tdc*.5M );
         }
         return tdc;
      }
      
      private static int ConvertRating( string rating )
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


