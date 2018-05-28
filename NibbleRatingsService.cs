using System;
using System.Collections;
using System.Diagnostics;

namespace RosterLib
{
	public class NibbleRatingsService : IRetrieveNibbleRatings
	{
		public Hashtable RatingsHt { get; set; }

		public static DateTime LastDateRanked { get; set; }

		public bool CacheHit { get; set; }

		public bool AuditIt { get; set; }

		public int WeeksToGoBack { get; set; }

		public NibbleRatingsService()
		{
			RatingsHt = new Hashtable();
			LastDateRanked = new DateTime( 1, 1, 1 );
			WeeksToGoBack = 1;
		}

		public NibbleTeamRating GetNibbleRatingFor( NflTeam team, DateTime when )
		{
			//  otherwise retrieve ratings they way they were at when
			if ( CacheIsDirty( when ) )
			{
				RankTeams( when );
				CacheHit = false;
			}
			else
				CacheHit = true;

			var ratings = RatingsFor( team.TeamCode );

			return ratings;
		}

		private NibbleTeamRating RatingsFor( string teamCode )
		{
			return (NibbleTeamRating) RatingsHt[ teamCode ];
		}

		private static bool CacheIsDirty( DateTime when )
		{
			var isDirty = true;
			if ( LastDateRanked != new DateTime( 1, 1, 1 ) )
			{
				if ( LastDateRanked.Equals( when ) )
					isDirty = false;
			}
			return isDirty;
		}

		private void RankTeams( DateTime when )
		{
#if DEBUG
			var stopwatch = new Stopwatch();
			stopwatch.Start();
#endif
			//  Get the upcoming week
			var theWeek = Utility.UpcomingWeek(when);
			var weeksDone = 0;
			while ( weeksDone < WeeksToGoBack )
			{
				theWeek = theWeek.PreviousWeek(theWeek, true, false );
				if ( theWeek.WeekNo <= Constants.K_WEEKS_IN_REGULAR_SEASON )
				{
					foreach (var game in theWeek.GameList())
					{
						var theGame = (NFLGame) game;
						RateGame(theGame);
					}
					weeksDone++;
				}
			}

			if ( AuditIt ) DumpRatingsHt();

			LastDateRanked = when;
#if DEBUG
			Utility.StopTheWatch( stopwatch, string.Format( "Ranking teams : {0:d}", when ) );
#endif
		}

		private void RateGame( NFLGame g )
		{
			if ( g.Played() )
			{
				if ( AuditIt ) Utility.Announce( string.Format( "Rating game {0}", g.ScoreOut() ) );
				//  workout the Average Score
				//AverageScore = Season.AverageScoreAfterWeek( g.WeekNo );
				const int averageScore = 21; //  this is pretty much fixed

				var adjustment = new NibbleGameRating( 0, 0, 0, 0 );
				var ff = FudgeFactor();
				adjustment.HomeOff = ( MaximumScore( g.HomeScore ) - averageScore ) / ff;
				adjustment.HomeDef = ( MaximumScore( g.AwayScore ) - averageScore ) / ff;
				adjustment.AwayOff = ( MaximumScore( g.AwayScore ) - averageScore ) / ff;
				adjustment.AwayDef = ( MaximumScore( g.HomeScore ) - averageScore ) / ff;

				if ( AuditIt )
				{
					Utility.Announce(string.Format("    Actual    score {0} {1:00} v {2} {3:00}",
					                               g.HomeTeam, g.HomeScore, g.AwayTeam, g.AwayScore));
					Utility.Announce(string.Format("    Adjustments  H {0} {1} - {2}",
					                               g.HomeTeam, adjustment.HomeOff, adjustment.HomeDef));
					Utility.Announce(string.Format("    Adjustments  A {0} {1} - {2}",
					                               g.AwayTeam, adjustment.AwayOff, adjustment.AwayDef));
				}
				AdjustRatings( adjustment, g.HomeTeam, g.AwayTeam);
			}
		}

		private void AdjustRatings( NibbleGameRating adjustment,
											 string homeTeam, string awayTeam )
		{
			Debug.Assert( ( Math.Abs( adjustment.HomeOff ) < 10 ), "adjustment too big",
				string.Format( "home team {0} adj={1}", homeTeam, adjustment.HomeOff ) );

			var homeRating = new NibbleTeamRating( adjustment.HomeOff, adjustment.HomeDef );
			var awayRating = new NibbleTeamRating( adjustment.AwayOff, adjustment.AwayDef );

			UpdateRating( homeTeam, homeRating );
			UpdateRating( awayTeam, awayRating );
		}

		private void UpdateRating( string teamCode, NibbleTeamRating rating )
		{
			if ( RatingsHt == null ) RatingsHt = new Hashtable();

			if ( ! RatingsHt.ContainsKey( teamCode ) )
				RatingsHt.Add( teamCode, new NibbleTeamRating( 0, 0 ) );

			var ratings = (NibbleTeamRating) RatingsHt[teamCode];
			ratings.Offence += rating.Offence;
			ratings.Defence += rating.Defence;
			RatingsHt[teamCode] = ratings;
		}

		private static int MaximumScore( int score )
		{
			const int maxScore = 35;
			if ( score > maxScore ) score = maxScore;
			return score;
		}

		static private int FudgeFactor()
		{
			return 4;
		}

		private void DumpRatingsHt()
		{
			var myEnumerator = RatingsHt.GetEnumerator();
			var i = 0;
			Utility.Announce( "\t-INDEX-\t-KEY-\t-VALUE-" );
			while ( myEnumerator.MoveNext() )
			{
				var ratings = (NibbleTeamRating) myEnumerator.Value;
				Utility.Announce( string.Format( "\t[{0}]:\t{1}\t{2}", i++, myEnumerator.Key, ratings ) );
			}
		}

	}

}

