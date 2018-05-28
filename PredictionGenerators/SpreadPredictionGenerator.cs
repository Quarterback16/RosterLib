using System;

namespace RosterLib.PredictionGenerators
{
	public class SpreadPredictionGenerator
	{
		public NflSeason Season { get; private set; }

		private readonly IStorePredictions _predictionStorer;

		public SpreadPredictionGenerator(string season, IStorePredictions predictionStorer)
		{
			Season = new NflSeason( season );
			_predictionStorer = predictionStorer;
		}

		public void GeneratePredictions()
		{
			foreach ( var game in Season.GameList )
			{
				if ( game.Spread != 0 )
				{
					game.CalculateSpreadResult();
					_predictionStorer.StorePrediction( "bookie", game, game.BookieTip );
				}
				else
					Console.WriteLine( $"No spread for {game.GameName()}" );
			}
		}
	}
}
