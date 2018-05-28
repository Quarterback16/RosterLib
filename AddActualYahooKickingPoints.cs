namespace RosterLib
{
	public class AddActualYahooKickingPoints
	{
		public AddActualYahooKickingPoints(YahooProjectedPointsMessage input)
		{
#if DEBUG
			Utility.Announce(string.Format("Calculating Kicking Points for {0} Game {1}",
				input.Player.PlayerNameShort, input.Game.GameName()));
#endif
			Process(input);
		}

		private static void Process(YahooProjectedPointsMessage input)
		{
			input.Player.Points += input.PlayerGameMetrics.FG * 3;
#if DEBUG
			Utility.Announce(string.Format("FG = {0} * 3 = {1}",
				input.PlayerGameMetrics.FG, input.PlayerGameMetrics.FG * 3));
#endif
			input.Player.Points += input.PlayerGameMetrics.Pat * 1;
#if DEBUG
			Utility.Announce(string.Format("Projected Pat = {0} * 1 = {1}",
				input.PlayerGameMetrics.Pat, input.PlayerGameMetrics.Pat * 1));
#endif
#if DEBUG
			Utility.Announce(string.Format("FP = {0}", input.Player.Points));
#endif
		}
	}
}
