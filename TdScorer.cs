using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	/// Applys the Simple Gridstats TD scoring rules scoring rules to a player.
	/// </summary>
	public class TdScorer : IRatePlayers
	{
		private bool m_ScoresOnly;

		public TdScorer( NFLWeek week )
		{
			Name = "Gridstats TD Scorer";
			Week = week;
		}

		#region IRatePlayers Members

		public Decimal RatePlayer(NFLPlayer plyr, NFLWeek week, bool takeCache = true )
		{
			// Points for Scores and points for stats
			if (week.WeekNo.Equals(0)) return 0;

			Week = week;  //  set the global week, other wise u will get the same week all the time
			plyr.Points = 0;  //  start from scratch

			#region  Passing

			int scorePoints = 0;
			int passIncrement = ScoresOnly ? 1 : 3;

			if ( plyr.PlayerCat.Equals( RosterLib.Constants.K_QUARTERBACK_CAT ) )
				scorePoints = PointsFor( plyr, passIncrement, RosterLib.Constants.K_SCORE_TD_PASS, "2");
			int statsPoints = PointsForStats( plyr, passIncrement, RosterLib.Constants.K_STATCODE_PASSING_YARDS, 100.0M );
			StatsLine( "YDp", plyr.PlayerName, statsPoints );
			int points = statsPoints > scorePoints ? statsPoints : scorePoints;
			plyr.Points += points;
			
			#endregion

			#region  Catching

            scorePoints = PointsFor(plyr, passIncrement, RosterLib.Constants.K_SCORE_TD_PASS, "1");
            statsPoints = PointsForStats(plyr, passIncrement, RosterLib.Constants.K_STATCODE_PASSES_CAUGHT, 5.0M);
			StatsLine( "Rec", plyr.PlayerName, statsPoints );
			points = statsPoints > scorePoints ? statsPoints : scorePoints;
			plyr.Points += points;		   
			
			#endregion

			#region  Running

			int runIncrement = ScoresOnly ? 1 : 6;
			scorePoints = PointsFor( plyr, runIncrement, RosterLib.Constants.K_SCORE_TD_RUN, "1" );
			statsPoints = PointsForStats( plyr, runIncrement, RosterLib.Constants.K_STATCODE_RUSHING_YARDS, 80.0M );
			StatsLine( "YDr", plyr.PlayerName, statsPoints );
			points = statsPoints > scorePoints ? statsPoints : scorePoints;
			plyr.Points += points;	
			
			#endregion
			
			#region  Kicking
			int kickIncrement = ScoresOnly ? 1 : 3;
			// 3 for a Fg
			points = PointsFor( plyr, kickIncrement, RosterLib.Constants.K_SCORE_FIELD_GOAL, "1" );
			plyr.Points += points;	
			
			//  1 for a PAT
			int patIncrement = ScoresOnly ? 0 : 1;
			points = PointsFor( plyr, patIncrement, RosterLib.Constants.K_SCORE_PAT, "1" );
			plyr.Points += points;	

			#endregion

			//RosterLib.Utility.Announce( 
			//   string.Format( "    {0,-15} has {1,3} in week {2,4}:{3,2}", 
			//         plyr.PlayerName, plyr.Points, week.Season, week.Week ) );

			return plyr.Points;
		}

		public string Name { get; set; }

		public XmlCache Master
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public NFLWeek Week { get; set; }

		#endregion

		private int PointsForStats(NFLPlayer plyr, int increment, string forStatType, decimal divisor)
		{
			decimal qty = 0.0M;
			int points = 0;

			DataSet ds = plyr.LastStats( forStatType, Week.WeekNo, Week.WeekNo, Week.Season);
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				qty = Decimal.Parse(dr["QTY"].ToString());
				decimal pts = Math.Floor(qty / divisor);
				points = Convert.ToInt32(pts) * increment;
			}

			switch (forStatType)
			{
				case Constants.K_STATCODE_PASSING_YARDS:
					plyr.ProjectedYDp = Convert.ToInt32(qty);
					break;

				case Constants.K_STATCODE_PASSES_CAUGHT:
					plyr.ProjectedReceptions = Convert.ToInt32(qty);
					break;

				case Constants.K_STATCODE_RUSHING_YARDS:
					plyr.ProjectedYDr = Convert.ToInt32( qty );
					break;

				default:
					Utility.Announce(string.Format("Unknown stat type {0}", forStatType));
					break;
			}
			return points;
		}

		private int PointsFor( NFLPlayer plyr, int increment, string forScoreType, string id )
		{
			DataSet ds = plyr.LastScores(forScoreType, Week.WeekNo, Week.WeekNo, Week.Season, id);
			int nScores = ds.Tables[0].Rows.Count;
			int points = nScores * increment;

			switch (forScoreType)
			{
				case RosterLib.Constants.K_SCORE_TD_PASS:
					if (plyr.PlayerCat.Equals(RosterLib.Constants.K_QUARTERBACK_CAT) && (id == "2"))
					{
						plyr.ProjectedTDp = nScores;
						PointsLine( "Tdp", plyr.PlayerNameShort,  points );
					}
					else
					{ 
						plyr.ProjectedTDc = nScores;
						PointsLine( "TDc", plyr.PlayerNameShort,  points );
					}
					break;

				case RosterLib.Constants.K_SCORE_TD_RUN:
					PointsLine( "Tdr", plyr.PlayerNameShort,  points );
					plyr.ProjectedTDr = nScores;
					break;

				case RosterLib.Constants.K_SCORE_FIELD_GOAL:
					PointsLine( "FG ", plyr.PlayerNameShort,  points );
					plyr.ProjectedFg = nScores;
					break;

				case Constants.K_SCORE_PAT:
					break;

				default:
					Utility.Announce(string.Format("TdScorer: Unknown score type {0}", forScoreType));
					break;

			}

			return points;
		}

		public bool ScoresOnly
		{
			get { return m_ScoresOnly; }
			set { m_ScoresOnly = value; }
		}

		public bool AnnounceIt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		private void PointsLine( string scoreType, string name, int pts )
		{
			RosterLib.Utility.Announce( string.Format( "    {2,3} for {0,-15}- {1,2} points", name, pts, scoreType ) );
		}

		private void StatsLine( string scoreType, string name, int pts )
		{
			RosterLib.Utility.Announce( string.Format( "    {2,3} for {0,-15}- {1,2} points", name, pts, scoreType ) );
		}


	}
}

