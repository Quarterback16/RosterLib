using RosterLib.Interfaces;
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
	public class StrengthOfSchedule : RosterGridReport
	{
		private ArrayList _teamList;

		private const string KFieldTeam = "TEAM";
		private const string KFieldSos = "SOS";
		private const string KFieldExpWins = "EXPWINS";
		private const string KFieldExpLosses = "EXPLOSSES";
		private const string KFieldWins = "WINS";
		private const string KFieldLosses = "LOSSES";
		private const string KFieldVariance = "VAR";


		public StrengthOfSchedule( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			//  Part 1 - Get the Teams for the season
			Season = timekeeper.Season;
		}

		/// <summary>
		/// Renders the object as a simple HTML report.
		/// </summary>
		public override void RenderAsHtml()
		{
            Name = "Strength of Schedule";
            var ds = Utility.TflWs.TeamsDs(Season);
            var dt = ds.Tables["Team"];
            _teamList = new ArrayList();
            foreach (DataRow dr in dt.Rows)
            {
                var t = new NflTeam(dr["TEAMID"].ToString(), Season,
                                    Int32.Parse(dr["WINS"].ToString()),
                                    dr["TEAMNAME"].ToString());
                t.StrengthOfSchedule();
                _teamList.Add(t);
            }
			var str = new SimpleTableReport(Name) {ColumnHeadings = true, DoRowNumbers = true};
			str.AddColumn(new ReportColumn("Team", KFieldTeam, "{0,-20}"));
			str.AddColumn(new ReportColumn("SoS", KFieldSos, "{0}"));
			str.AddColumn(new ReportColumn("Exp W", KFieldExpWins, "{0}"));
			str.AddColumn(new ReportColumn("Exp L", KFieldExpLosses, "{0}"));
			str.AddColumn(new ReportColumn("Prev W", KFieldWins, "{0}"));
			str.AddColumn(new ReportColumn("Prev L", KFieldLosses, "{0}"));
			str.AddColumn(new ReportColumn("Var", KFieldVariance, "{0}"));
			str.LoadBody(BuildTable());
			str.RenderAsHtml( OutputFilename(), true );
		}

		public override string OutputFilename()
		{
			return string.Format( "{0}{1}/StrengthOfSchedule.htm", Utility.OutputDirectory(), Season );
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add(KFieldTeam, typeof (String));
			cols.Add(KFieldSos, typeof (String));
			cols.Add(KFieldExpWins, typeof (Int32));
			cols.Add(KFieldExpLosses, typeof (Int32));
			cols.Add(KFieldWins, typeof (Int32));
			cols.Add(KFieldLosses, typeof (Int32));
			cols.Add(KFieldVariance, typeof (Int32));
			foreach (NflTeam t in _teamList)
			{
				var sos = string.Format("{0:0.##0}", t.SoS);
				var dr = dt.NewRow();
				dr[KFieldTeam] = t.Name;
				dr[KFieldSos] = sos;
				dr[KFieldExpWins] = t.ExpWins;
				dr[KFieldExpLosses] = t.ExpLosses;
				dr[KFieldWins] = t.Wins;
				dr[KFieldLosses] = t.Losses;
				dr[KFieldVariance] = t.ExpWins - t.Wins;
				dt.Rows.Add(dr);
			}
			dt.DefaultView.Sort = KFieldSos + " ASC";
			return dt;
		}
	}
}