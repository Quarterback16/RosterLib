using System;
using System.Collections.Generic;
using System.Data;
using RosterLib.Helpers;

namespace RosterLib
{
   /// <summary>
   ///   Simple table report on who will get all the FP for a given week
   ///   1) Implement Render
   ///   2) Implement Build Table
   /// </summary>
   public class FantasyProjectionReport 
   {
      public string Season { get; set; }
      public string Week { get; set; }

      public string TeamFilter { get; set; }
      public string CategoryFilter { get; set; }

      public string League { get; set; }

      public IPlayerGameMetricsDao PgmDao { get; set; }
      public List<PlayerGameMetrics> PgmList { get; set; }
      public IRatePlayers Scorer { get; set; }

      public FantasyProjectionReport( string season, string week, IPlayerGameMetricsDao dao,
         IRatePlayers scorer )
      {
         Season = season;
         Week = ( week.Length == 1 ) ? "0" + week: week;
         PgmDao = dao;
         Scorer = scorer;
         Scorer.Week = new NFLWeek( Season, Week );
         League = Constants.K_LEAGUE_Gridstats_NFL1;
      }

      public FantasyProjectionReport()
      {
      }


      public void RenderAll()
      {
         Week = (Week.Length == 1) ? "0" + Week : Week;
         Scorer.Week = new NFLWeek(Season, Week);
         Render();

         CategoryFilter = Constants.K_QUARTERBACK_CAT;
         Render();

         RenderRunningbacks();

         CategoryFilter = Constants.K_RECEIVER_CAT;
         Render();

         CategoryFilter = Constants.K_KICKER_CAT;
         Render();
      }

      public void RenderRunningbacks()
      {
         CategoryFilter = Constants.K_RUNNINGBACK_CAT;
         Render();
      }

      private DataTable BuildTable()
      {
         var dt = new DataTable();
         var cols = dt.Columns;
         cols.Add( "SLOT", typeof( String ) );
         cols.Add( "PLAYER", typeof( String ) );
         cols.Add( "TEAM", typeof( String ) );
         cols.Add( "FTEAM", typeof( String ) );
         cols.Add( "POS", typeof( String ) );
         cols.Add( "PTS", typeof( Int16 ) );
         cols.Add( "UNIT", typeof( String ) );
         cols.Add( "ROLE", typeof( String ) );
         cols.Add( "GAME", typeof( String ) );
         cols.Add( "OPPRATE", typeof( String ) );
         cols.Add( "SPREAD", typeof( Decimal ) );
         cols.Add( "TOTAL", typeof( Decimal ) );
         cols.Add( "BOOKIE", typeof( String ) );
         cols.Add( "ACTUAL", typeof( Int16 ) );

         PgmList = PgmDao.GetWeek( Season, Week );
         var week = Scorer.Week;

         foreach ( var pgm in PgmList )
         {
            var p = new NFLPlayer( pgm.PlayerId );

            if ( !string.IsNullOrEmpty( TeamFilter ) )
            {
               if ( p.TeamCode != TeamFilter )
                  continue;
            }

            if ( !string.IsNullOrEmpty( CategoryFilter ) )
            {
               if ( p.PlayerCat != CategoryFilter )
                  continue;
            }

            p.LoadOwner( League );
            var dr = dt.NewRow();

            var game = new NFLGame( pgm.GameKey );
            p.LoadProjections( pgm );

            var opponent = p.CurrTeam.OpponentFor( Season, Int32.Parse( Week ) );
            //  player actually available
            dr[ "PLAYER" ] = p.Url( p.PlayerName, forceReport: false );
            dr[ "TEAM" ] = p.CurrTeam.TeamCode;
            dr[ "FTEAM" ] = p.Owner;
            dr[ "ROLE" ] = p.PlayerRole;
            dr[ "POS" ] = p.PlayerPos;

            dr[ "PTS" ] = Scorer.RatePlayer( p, week );
            dr[ "UNIT" ] = game.ProjectionLink();
            dr[ "GAME" ] = string.Format( "{0} {1} {2}",
                                       game.GameDay(), game.Hour, game.OpponentOut( p.CurrTeam.TeamCode ) );
            dr[ "SPREAD" ] = game.GetSpread();
            dr[ "TOTAL" ] = game.Total;
            game.CalculateSpreadResult();
            dr[ "BOOKIE" ] = game.BookieTip.PredictedScore();

            if ( opponent != null )
               dr[ "OPPRATE" ] = opponent.Ratings;

            //if ( game.Played() )
            //	dr[ "ACTUAL" ] = ActualPoints( p );
            dt.Rows.Add( dr );
         }
         dt.DefaultView.Sort = "PTS DESC";
         return dt;
      }

      public void Render()
      {
         var str = new SimpleTableReport( string.Format( "FP Projection {0}-{1}", Season, Week ) )
         { 
            ReportHeader = string.Format( "Run Date: {0: dddd dd MMM yyyy}", DateTime.Now ) 
         };
         StyleHelper.AddStyle( str );
         str.ColumnHeadings = true;
         str.DoRowNumbers = true;
         str.ShowElapsedTime = false;
         str.IsFooter = false;
         str.AddColumn(new ReportColumn("Player", "PLAYER", "{0}"));
         str.AddColumn(new ReportColumn("Team", "TEAM", "{0}"));
         str.AddColumn( new ReportColumn( "Owner", "FTEAM", "{0}" ) );
         str.AddColumn(new ReportColumn("Pos", "POS", "{0}"));
         str.AddColumn(new ReportColumn("FPoints", "PTS", "{0}"));
         str.AddColumn( new ReportColumn( "Unit", "UNIT", "{0}" ) );
         str.AddColumn(new ReportColumn("Role", "ROLE", "{0}"));
         str.AddColumn(new ReportColumn("Game", "GAME", "{0}"));
         str.AddColumn(new ReportColumn("OppUnit", "OPPRATE", "{0}"));
         str.AddColumn(new ReportColumn("Spread", "SPREAD", "{0:##.#}"));
         str.AddColumn(new ReportColumn("Total", "TOTAL", "{0:##.#}"));
         str.AddColumn( new ReportColumn( "Bookie", "BOOKIE", "{0}" ) );
         str.AddColumn(new ReportColumn("Actual", "ACTUAL", "{0:##}"));
         str.LoadBody( BuildTable() );
         var fileOut = FileName();
         str.RenderAsHtml(fileOut, true);
      }

      public string FileName()
      {
         var cat = string.Empty;
         if ( !string.IsNullOrEmpty( CategoryFilter ) )
            cat = "-" + CategoryFilter;
         var team = string.Empty;
         if ( !string.IsNullOrEmpty( TeamFilter ) )
            team = "-" + TeamFilter;
         return string.Format( "{0}{1}//Projections//Projected-FP-{3}-WK{2}{4}{5}.htm",
            Utility.OutputDirectory(), Season, Week, League, cat, team );
      }
   }
}
