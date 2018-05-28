using System;
using System.Collections;
using System.Data;

namespace RosterLib
{
	public class RenderUnitStatsToHtml : IRenderWeekly
	{
		private const string FieldFormat = "Wk{0:0#}";

		public string FileOut { get; set; }

		public string RenderData(ArrayList unitList, string sHead, NFLWeek week)
		{
			//  Output the list
			var tu1 = (TeamUnit) unitList[0];
			var r = new SimpleTableReport {ReportHeader = sHead, ReportFooter = "", DoRowNumbers = true};
			var ds = LoadData(unitList, week);
			r.AddColumn(new ReportColumn("Name", "TEAM", "{0,-15}"));
			r.AddColumn(new ReportColumn("Rating", "RATING", "{0,-1}"));
			r.AddColumn(new ReportColumn("Total", "tot", "{0,5}"));

			const int startAt = Constants.K_WEEKS_IN_A_SEASON;

			var currentWeek = new NFLWeek(Int32.Parse(Utility.CurrentSeason()), Int32.Parse(Utility.CurrentWeek()), false);

			for (var w = startAt; w > 0; w--)
			{
				var header = string.Format("Week {0}", currentWeek.Week);
				var fieldName = string.Format(FieldFormat, currentWeek.WeekNo);

				r.AddColumn(new ReportColumn(header, fieldName, "{0,5}", tu1.BGPicker));
				currentWeek = currentWeek.PreviousWeek(currentWeek, true, false );
			}

			var dt = ds.Tables[0];

			dt.DefaultView.Sort = "tot " + tu1.SortDirection();
			r.LoadBody(dt);
			FileOut = string.Format( "{0}Units\\{1}.htm", Utility.OutputDirectory(), sHead );
			r.RenderAsHtml( FileOut, true);
			return FileOut;
		}

		public DataSet LoadData(ArrayList unitList, NFLWeek startWeek)
		{
			//  set up data columns
			var ds = new DataSet();
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add("Team", typeof (String));
			cols.Add("Rating", typeof (String));
			cols.Add("tot", typeof (Int32));

			var currentWeek = new NFLWeek(Int32.Parse(Utility.CurrentSeason()), Int32.Parse(Utility.CurrentWeek()),
			                                  loadGames: true);

			//  Add a column for each week under review
			for (var w = Constants.K_WEEKS_IN_A_SEASON; w > 0; w--)
			{
				currentWeek = currentWeek.PreviousWeek(currentWeek, true, false);

				var fieldName = string.Format(FieldFormat, currentWeek.WeekNo);
				cols.Add(fieldName, typeof (Int32));
			}

			//  generate the data for each unit
			foreach (TeamUnit tu in unitList)
			{
				var nTot = 0.0M;
				var dr = dt.NewRow();
				dr["Team"] = tu.Team.NameOut();
				dr["Rating"] = tu.Rating();

				//  variable number of weeks
				var weekCounter = Constants.K_WEEKS_IN_A_SEASON;
				var scoreWeek = startWeek;
				do
				{
					decimal nScore = tu.GetStat(scoreWeek);
					if (nScore > -1)
						nTot += nScore;

					dr[string.Format(FieldFormat, scoreWeek.WeekNo)] = nScore;

					scoreWeek = scoreWeek.PreviousWeek(scoreWeek, true, false);

					if (scoreWeek.Season != Utility.CurrentSeason()) //  only shows current season
						weekCounter = 0;

					weekCounter--;
				} while (weekCounter > 0);

				dr["tot"] = nTot;
				dt.Rows.Add(dr);
			}
			ds.Tables.Add(dt);
			return ds;
		}
	}
}