using NLog;
using RosterLib.Interfaces;
using System;
using System.Collections.Generic;

namespace RosterLib
{
	public class PerformanceReportGenerator : RosterGridReport
	{
		public PlayerLister Lister { get; set; }

		public List<PerformanceReportConfig> Configs { get; set; }

		public List<RosterGridLeague> Leagues { get; set; }

		public PerformanceReportGenerator( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Logger = LogManager.GetCurrentClassLogger();
			Name = "Fantasy Performance Reports";
			Lister = new PlayerLister( timekeeper )
			{
				WeeksToGoBack = 1,
				StartersOnly = false
			};

			var master = new YahooMaster( "Yahoo", "YahooOutput.xml" );
			Logger.Trace( "  using {0} which has {1} stats", master.Filename, master.TheHt.Count );

			var theWeek =
			   new NFLWeek( Int32.Parse( timekeeper.CurrentSeason() ),
			   weekIn: Int32.Parse( timekeeper.PreviousWeek() ),
			   loadGames: false );

			var gs = new EspnScorer( theWeek ) { Master = master };

			Configs = new List<PerformanceReportConfig>
			{
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_QUARTERBACK_CAT,
					 Position = "QB",
					 Scorer = gs,
					 Week = theWeek
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RUNNINGBACK_CAT,
					 Position = "RB",
					 Scorer = gs,
					 Week = theWeek
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RECEIVER_CAT,
					 Position = "WR",
					 Scorer = gs,
					 Week = theWeek
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RECEIVER_CAT,
					 Position = "TE",
					 Scorer = gs,
					 Week = theWeek
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_KICKER_CAT,
					 Position = "PK",
					 Scorer = gs,
					 Week = theWeek
				  },
               //  4 weeks back
               new PerformanceReportConfig
				  {
					 Category = Constants.K_QUARTERBACK_CAT,
					 Position = "QB",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 4
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RUNNINGBACK_CAT,
					 Position = "RB",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 4
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RECEIVER_CAT,
					 Position = "WR",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 4
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RECEIVER_CAT,
					 Position = "TE",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 4
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_KICKER_CAT,
					 Position = "PK",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 4
				  },

               //  1 week back
               new PerformanceReportConfig
				  {
					 Category = Constants.K_QUARTERBACK_CAT,
					 Position = "QB",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 1
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RUNNINGBACK_CAT,
					 Position = "RB",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 1
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RECEIVER_CAT,
					 Position = "WR",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 1
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_RECEIVER_CAT,
					 Position = "TE",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 1
				  },
			   new PerformanceReportConfig
				  {
					 Category = Constants.K_KICKER_CAT,
					 Position = "PK",
					 Scorer = gs,
					 Week = theWeek,
					 WeeksToGoBack = 1
				  },
			};

			Leagues = new List<RosterGridLeague>();
			Leagues.Add( new RosterGridLeague { Id = Constants.K_LEAGUE_Yahoo, Name = "Spitzys League" } );
#if !DEBUG
         Leagues.Add( new RosterGridLeague { Id = Constants.K_LEAGUE_Gridstats_NFL1, Name = "Gridstats GS1" } );
         //Leagues.Add( new RosterGridLeague { Id = Constants.K_LEAGUE_Rants_n_Raves, Name = "NFL.COM" } );
#endif
		}

		public override void RenderAsHtml()
		{
			foreach ( var league in Leagues )
			{
				foreach ( var rpt in Configs )
				{
					GenerateReport( rpt, league.Id );
					Console.WriteLine( "Done {0}", FileOut );
				}
			}
		}

		public void GenerateReport( PerformanceReportConfig rpt, string leagueId )
		{
			Lister.SetScorer( rpt.Scorer );
			Lister.SetFormat( "weekly" );
			Lister.AllWeeks = false; //  just the regular saeason
			Lister.Season = rpt.Week.Season;
			Lister.RenderToCsv = false;
			Lister.Week = rpt.Week.WeekNo;
			Lister.Collect( rpt.Category, sPos: rpt.Position, fantasyLeague: leagueId );
			Lister.WeeksToGoBack = rpt.WeeksToGoBack;
			string targetFile;
			if ( rpt.WeeksToGoBack > 0 )
				targetFile = string.Format( "{4}{3}//Performance//{2}-Yahoo {1} Performance last {5} upto Week {0:0#}.htm",
				rpt.Week.WeekNo, rpt.Position, leagueId, Lister.Season, Utility.OutputDirectory(), rpt.WeeksToGoBack );
			else
				targetFile = string.Format( "{4}{3}//Performance//{2}-Yahoo {1} Performance upto Week {0:0#}.htm",
				rpt.Week.WeekNo, rpt.Position, leagueId, Lister.Season, Utility.OutputDirectory() );
			Lister.Render( targetFile );
			FileOut = targetFile;
			Lister.Clear();
		}

		public override string OutputFilename()
		{
			return FileOut;
		}
	}

	public class PerformanceReportConfig
	{
		public IRatePlayers Scorer { get; set; }

		public NFLWeek Week { get; set; }

		public string Category { get; set; }

		public string Position { get; set; }

		public int WeeksToGoBack { get; set; }
	}
}