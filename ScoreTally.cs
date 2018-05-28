using System;
using System.Data;
using System.Linq;
using RosterLib.Helpers;
using RosterLib.Interfaces;

namespace RosterLib
{
   public class ScoreTally : RosterGridReport
   {
      public NflSeason NflSeason { get; set; }

      public string Year { get; set; }

      public string ScopeInFocus { get; set; }

      public bool UsingPredictions { get; set; }

      public bool ForceRefresh { get; set; }

      public ScoreTally( IKeepTheTime timekeeper, string scope, bool usingPredictions ) : base( timekeeper )
	  {
         Year = timekeeper.Season;
         ScopeInFocus = scope;
         UsingPredictions = usingPredictions;
         ForceRefresh = false;
      }

      public ScoreTally( IKeepTheTime timekeeper ) : base( timekeeper )
	  {
         Name = "Team Output Projections";
         LastRun = Utility.TflWs.GetLastRun(Name);
         Year = timekeeper.CurrentSeason();
         ScopeInFocus = "All Teams";
         UsingPredictions = true;
         ForceRefresh = false;
      }

      public override string OutputFilename()
      {
         return FileName();
      }

      public override void RenderAsHtml()
      {
         Render();
         Finish();
      }

      public void Render()
      {
         NflSeason = new NflSeason( Year, loadGames: false, loadDivisions: false);
         NflSeason.LoadRegularWeeks();
         var str = new SimpleTableReport( string.Format( "Score Grid {0}-{1}", NflSeason.Year, ScopeInFocus ) ) { ReportHeader = ScopeInFocus };
         StyleHelper.AddStyle( str );
         str.ColumnHeadings = true;
         str.DoRowNumbers = false;
         str.ShowElapsedTime = false;
         str.IsFooter = false;
         str.AddColumn( new ReportColumn( "Stat", "STAT", "{0}" ) );
         str.AddColumn( new ReportColumn( "Total", "TOT", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk01", "WK01", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk02", "WK02", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk03", "WK03", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk04", "WK04", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk05", "WK05", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk06", "WK06", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk07", "WK07", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk08", "WK08", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk09", "WK09", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk10", "WK10", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk11", "WK11", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk12", "WK12", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk13", "WK13", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk14", "WK14", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk15", "WK15", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk16", "WK16", "{0}" ) );
         str.AddColumn( new ReportColumn( "Wk17", "WK17", "{0}" ) );
         str.LoadBody( BuildTable() );
         //str.SubHeader = SubHeading();
         str.RenderAsHtml( FileName(), true );
      }

      public string FileName()
      {
         if ( UsingPredictions )
            return string.Format( "{0}{1}//projections//ScoreGrid-{2}.htm",
               Utility.OutputDirectory(), NflSeason.Year, ScopeInFocus );
         else
            return string.Format( "{0}{1}//stats//ScoreGrid-{2}.htm",
               Utility.OutputDirectory(), NflSeason.Year, ScopeInFocus );
      }

      private DataTable BuildTable()
      {
         var dt = new DataTable();
         var cols = dt.Columns;
         cols.Add( "STAT", typeof( String ) );
         cols.Add( "TOT", typeof( Decimal ) );
         for ( var i = 1; i <= 17; i++ )
            cols.Add( string.Format( "WK{0:00}", i ), typeof( String ) );

         if ( UsingPredictions )
         {
            WriteGameCountLine( dt );
            WriteTallyLine( dt, "TDp" );
            WriteTallyLine( dt, "TDr" );
            WriteTallyLine( dt, "TDd" );
            WriteTallyLine( dt, "TDs" );
            WriteTallyLine( dt, "FG" );
         }
         else
         {
            WriteActualGameCountLine( dt );
            WriteCountLine( dt, "TDp" );
            WriteCountLine( dt, "YDp" );
            WriteCountLine( dt, "YDr" );
            WriteCountLine( dt, "TDr" );
            WriteCountLine( dt, "TDd" );
            WriteCountLine( dt, "TDs" );
            WriteCountLine( dt, "FG" );
         }

         return dt;
      }

      private void WriteActualGameCountLine( DataTable dt )
      {
         var dr = dt.NewRow();
         dr[ "STAT" ] = "G";

         var tot = 0;
         foreach ( var week in NflSeason.RegularWeeks )
         {
            var weekNo = string.Format( "WK{0}", week.Week );
            var actuals = GetActuals( NflSeason.Year, week.Week );
            var nInWeek = 0;
            var predTab = actuals.Tables[ "sched" ];
            foreach ( DataRow row in predTab.Rows )
            {
               nInWeek++;
            }
            if ( nInWeek > 0 )
            {
               tot += nInWeek;
               dr[ weekNo ] = string.Format( "{0}", nInWeek );
            }
         }
         dr[ "TOT" ] = tot;
         dt.Rows.Add( dr );
      }

      private void WriteGameCountLine( DataTable dt )
      {
         var dr = dt.NewRow();
         dr[ "STAT" ] = "G";

         var tot = 0;
         foreach ( var week in NflSeason.RegularWeeks )
         {
            var weekNo = string.Format( "WK{0}", week.Week );
            var preds = GetPredictions( NflSeason.Year, week.Week );
            var predTab = preds.Tables[ "prediction" ];
            var nInWeek = predTab.Rows.Cast<DataRow>().Count();
            if (nInWeek <= 0) continue;
            tot += nInWeek;
            dr[ weekNo ] = string.Format( "{0}", nInWeek );
         }
         dr[ "TOT" ] = tot;
         dt.Rows.Add( dr );
      }

      private void WriteCountLine( DataTable dt, string stat )
      {
         var dr = dt.NewRow();
         dr[ "STAT" ] = stat;

         var tot = 0;
         foreach ( var week in NflSeason.RegularWeeks )
         {
            var weekNo = string.Format( "WK{0}", week.Week );
            var actuals = GetActuals( NflSeason.Year, week.Week );
            var nInWeek = 0;
            var predTab = actuals.Tables[ "sched" ];
            foreach ( DataRow row in predTab.Rows )
            {
               var g = new NFLGame( row );
#if DEBUG
               Utility.Announce( g.ScoreOut() );
#endif
               if ( g.TotalTds() == 0 || ForceRefresh )
                  g.RefreshTotals();

               g.TallyMetrics( string.Empty );

               switch ( stat )
               {
                  case "TDr":
                     nInWeek += g.HomeTDr + g.AwayTDr;
                     break;
                  case "TDp":
                     nInWeek += g.HomeTDp + g.AwayTDp;
                     break;
                  case "TDd":
                     nInWeek += g.HomeTDd + g.AwayTDd;
                     break;
                  case "TDs":
                     nInWeek += g.HomeTDs + g.AwayTDs;
                     break;
                  case "FG":
                     nInWeek += g.HomeFg + g.AwayFg;
                     break;
                  case "YDp":
                     nInWeek += g.HomeYDp + g.AwayYDp;
                     break;
                  case "YDr":
                     nInWeek += g.HomeYDr + g.AwayYDr;
                     break;

                  default:
                     break;
               }
            }
            if ( nInWeek > 0 )
            {
               tot += nInWeek;
               dr[ weekNo ] = string.Format( "{0}", nInWeek );
            }
#if DEBUG
            //break;  //  dont iterate v not got time
#endif
         }
         dr[ "TOT" ] = tot;
         dt.Rows.Add( dr );
      }

      private void WriteTallyLine( DataTable dt, string stat )
      {
         var dr = dt.NewRow();
         dr[ "STAT" ] = stat;

         var tot = 0;
         foreach ( var week in NflSeason.RegularWeeks )
         {
            var weekNo = string.Format( "WK{0}", week.Week );
            var preds = GetPredictions( NflSeason.Year, week.Week );
            var nInWeek = 0;
            var predTab = preds.Tables[ "prediction" ];
            foreach ( DataRow row in predTab.Rows )
            {
               nInWeek += Int32.Parse( row[ string.Format( "H{0}", stat ) ].ToString() )
                        + Int32.Parse( row[ string.Format( "A{0}", stat ) ].ToString() );
            }
            if (nInWeek <= 0) continue;
            tot += nInWeek;
            dr[ weekNo ] = string.Format( "{0}", nInWeek );
         }
         dr[ "TOT" ] = tot;
         dt.Rows.Add( dr );
      }

      private static DataSet GetPredictions( string season, string week )
      {
         var ds = Utility.TflWs.GetPrediction( "unit", season, week );
         return ds;
      }

      private DataSet GetActuals( string season, string week )
      {
         var ds = Utility.TflWs.GetGames( season, week );
         return ds;
      }
   }
}

