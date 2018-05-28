using System;
using System.Collections.Generic;
using System.Globalization;
using RosterLib.Interfaces;

namespace RosterLib
{
   public class FantasyProjectionReporter : RosterGridReport
   {
      public FantasyProjectionReport FPR { get; set; }
      public List<RosterGridLeague> Leagues { get; set; }

      public List<FantasyProjectionReportConfig> Configs { get; set; }

      public string Week { get; set; }

      public IRatePlayers Scorer { get; set; }

      public FantasyProjectionReporter(  IKeepTheTime timekeeper) : base( timekeeper )
		{
         Name = "Fantasy Point Projections";
         TimeKeeper = timekeeper;
         var dao = new DbfPlayerGameMetricsDao();
         var scorer = new YahooProjectionScorer();
         var theWeek = TimeKeeper.CurrentWeek( DateTime.Now );
         if (theWeek.Equals( 0 )) theWeek = 1;

         Configs = new List<FantasyProjectionReportConfig>
         {
            new FantasyProjectionReportConfig
            {
               Season = TimeKeeper.CurrentSeason( DateTime.Now ),
               Week = theWeek.ToString( CultureInfo.InvariantCulture ),
               Dao = dao,
               Scorer = scorer
            }
         };

         Leagues = new List<RosterGridLeague>();
         //Leagues.Add(new RosterGridLeague { 
         //   Id = Constants.K_LEAGUE_Gridstats_NFL1, Name = "Gridstats GS1" 
         //});
         Leagues.Add( new RosterGridLeague
         {
            Id = Constants.K_LEAGUE_Yahoo,
            Name = "Spitzys"
         } );
         //Leagues.Add( new RosterGridLeague
         //{
         //   Id = Constants.K_LEAGUE_Rants_n_Raves,
         //   Name = "NFL.com"
         //} );
      }

      private void GenerateReport(FantasyProjectionReportConfig rpt)
      {
         FPR.Season = rpt.Season;
         FPR.Week = rpt.Week;
         FPR.PgmDao = rpt.Dao;
         FPR.Scorer = rpt.Scorer;
         FPR.League = rpt.League;
         FPR.RenderAll();
      }

      public override void RenderAsHtml()
      {
         FPR = new FantasyProjectionReport();

         foreach (var league in Leagues)
         {
            foreach (var rpt in Configs)
            {
               rpt.League = league.Id;
               GenerateReport(rpt);
            }
         }
      }
   }

   public class FantasyProjectionReportConfig
   {
      public string League { get; set; }
      public string Season { get; set; }

      public string Week { get; set; }

      public IPlayerGameMetricsDao Dao { get; set; }

      public IRatePlayers Scorer { get; set; }
   }
}
