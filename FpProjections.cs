using RosterLib.Interfaces;
using System.Collections.Generic;

namespace RosterLib
{
   public class FpProjections : RosterGridReport
   {
      public string Week { get; set; }
      public List<FpProjectionConfig> Configs { get; set; }

      public FantasyProjectionReport Fpr { get; set; }

      public IPlayerGameMetricsDao Dao { get; set; }

      public IRatePlayers Scorer { get; set; }

      public FpProjections( IKeepTheTime timekeeper ) : base( timekeeper )
		{
         Name = "FP Projections";
         Fpr = new FantasyProjectionReport();
         Dao = new DbfPlayerGameMetricsDao();  //  for fetching the metrics (but they need generation)
         Scorer = new YahooProjectionScorer();
         Configure();
      }

      public override void RenderAsHtml()
      {
         foreach (FpProjectionConfig rpt in Configs)
         {
            Generate( rpt );
         }
      }

      public override string OutputFilename()
      {
         return Fpr.FileName();
      }

      private void Generate(FpProjectionConfig rpt)
      {
         Fpr.Season = rpt.Season;
         Fpr.Week = rpt.Week;
         Fpr.League = rpt.League;
         Fpr.Scorer = rpt.Scorer;
         Fpr.PgmDao = rpt.Dao;
         Fpr.RenderAll();
      }

      private void Configure()
      {
         Configs = new List<FpProjectionConfig>();

         var config1 = new FpProjectionConfig { 
            Season = Season, 
            Week = Utility.NextWeek(),
            League = Constants.K_LEAGUE_Gridstats_NFL1,
            Scorer = Scorer,
            Dao = Dao };

         Configs.Add( config1 );

         var config2 = new FpProjectionConfig
         {
            Season = Season,
            Week = Utility.NextWeek(),
            League = Constants.K_LEAGUE_Rants_n_Raves,
            Scorer = Scorer,
            Dao = Dao
         };

         Configs.Add(config2);

         var config3 = new FpProjectionConfig
         {
            Season = Season,
            Week = Utility.NextWeek(),
            League = Constants.K_LEAGUE_Yahoo,
            Scorer = Scorer,
            Dao = Dao
         };

         Configs.Add(config3);
      }
   }

   public class FpProjectionConfig
   {
      public string League { get; set; }

      public string Season { get; set; }

      public string Week { get; set; }

      public IRatePlayers Scorer { get; set; }

      public IPlayerGameMetricsDao Dao { get; set; }

   }
}
