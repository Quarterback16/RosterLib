using System;

namespace RosterLib
{
   class GordanPredictor : IPrognosticate
   {
		public bool AuditTrail { get; set; }

		public bool TakeActuals { get; set; }

   	#region IPrognosticate Members

      public NFLResult PredictGame(NFLGame game, IStorePredictions persistor, DateTime predictionDate)
      {
         if ( game.HomeNflTeam == null ) game.HomeNflTeam = Masters.Tm.GetTeam( game.Season + game.HomeTeam );
         if ( game.AwayNflTeam == null ) game.AwayNflTeam = Masters.Tm.GetTeam( game.Season + game.AwayTeam );

         var homeLetter = game.HomeNflTeam.LetterRating[ 0 ].Trim();
         var awayLetter = game.AwayNflTeam.LetterRating[ 0 ].Trim();
         //  home team
         var ratingGap = DanGordan.RatingGap( homeLetter, awayLetter );
         //  home teams get 2.5 points home field advantage
         ratingGap += 2.5M;
         //  if the rating gap is positive its a home win
         game.GordanLine = ratingGap;

         var decimalEquivalent = DanGordan.CalculateSpreadsAsDecimals( ratingGap );
         if ( ratingGap > 0 )
         {
            //  home win
            game.HomeDecEquivalent = decimalEquivalent; //  .5 pt win = .507 to home
            game.AwayDecEquivalent = 1.0M - decimalEquivalent; //              .493 to away
         }
         else
         {
            //  away win
            game.AwayDecEquivalent = decimalEquivalent; //  .5 pt win = .507 to away
            game.HomeDecEquivalent = 1.0M - decimalEquivalent; //              .493 to home
         }

         var homeScore = 21 + Convert.ToInt32( ( ratingGap/2 ) );
         var awayScore = 21 + Convert.ToInt32( ( ratingGap*-1 )/2 );

         return new NFLResult( game.HomeTeam, homeScore, game.AwayTeam, awayScore );
      }


   	#endregion
   }
}