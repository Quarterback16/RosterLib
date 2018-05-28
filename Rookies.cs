using RosterLib.Interfaces;
using RosterLib.ReportGenerators;
using System.Collections.Generic;

namespace RosterLib
{
	public class Rookies : RosterGridReport
	{
		public string LeagueCode { get; set; }

		public RookieReportGenerator Generator { get; set; }

		public List<RookieConfig> Configs { get; set; }

		public List<RosterGridLeague> Leagues { get; set; }

		public Rookies( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Rookies";
			SetLastRunDate();

			Configs = new List<RookieConfig>
			{
				new RookieConfig { Category = Constants.K_QUARTERBACK_CAT, Position = "QB" },
				new RookieConfig { Category = Constants.K_RUNNINGBACK_CAT, Position = "RB" },
				new RookieConfig { Category = Constants.K_RECEIVER_CAT, Position = "WR" },
				new RookieConfig { Category = Constants.K_RECEIVER_CAT, Position = "TE" },
				new RookieConfig { Category = Constants.K_KICKER_CAT, Position = "K" }
			};
			Leagues = new List<RosterGridLeague>
			{
				new RosterGridLeague
				{
					Id = Constants.K_LEAGUE_Gridstats_NFL1,
					Name = "Gridstats GS1"
				}
			};
			Generator = new RookieReportGenerator();
		}

		public override void RenderAsHtml()
		{
			foreach ( var league in Leagues )
			{
				LeagueCode = league.Id;
				foreach ( RookieConfig rpt in Configs )
				{
					Generator.GenerateRookieReport( rpt, LeagueCode, Season );
				}
			}
		}
	}

	public class RookieConfig
	{
		public string Category { get; set; }

		public string Position { get; set; }
	}
}