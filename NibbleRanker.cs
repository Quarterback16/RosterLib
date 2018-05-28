using System;
using System.Collections;
using System.Data;
using System.Diagnostics;

namespace RosterLib
{
	/// <summary>
	///   Used to generate the Nibble rankings and put them into memory.
	/// 
	///   ratings can be randomly accessed from the SeasonMaster.
	/// </summary>
	public class NibbleRanker
	{
		private NflSeason _mSeason;
		private decimal _mAverageScore;
		private int _mMaxScore;

		//  this affects your ratings as results are weighted heavier if more current

		private Hashtable _mRates; //  HashTable to store ratings for a season

		#region  Accessors

		public NibbleTimePoint PointInTime { get; set; }

		public Hashtable Rates
		{
			get { return _mRates; }
			set { _mRates = value; }
		}

		protected NflSeason Season
		{
			get { return _mSeason; }
			set { _mSeason = value; }
		}

		public decimal AverageScore
		{
			get { return _mAverageScore; }
			set { _mAverageScore = value; }
		}

		public int MaxScore
		{
			get { return _mMaxScore; }
			set { _mMaxScore = value; }
		}

		#endregion

		#region  Constructors

		public NibbleRanker()
		{
			_mSeason = new NflSeason( Utility.CurrentSeason() );
			LoadRates();
		}

		public NibbleRanker( string season )
		{
			_mSeason = Masters.Sm.GetSeason( season );
		}

		#endregion

		/// <summary>
		///   Loads the rates from xml
		/// </summary>
		private void LoadRates()
		{
			_mRates = new Hashtable();
			foreach ( string teamKey in _mSeason.TeamKeyList )
			{
				//int[,] teamRates = Masters.Sm.GetNibbleRate( teamKey, _mSeason.Year );
				_mRates.Add( teamKey, new NibbleTeamRating(0,0) );
			}
			Utility.Announce( string.Format( "{0} teams loaded from the XML", Rates.Count ) );
		}

		/// <summary>
		///   Recalculates the ratings
		/// </summary>
		public void RefreshRatings( int toWeek )
		{
			for ( int w = 1; w < toWeek+1; w++ )
			{
				var ds = Utility.TflWs.GetGames( Int32.Parse( Season.Year ), w );
				var dt = ds.Tables[ "sched" ];
				foreach ( DataRow dr in dt.Rows )
				{
					var g = new NFLGame( dr );
					RateGame( g );
				}
			}
		}

		private void RateGame( NFLGame g )
		{
			if ( g.Played() )
			{
				Utility.Announce( string.Format( "Rating game {0}", g.GameName() ) );
				//  workout the Average Score
				//AverageScore = Season.AverageScoreAfterWeek( g.WeekNo );
				AverageScore = 21;  //  this is pretty much fixed

				//  workout adjustments to the previous ratings
				NibbleGameRating startingGameRating;
				if ( g.Week.Equals( "01" ) )
					startingGameRating = new NibbleGameRating( 0, 0, 0, 0 );
				else
					// Get previous rating
					startingGameRating = GetRating( g );

				var projHome = (int) ( AverageScore + ( ( startingGameRating.HomeOff + startingGameRating.AwayDef )/2 ) );
				projHome += 3;  // home field advantage
				var projAway = (int) ( AverageScore + ( ( startingGameRating.AwayOff + startingGameRating.HomeDef )/2 ) );

				var adjustment = new NibbleGameRating( 0, 0, 0, 0 );
				var ff = FudgeFactor( g.Week );
				adjustment.HomeOff = ( MaximumScore( g.HomeScore ) - projHome ) / ff;
				adjustment.HomeDef = ( MaximumScore( g.AwayScore ) - projAway ) / ff;
				adjustment.AwayOff = ( MaximumScore( g.AwayScore ) - projAway ) / ff;
				adjustment.AwayDef = ( MaximumScore( g.HomeScore ) - projHome ) / ff;

				Utility.Announce(string.Format("    Projected score {0} {1:00} v {2} {3:00}",
																g.HomeTeam, projHome, g.AwayTeam, projAway));
				Utility.Announce(string.Format("    Actual    score {0} {1:00} v {2} {3:00}",
																g.HomeTeam, g.HomeScore, g.AwayTeam, g.AwayScore));
				Utility.Announce(string.Format("    Adjustments  H {0} {1} - {2}",
																g.HomeTeam, adjustment.HomeOff, adjustment.HomeDef ));
				Utility.Announce(string.Format("    Adjustments  A {0} {1} - {2}",
																g.AwayTeam, adjustment.AwayOff, adjustment.AwayDef));

				AdjustRatings( startingGameRating, adjustment, g.HomeTeam, g.AwayTeam, g.Week );
			}
		}

		public NibbleGameRating GetRating( NFLGame g )
		{
			var homeKey = KeyFor( g.HomeTeam );
			var awayKey = KeyFor( g.AwayTeam );
			var prevWeek = g.WeekNo - 2;  // one for zero based and one for the week
			var homeRatings = (NibbleTeamRating[]) Rates[ homeKey ];
			var homeRating = homeRatings[ prevWeek ];
			var awayRatings = (NibbleTeamRating[]) Rates[ awayKey ];
			var awayRating = awayRatings[ prevWeek ];
			var gameGameRating =
				new NibbleGameRating( homeRating.Offence, homeRating.Defence, awayRating.Offence, awayRating.Defence );


			Utility.Announce(string.Format( "  Week {6:00} Game Rating {0} {1}-{2}  v {3} {4}-{5}",
														g.HomeTeam, homeRating.Offence, homeRating.Defence,
														g.AwayTeam, awayRating.Offence, awayRating.Defence,
														g.WeekNo ) );

			return gameGameRating;
		}

		static private bool TestTeam( NFLGame g )
		{
			const string testTeam = "BR";
			return ( g.HomeTeam == testTeam ) || ( g.AwayTeam == testTeam );
		}

		private void AdjustRatings( NibbleGameRating startingGameRating, NibbleGameRating adjustment,
											 string homeTeam, string awayTeam, string week )
		{
			Debug.Assert( ( Math.Abs(adjustment.HomeOff ) < 10 ), "adjustment too big",
				string.Format("Week {0} home team {1} adj={2}", week, homeTeam, adjustment.HomeOff));

			var newHomeOff = startingGameRating.HomeOff + adjustment.HomeOff;
			var newHomeDef = startingGameRating.HomeDef + adjustment.HomeDef;
			var newAwayOff = startingGameRating.AwayOff + adjustment.AwayOff;
			var newAwayDef = startingGameRating.AwayDef + adjustment.AwayDef;
			var homeRating = new NibbleTeamRating( newHomeOff, newHomeDef );
			var awayRating = new NibbleTeamRating( newAwayOff, newAwayDef );
			var homeKey = KeyFor( homeTeam );
			var awayKey = KeyFor( awayTeam );
			UpdateRating( homeKey, homeRating, Int32.Parse( week ) );
			UpdateRating( awayKey, awayRating, Int32.Parse( week ) );
		}

		private string KeyFor( string team )
		{
			return string.Format( "{0}{1}", Season.Year, team );
		}

		private void UpdateRating( string teamCode, NibbleTeamRating rating, int week )
		{
			Utility.Announce( string.Format( "    Updating rating for {0}", teamCode ) );
			if ( Rates == null ) Rates = new Hashtable();
			if ( Rates.ContainsKey( teamCode ) )
			{
				var ratings = (NibbleTeamRating[]) Rates[ teamCode ]; // get the array for the team
				var newRating = ratings[ week - 2 ];
				newRating.Offence += rating.Offence;
				newRating.Defence += rating.Defence;
				ratings[ week - 1 ] = newRating;
				Rates[ teamCode ] = ratings;
			}
			else
			{
				var ratings = new NibbleTeamRating[Season.Weeks];
				ratings[ week - 1 ] = rating;
				Rates.Add( teamCode, ratings );
			}
		}

		/// <summary>
		///  Dump out the ratings
		/// </summary>
		public void DumpRatings()
		{
			var myEnumerator = Rates.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var ratings = (NibbleTeamRating) myEnumerator.Value;
				Utility.Announce( string.Format( "Season {0}:- ", myEnumerator.Key ) );
				Utility.Announce( string.Format( "\t[{0}]\t[{1}]", ratings.Offence, ratings.Defence ) );
			}
		}

		/// <summary>
		///   Fudge factor is used to weight the most recent games as heavier
		///   This gain brings in the currency factor!  Games have to be rated using
		///   a context time point!
		/// </summary>
		/// <returns></returns>
		static private int FudgeFactor( string gameWeek )
		{
			//int nWeeksAgo = PointInTime.week - Int32.Parse( gameWeek );
			//int ff = Math.Min( nWeeksAgo, 6 );
			var ff = 3;
			return ff+1;
		}

		private int MaximumScore( int score )
		{
			MaxScore = 35;
			if ( score > MaxScore ) score = MaxScore;
			return score;
		}

		#region  Accessors

		public int GetTeamRate( string teamCode, string week, string offDefence )
		{
			if ( _mRates == null ) LoadRates();
			int[,] rating = (int[,]) _mRates[ teamCode + _mSeason.Year ];
			int rate = offDefence == "offence" ? rating[ Int32.Parse( week ), 0 ] : rating[ Int32.Parse( week ), 1 ];
			return rate;
		}

		#endregion
	}

	#region  Structures


	public class NibbleTeamRating
	{
		public int Offence, Defence;

		public NibbleTeamRating( int o, int d )
		{
			Offence = o;
			Defence = d;
		}

		public override string ToString()
		{
			return string.Format("{0}-{1}", Offence, Defence);
		}
	}

	public struct NibbleGameRating
	{
		public int HomeOff, HomeDef, AwayOff, AwayDef;

		public NibbleGameRating( int ho, int hd, int ao, int ad )
		{
			HomeOff = ho;
			HomeDef = hd;
			AwayOff = ao;
			AwayDef = ad;
		}
	}

	public struct NibbleTimePoint
	{
		public int Season, Week;

		public NibbleTimePoint( int s, int w )
		{
			Season = s;
			Week = w;
		}
	}

	#endregion
}