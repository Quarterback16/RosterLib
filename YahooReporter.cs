namespace RosterLib
{
	public class YahooReporter
	{
		//  uses a pipeline to process a YahooProjectedPointsMessage
		public PipeLine<YahooProjectedPointsMessage> yahooPipeline { get; set; }

		public YahooProjectedPointsMessage Calculate(NFLPlayer p, NFLGame g)
		{
			if (yahooPipeline == null) InitialiseThePipeLine();

			var msg = new YahooProjectedPointsMessage { Player = p };
			if (g.IsBye()) return msg;

			msg.Player.Points = 0;
			msg.Game = g;

			if (yahooPipeline != null) yahooPipeline.Execute(msg);
			return msg;
		}

		private void InitialiseThePipeLine()
		{
			yahooPipeline = new PipeLine<YahooProjectedPointsMessage>();
			yahooPipeline.Register(msg => new LoadPlayerGameMetric(msg));
			yahooPipeline.Register(msg => new AddActualYahooPassingPoints(msg));
			yahooPipeline.Register(msg => new AddActualYahooRushingPoints(msg));
			yahooPipeline.Register(msg => new AddActualYahooReceivingPoints(msg));
			yahooPipeline.Register(msg => new AddActualYahooKickingPoints(msg));
		}
	}
}