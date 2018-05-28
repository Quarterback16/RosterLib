using System;
using System.Data;


namespace RosterLib
{
	public class StarScorer : IRatePlayers
	{
		public NFLWeek Week { get; set; }

		public string Name { get; set; }

		public XmlCache Master
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool ScoresOnly { get; set; }

		public string Season { get; set; }
		public bool AnnounceIt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public decimal RatePlayer( NFLPlayer plyr, NFLWeek week, bool takeCache = true )
		{
			Week = week;

			plyr.Points = StarsFor( plyr );

			return plyr.Points;
		}

		private decimal StarsFor( NFLPlayer plyr )
		{
			//  You get a star for 300+YDp or 3+TDps
			decimal nStars = 0M;

			//  Have to do it week by week
			for (int w = Week.WeekNo; w <= Week.WeekNo; w++)
			{
				if ( 
					  ( TDpForWeek( w, plyr ) > 2 ) ||
					  ( TDrForWeek( w, plyr ) > 1 ) ||
					  (  TDcForWeek(w, plyr ) > 1 ) ||
					  ( CalculateStats(plyr, w, RosterLib.Constants.K_STATCODE_PASSING_YARDS) >= 300 ) || 
					  ( CalculateStats(plyr, w, RosterLib.Constants.K_STATCODE_RUSHING_YARDS) >= 160 ) || 
					  ( CalculateStats(plyr, w, RosterLib.Constants.K_STATCODE_PASSES_CAUGHT) >= 10  ) ||
					  ( CalculateStats(plyr, w, RosterLib.Constants.K_STATCODE_RECEPTION_YARDS) >= 160 )
					)
					nStars++;
			}

			RosterLib.Utility.Announce( string.Format( "{0,-15} in week {2,-2} has {1} stars", 
				plyr.PlayerName, nStars, Week.WeekNo ) );

			return nStars;
		}

		#region  Worker routines

		private decimal CalculateStats( NFLPlayer plyr, int w, string statType )
		{
			decimal nStats = 0.0M;
			DataSet ds = plyr.LastStats( statType, w, w, Season);
			if (ds != null)
			{
				foreach (DataRow dr in ds.Tables[0].Rows)
					nStats += Decimal.Parse(dr["QTY"].ToString());
			}
			return nStats;
		}

		private int TDrForWeek(int w, NFLPlayer plyr)
		{
            return CalculateScore(plyr, w, RosterLib.Constants.K_SCORE_TD_RUN, "1");
		}
		
		private int TDpForWeek(int w, NFLPlayer plyr)
		{
            return CalculateScore(plyr, w, RosterLib.Constants.K_SCORE_TD_PASS, "2");
		}

		private int TDcForWeek(int w, NFLPlayer plyr)
		{
            return CalculateScore(plyr, w, RosterLib.Constants.K_SCORE_TD_CATCH, "1");
		}
		
		private int CalculateScore( NFLPlayer plyr, int w, string scoreType, string playerId )
		{
			int nTDp = 0;
			DataSet ds = plyr.LastScores( scoreType, w, w, Season, playerId );
			if ( ds != null ) nTDp = ds.Tables[0].Rows.Count;
			return nTDp;
		}

		#endregion

		public int RateTeam( NflTeam team )
		{
			throw new NotImplementedException();
		}
	}
}
