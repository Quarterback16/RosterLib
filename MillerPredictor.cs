using System;

namespace RosterLib
{
   /// <summary>
   ///   Uses the simple algorithm documented by J R Miller in his book -
   ///   "How Professional Gamblers Beat the Pro Football Pointspread"
   ///    Expect > 60% straight up and > 54% ATS
   /// </summary>
   public class MillerPredictor : IPrognosticate
   {
      #region IPrognosticate Members

      public bool AuditTrail { get; set; }
      public bool TakeActuals { get; set; }

      public NFLResult PredictGame(NFLGame game, IStorePredictions persistor, DateTime predictionDate)
      {
         var result = new NFLResult {AwayTeam = game.AwayTeam, HomeTeam = game.HomeTeam};

         var homeTeam = game.HomeNflTeam;
         var awayTeam = game.AwayNflTeam;

         var nHomeOff = Utility.OffRating(homeTeam, predictionDate);
         var nAwayOff = Utility.OffRating(awayTeam, predictionDate);
         var nHomeDef = Utility.DefRating(homeTeam, predictionDate);
         var nAwayDef = Utility.DefRating(awayTeam, predictionDate);
         result.HomeScore = (nHomeOff + nAwayDef) - 20;
         result.AwayScore = (nAwayOff + nHomeDef) - 20;

         persistor.StorePrediction("Miller", game, result);

         if (AuditTrail)
            Utility.Announce(string.Format("{0}-{1}:{2}", game.GameCodeOut(), game.GameCode, result.LogResult()));

         return result;
      }

      #endregion
   }
}