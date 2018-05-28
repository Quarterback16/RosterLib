using RosterLib.Interfaces;
using System;

namespace RosterLib
{
	public class PlayerCareerReport : RosterGridReport
	{
		public NFLRosterReport RosterReport { get; set; }

		public int ReportsToDo { get; private set; }

		public PlayerCareerReport( IKeepTheTime timekeeper, int numberOfReportsTodo = 20) : base( timekeeper )
		{
			Season = timekeeper.CurrentSeason( DateTime.Now );
			ReportsToDo = numberOfReportsTodo;
		}

		public override void RenderAsHtml()
		{
			Name = "Career Reports";
			RosterReport = new NFLRosterReport( Season );
			RosterReport.LoadAfc();
			RosterReport.PlayerReports( ReportsToDo, TimeKeeper );
		}

		public override string OutputFilename()
		{
			var fileName = $"{Utility.OutputDirectory()}\\Players\\Errors.htm";
			return fileName;
		}
	}
}