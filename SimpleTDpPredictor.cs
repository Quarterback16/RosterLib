using System;

namespace RosterLib
{
   /// <summary>
   ///   Predicts the TDps primarily for a QB
   /// </summary>
   public class SimpleTDpPredictor : IPredictTDp
   {

      public Int32 PredictTDp(NFLPlayer plyr, string season, int week)
      {
         //  Predict the number of FGs this player will kick
         int tdp = 0;
         //  starters only
         if (plyr.IsStarter() && (plyr.PlayerCat == RosterLib.Constants.K_QUARTERBACK_CAT))
         {
            if (plyr.CurrTeam.Ratings.Equals("CCCCCC")) plyr.CurrTeam.SetRecord(season, skipPostseason: false);
            
            //  who are the opponents
            NflTeam opponent = plyr.CurrTeam.OpponentFor(season, week);
            if (opponent != null)
            {
               if (opponent.Ratings.Equals("CCCCCC")) opponent.SetRecord(season, skipPostseason: false);  // Incase not initialised
               //  not on a bye
               tdp = 1;
               int diff = ConvertRating(plyr.CurrTeam.PoRating()) - ConvertRating(opponent.PdRating());
               if (diff > 0) tdp += 1;
               if (diff > 2) tdp += 1;
               if (diff < -2) tdp -= 1;
            }

            //  What is the Game
            NFLGame game = plyr.CurrTeam.GameFor(season, week);
            if (game != null)
            {
               if (game.IsHome(plyr.CurrTeam.TeamCode)) tdp += 1;
               if (game.IsBadWeather()) tdp -= 1;
            }

         }
         return tdp;
      }
      
      private int ConvertRating( string rating )
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


