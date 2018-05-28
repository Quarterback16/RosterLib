using System;


namespace RosterLib
{
	/// <summary>
	///  filter for calculating yahoo points
	/// </summary>
	public class AddActualYahooRushingPoints
	{
		public AddActualYahooRushingPoints(YahooProjectedPointsMessage input)
		{
#if DEBUG
			Utility.Announce(string.Format("Calculating Rushing Points for {0} Game {1}",
				input.Player.PlayerNameShort, input.Game.GameName()));
#endif
			Process(input);
		}

		private void Process(YahooProjectedPointsMessage input)
		{
			input.Player.Points += input.PlayerGameMetrics.TDr * 6;
#if DEBUG
			Utility.Announce(string.Format("TDr = {0} * 6 = {1}",
				input.PlayerGameMetrics.TDr, input.PlayerGameMetrics.TDr * 6));
#endif
			var yardagePts = Math.Floor((decimal) input.PlayerGameMetrics.YDr / 10);
#if DEBUG
			Utility.Announce(string.Format("YDr = {0} / 10 = {1}",
				input.PlayerGameMetrics.YDr, input.PlayerGameMetrics.YDr / 10));
#endif
			input.Player.Points += yardagePts;
			//TODO:  -2 for Fumbles
			//TODO:  +2 per PAT run
#if DEBUG
			Utility.Announce(string.Format("FP = {0}", input.Player.Points));
#endif
		}

	}
}
