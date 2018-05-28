using System;
using TFLLib;

namespace RosterLib
{
   /// <summary>
   /// Summary description for Nibble Predictor Lock Scheme.
   /// </summary>
   public class OverUnderScheme : IScheme
   {
      protected DataLibrarian TflWs;
   	private const decimal Marg = 3.0M;
   	private decimal _diff;

		#region  Accessors

   	public string Name { get; set; }

   	public int M_wins { get; set; }

   	public int Losses { get; set; }

   	public int Pushes { get; set; }

   	#endregion

      public OverUnderScheme( DataLibrarian tflWsIn ) 
      {
      	Pushes = 0;
      	Losses = 0;
      	M_wins = 0;
      	//   
         //   If the variance is greater than 3 we have a bet
         //
         TflWs = tflWsIn;
         Name = "Over Under";
      }

      public NFLBet IsBettable( NFLGame game )
      {
         NFLBet bet = null;
			var bOver = false;

			if ( Decimal.Compare( game.Total, 0M ) != 0 )
			{
				var pred = new NibblePredictor();
				//  Predict game
				var res = pred.PredictGame( game, new FakePredictionStorer(), DateTime.Now);
				var resultTotal = res.AwayScore + res.HomeScore;
				//  if differs from spread by 10 or more its a lock
				_diff = Math.Abs( game.Total - resultTotal );
				if ( Decimal.Compare( _diff, Marg ) > 0 )
				{
					var typeBet = resultTotal.ToString();
					if ( resultTotal > game.Total )
					{
						typeBet += " Over";
						bOver = true;
					}
					else
						typeBet += " Under";
					typeBet += string.Format( " ({0}) by {1}", game.Total, _diff );

					bet = new NFLBet(res.WinningTeam(), game, typeBet, ConfidenceLevel()) {Type = BetType.Total};
					if ( bOver )
						bet.Over = true;
					else
						bet.Under = true;
				}
			}
         return bet;
      }

      public Confidence ConfidenceLevel()
      {
			var myConfidence = Confidence.Good;
			if ( Decimal.Compare( _diff, 6.5M ) > 0 )
				myConfidence = Confidence.VeryGood;
			if ( Decimal.Compare( _diff, 9.5M ) > 0 )
				myConfidence = Confidence.High;
         return myConfidence;
      }

   	public decimal BackTest()
   	{
			return 0.0M;
   	}

   }
}

