using System;
using System.Collections;
using RosterLib.Interfaces;

namespace RosterLib
{
	/// <summary>
	/// Summary description for BalanceReport.
	/// </summary>
	public class BalanceReport : RosterGridReport
	{
      public MetricsBase MetricsBase { get; set; }

      public ArrayList TeamList { get; set; }

      public BalanceReport(IKeepTheTime tk ) : base( tk)
      {
         TimeKeeper = tk;
         Season = TimeKeeper.PreviousSeason();
      }

		public override string OutputFilename()
		{
			return string.Format( "{0}{1}/Balance.htm", Utility.OutputDirectory(), Season );
		}

		/// <summary>
		/// Renders the object as a simple HTML report.
		/// </summary>
		public void Render()
		{
         MetricsBase = new MetricsBase(new PreStyleBreakdown(), Season) { DoBreakdowns = true };
         MetricsBase.Load(Season, skipPostseason:true);
         TeamList = MetricsBase.TeamList;
			var str = new SimpleTableReport( string.Format( "Balance Report {0}", Season ) );
			str.AddStyle(  "#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle(  "#main { margin-left:1em; }" );
			str.AddStyle(  "#dtStamp { font-size:0.8em; }" );
			str.AddStyle(  ".end { clear: both; }" );
			str.AddStyle(  ".gponame { color:white; background:black }" );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Team",      "TEAM",   "{0}", typeof( String )       ) ); 
			str.AddColumn( new ReportColumn( "Rating",    "RATING", "{0}", typeof( String )       ) ); 
			str.AddColumn( new ReportColumn( "Plays",     "PLAYS",  "{0}", typeof( Int32 ), true       ) ); 
			str.AddColumn( new ReportColumn( "Passes",    "PASSES", "{0}", typeof( Int32 ), true       ) ); 
			str.AddColumn( new ReportColumn( "Runs",      "RUNS",   "{0}", typeof( Int32 ), true       ) ); 
			str.AddColumn( new ReportColumn( "Pass%",     "PPERCENT",  "{0:0.00}", typeof( decimal ), false ) ); 
			str.AddColumn( new ReportColumn( "Run%",      "RPERCENT",  "{0:0.00}", typeof( decimal ), false ) );
			str.AddColumn( new ReportColumn( "Tdp", "TDPASSES", "{0}", typeof( string ) ) );
			str.AddColumn( new ReportColumn( "Tdr", "TDRUNS", "{0}",   typeof( String ) ) ); 
			BuildTable( str );
			str.SetSortOrder( "PASSES DESC");
			str.RenderAsHtml( OutputFilename(), true );
		}		
		
		private void BuildTable( SimpleTableReport str )
		{
			var totTDp = 0;
			var totTDr = 0;
		   if (TeamList == null) return;

		   foreach ( NflTeam t in TeamList )
		   {
		      if (t == null) continue;

		      var dr = str.Body.NewRow();
		      t.TallyPlays( Season, skipPostseason: true );
		      dr[ "TEAM" ] = t.NameOut();
		      dr[ "RATING" ] = t.Ratings;
		      dr[ "PLAYS" ]   = t.Passes + t.Runs;
		      dr[ "PASSES" ]  = t.Passes;
		      dr[ "RUNS" ]    = t.Runs;
		      dr[ "PPERCENT" ] = Utility.Percent( t.Passes, t.Passes + t.Runs );
		      dr[ "RPERCENT" ] = Utility.Percent(t.Runs, t.Passes + t.Runs);
		      dr[ "TDPASSES" ] = string.Format( "<a href='{0}'>{1}</a>", t.TdpBreakdownLink(), t.Tdp );
		      dr[ "TDRUNS" ] = string.Format( "<a href='{0}'>{1}</a>", t.TdrBreakdownLink(), t.Tdr );
		      str.Body.Rows.Add( dr );
		      totTDp += t.Tdp;
		      totTDr += t.Tdr;
		   }
		   str.ReportFooter = string.Format("Total TDr : {0}<br>Total TDp : {1}", totTDr, totTDp );
		   str.IsFooter = true;
		   Utility.Announce(str.ReportFooter);
		}
	
	}
}
