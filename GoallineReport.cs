using RosterLib.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class GoallineReport : RosterGridReport, IHtmlReport
	{
		public GoallineReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Goalline Report";
			Season = timekeeper.Season;
			Week = timekeeper.Week;
		}

		public Dictionary<string, NflTeam> TeamList { get; set; }

		public string Week { get; set; }

		public override void RenderAsHtml()
		{
			Render();
		}

		public void Render()
		{
			LoadTeams();  //  By each Team

			var heading = Week == null ? "GL Scores Season " + Season : "Scores : Week " + Week;
			var str = new SimpleTableReport( heading )
			{
				ColumnHeadings = true,
				DoRowNumbers = true
			};
			str.AddColumn( new ReportColumn( "Team", "TEAM", "{0,-20}" ) );
			str.AddColumn( new ReportColumn( "Total", "TOTAL", "{0}" ) );
			AddWeeklyColumns( str );

			str.LoadBody( BuildAndLoadDataTable() );

			FileOut = Week == null ?
				$"{Utility.OutputDirectory()}{Season}//Scores//GLScores.htm"
				: $"{Utility.OutputDirectory()}{Season}//Scores//GLScores-{Week}.htm";
			str.RenderAsHtml( FileOut, persist: true );
		}

		public override string OutputFilename()
		{
			return FileOut;
		}

		private static void AddWeeklyColumns( SimpleTableReport str )
		{
			for ( int i = 1; i < 18; i++ )
			{
				var fieldName = string.Format( "Wk{0:0#}", i );
				str.AddColumn( new ReportColumn( fieldName, fieldName, "{0}" ) );
			}
		}

		private DataTable BuildAndLoadDataTable()
		{
			DataTable dt = BuildDataTable();

			var scores = Week == null ?
			   Utility.TflWs.ScoresDs( Season )
			   : Utility.TflWs.ScoresDs( Season, Week );

			LoadDataTable( dt, scores );

			dt.DefaultView.Sort = "TOTAL DESC";
			return dt;
		}

		private void LoadDataTable( DataTable dt, DataSet scores )
		{
			var scoresTable = scores.Tables[ 0 ];

			foreach ( DataRow dr in scoresTable.Rows )
				IncrementTeamwith( dr );

			foreach ( KeyValuePair<string, NflTeam> team in TeamList )
			{
				DataRow dr = dt.NewRow();
				dr[ "TEAM" ] = team.Value.Name;
				dr[ "TOTAL" ] = team.Value.TotTDs;
				AddWeeklyScorers( dr, team );
				dt.Rows.Add( dr );
			}
		}

		private DataTable BuildDataTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "TEAM", typeof( String ) );
			cols.Add( "TOTAL", typeof( Int32 ) );
			AddWeeklyReportCols( cols );
			return dt;
		}

		private void AddWeeklyReportCols( DataColumnCollection cols )
		{
			for ( int i = 1; i < 18; i++ )
			{
				var fieldName = string.Format( "Wk{0:0#}", i );
				cols.Add( fieldName, typeof( String ) );
			}
		}

		private void AddWeeklyScorers( DataRow dr, KeyValuePair<string, NflTeam> team )
		{
			var theTeam = team.Value;
			IDictionaryEnumerator myEnumerator = theTeam.MetricsHt.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var scorer = myEnumerator.Value;
				var field = string.Format( "Wk{0}", myEnumerator.Key );
				dr[ field ] = scorer;
			}
		}

		private void IncrementTeamwith( DataRow dr )
		{
			// use public Hashtable MetricsHt to store scorers
			var scoreType = dr[ "SCORE" ].ToString();
			var distance = dr[ "DISTANCE" ].ToString();
			var teamCode = dr[ "TEAM" ].ToString();
			var weekNo = System.Convert.ToInt32( dr[ "WEEK" ].ToString() );

			if ( TeamList.ContainsKey( teamCode ) )
			{
				var t = TeamList[ teamCode ];

				if ( scoreType.Equals( Constants.K_SCORE_TD_RUN ) && distance == "1" )
				{
					if ( weekNo < 18 )
					{
						t.TotTDr++;
						t.TotTDs++;
						AddGoallineScorer( dr, t );
					}
				}
			}
		}

		private static void AddGoallineScorer( DataRow dr, NflTeam t )
		{
			var scorer = dr[ "PLAYERID1" ].ToString();
			var player = new NFLPlayer( scorer );
			scorer = player.PlayerNameShort;
			var htKey = dr[ "WEEK" ].ToString();  //  data is stored 01
			if ( t.MetricsHt.ContainsKey( htKey ) )
				AppendScorer( t, scorer, htKey );
			else
				t.MetricsHt.Add( htKey, scorer );
		}

		private static void AppendScorer( NflTeam t, string scorer, string htKey )
		{
			var oldScorer = t.MetricsHt[ htKey ];
			t.MetricsHt[ htKey ] = oldScorer + "<br>" + scorer;
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
												 dr[ "TEAMNAME" ].ToString() )
				{
					MetricsHt = new Hashtable()
				};
				TeamList.Add( t.TeamCode, t );
			}
		}
	}
}