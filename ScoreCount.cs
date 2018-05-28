using System;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class ScoreCount
	{
		public string Season { get; set; }

		private readonly SimpleTableReport _r;

		private readonly List<BaseScore> _scoreList;
		private readonly DataTable _dt;

		public ScoreCount(string season)
		{
			Season = season;
			_r = new SimpleTableReport(string.Format("Score Counts : {0}", Season))
			    	{ColumnHeadings = true, Totals = false, DoRowNumbers = false};
			_r.AddColumn(new ReportColumn("Score Type", "SCORE", "{0,-20}"));
			_r.AddColumn(new ReportColumn("Count", "VALUE", "{0}", true));
			_dt = new DataTable();
			var sf = new ScoreFactory();
			_scoreList = sf.GetAllScoreTypes();
			Load();
		}

		public void Load()
		{
			_r.LoadBody(BuildTable());
		}

		private DataTable BuildTable()
		{
			var cols = _dt.Columns;
			cols.Add("SCORE", typeof (String));
			cols.Add("VALUE", typeof (Int32));

			if (_scoreList != null)
			{
				foreach (var s in _scoreList)
				{
					var dr = _dt.NewRow();
					dr["SCORE"] = s.Name;
					dr["VALUE"] = Utility.TflWs.CountScoresByType(Season, s.ScoreType);
					_dt.Rows.Add(dr);
				}
				_dt.DefaultView.Sort = "VALUE DESC";
			}
			return _dt;
		}

		public void RenderToHtml()
		{
			_r.RenderAsHtml(string.Format("{0}ScoreCounts\\ScoreCount{1}.htm",
			                             Utility.OutputDirectory(), Season), true);
		}
	}
}