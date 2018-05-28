using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	/// Applys the Gridstats scoring rules to a player.
	/// </summary>
	public class GS4Scorer : IRatePlayers
	{
		public bool AnnounceIt { get; set; }

		public GS4Scorer( NFLWeek week )
		{
			Name = "Gridstats Scorer";
			Week = week;
		}

		#region IRatePlayers Members

		public Decimal RatePlayer( NFLPlayer plyr, NFLWeek week, bool takeCache = true )
		{
			// Points for Scores and points for stats
			if ( week.WeekNo.Equals( 0 ) ) return 0;

			if ( takeCache )
			{
				// Get the points from the XML cache if it is there
				var theKey = string.Format( "{0}:{1:00}:{2}", week.Season, week.WeekNo, plyr.PlayerCode );
				if ( Master != null )
				{
					var qty = Master.GetStat( theKey );
					return qty;
				}
			}

			Week = week;  //  set the global week, other wise u will get the same week all the time
			plyr.Points = 0;  //  start from scratch

			#region Passing

			decimal scorePoints = 0M;
			var passIncrement = ScoresOnly ? 1 : 3;

			if ( plyr.PlayerCat.Equals( Constants.K_QUARTERBACK_CAT ) )
				scorePoints = PointsFor( plyr, passIncrement, Constants.K_SCORE_TD_PASS, "2" );
			var statsPoints = PointsForStats( plyr, passIncrement, Constants.K_STATCODE_PASSING_YARDS, 100.0M );
			StatsLine( "YDp", plyr.PlayerName, statsPoints );
			var points = statsPoints > scorePoints ? statsPoints : scorePoints;
			plyr.Points += points;

			#endregion Passing

			#region Catching

			scorePoints = PointsFor( plyr, passIncrement, RosterLib.Constants.K_SCORE_TD_PASS, "1" );
			statsPoints = PointsForStats( plyr, passIncrement, RosterLib.Constants.K_STATCODE_PASSES_CAUGHT, 5.0M );
			StatsLine( "Rec", plyr.PlayerName, statsPoints );
			points = statsPoints > scorePoints ? statsPoints : scorePoints;
			plyr.Points += points;

			#endregion Catching

			#region Running

			var runIncrement = ScoresOnly ? 1 : 6;
			scorePoints = PointsFor( plyr, runIncrement, RosterLib.Constants.K_SCORE_TD_RUN, "1" );
			statsPoints = PointsForStats( plyr, runIncrement, RosterLib.Constants.K_STATCODE_RUSHING_YARDS, 80.0M );
			StatsLine( "YDr", plyr.PlayerName, statsPoints );
			points = statsPoints > scorePoints ? statsPoints : scorePoints;
			plyr.Points += points;

			#endregion Running

			#region Kicking

			if ( plyr.PlayerCat.Equals( Constants.K_KICKER_CAT ) )
			{
				var kickIncrement = ScoresOnly ? 1 : 3;
				// 3 for a Fg
				points = PointsFor( plyr, kickIncrement, Constants.K_SCORE_FIELD_GOAL, "1" );
				plyr.Points += points;

				//  1 for a PAT
				var patIncrement = ScoresOnly ? 0 : 1;
				points = PointsFor( plyr, patIncrement, Constants.K_SCORE_PAT, "1" );
				plyr.Points += points;
			}

			#endregion Kicking

#if DEBUG
			Utility.Announce(
			   string.Format( "    {0,-15} has {1,3} in week {2,4}:{3,2}",
					 plyr.PlayerName, plyr.Points, week.Season, week.Week ) );
			Utility.Announce( " " );
#endif
			return plyr.Points;
		}

		public string Name { get; set; }

		public XmlCache Master { get; set; }

		public NFLWeek Week { get; set; }

		#endregion IRatePlayers Members

		private int PointsForStats(
			NFLPlayer plyr,
			int increment,
			string forStatType,
			decimal divisor )
		{
			var qty = 0.0M;
			var points = 0;

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

				default:
					Utility.Announce( string.Format( "Unknown stat type {0}", forStatType ) );
					break;
			}
#if DEBUG
			Utility.Announce( string.Format( "{0} gets {1} for stat type {2}",
			   plyr.PlayerNameShort, qty, Utility.StatTypeOut( forStatType ) ) );
#endif
			return points;
		}

		private decimal PointsFor( NFLPlayer plyr, int increment, string forScoreType, string id )
		{
			// nScores could be TDs, FGs or PATs
			decimal nScores = 0M;
			if ( Week.HasPassed() )
			{
				var ds = plyr.LastScores( forScoreType, Week.WeekNo, Week.WeekNo, Week.Season, id );
				nScores = ds.Tables[ 0 ].Rows.Count;
			}
			else
			{
				var dao = new DbfPlayerGameMetricsDao();
				var gameCode = Week.GameCodeFor( plyr.TeamCode );
				if ( !string.IsNullOrEmpty( gameCode ) )
				{
					var pgm = dao.GetPlayerWeek( gameCode, plyr.PlayerCode );
					nScores = pgm.ProjectedScoresOfType( forScoreType, id );
				}
			}
			decimal points = nScores * increment;

			switch ( forScoreType )
			{
				case Constants.K_SCORE_TD_PASS:
					if ( plyr.PlayerCat.Equals( Constants.K_QUARTERBACK_CAT ) && ( id == "2" ) )
					{
						plyr.ProjectedTDp = nScores;
						PointsLine( "Tdp", plyr.PlayerNameShort, points );
					}
					else
					{
						plyr.ProjectedTDc = nScores;
						PointsLine( "TDc", plyr.PlayerNameShort, points );
					}
					break;

				case Constants.K_SCORE_TD_RUN:
					PointsLine( "Tdr", plyr.PlayerNameShort, points );
					plyr.ProjectedTDr = nScores;
					break;

				case Constants.K_SCORE_FIELD_GOAL:
					PointsLine( "FG ", plyr.PlayerNameShort, points );
					plyr.ProjectedFg = ( int ) nScores;
					break;

				case Constants.K_SCORE_PAT:
					break;

				default:
					Utility.Announce( string.Format( "GS4Scorer: Unknown score type {0}", forScoreType ) );
					break;
			}
#if DEBUG
			Utility.Announce( $"{plyr.PlayerNameShort} gets {points} for score type {Utility.ScoreTypeOut( forScoreType )}" );
#endif
			return points;
		}

		public bool ScoresOnly { get; set; }

		private static void PointsLine( string scoreType, string name, decimal pts )
		{
#if DEBUG
			if ( pts > 0 )
				Utility.Announce( string.Format( "    {2,3} for {0,-15}- {1,2} points",
				   name, pts, Utility.ScoreTypeOut( scoreType ) ) );
#endif
		}

		private static void StatsLine( string scoreType, string name, int pts )
		{
#if DEBUG
			if ( pts > 0 )
				Utility.Announce( string.Format( "    {2,3} for {0,-15}- {1,2} points",
				   name, pts, Utility.ScoreTypeOut( scoreType ) ) );
#endif
		}
	}
}