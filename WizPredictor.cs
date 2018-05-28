using System;

namespace RosterLib
{
	/// <summary>
	///  A tipping algorithm based on the Wizardry Experience Points
	///  rating system.  EP Points differential is used to guess the winner
	/// </summary>
	public class WizPredictor : IPrognosticate
	{
		public bool AuditTrail { get; set; }

		public bool TakeActuals { get; set; }


		public NFLResult PredictGame(NFLGame game, IStorePredictions persistor, DateTime predictionDate)
      {
         NFLResult res;

         //RosterLib.Utility.Announce( string.Format( "WizPredictor.PredictGame: Wk{2} {0}@{1}", 
         //   game.AwayTeam, game.HomeTeam, game.Week ) );

			if ( game.Played() )
				res = new NFLResult( game.HomeTeam, game.HomeScore, game.AwayTeam, game.AwayScore );
			else
			{
				if ( game.HomeNflTeam == null ) game.HomeNflTeam = Masters.Tm.GetTeam( game.Season, game.HomeTeam );
				if ( game.AwayNflTeam == null ) game.AwayNflTeam = Masters.Tm.GetTeam( game.Season, game.AwayTeam );
				var homeMatrix = game.HomeNflTeam.GetMatrix();
				var awayMatrix = game.AwayNflTeam.GetMatrix();

				//  Homescore = Tdp*7 + Tdr*7 + 3
				var homePointsPassing = Tdp( homeMatrix.PoPoints - awayMatrix.PdPoints );
				var homePointsRunning = Tdr( homeMatrix.RoPoints - awayMatrix.RdPoints );
				var homeScore = homePointsPassing + homePointsRunning + 3;

				//  Awayscore = Tdp*7 + Tdr*7
				var awayPointsPassing = Tdp( awayMatrix.PoPoints - homeMatrix.PdPoints );
				var awayPointsRunning = Tdr( awayMatrix.RoPoints - homeMatrix.RdPoints );
				var awayScore = awayPointsPassing + awayPointsRunning;

				if ( homeScore == awayScore ) homeScore++;  //  no ties

				res = new NFLResult(game.HomeTeam, homeScore, game.AwayTeam, awayScore)
				      	{
				      		AwayTDp = (awayPointsPassing/7),
				      		HomeTDp = (homePointsPassing/7),
				      		AwayTDr = (awayPointsRunning/7),
				      		HomeTDr = (homePointsRunning/7)
				      	};
				if ( AuditTrail )
				{
					Utility.Announce( string.Format( "Wiz Prediction Wk{4} {0}@{1} {2}-{3} ", 
						game.AwayTeam, game.HomeTeam, res.AwayScore, res.HomeScore, game.Week ) );
					Utility.Announce( string.Format( "  hTDr({0}) hTDp({1}) aTDr({2}) aTDp({3})",
						res.HomeTDr, res.HomeTDp, res.AwayTDr, res.AwayTDp ));
					Utility.Announce( string.Format( "  hPOpts({0:#.#}) hROpts({1:#.#}) aRDpts({2:#.#}) aPDpts({3:#.#})",
						homeMatrix.PoPoints, homeMatrix.RoPoints, awayMatrix.RdPoints, awayMatrix.PdPoints ));
					Utility.Announce( string.Format( "  aPOpts({0:#.#}) aROpts({1:#.#}) hRDpts({2:#.#}) hPDpts({3:#.#})",
						awayMatrix.PoPoints, awayMatrix.RoPoints, homeMatrix.RdPoints, homeMatrix.PdPoints ));
				}
			}
         return res;
      }

		
		/// <summary>
		///   These numbers have to be calibrated based on the differential frequencies.
		///   Total TDps should sum about 650
		/// </summary>
		/// <param name="differential">The differential.</param>
		/// <returns></returns>
      private static int Tdp( decimal differential )
      {
			//  differnential needs to be pro-rataed out to a full season
			//  for the differentials (based on the whole of last season)
			//  to be meaningfull
			if ( Utility.CurrentWeek() != "0" )
			{
				//  We are mid season
				decimal seasonGone = ( Decimal.Parse( Utility.CurrentWeek() )/17.0M );
				decimal multiplier = 1.0M/seasonGone;
				differential = differential*multiplier;
			}
			var points = 0;
         if (differential > 2.0M)
         {
            if (differential > 7.0M)
            {
               if (differential > 10.0M)
               {
                  if (differential > 15.0M)
                  {
                  	if (differential > 18.0M)
                  		points = (5*7);
                  	else
                  		points = (4*7);
                  }
						else
							points = ( 3*7 );
					}
					else
						points = ( 2*7 );
				}
				else
					points = 7;
			}
			return points;
      }

      /// <summary>
      ///   TDs should total around 425
      /// </summary>
      /// <param name="differential"></param>
      /// <returns></returns>
      private static int Tdr( decimal differential )
      {
         var points = 0;
         if ( differential > -5.0M )
         {
            if ( differential > 0.0M )
            {
					if ( differential > 3.0M )
					{
						if ( differential > 8.0M )
						{
							if (differential > 13.0M)
								points = (5*7);
							else
								points = (4*7);
						}
						else
							points = ( 3*7 );
					}
            	else
						points = ( 2*7 );
            }
            else
               points = 7;
         }
         return points;
      }
	}

}
