using RosterLib.Interfaces;
using System.Data;
using System.Text;

namespace RosterLib.RosterGridReports
{
	public class PlayoffTeamsReport : RosterGridReport
	{
		public SimplePreReport Report { get; set; }

		public int Week { get; set; }

		public PlayoffTeamsReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Report = new SimplePreReport
			{
				ReportType = "Playoff Teams",
				Folder = "Playoffs",
				Season = timekeeper.Season,
				InstanceName = string.Format( "Playoff-Week-{0:0#}", timekeeper.Week )
			};
		}

		public override void RenderAsHtml()
		{
			Report.Body = GenerateBody();
			Report.RenderHtml();
			FileOut = Report.FileOut;
		}

		private string GenerateBody()
		{
			var bodyOut = new StringBuilder();

			var ds = Utility.TflWs.GetTeams( Utility.CurrentSeason() );
			var dt = ds.Tables[ 0 ];
			foreach ( DataRow dr in dt.Rows )
			{
				var theKey = dr[ "TEAMID" ].ToString();
				var team = new NflTeam( theKey );
				var inPlayoffs = false;
				if ( team.Above500() )
				{
					bodyOut.AppendLine( $"{team.NameOut(),-25} {team.RecordOut()}" );
					inPlayoffs = true;
				}
				Utility.TflWs.UpdatePlayoff( team.Season, team.TeamCode, inPlayoffs );
			}

			return bodyOut.ToString();
		}
	}
}