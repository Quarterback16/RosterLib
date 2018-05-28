using System;


namespace RosterLib
{
   class GordanWeeklyPredictor : IPrognosticate
   {
   	#region IPrognosticate Members

		public bool TakeActuals { get; set; }


      public NFLResult PredictGame(NFLGame game, IStorePredictions persistor, DateTime predictionDate)
      {
         if (game.HomeNflTeam == null) game.HomeNflTeam = Masters.Tm.GetTeam(game.Season + game.HomeTeam);
         if (game.AwayNflTeam == null) game.AwayNflTeam = Masters.Tm.GetTeam(game.Season + game.AwayTeam);

         game.HomeNflTeam.LetterRating = Masters.Sm.GetGordan(game.Season, game.Season + game.HomeTeam);
         game.AwayNflTeam.LetterRating = Masters.Sm.GetGordan(game.Season, game.Season + game.AwayTeam);

         var weekIndex = Int32.Parse(game.Week) - 1;
         var homeLetter = game.HomeNflTeam.LetterRating[weekIndex].Trim();
         var awayLetter = game.AwayNflTeam.LetterRating[weekIndex].Trim();
         //  home team
         var ratingGap = DanGordan.RatingGap(homeLetter, awayLetter);
         //  home teams get 2.5 points home field advantage
         ratingGap += 2.5M;
         //
         //  I give 3/4 strength to the letter power rating because it is adjusted every week and
         //  better takes into account a team's current form.  The number power ratings though more
         //  objective, are totally dependant on Scores, which at times can be deceptive.
         var homeNumberAvgRating = ( game.HomeNflTeam.NumberRating[ 0 ] + 
            game.HomeNflTeam.NumberRating[ weekIndex ] ) / 2.0M;
         var awayNumberAvgRating = ( game.AwayNflTeam.NumberRating[ 0 ] + 
            game.AwayNflTeam.NumberRating[ weekIndex ] ) / 2.0M;
         //  home advantage
         homeNumberAvgRating += 2.5M;
         var numberRating = ( homeNumberAvgRating - awayNumberAvgRating );

         //  if the rating gap is positive its a home win
         game.GordanLine = RoundToNearestHalfPoint((0.75M * ratingGap) + (0.25M * numberRating));

         var homeScore = 21 + Convert.ToInt32((ratingGap / 2));
         var awayScore = 21 + Convert.ToInt32((ratingGap * -1) / 2);

         return new NFLResult(game.HomeTeam, homeScore, game.AwayTeam, awayScore);
      }

   	public bool AuditTrail { get; set; }

   	#endregion

      private static decimal RoundToNearestHalfPoint(decimal line)
      {
         //  7.3 --> 7.5,  6.6 --> 7.0
         var doubleit = line * 2.0M;
         if (line > 0)
            doubleit += .5M;  // 15.1
         else
            doubleit -= .5M;
         //  INT() it
         var intIt = Convert.ToInt32(doubleit);  //  15
         var decIt = Convert.ToDecimal(intIt);  // 15.0
         return decIt / 2.0M;
      }
   }
}
