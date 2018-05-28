using RosterLib.Interfaces;
using System;
using System.Text;

namespace RosterLib.RosterGridReports
{
	public class RetirePlayersReport : RosterGridReport
	{
		public NFLRosterReport RosterReport { get; set; }

		public SimplePreReport PreReport { get; set; }

		public RetirePlayersReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Retire-Player-Reports";
			Season = timekeeper.CurrentSeason( DateTime.Now );
			PreReport = new SimplePreReport
			{
				ReportType = Name,
				Folder = "Players",
				Season = Season,
				InstanceName = $"PlayersProbablyRetired-{Season}"
			};
		}

		public override void RenderAsHtml()
		{
			var body = new StringBuilder();
			RosterReport = new NFLRosterReport( Season );
			RosterReport.LoadAfc();
			RosterReport.RetirePlayers( body );

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
