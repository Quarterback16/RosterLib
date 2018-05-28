using NLog;
using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	/// Applys the ESPN scoring rules to a player.
	/// </summary>
	public class EspnScorer : IRatePlayers
	{
		public Logger Logger { get; set; }

		public bool AnnounceIt { get; set; }

		public XmlCache Master { get; set; }

		public EspnScorer( NFLWeek week )
		{
			Name = "ESPN Scorer";
			Week = week;
			AnnounceIt = false;
		}

		#region IRatePlayers Members

		public bool ScoresOnly { get; set; }

		public Decimal RatePlayer( NFLPlayer plyr, NFLWeek week, bool takeCache = true )
		{
			// Points for Scores and points for stats
			if ( week.WeekNo.Equals( 0 ) ) return 0;

			if ( takeCache )
			{
				// Get the points from the XML cache if it is there
				var theKey = $"{week.Season}:{week.WeekNo:00}:{plyr.PlayerCode}";
				if ( Master != null )
				{
					var qty = Master.GetStat( theKey );
					plyr.Points = qty;
					if ( AnnounceIt )
						AnnounceTotal( plyr, week );
					return qty;
				}
			}

			Week = week;  //  set the global week, other wise u will get the same week all the time
			plyr.Points = 0;  //  start from scratch

			const string formatStr = "{0,-20} has {1:00.0} pts for {2}";

			#region Passing

			//  4 pts for a Tdp
			plyr.Points += PointsFor( plyr, 4, Constants.K_SCORE_TD_PASS, id: "2" );
			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, plyr.Points, "Tdp" ) );

			//  2 pts for a PAT pass
			var ptsForPATPasses = PointsFor( plyr, 2, Constants.K_SCORE_PAT_PASS, id: "2" );
			plyr.Points += ptsForPATPasses;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForPATPasses, "PAT passes" ) );

			//  1 pt / 25 YDp
			var ptsForYDp = PointsForStats( plyr: plyr, increment: 1, forStatType: Constants.K_STATCODE_PASSING_YARDS, divisor: 25.0M );
			plyr.Points += ptsForYDp;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForYDp, "YDp" ) );

			//  -2 pts for an Interception
			var ptsForInts = PointsForStats( plyr: plyr, increment: -2, forStatType: Constants.K_STATCODE_INTERCEPTIONS_THROWN, divisor: 1.0M );
			plyr.Points += ptsForInts;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForInts, "Interceptions" ) );

			#endregion Passing

			#region Catching

			var ptsForTDcatches = PointsFor( plyr, 6, Constants.K_SCORE_TD_PASS, id: "1" );
			plyr.Points += ptsForTDcatches;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForTDcatches, "TD catches" ) );

			var ptsForPATcatches = PointsFor( plyr, 2, Constants.K_SCORE_PAT_PASS, id: "1" );
			plyr.Points += ptsForPATcatches;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForPATcatches, "PAT catches" ) );

			//  1 pt / 10 yds
			var ptsForYDs = PointsForStats( plyr: plyr, increment: 1, forStatType: Constants.K_STATCODE_RECEPTION_YARDS, divisor: 10.0M );
			plyr.Points += ptsForYDs;
			if ( ptsForYDs < 0 )
				ptsForYDs = 0;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForYDs, "YDc" ) );

			#endregion Catching

			#region Running

			var ptsForTDruns = PointsFor( plyr, 6, Constants.K_SCORE_TD_RUN, id: "1" );
			plyr.Points += ptsForTDruns;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForTDruns, "TD runs" ) );

			var ptsForPaTruns = PointsFor( plyr, 2, Constants.K_SCORE_PAT_RUN, id: "1" );
			plyr.Points += ptsForPaTruns;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForPaTruns, "PAT runs" ) );

			//  1 pt / 10 yds
			var ptsForYDr = PointsForStats( plyr: plyr, increment: 1, forStatType: Constants.K_STATCODE_RUSHING_YARDS, divisor: 10.0M );
			plyr.Points += ptsForYDr;
			if ( ptsForYDr < 0 )
				ptsForYDr = 0;

			if ( AnnounceIt )
				Announce(string.Format( formatStr, plyr.PlayerName, ptsForYDr, "YDr" ) );

			#endregion Running

			#region Kicking

			if ( plyr.PlayerCat.Equals( Constants.K_KICKER_CAT ) )
			{
				plyr.Points += PointsFor( plyr, 3, Constants.K_SCORE_FIELD_GOAL, "1" );
				plyr.Points += PointsFor( plyr, 1, Constants.K_SCORE_PAT, "1" );
			}

			#endregion Kicking

			if ( AnnounceIt )
				AnnounceTotal( plyr, week );

			return plyr.Points;
		}

		private void AnnounceTotal( NFLPlayer plyr, NFLWeek week )
		{
			Utility.Announce(
			   string.Format( "{0,-20} has {1:00.0} total pts in week {2}:{3}",
							  plyr.PlayerName, plyr.Points, week.Season, week.Week ) );
			Announce("--------------------------------------------------" );
		}

		public string Name { get; set; }

		public NFLWeek Week { get; set; }

		#endregion IRatePlayers Members

		private int PointsForStats( NFLPlayer plyr, int increment, string forStatType, decimal divisor )
		{
			var qty = 0.0M;
			var points = 0;

			// Get stats for last week?
			var ds = plyr.LastStats( forStatType, Week.WeekNo, Week.WeekNo, Week.Season );
			foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
			{
				qty = Decimal.Parse( dr[ "QTY" ].ToString() );
				var pts = Math.Floor( qty / divisor );
				points = Convert.ToInt32( pts ) * increment;
			}

			switch ( forStatType )
			{
				case Constants.K_STATCODE_PASSING_YARDS:
					plyr.ProjectedYDp = Convert.ToInt32( qty );
					break;

				case Constants.K_STATCODE_PASSES_CAUGHT:
					plyr.ProjectedReceptions = Convert.ToInt32( qty );
					break;

				case Constants.K_STATCODE_RUSHING_YARDS:
					plyr.ProjectedYDr = Convert.ToInt32( qty );
					break;

				case Constants.K_STATCODE_INTERCEPTIONS_THROWN:
					break;

				case Constants.K_STATCODE_RECEPTION_YARDS:
					break;

				default:
					Announce($"Unknown stat type {forStatType}" );
					break;
			}
			return points;
		}

		private int PointsFor( NFLPlayer plyr, int increment, string forScoreType, string id )
		{
			var ds = plyr.LastScores( forScoreType, Week.WeekNo, Week.WeekNo, Week.Season, id );
			var nScores = ds.Tables[ 0 ].Rows.Count;
			var points = nScores * increment;

			switch ( forScoreType )
			{
				case Constants.K_SCORE_TD_PASS:
					if ( plyr.PlayerCat.Equals( Constants.K_QUARTERBACK_CAT ) && ( id == "2" ) )
						plyr.ProjectedTDp = nScores;
					else
						plyr.ProjectedTDc = nScores;
					break;

				case Constants.K_SCORE_TD_RUN:
					plyr.ProjectedTDr = nScores;
					break;

				case Constants.K_SCORE_FIELD_GOAL:
					plyr.ProjectedFg = nScores;
					points = ScoreFG( table: ds.Tables[ 0 ] );
					break;

				case Constants.K_SCORE_PAT_PASS:
					break;

				case Constants.K_SCORE_PAT:
					break;

				case Constants.K_SCORE_PAT_RUN:
					break;

				default:
					Announce(string.Format( "EspnScorer : Unknown score type {0}", forScoreType ) );
					break;
			}
			return points;
		}

		private static int ScoreFG( DataTable table )
		{
			var pts = 0;
			foreach ( DataRow dr in table.Rows )
			{
				if ( Int32.Parse( dr[ "DISTANCE" ].ToString() ) > 49 )
					pts += 5;
				else if ( Int32.Parse( dr[ "DISTANCE" ].ToString() ) > 39 )
					pts += 4;
				else
					pts += 3;
			}
			return pts;
		}

		public void Announce( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Info( "   " + message );
		}
	}
}