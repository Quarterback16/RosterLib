namespace RosterLib
{
	public interface IGetPredictions
	{
		Prediction Get(string method, string season, string week, string gameCode);
	}
}
