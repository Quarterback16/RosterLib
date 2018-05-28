using System;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class ScoresReport : IHtmlReport
	{
		#region  Properties

		public string FileOut {get; set;}

		public Dictionary<string, NflTeam> TeamList { get; set; }

		public string Season { get; set; }

		public string Week { get; set; }

		#endregion

		public void Render()
		{
			LoadTeams();

			var heading = Week == null ? "Scores Season " + Season : "Scores : Week " + Week;
			var str = new SimpleTableReport( heading );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.AddColumn( new ReportColumn( "Team", "TEAM", "{0,-20}" ) );
			str.AddColumn( new ReportColumn( "Total", "TOTAL", "{0}" ) );
			str.AddColumn( new ReportColumn( "Passes", "TDp", "{0}" ) );
			str.AddColumn( new ReportColumn( "Runs", "TDr", "{0}" ) );
			str.AddColumn( new ReportColumn( "Punt returns", "TDt", "{0}" ) );
			str.AddColumn( new ReportColumn( "KO returns", "TDk", "{0}" ) );
			str.AddColumn( new ReportColumn( "Int returns", "TDi", "{0}" ) );
			str.AddColumn( new ReportColumn( "Fumble returns", "TDf", "{0}" ) );
			str.LoadBody( BuildTable() );
			FileOut = Week == null ? string.Format( "{0}//Scores/{1}//Scores.htm", Utility.OutputDirectory(), Season )
				: string.Format( "{0}//Scores/{1}//Scores-{2}.htm",Utility.OutputDirectory(), Season, Week );
			str.RenderAsHtml( FileOut, true );
		}

		private void LoadTeams()
		{
			var ds = Utility.TflWs.TeamsDs( Season );
			DataTable dt = ds.Tables[ "Team" ];
			TeamList = new Dictionary<string, NflTeam>();

			foreach ( DataRow dr in dt.Rows )
			{
				var t = new NflTeam( dr[ "TEAMID" ].ToString(), Season,
												 Int32.Parse( dr[ "WINS" ].ToString() ),
												 dr[ "TEAMNAME" ].ToString() );
				TeamList.Add( t.TeamCode, t );
			}
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "TEAM", typeof( String ) );
			cols.Add( "TOTAL", typeof( Int32 ) );
			cols.Add( "TDp", typeof( Int32 ) );
			cols.Add( "TDr", typeof( Int32 ) );
			cols.Add( "TDt", typeof( Int32 ) );
			cols.Add( "TDk", typeof( Int32 ) );
			cols.Add( "TDi", typeof( Int32 ) );
			cols.Add( "TDf", typeof( Int32 ) );

			var ds = Week == null ? Utility.TflWs.ScoresDs( Season ) : Utility.TflWs.ScoresDs( Season, Week );

			var dt2 = ds.Tables[ 0 ];

			foreach ( DataRow dr in dt2.Rows )
				IncrementTeamwith( dr );

			foreach ( KeyValuePair<string, NflTeam> team in TeamList )
			{
				DataRow dr = dt.NewRow();
				dr[ "TEAM" ] = team.Value.Name;
				dr[ "TOTAL" ] = team.Value.TotTDs;
				dr[ "TDp" ] = team.Value.TotTDp;
				dr[ "TDr" ] = team.Value.TotTDr;
				dr[ "TDt" ] = team.Value.TotTDt;
				dr[ "TDk" ] = team.Value.TotTDk;
				dr[ "TDi" ] = team.Value.TotTDi;
				dr[ "TDf" ] = team.Value.TotTDf;
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = "TOTAL DESC";
			return dt;
		}

		private void IncrementTeamwith( DataRow dr )
		{
			var scoreType = dr[ "SCORE" ].ToString();
			var teamCode = dr[ "TEAM" ].ToString();

			if ( TeamList.ContainsKey( teamCode ) )
			{
				var t = TeamList[ teamCode ];

				if ( scoreType.Equals( Constants.K_SCORE_TD_PASS ) )
				{
					t.TotTDp++;
					t.TotTDs++;
				}

				if ( scoreType.Equals( Constants.K_SCORE_TD_RUN ) )
				{
					t.TotTDr++;
					t.TotTDs++;
				}

				if ( scoreType.Equals( Constants.K_SCORE_PUNT_RETURN ) )
				{
					t.TotTDt++;
					t.TotTDs++;
				}
				
				if ( scoreType.Equals( Constants.K_SCORE_KICK_RETURN ) )
				{
					t.TotTDk++;
					t.TotTDs++;
				}

				if ( scoreType.Equals( Constants.K_SCORE_FUMBLE_RETURN ) )
				{
					t.TotTDf++;
					t.TotTDs++;
				}

				if ( scoreType.Equals( Constants.K_SCORE_INTERCEPT_RETURN ) )
				{
					t.TotTDi++;
					t.TotTDs++;
				}

			}				

		}

	}
}
