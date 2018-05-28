using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	/// Applys the ESPN scoring rules to a player.
	/// </summary>
	public class ReturnerScorer : IRatePlayers
	{
		public ReturnerScorer( NFLWeek week )
		{
			Name = "Returner Scorer";
			Week = week;
		}

		#region IRatePlayers Members

		public bool ScoresOnly { get; set; }

		public Decimal RatePlayer( NFLPlayer plyr, NFLWeek week, bool takeCache = true )
		{
			// Points for Scores and points for stats
			if (week.WeekNo.Equals(0)) return 0;

			Week = week;  //  set the global week, other wise u will get the same week all the time
			plyr.Points = 0;  //  start from scratch

			#region  Punt Returns

			//  1 pt for a Punt return
			plyr.Points += PointsFor( plyr, 1, RosterLib.Constants.K_SCORE_PUNT_RETURN );
			//RosterLib.Utility.Announce( string.Format( "  {0} has {1} points for {2} in {3}:{4:0#}",
			//   plyr.PlayerName, plyr.Points, RosterLib.Constants.K_SCORE_PUNT_RETURN, week.Season, week.WeekNo ) );
	
			#endregion

			#region  Kick Off Returns

			//  1 pts for a Kick-off return
			plyr.Points += PointsFor( plyr, 1, RosterLib.Constants.K_SCORE_KICK_RETURN );
			//RosterLib.Utility.Announce( string.Format( "  {0} has {1} points for {2} in {3}:{4:0#}",
			//   plyr.PlayerName, plyr.Points, RosterLib.Constants.K_SCORE_KICK_RETURN, week.Season, week.WeekNo ) );
		
			#endregion

			RosterLib.Utility.Announce( string.Format( "    {0} has {1} returns in week {2}:{3}", 
				plyr.PlayerName, plyr.Points, week.Season, week.Week ) );

			return plyr.Points;
		}

		public string Name { get; set; }

		public XmlCache Master
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public NFLWeek Week { get; set; }
		public bool AnnounceIt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		#endregion

		private int PointsFor( NFLPlayer plyr, int increment, string forScoreType )
		{
			int points = 0;
			var ds = plyr.LastScores( forScoreType, Week.WeekNo, Week.WeekNo, Week.Season, id:"1" );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				switch (forScoreType)
				{
                    case RosterLib.Constants.K_SCORE_KICK_RETURN:
						points += increment;
						break;

                    case RosterLib.Constants.K_SCORE_PUNT_RETURN:
						points += increment;
						break;

					default:
						//RosterLib.Utility.Announce(string.Format("Unknown score type {0}", forScoreType));
						break;
				}
			}
			return points;
		}

	}
}


