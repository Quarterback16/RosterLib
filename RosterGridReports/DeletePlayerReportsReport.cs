using RosterLib.Interfaces;
using System;
using System.Text;

namespace RosterLib.RosterGridReports
{
	public class DeletePlayerReportsReport : RosterGridReport
	{
		public NFLRosterReport RosterReport { get; set; }

		public SimplePreReport PreReport { get; set; }

		public DeletePlayerReportsReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Delete-Stale-Player-Reports";
			Season = timekeeper.CurrentSeason( DateTime.Now );
			PreReport = new SimplePreReport
			{
				ReportType = Name,
				Folder = "Players",
				Season = Season,
				InstanceName = $"PlayerReportsDeleted-{Season}"
			};
		}

		public override void RenderAsHtml()
		{
			var body = new StringBuilder();
			RosterReport = new NFLRosterReport( Season );
			RosterReport.LoadAfc();
			RosterReport.DeletePlayerReports(body);

			OutputReport( body.ToString() );
			Finish();
		}

		private void OutputReport( string body )
		{
			PreReport.Body = body;
			PreReport.RenderHtml();
			FileOut = PreReport.FileOut;
		}

		public override string OutputFilename()
		{
			return $"{Utility.OutputDirectory()}{Season}/{PreReport.Folder}/{PreReport.InstanceName}.htm";
		}
	}
}
