namespace RosterLib
{
   public class SpreadTips
	{
		protected IStorePredictions Storer;

		public SpreadTips( IStorePredictions storer )
		{
			Storer = storer;
		}

		public void SaveTipsFor( string season )
		{
			var theSeason = new NflSeason( season );
			foreach ( var game in theSeason.GameList )
			{
				if ( game.Spread > 0 )
				{
					game.CalculateSpreadResult();
					Storer.StorePrediction( "bookie", game, game.BookieTip );
				}
			}

		}
	}
}
