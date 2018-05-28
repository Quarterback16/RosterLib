using System;

namespace RosterLib
{
	/// <summary>
	/// Summary description for IPrognosticate.
	/// </summary>
	public interface IPrognosticate
	{
      NFLResult PredictGame(NFLGame game, IStorePredictions persistor, DateTime predictionDate);

		bool AuditTrail
		{
			get;
			set;
		}

		bool TakeActuals
		{
			get;
			set;
		}
	}
}
