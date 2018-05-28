using System;

namespace RosterLib
{
	public class SimplePredictor : IPrognosticate
	{
		#region IPrognosticate Members

		public bool AuditTrail { get; set; }

		public bool TakeActuals { get; set; }


		public NFLResult PredictGame(NFLGame game, IStorePredictions persistor, DateTime predictionDate)
		{
			var res = game.Played() ? new NFLResult( game.HomeTeam, game.HomeScore, game.AwayTeam, game.AwayScore ) 
			                	: new NFLResult( game.HomeTeam, 21, game.AwayTeam, 17 );

			return res;
		}

		#endregion
	}
}
