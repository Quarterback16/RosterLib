using NLog;
using System;
using System.Diagnostics;

namespace RosterLib
{
	public class UnitPredictor : IPrognosticate
	{
		private readonly int[,] _tdp;
		private readonly int[,] _ydp;
		private readonly int[,] _tdr;
		private readonly int[,] _ydr;
		private readonly int[,] _ppr;

		private int _homeTDp;
		private int _homeTDr;
		private int _homeTDd;
		private int _homeTDs;
		private int _homeYDr;
		private int _homeYDp;
		private int _homeFg;
		private int _awayTDr;
		private int _awayTDp;
		private int _awayTDd;
		private int _awayTDs;
		private int _awayFg;
		private int _awayYDr;
		private int _awayYDp;

		public IRetrieveUnitRatings RatingsService;

		public IStorePredictions PredictionStorer { get; set; }

		public bool AuditTrail { get; set; }

		public bool TakeActuals { get; set; }

		public bool StorePrediction { get; set; }

		public bool WriteProjection { get; set; }

		public Logger Logger { get; set; }

		public UnitPredictor()
		{
			//  Matrix for predicting TD passes POvPD
			_tdp = new[ , ]
				{
					{1, 1, 0, 0, 0},
					{2, 1, 1, 0, 0},
					{2, 2, 1, 1, 0},
					{3, 2, 2, 1, 1},
					{4, 3, 2, 2, 1}
				};

			//  Matrix for predicting YDp POvPD
			_ydp = new[ , ]
				{
					{225, 200, 150, 100, 050},
					{250, 225, 200, 150, 100},
					{300, 250, 225, 200, 150},
					{350, 300, 250, 225, 200},
					{400, 350, 300, 250, 225}
				};

			//  Matrix for predicting TD runs  ROvRD
			_tdr = new[ , ]
				{
					{1, 0, 0, 0, 0},
					{1, 1, 0, 0, 0},
					{2, 1, 1, 0, 0},
					{2, 2, 1, 1, 0},
					{3, 2, 2, 1, 1}
				};

			//  Matrix for predicting YD5  ROvRD
			_ydr = new[ , ]
				{
					{116, 125, 75,  55,   45},
					{125, 116, 100, 75,   55 },
					{140, 125, 116, 100,  75 },
					{160, 140, 125, 116, 100},
					{200, 160, 140, 125, 116}
				};

			//  Matrix for predicting TD passes adjustment based on Pass protection
			_ppr = new[ , ]
				{
					{-1, -1, -2, -2, -3},
					{0, 0, 0, -1, -2},
					{1, 0, 0, 0, -1},
					{1, 1, 0, 0, 0},
					{2, 1, 1, 0, 0}
				};
			PredictionStorer = new DbfPredictionStorer();
			WriteProjection = true;
			StorePrediction = true;
		}

		public NFLResult PredictGame( NFLGame game, IStorePredictions persistor, DateTime predictionDate )
		{
			if ( TakeActuals )
				if ( game.Played() )
					return game.Result;

			const int homeRating = 0;
			const int awayRating = 0;

			if ( game.HomeNflTeam == null ) game.HomeNflTeam = Masters.Tm.GetTeam( game.Season + game.HomeTeam );
			if ( game.AwayNflTeam == null ) game.AwayNflTeam = Masters.Tm.GetTeam( game.Season + game.AwayTeam );

			var homeMetrics = CalculateGameMetrics( true, game, predictionDate );
			var awayMetrics = CalculateGameMetrics( false, game, predictionDate );
			var homeScore = homeMetrics.Score;
			var awayScore = awayMetrics.Score;

			if ( homeScore == awayScore ) homeScore++;  //  no ties

			game.Result = new NFLResult( game.HomeTeam, homeScore, game.AwayTeam, awayScore )
			{
				HomeTDp = _homeTDp,
				HomeTDr = _homeTDr,
				HomeFg = _homeFg,
				HomeTDd = _homeTDd,
				HomeTDs = _homeTDs,
				HomeYDp = homeMetrics.YDp,
				HomeYDr = homeMetrics.YDr,
				AwayTDr = _awayTDr,
				AwayTDp = _awayTDp,
				AwayTDs = awayMetrics.TDs,
				AwayTDd = awayMetrics.TDd,
				AwayYDp = awayMetrics.YDp,
				AwayYDr = awayMetrics.YDr,
				AwayFg = _awayFg
			};

			if ( AuditTrail )
				AuditIt( game, game.Result, homeRating, awayRating );

			if ( StorePrediction )
				StorePredictedResult( game, game.Result );

			//  pulled this out to its own job "GameProjectionReportsJob"
			//if ( WriteProjection )
			//	game.WriteProjection();

			return game.Result;
		}

		private void StorePredictedResult( NFLGame game, NFLResult nflResult )
		{
			PredictionStorer.StorePrediction( "unit", game, nflResult );
		}

		private GameMetrics CalculateGameMetrics( bool isHome, NFLGame game, DateTime focusDate )
		{
			string teamRatings;
			string oppRatings;

			var gm = new GameMetrics();

			if ( isHome )
			{
				oppRatings = RatingsService.GetUnitRatingsFor( game.AwayNflTeam, focusDate );
				teamRatings = RatingsService.GetUnitRatingsFor( game.HomeNflTeam, focusDate );
				game.AwayNflTeam.Ratings = oppRatings;
				game.HomeNflTeam.Ratings = teamRatings;
			}
			else
			{
				teamRatings = RatingsService.GetUnitRatingsFor( game.AwayNflTeam, focusDate );
				oppRatings = RatingsService.GetUnitRatingsFor( game.HomeNflTeam, focusDate );
				game.HomeNflTeam.Ratings = oppRatings;
				game.AwayNflTeam.Ratings = teamRatings;
			}

			var score = 0;
			if ( string.IsNullOrEmpty( teamRatings ) || string.IsNullOrEmpty( oppRatings ) )
				Announce( "Ratings not found - skipping score calculation" );
			else
			{
				var fg = isHome ? 2 : 1;
				if ( game.IsDomeGame() )
					fg++;
				else
				{
					if ( game.IsBadWeather() )
						fg--;
				}

				//  Part 1 - Calculate Tdp
				var po = teamRatings.Substring( 0, 1 );
				var pd = oppRatings.Substring( 5, 1 );
				var tdp = TouchdownPasses( po, pd );
				var ydp = YardsPassing( po, pd );
				gm.TDp = tdp;
				gm.YDp = ydp;

				//  Part 2 - Adjust for protection
				var pp = teamRatings.Substring( 2, 1 );
				var pr = oppRatings.Substring( 3, 1 );
				var ppr = ProtectionAdjustment( pp, pr );

				tdp += ppr;
				if ( tdp < 0 ) tdp = 0;

				//  Part 3 - Calculate Tdr
				var ro = teamRatings.Substring( 1, 1 );
				var rd = oppRatings.Substring( 4, 1 );
				var tdr = TouchdownRuns( ro, rd );
				var ydr = YardsRushing( ro, rd );
				gm.TDp = tdp;
				gm.TDr = tdr;
				gm.YDr = ydr;

				var tdd = DefensiveScores( game.IsDivisionalGame(), isHome );

				var tds = SpecialTeamScores( game.IsMondayNight(), isHome );

				gm.TDd = tdd;
				gm.TDs = tds;
				gm.FG = fg;

				//TODO:  Short week adjustment
				//TODO:  Opponent had Route last game adjustment
				//TODO:  Revenge adjustment

				// Total up all the parts of a score
				score = ( tdp + tdr + tdd + tds ) * 7 + ( fg * 3 );

				DumpCalculations( isHome, game, tdr, pr, pp, ro, tds, tdd, rd, oppRatings,
					teamRatings, ppr, tdp, score, pd, po, fg );

				if ( isHome )
				{
					_homeTDp = tdp;
					_homeTDr = tdr;
					_homeFg = fg;
					_homeTDd = tdd;
					_homeTDs = tds;
					_homeYDr = ydr;
					_homeYDp = ydp;
				}
				else
				{
					_awayTDp = tdp;
					_awayTDr = tdr;
					_awayFg = fg;
					_awayTDd = tdd;
					_awayTDs = tds;
					_awayYDr = ydr;
					_awayYDp = ydp;
				}
			}
			gm.Score = score;
			return gm;
		}

		[Conditional( "DEBUG" )]
		private void DumpCalculations( bool isHome, NFLGame game, int tdr, string pr, string pp, string ro, int tds,
											 int tdd, string rd, string oppRatings, string teamRatings, int ppr, int tdp,
											 int score, string pd, string po, int fg )
		{
			var team = isHome ? game.HomeTeamName : game.AwayTeamName;
			Announce( string.Format( "team {2} Ratings : {0}-{3} opponentRatings {1}-{4}",
													 teamRatings, oppRatings, team,
													 Utility.RatingPts( teamRatings ),
													 Utility.RatingPts( oppRatings ) ) );
			if ( game.IsDomeGame() )
				Announce( "Adding FG for Dome game" );
			if ( game.IsBadWeather() )
				Announce( "Subtracting FG for bad weather" );
			Announce( string.Format( "PO-{1} v PD-{2}:TD passes: {0}", tdp - ppr, po, pd ) );
			Announce( string.Format( "PP-{1} v PR-{2}:TD passes: {0}", ppr, pp, pr ) );
			Announce( string.Format( "RO-{1} v RD-{2}:TD runs: {0}", tdr, ro, rd ) );
			Announce( string.Format( "Field goals: {0}", fg ) );
			Announce( string.Format( "Defensive Scores: {0}", tdd ) );
			Announce( string.Format( "Special Team Scores: {0}", tds ) );
			Announce( string.Format( "Total Score: {0}", score ) );
		}

		private static int SpecialTeamScores( bool isPrime, bool isHome )
		{
			if ( isPrime && isHome )
				return 1;
			return 0;
		}

		private static int DefensiveScores( bool isDivisional, bool isHome )
		{
			if ( isDivisional && isHome )
				return 1;
			return 0;
		}

		private int ProtectionAdjustment( string ppRating, string prRating )
		{
			var ppindex = ConvertRating( ppRating );
			var prindex = ConvertRating( prRating );
			return _ppr[ prindex, ppindex ];
		}

		public int TouchdownPasses( string poRating, string pdRating )
		{
			var poindex = ConvertRating( poRating );
			var pdindex = ConvertRating( pdRating );
			return _tdp[ pdindex, poindex ];
		}

		public int YardsPassing( string poRating, string pdRating )
		{
			var poindex = ConvertRating( poRating );
			var pdindex = ConvertRating( pdRating );
			return _ydp[ pdindex, poindex ];
		}

		private int TouchdownRuns( string roRating, string rdRating )
		{
			var roindex = ConvertRating( roRating );
			var rdindex = ConvertRating( rdRating );
			return _tdr[ rdindex, roindex ];
		}

		private int YardsRushing( string roRating, string rdRating )
		{
			var roindex = ConvertRating( roRating );
			var rdindex = ConvertRating( rdRating );
			return _ydr[ rdindex, roindex ];
		}

		private static int ConvertRating( string rating )
		{
			int val;
			switch ( rating )
			{
				case "A":
					val = 0;
					break;

				case "B":
					val = 1;
					break;

				case "C":
					val = 2;
					break;

				case "D":
					val = 3;
					break;

				default:
					val = 4;
					break;
			}
			return val;
		}

		private void AuditIt( NFLGame game, NFLResult res, int homeRating, int awayRating )
		{
			const string debugTeamCode = "SF";
			var debugTeamRank = "(0-0)";
#if DEBUG
			var strVenue = "unknown";
#endif
			var oppTeamCode = "YY";
			var oppRank = "(0-0)";
			const string rankFormat = "({0})";
			var bAudit = false;
			if ( game.HomeTeam.Equals( debugTeamCode ) )
			{
				bAudit = true;
#if DEBUG
				strVenue = "Home";
#endif
				oppTeamCode = game.AwayTeam;
				oppRank = string.Format( rankFormat, awayRating );
				debugTeamRank = string.Format( rankFormat, homeRating );
			}
			if ( game.AwayTeam.Equals( debugTeamCode ) )
			{
				bAudit = true;
#if DEBUG
				strVenue = "Away";
#endif
				oppTeamCode = game.HomeTeam;
				oppRank = string.Format( rankFormat, homeRating );
				debugTeamRank = string.Format( rankFormat, awayRating );
			}
			if ( !bAudit ) return;

#if DEBUG
			Announce(string.Format(" {5} Debug Team {0} {1}, is {2} vs {3} {4}",
			                               debugTeamCode, debugTeamRank, strVenue, oppTeamCode, oppRank, game.GameCodeOut()));

			Announce(res.LogResult());  //  verbosity
#endif
		}

		private void Announce( string msg )
		{
			if ( Logger == null )
				Logger = NLog.LogManager.GetCurrentClassLogger();

			Logger.Info( msg );
		}
	}
}