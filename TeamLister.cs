using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace RosterLib
{
	/// <summary>
	///  A Team Lister just lists teams.
	/// </summary>
	public class TeamLister
	{
		private readonly ArrayList _teamList;
		private IRateTeams _myScorer;

		public string FileOut { get; set; }

		public string Heading { get; set; }

		public string SubHeading { get; set; }

		public TeamLister()
		{
			_teamList = new ArrayList();
			var ds = Utility.TflWs.TeamsDs(Utility.CurrentSeason());
			var dt = ds.Tables[0];
			foreach (var t in from DataRow dr in dt.Rows
			                  select Masters.Tm.GetTeam(Utility.CurrentSeason(), dr["TEAMID"].ToString()))
				_teamList.Add(t);
			Heading = "Not Set";
#if DEBUG
			Utility.Announce($"TeamLister.init {_teamList.Count} teams added to the list");
#endif
		}

		public TeamLister( NflTeam team )
		{
			_teamList = new ArrayList();
			_teamList.Add( team );
		}

		public void SetScorer( IRateTeams ss )
		{
			_myScorer = ss;
			foreach (NflTeam t in _teamList)
				_myScorer.RateTeam(t);
		}

		public void Render(string header)
		{
			//  Output the list
			Heading = header;
			Utility.Announce("TeamListing " + Heading);
			var r = new SimpleTableReport(Heading, "") {DoRowNumbers = true};

			if (!string.IsNullOrEmpty(SubHeading))
				r.SubHeader = SubHeading;

			var ds = LoadData();

			r.AddColumn(new ReportColumn("Name", "NAME", "{0,-15}"));
			r.AddColumn(new ReportColumn("Sacks", "SACKS", "{0,9}"));
			r.AddColumn(new ReportColumn("Interceptions", "INTERCEPTS", "{0,4}"));
			r.AddColumn(new ReportColumn("Points", "POINTS", "{0,5}"));
			var dt = ds.Tables[0];
			dt.DefaultView.Sort = "Points DESC";
			r.LoadBody(dt);
			FileOut = string.Format("{0}{1}.htm", Utility.OutputDirectory(), Heading);
			r.RenderAsHtml( FileOut, true );
		}


		public void Render()
		{
			//RosterLib.Utility.Announce( "TeamListing " + Heading );
			var r = new SimpleTableReport(Heading, "") {DoRowNumbers = true};
			var ds = LoadData();
			r.AddColumn(new ReportColumn("Name", "NAME", "{0,-15}"));
			r.AddColumn(new ReportColumn("Scores", "SCORES", "{0,9}"));
			r.AddColumn(new ReportColumn("Sacks", "SACKS", "{0,9}"));
			r.AddColumn(new ReportColumn("Interceptions", "INTERCEPTS", "{0,4}"));
			r.AddColumn(new ReportColumn("Points", "POINTS", "{0,5}"));
			var dt = ds.Tables[0];
			dt.DefaultView.Sort = "Points DESC";
			r.LoadBody(dt);
			FileOut = string.Format("{0}{1}.htm", Utility.OutputDirectory(), Heading);
			r.RenderAsHtml( FileOut, true);
		}

		private DataSet LoadTeams(ICalculate myCalculator)
		{
			var ds = new DataSet();
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add("Name", typeof (String));
			cols.Add("Rating", typeof (String));
			cols.Add("Sacks", typeof (Int32));
			cols.Add("Scores", typeof (Int32));
			cols.Add("FPAVG", typeof (Decimal));
			cols.Add("INTERCEPTS", typeof (Int32));
			cols.Add("Points", typeof (Int32));
			cols.Add("Allowed", typeof (Int32));
			cols.Add("YDpAllowed", typeof (Int32));
			cols.Add("YDrAllowed", typeof (Int32));

			foreach (NflTeam t in _teamList)
			{
				Utility.Announce(string.Format("Doing {0} ", t.NameOut()));
            if (string.IsNullOrEmpty(t.Ratings)) t.SetRecord(myCalculator.StartWeek.Season, skipPostseason: false);
				var dr = dt.NewRow();
				t.CalculateDefensiveScoring(myCalculator);
				dr["Name"] = t.Name;
				dr["RATING"] = t.Ratings.Substring(3, 3);
				dr["Scores"] = t.DefensiveScores;
				dr["Sacks"] = t.TotSacks;
				dr["INTERCEPTS"] = t.TotInterceptions;
				if (t.Games > 0)
				{
					dr["YDRALLOWED"] = t.TotYdrAllowed/t.Games;
					dr["YDPALLOWED"] = t.TotYDpAllowed/t.Games;
					dr["Allowed"] = t.PtsAgin/t.Games;
					dr["FPAVG"] = t.FantasyPoints/t.Games;
				}
				dr["Points"] = t.FantasyPoints;
				dt.Rows.Add(dr);
				Utility.Announce("---------------------------------------------------------------");
			}
			ds.Tables.Add(dt);
			return ds;
		}

		public string RenderTeams(ICalculate myCalculator)
		{
			Utility.Announce("TeamListing " + Heading);
			var r = new SimpleTableReport(Heading, "") {DoRowNumbers = true};

			var ds = LoadTeams(myCalculator);

			//  define the output
			r.AddColumn(new ReportColumn("Name", "NAME", "{0,-15}"));
			r.AddColumn(new ReportColumn("Rating", "RATING", "{0,-10}"));
			r.AddColumn(new ReportColumn("F Avg", "FPAVG", "{0,5:###.0}"));
			r.AddColumn(new ReportColumn("F Points", "POINTS", "{0,5}"));
			r.AddColumn(new ReportColumn("Scores", "SCORES", "{0,9}"));
			r.AddColumn(new ReportColumn("Sacks", "SACKS", "{0,5}"));
			r.AddColumn(new ReportColumn("Ints", "INTERCEPTS", "{0,4}"));
			r.AddColumn(new ReportColumn("Avg YDr allwd", "YDRALLOWED", "{0,5}"));
			r.AddColumn(new ReportColumn("Avg YDp allwd", "YDPALLOWED", "{0,5}"));
			r.AddColumn(new ReportColumn("Points allwd", "ALLOWED", "{0,5}"));

			var dt = ds.Tables[0];
			dt.DefaultView.Sort = "FPAVG DESC";
			r.LoadBody(dt);
			FileOut = string.Format("{0}{1}.htm", Utility.OutputDirectory(), Heading);
			r.RenderAsHtml( FileOut, true);
			return FileOut;
		}


		private DataSet LoadData()
		{
			var ds = new DataSet();
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add("Name", typeof (String));
			cols.Add( "Scores", typeof( Int32 ) );
			cols.Add("Sacks", typeof (Int32));
			cols.Add("INTERCEPTS", typeof (Int32));
			cols.Add("Points", typeof (Int32));

			foreach (NflTeam t in _teamList)
			{
				if (t.Points > 0)
				{
					var dr = dt.NewRow();
					dr["Name"] = t.Name;
					dr["Scores"] = t.DefensiveScores;
					dr["Sacks"] = t.ProjectedSacks;
					dr["INTERCEPTS"] = t.ProjectedSteals;
					dr["Points"] = t.Points;
					dt.Rows.Add(dr);
				}
			}
			ds.Tables.Add(dt);
			return ds;
		}

		#region Team to Beat - for defensive matchup analysis

		public string RenderTeamToBeat(ICalculate myCalculator)
		{
#if DEBUG
			Utility.Announce( "RenderTeamToBeat " + Heading );
#endif
			var r = new SimpleTableReport(Heading, "") {DoRowNumbers = true};
			//  Crunch the numbers
			var ds = LoadTeamToBeat(myCalculator);
			//  define the output
			r.AddColumn(new ReportColumn( "Name", "NAME", "{0,-15}"));
			r.AddColumn(new ReportColumn( "Off Rating", "RATING", "{0,-10}"));
			r.AddColumn(new ReportColumn( "Opponent", "OPP", "{0,-10}" ) );
			r.AddColumn(new ReportColumn( "FP allwd", "POINTS", "{0,5}"));
			r.AddColumn(new ReportColumn( "FP Avg allwd", "FPAVG", "{0,5:###.0}"));
			r.AddColumn(new ReportColumn( "Scores allwd", "SCORES", "{0,9}"));
			r.AddColumn(new ReportColumn( "Sacks allwd", "SACKS", "{0,5}"));
			r.AddColumn(new ReportColumn( "Ints allwd", "INTERCEPTS", "{0,4}"));
			r.AddColumn(new ReportColumn( "Avg Pts Scored", "ALLOWED", "{0,5}"));

			var dt = ds.Tables[0];
			dt.DefaultView.Sort = "FPAVG DESC";
			r.LoadBody(dt);
			FileOut = string.Format("{0}{1}.htm", Utility.OutputDirectory(), Heading);
			r.RenderAsHtml( FileOut, true);
			return FileOut;
		}

		private DataSet LoadTeamToBeat(ICalculate myCalculator)
		{
			var ds = new DataSet();
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add("Name", typeof (String));
			cols.Add("Rating", typeof (String));
			cols.Add( "OPP", typeof( String ) );
			cols.Add("Sacks", typeof (Int32));
			cols.Add("Scores", typeof (Int32));
			cols.Add("INTERCEPTS", typeof (Int32));
			cols.Add("Points", typeof (Int32));
			cols.Add("FPAVG", typeof (Decimal));
			cols.Add("Allowed", typeof (Int32));

			foreach (NflTeam t in _teamList)
			{
#if DEBUG
				//Utility.Announce(string.Format("Doing {0} ", t.NameOut()));
#endif
				t.FantasyPoints = 0;
				t.DefensiveScores = 0;
				t.TotInterceptions = 0;
				t.TotSacks = 0;

            if (string.IsNullOrEmpty(t.Ratings)) t.SetRecord(myCalculator.StartWeek.Season, skipPostseason: false);
				var dr = dt.NewRow();
				t.CalculateDefensiveScoring( myCalculator, doOpponent: true );
				dr["Name"] = t.Name;
				dr["RATING"] = t.Ratings.Substring(0, 3);
				var g = t.GameFor( myCalculator.StartWeek.Season, myCalculator.StartWeek.WeekNo );
				var opp = "?";
				if ( g == null )
					opp = "BYE";
				else
					opp = g.OpponentOut( t.TeamCode );

				dr["OPP"] = opp;
				dr["Scores"] = t.DefensiveScores;
				dr["Sacks"] = t.TotSacks;
				dr["INTERCEPTS"] = t.TotInterceptions;
				dr["Points"] = t.FantasyPoints;
				if (t.Games > 0)
				{
					dr["Allowed"] = t.PtsAgin / t.Games;
					dr["FPAVG"] = t.FantasyPoints/t.Games;
				}

				dt.Rows.Add(dr);
				Utility.Announce("---------------------------------------------------------------");
			}
			ds.Tables.Add(dt);
			return ds;
		}

		#endregion
	}
}