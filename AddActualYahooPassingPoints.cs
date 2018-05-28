using System;


namespace RosterLib
{
	/// <summary>
	///  filter for calculating yahoo points
	/// </summary>
	public class AddActualYahooPassingPoints
	{
		public AddActualYahooPassingPoints(YahooProjectedPointsMessage input)
		{
#if DEBUG
			Utility.Announce(string.Format("Calculating Actual Passing Points for {0} Game {1}",
				input.Player.PlayerNameShort, input.Game.GameName()));
#endif
			Process(input);
		}

		private static void Process(YahooProjectedPointsMessage input)
		{
			input.Player.Points += input.PlayerGameMetrics.TDp * 4;
#if DEBUG
			Utility.Announce(string.Format("TDp = {0} * 4 = {1}", input.PlayerGameMetrics.TDp, input.PlayerGameMetrics.TDp * 4));
#endif
			var yardagePts = Math.Floor((decimal) input.PlayerGameMetrics.YDp / 25);
#if DEBUG
			Utility.Announce(string.Format("YDp = {0} / 25 = {1}", input.PlayerGameMetrics.YDp, input.PlayerGameMetrics.YDp / 25));
#endif
			input.Player.Points += yardagePts;
			//TODO:  -1 for Interceptions
			//TODO:  +2 per PAT pass
#if DEBUG
			Utility.Announce(string.Format("FP = {0}", input.Player.Points));
#endif
		}

	}
}
