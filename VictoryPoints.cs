using System;
using System.Collections;
using System.Data;

namespace RosterLib
{
	/// <summary>
	///   An ranking of teams by "Victory Points".  VP are gained by beating a team 
	///   who has beaten other teams.  The number of VP is equal to the Current M_wins 
	///   for the opponent defeated.
	/// </summary>
	public class VictoryPoints
	{
		private readonly ArrayList _teamList;
		private readonly string _season;

		public string FileOut { get; set; }

		public VictoryPoints( string season )
		{
			//  Part 1 - Get the Teams for the season
			_season = season;
			var ds = Utility.TflWs.TeamsDs( season );
			var dt = ds.Tables[ "Team" ];
			_teamList = new ArrayList();
			//  Part 2 - Iterate through the teams
			foreach ( DataRow dr in dt.Rows )
			{
				var t = new NflTeam( dr[ "TEAMID" ].ToString(), season,
												 Int32.Parse( dr[ "WINS" ].ToString() ),
												 dr[ "TEAMNAME" ].ToString() );
				t.CountVictoryPoints();
				_teamList.Add( t );
			}
		}

		/// <summary>
		/// Renders the object as a simple HTML report.
		/// </summary>
		public void RenderAsHtml()
		{
			FileOut = string.Format( "{2}{0}//victory//vp-{1:0#}.htm", _season, Int32.Parse( Utility.CurrentWeek() ),
				Utility.OutputDirectory() );
			var str = new SimpleTableReport( "Victory Points : Week " + Utility.CurrentWeek() )
			          	{ColumnHeadings = true, DoRowNumbers = true};
			str.AddColumn( new ReportColumn( "Team", "TEAM", "{0,-20}" ) );
			str.AddColumn( new ReportColumn( "VP", "VP", "{0}" ) );
			str.LoadBody( BuildTable() );
         str.RenderAsHtml( FileOut, true );
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "TEAM", typeof ( String ) );
			cols.Add( "VP", typeof ( Int32 ) );

			foreach ( NflTeam t in _teamList )
			{
				var vp = string.Format( "{0,3}", t.VictoryPoints );
				var dr = dt.NewRow();
				dr[ "TEAM" ] = t.Name;
				dr[ "VP" ] = vp;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = "VP DESC";
			return dt;
		}
	}
}