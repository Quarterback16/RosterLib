using RosterLib.Interfaces;
using System;

namespace RosterLib
{
	public class SeasonProjectionReport : RosterGridReport
	{
		public NFLRosterReport RosterReport { get; set; }

		public string MetricName { get; set; }
		public string Week { get; set; }

		public SeasonProjectionReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Season = timekeeper.CurrentSeason( DateTime.Now );
			Week = $"{timekeeper.CurrentWeek( DateTime.Now ):00}";
		}

		public override void RenderAsHtml()
		{
			Name = "Season Projections";
			RosterReport = new NFLRosterReport( Season );
			MetricName = "Spread";
			RosterReport.SeasonProjection( MetricName, Season, Week, DateTime.Now );
			SetLastRunDate();
		}

		public override string OutputFilename()
		{
			var fileName =
				$"{Utility.OutputDirectory()}{Season}\\Projections\\Proj-{MetricName}-{Season}.htm";
			return fileName;
		}
	}
}