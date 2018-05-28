namespace RosterLib
{
	public interface IStorePredictions
	{
		void StorePrediction( string method, NFLGame game, NFLResult result );
	}
}
