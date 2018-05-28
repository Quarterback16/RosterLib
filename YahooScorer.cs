using NLog;
using RosterLib.Interfaces;
using RosterLib.Services;
using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	/// Applys the Yahoo scoring rules to a player after the stats have been recorded.
	/// </summary>
	public class YahooScorer : IRatePlayers
	{
		public Logger Logger { get; set; }

		public IPlayerGameMetricsDao PgmDao { get; set; }

		public IYahooStatService YahooStatService { get; set; }

		public bool WeekHasPassed { get; set; }

        public bool UseProjections { get; set; }

        public string GameKey { get; set; }

		public NFLGame Game { get; set; }

		public YahooScorer( NFLWeek week )
		{
			Name = "Yahoo Scorer";
			Week = week;
			PgmDao = new DbfPlayerGameMetricsDao();  //   should be injected
			YahooStatService = new YahooStatService();
			Logger = LogManager.GetCurrentClassLogger();
            UseProjections = true;
		}

		#region IRatePlayers Members

		public bool ScoresOnly { get; set; }

		public Decimal RatePlayer( NFLPlayer plyr, NFLWeek week, bool takeCache = true )
		{
			// Points for Scores and points for stats
			if ( week.WeekNo.Equals( 0 ) ) return 0;

			if ( takeCache )
			{
				//  Check the stats service first
				if ( YahooStatService.IsStat( 
                    plyr.PlayerCode, 
                    week.Season, 
                    week.Week ) )
				{
					return YahooStatService.GetStat( 
                        plyr.PlayerCode, 
                        week.Season, 
                        week.Week );
				}
			}

			if ( plyr.TeamCode == null )
			{
				Logger.Error( "{0} has a null teamcode", plyr );
				return 0;
			}
			GameKey = Week.GameCodeFor( plyr.TeamCode );  //  slow?
			if ( string.IsNullOrEmpty( GameKey ) ) return 0;

			Game = new NFLGame( GameKey );
			WeekHasPassed = Game.Played(addDay:false);
			if ( !WeekHasPassed ) return plyr.Points;

			Week = week;  //  set the global week, other wise u will get the same week all the time
			plyr.Points = 0;  //  start from scratch

			#region Passing

			//  4 pts for a Tdp
			var tdpPts = PointsFor( plyr, 4, Constants.K_SCORE_TD_PASS, id: "2" );
			plyr.Points += tdpPts;

			Announce( $"{plyr.PlayerName} has {tdpPts} points for Tdp" );

			//  2 pts for a PAT pass
			var ptsForPATpasses = PointsFor( plyr, 2, Constants.K_SCORE_PAT_PASS, id: "2" );
			plyr.Points += ptsForPATpasses;

			Announce( $"{plyr.PlayerName} has {ptsForPATpasses} points for PAT passes" );

			//  1 pt / 25 YDp
			var ptsForYDp = PointsForStats( plyr: plyr, increment: 1, forStatType: Constants.K_STATCODE_PASSING_YARDS, divisor: 25.0M );
			plyr.Points += ptsForYDp;

			Announce( $"{plyr.PlayerName} has {ptsForYDp} points for {plyr.ProjectedYDp} YDp" );

			//  -2 pts for an Interception
			var ptsForInts = PointsForStats(
			   plyr: plyr, increment: -1, forStatType: Constants.K_STATCODE_INTERCEPTIONS_THROWN, divisor: 1.0M );
			plyr.Points += ptsForInts;

			Announce( $"{plyr.PlayerName} has {ptsForInts} points for Interceptions" );

			#endregion Passing

			#region Catching

			//  6 pts for a TD catch
			var ptsForTDcatches = PointsFor( plyr, 6, Constants.K_SCORE_TD_PASS, id: "1" );
			plyr.Points += ptsForTDcatches;
			//  2 points for a 2 point conversion
			var ptsForPATcatches = PointsFor( plyr, 2, Constants.K_SCORE_PAT_PASS, id: "1" );
			plyr.Points += ptsForPATcatches;

			Announce( $"{plyr.PlayerName} has {ptsForPATcatches} points for PAT catches" );

			//  1 pt / 10 yds
			var ptsForYDs = PointsForStats(
			   plyr: plyr,
			   increment: 1,
			   forStatType: Constants.K_STATCODE_RECEPTION_YARDS,
			   divisor: 10.0M );

			plyr.Points += ptsForYDs;

			Announce( $"{plyr.PlayerName} has {ptsForYDs} points for YDc" );

			#endregion Catching

			#region Running

			//  6 points for TD run
			var ptsForTDruns = PointsFor( plyr, 6, Constants.K_SCORE_TD_RUN, id: "1" );
			plyr.Points += ptsForTDruns;

			Announce( $"{plyr.PlayerName} has {ptsForTDruns} points for TD runs" );

			var ptsForPaTruns = PointsFor( plyr, 2, Constants.K_SCORE_PAT_RUN, id: "1" );
			plyr.Points += ptsForPaTruns;

			Announce( $"{plyr.PlayerName} has {ptsForPaTruns} points for PAT runs" );

			//  1 pt / 10 yds
			var ptsForYDr = PointsForStats(
			   plyr: plyr, increment: 1, forStatType: Constants.K_STATCODE_RUSHING_YARDS, divisor: 10.0M );
			plyr.Points += ptsForYDr;

			Announce( $"{plyr.PlayerName} has {ptsForYDr} points for YDr" );

			#endregion Running

			#region Kicking

			plyr.Points += PointsFor( plyr, 3, Constants.K_SCORE_FIELD_GOAL, "1" );

			plyr.Points += PointsFor( plyr, 1, Constants.K_SCORE_PAT, "1" );

			#endregion Kicking

			Announce( $"{plyr.PlayerName} has {plyr.Points} in week {week.Season}:{week.Week}" );

			return plyr.Points;
		}

		public void Announce( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Trace( "   " + message );
		}

		public string Name { get; set; }

		public XmlCache Master
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public NFLWeek Week { get; set; }
		public bool AnnounceIt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		#endregion IRatePlayers Members

		private decimal PointsForStats( NFLPlayer plyr, int increment, string forStatType, decimal divisor )
		{
			var qty = 0.0M;
			if ( WeekHasPassed )
			{
				var ds = plyr.LastStats( forStatType, Week.WeekNo, Week.WeekNo, Week.Season );
				foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
					qty = Decimal.Parse( dr[ "QTY" ].ToString() );

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
						Utility.Announce( string.Format( "Unknown stat type {0}", forStatType ) );
						break;
				}
				plyr.AddMetric( forStatType, GameKey, qty, 0 );
			}
			else
			{
				//  game not played yet
				if ( !string.IsNullOrEmpty( GameKey ) )
				{
					var pgm = PgmDao.GetPlayerWeek( GameKey, plyr.PlayerCode );
					qty = PgmDao.ProjectedStatsOfType( forStatType, pgm );
				}
			}

			//var pts = Math.Floor( qty / divisor );
			var pts = qty / divisor;
			//var points = Convert.ToInt32( pts ) * increment;
			var points = pts * increment;

			return points;
		}

		private decimal PointsFor(
		   NFLPlayer plyr, int increment,
		   string forScoreType, string id )
		{
			decimal nScores;
			var ds = new DataSet();
			if ( WeekHasPassed )
			{
				ds = plyr.LastScores(
				   forScoreType, 
                   Week.WeekNo, 
                   Week.WeekNo, 
                   Week.Season, id );
				nScores = ds.Tables[ 0 ].Rows.Count;
				if ( forScoreType == Constants.K_SCORE_TD_PASS && id == "1" )
					forScoreType = Constants.K_SCORE_TD_CATCH;
				plyr.AddMetric( forScoreType, GameKey, nScores );
			}
			else
			{
                if ( UseProjections )
                {
                    var dao = new DbfPlayerGameMetricsDao();
                    var pgm = dao.GetPlayerWeek( GameKey, plyr.PlayerCode );
                    nScores = pgm.ProjectedScoresOfType( forScoreType, id );
                }
                else
                    nScores = 0;
			}
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
					plyr.ProjectedFg = ( int ) nScores;
					if ( WeekHasPassed )
						points = ScoreFG( table: ds.Tables[ 0 ] );
					break;

				case Constants.K_SCORE_PAT_PASS:
					break;

				case Constants.K_SCORE_PAT:
					break;

				case Constants.K_SCORE_PAT_RUN:
					break;

				case Constants.K_SCORE_TD_CATCH:
					plyr.ProjectedTDc = nScores;
					break;

				default:
					Utility.Announce( string.Format(
					   "YahooScorer: Unknown score type {0}", forScoreType ) );
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
	}
}