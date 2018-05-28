using RosterLib.Interfaces;
using System.Collections.Generic;

namespace RosterLib
{
	/// <summary>
	///   A suite of sub reports, for each category
	/// </summary>
	public class OldRosterGridReports : RosterGridReport
	{
		public string LastOutput { get; set; }
		public NFLRosterGrid RosterGrid { get; set; }
		public List<RosterGridConfig> Configs { get; set; }
		public List<RosterGridLeague> Leagues { get; set; }

		public OldRosterGridReports(IKeepTheTime timekeeper) : base( timekeeper )
		{
			Name = "Old Roster Grids";
			RosterGrid = new NFLRosterGrid
			{
				Season = timekeeper.Season,
				CurrentLeague = Constants.K_LEAGUE_Gridstats_NFL1,
				Focus = "QB"
			};

			LastOutput = string.Format( "{0}{3}\\Rosters\\{1}\\Roster-{2}.htm",
				   Utility.OutputDirectory(), RosterGrid.CurrentLeague, RosterGrid.Focus, Season );

			Configs = new List<RosterGridConfig>
			{
				new RosterGridConfig { Category = Constants.K_QUARTERBACK_CAT, Focus = "QB" },
				new RosterGridConfig { Category = Constants.K_RUNNINGBACK_CAT, Focus = "RB" },
				new RosterGridConfig { Category = Constants.K_RECEIVER_CAT, Focus = "WR" },
				new RosterGridConfig { Category = Constants.K_RECEIVER_CAT, Focus = "TE" },
				new RosterGridConfig { Category = Constants.K_KICKER_CAT, Focus = "K" }
			};
			Leagues = new List<RosterGridLeague>
			{
				new RosterGridLeague { Id = Constants.K_LEAGUE_Gridstats_NFL1, Name = "Gridstats GS1" },
				new RosterGridLeague { Id = Constants.K_LEAGUE_Yahoo, Name = "Spitzys League" }
			};
		}

		public override void RenderAsHtml()
		{
			foreach ( var league in Leagues )
			{
				RosterGrid.CurrentLeague = league.Id;
				foreach ( var rpt in Configs )
					GenerateReport( rpt );
			}
		}

		private void GenerateReport( RosterGridConfig rpt )
		{
			Utility.CurrentLeague = RosterGrid.CurrentLeague;
			RosterGrid.Focus = rpt.Focus;
			RosterGrid.StrCat = rpt.Category;
			RosterGrid.LoadDivisions();  //  gets the player data
			LastOutput = RosterGrid.CurrentRoster();
		}

		public override string OutputFilename()
		{
			return LastOutput;
		}
	}

	public class RosterGridConfig
	{
		public string Category { get; set; }
		public string Focus { get; set; }
		public bool DoInjuries { get; set; }
	}

	public class RosterGridLeague
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}
}
