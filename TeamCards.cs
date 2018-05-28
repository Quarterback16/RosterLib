using RosterLib.Interfaces;
using System.Collections.Generic;

namespace RosterLib
{
	public class TeamCards : RosterGridReport
	{
		public string LastOutput { get; set; }

		public NFLRosterReport RosterReport { get; set; }

		public List<RosterGridLeague> Leagues { get; set; }

		public bool DoPlayerReports { get; set; }

		public TeamCards( 
            IKeepTheTime timekeeper, 
            bool doPlayerReports ) : base( timekeeper )
		{
			Name = "Team Cards";
			DoPlayerReports = doPlayerReports;
			Leagues = new List<RosterGridLeague>
		   {
			  new RosterGridLeague
              {
                  Id = Constants.K_LEAGUE_Gridstats_NFL1,
                  Name = "Gridstats GS1"
              }
		   };
		}

		public override void RenderAsHtml()
		{
			RosterReport = new NFLRosterReport( Season ) {
                Season = Season
            };
			foreach ( var league in Leagues )
			{
				RosterReport.TeamCards();
			}
			if ( DoPlayerReports )
				RosterReport.PlayerReports( 100, TimeKeeper );
		}

		public override string OutputFilename()
		{
			var fileOut = $"{Utility.OutputDirectory()}{Season}/TeamCards/TeamCard_{"SF"}.htm";
			return fileOut;
		}
	}
}