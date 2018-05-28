using System;
using System.Data;
using RosterLib.Helpers;

namespace RosterLib
{
   public class StatGrid 
   {
      public NflSeason Season { get; set; }

      public string StatInFocus { get; set; }

      public StatMaster StatMaster { get; set; }

      public StatGrid( string season )
      {
         Season = new NflSeason( season );
         StatMaster = new StatMaster("Stats", "stats.xml");
      }

      public StatGrid( string season, string statType )
      {
         Season = new NflSeason( season );
         StatInFocus = statType;
         StatMaster = new StatMaster( "Stats", "stats.xml" );
      }

      public StatGrid( NflSeason season, string statType, StatMaster statMaster )
      {
         Season = season;
         StatInFocus = statType;
         StatMaster = statMaster;
      }

      public void ReGenAll()
      {
         StatMaster.Calculate( Season.Year );
      }

      public void DumpXml()
      {
         StatMaster.Dump2Xml();
      }

      public void Render()
      {
         var str = new SimpleTableReport( $"Stat Grid {Season.Year}-{StatInFocus}" )
                     {ReportHeader = StatInFocus};
         StyleHelper.AddStyle( str );
         str.ColumnHeadings = true;
         str.DoRowNumbers = true;
         str.ShowElapsedTime = false;
         str.IsFooter = false;
         str.Totals = true;
         str.AddColumn( new ReportColumn( "Team", "TEAM", "{0}" ) );
         str.AddColumn( new ReportColumn( "Total", "TOT", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk01", "WK01", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk02", "WK02", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk03", "WK03", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk04", "WK04", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk05", "WK05", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk06", "WK06", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk07", "WK07", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk08", "WK08", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk09", "WK09", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk10", "WK10", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk11", "WK11", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk12", "WK12", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk13", "WK13", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk14", "WK14", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk15", "WK15", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk16", "WK16", "{0}", tally: true ) );
         str.AddColumn( new ReportColumn( "Wk17", "WK17", "{0}", tally: true ) );
         str.LoadBody( BuildTable() );
         //str.SubHeader = SubHeading();
         str.RenderAsHtml( FileName(), true );			
      }

      public string FileName()
      {
         var fileName = $@"{Utility.OutputDirectory()}{
			 Season.Year
			 }//stats//StatGrid-{StatInFocus}.htm";
         return fileName;
      }

      private DataTable BuildTable()
      {
         var dt = new DataTable();
         var cols = dt.Columns;
         cols.Add( "TEAM", typeof( String ) );
         cols.Add( "TOT", typeof( Decimal ) );
         for ( var i = 1; i <= 17; i++ )
            cols.Add( string.Format( "WK{0:00}", i ), typeof (String) );

         foreach ( var team in Season.TeamList )
         {
            var week01 = StatMaster.GetStat( Season.Year, "01", team.TeamCode, StatInFocus );
            var week02 = StatMaster.GetStat( Season.Year, "02", team.TeamCode, StatInFocus );
            var week03 = StatMaster.GetStat( Season.Year, "03", team.TeamCode, StatInFocus );
            var week04 = StatMaster.GetStat( Season.Year, "04", team.TeamCode, StatInFocus );
            var week05 = StatMaster.GetStat( Season.Year, "05", team.TeamCode, StatInFocus );
            var week06 = StatMaster.GetStat( Season.Year, "06", team.TeamCode, StatInFocus );
            var week07 = StatMaster.GetStat( Season.Year, "07", team.TeamCode, StatInFocus );
            var week08 = StatMaster.GetStat( Season.Year, "08", team.TeamCode, StatInFocus );
            var week09 = StatMaster.GetStat( Season.Year, "09", team.TeamCode, StatInFocus );
            var week10 = StatMaster.GetStat( Season.Year, "10", team.TeamCode, StatInFocus );
            var week11 = StatMaster.GetStat( Season.Year, "11", team.TeamCode, StatInFocus );
            var week12 = StatMaster.GetStat( Season.Year, "12", team.TeamCode, StatInFocus );
            var week13 = StatMaster.GetStat( Season.Year, "13", team.TeamCode, StatInFocus );
            var week14 = StatMaster.GetStat( Season.Year, "14", team.TeamCode, StatInFocus );
            var week15 = StatMaster.GetStat( Season.Year, "15", team.TeamCode, StatInFocus );
            var week16 = StatMaster.GetStat( Season.Year, "16", team.TeamCode, StatInFocus );
            var week17 = StatMaster.GetStat( Season.Year, "17", team.TeamCode, StatInFocus );

            var dr = dt.NewRow();
            dr[ "TEAM" ] = team.TeamCode;
            if ( week01.Quantity > 0 ) dr[ "WK01" ] = week01.StatOut();
            if ( week02.Quantity > 0 ) dr[ "WK02" ] = week02.StatOut();
            if ( week03.Quantity > 0 ) dr[ "WK03" ] = week03.StatOut();
            if ( week04.Quantity > 0 ) dr[ "WK04" ] = week04.StatOut();
            if ( week05.Quantity > 0 ) dr[ "WK05" ] = week05.StatOut();
            if ( week06.Quantity > 0 ) dr[ "WK06" ] = week06.StatOut();
            if ( week07.Quantity > 0 ) dr[ "WK07" ] = week07.StatOut();
            if ( week08.Quantity > 0 ) dr[ "WK08" ] = week08.StatOut();
            if ( week09.Quantity > 0 ) dr[ "WK09" ] = week09.StatOut();
            if ( week10.Quantity > 0 ) dr[ "WK10" ] = week10.StatOut();
            if ( week11.Quantity > 0 ) dr[ "WK11" ] = week11.StatOut();
            if ( week12.Quantity > 0 ) dr[ "WK12" ] = week12.StatOut();
            if ( week13.Quantity > 0 ) dr[ "WK13" ] = week13.StatOut();
            if ( week14.Quantity > 0 ) dr[ "WK14" ] = week14.StatOut();
            if ( week15.Quantity > 0 ) dr[ "WK15" ] = week15.StatOut();
            if ( week16.Quantity > 0 ) dr[ "WK16" ] = week16.StatOut();
            if ( week17.Quantity > 0 ) dr[ "WK17" ] = week17.StatOut();

            dr[ "TOT" ] = week01.Quantity + week02.Quantity + week03.Quantity + week04.Quantity
               + week05.Quantity + week06.Quantity + week07.Quantity + week08.Quantity + week09.Quantity
               + week10.Quantity + week11.Quantity + week12.Quantity + week13.Quantity + week14.Quantity
               + week15.Quantity + week16.Quantity + week17.Quantity;

            dt.Rows.Add( dr );					
         }
         dt.DefaultView.Sort = "TOT DESC";
         return dt;
      }
   }
}
