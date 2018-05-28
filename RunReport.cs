using RosterLib.Interfaces;
using System;
using System.Data;

namespace RosterLib
{
	public class RunReport : RosterGridReport
	{
		public RunReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Run Report";
		}

		public override string OutputFilename()
		{
			return $"{Utility.OutputDirectory()}{Season}/{Name}.htm";
		}

		public override void RenderAsHtml()
		{
			var str = new SimpleTableReport( "Run Report" );
			str.AddDenisStyle();
			str.ColumnHeadings = true;
			str.DoRowNumbers = false;
			str.ShowElapsedTime = true;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Machine", "MACHINE", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Report", "REPORT", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Finished", "FINISHED", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "At", "FINISHAT", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Elapsed", "ELAPSED", "{0}", typeof( string ) ) );

			BuildTable( str );
			//str.SetSortOrder( "FINISHED DESC" );
			str.RenderAsHtml( OutputFilename(), true );
			Finish();
		}

		private static void BuildTable( SimpleTableReport str )
		{
			var dt = Utility.TflWs.GetRuns( DateTime.Now.Subtract( new TimeSpan( 7 * 24, 0, 0 ) ) );
			var cutOffDate = DateTime.Now.Subtract( new TimeSpan( 12, 0, 0, 0 ) );

			foreach ( DataRow r in dt.Rows )
			{
				var dr = str.Body.NewRow();
				var dRun = DateTime.Parse( r[ "FINISHED" ].ToString() );
				if ( dRun <= cutOffDate ) continue;

				dr[ "MACHINE" ] = r[ "MACHINE" ];
				dr[ "FINISHED" ] = string.Format( "{0:ddd dd MMM yyyy}", dRun );
				dr[ "FINISHAT" ] = r[ "FINISHAT" ];
				dr[ "ELAPSED" ] = string.Format( "{0,2}:{1,2:00}:{2,2:00}", r[ "HRS" ], r[ "MINS" ], r[ "SECS" ] );
				dr[ "REPORT" ] = r[ "STEP" ];

				str.Body.Rows.Add( dr );
			}
		}
	}
}