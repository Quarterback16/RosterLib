namespace RosterLib
{
   public class MillerTips
	{
		public void TipSeason( string season )
		{
			//  load a season
			var nflSeason = new NflSeason( season );
			var predictionStorer = new DbfPredictionStorer();
			var mp = new MillerPredictor { AuditTrail = true };

			//  for each game
			foreach ( NFLGame game in nflSeason.GameList )
			{
				if ( ! game.IsPlayoff() )
				{
					//    predict game
					var result = mp.PredictGame( game, predictionStorer, game.GameDate );
				}
			}
		}
	}
}
