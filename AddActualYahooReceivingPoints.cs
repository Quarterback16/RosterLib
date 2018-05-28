using System;


namespace RosterLib
{
	/// <summary>
	///  filter for calculating yahoo points
	/// </summary>
	public class AddActualYahooReceivingPoints
	{
		public AddActualYahooReceivingPoints(YahooProjectedPointsMessage input)
		{
#if DEBUG
			Utility.Announce(string.Format("Calculating Receiving Points for {0} Game {1}",
				input.Player.PlayerNameShort, input.Game.GameName()));
#endif
			Process(input);
		}

		private static void Process(YahooProjectedPointsMessage input)
		{
			input.Player.Points += input.PlayerGameMetrics.TDc * 6;
#if DEBUG
			Utility.Announce(string.Format("TDc = {0} * 6 = {1}",
				input.PlayerGameMetrics.TDc, input.PlayerGameMetrics.TDc * 6));
#endif
			var yardagePts = Math.Floor((decimal) input.PlayerGameMetrics.YDc / 10);
#if DEBUG
			Utility.Announce(string.Format("YDc = {0} / 10 = {1}",
				input.PlayerGameMetrics.YDc, input.PlayerGameMetrics.YDc / 10));
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
