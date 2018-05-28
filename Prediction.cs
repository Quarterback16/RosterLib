using System;

namespace RosterLib
{
	public class Prediction
	{
		public string Method { get; set; }
		public string Season { get; set; }
		public string Week { get; set; }
		public string GameCode { get; set; }
		public int HomeScore { get; set; }
		public int AwayScore { get; set; }
		public TipResult Result { get; set; }
		public decimal PredictedSpread { get; set; }
		public NFLResult NflResult { get; set; }

		public Prediction()
		{
			
		}

		public Prediction( System.Data.DataRow dr )
		{
			if (dr == null) return;
			Method = dr["Method"].ToString();
			Season = dr["SEASON"].ToString();
			Week = dr["WEEK"].ToString();
			GameCode = dr["GAMECODE"].ToString();
			HomeScore = Int32.Parse( dr["HOMESCORE"].ToString() );
			AwayScore = Int32.Parse( dr[ "AWAYSCORE" ].ToString() );
			PredictedSpread = HomeScore - AwayScore;
		}

		public Prediction( string method, string season, string week, string gameCode, int homeScore, Int16 awayScore )
		{
			Method = method;
			Season = season;
			Week = week;
			GameCode = gameCode;
			HomeScore = homeScore;
			AwayScore = awayScore;
			PredictedSpread = HomeScore - AwayScore;
		}

		public TipResult CheckResult()
		{
			var game = new NFLGame(Utility.GameKey(Season, Week, GameCode));
			if (game.Played())
			{
				Result = HomeWinTipped()
				         	? (game.HomeWin() ? TipResult.Correct : TipResult.Incorrect)
				         	: (game.AwayWin() ? TipResult.Correct : TipResult.Incorrect);
			}
			else
				Result = TipResult.None;
			return Result;
		}

		public TipResult CheckResultAts()
		{
			var game = new NFLGame( Utility.GameKey( Season, Week, GameCode ) );
			Result = TipResult.None;
			var betOn = String.Empty;
			if ( game.Played() )
			{
				if ( game.Spread < 0 )
				{
					//  Away team favoured
					if (PredictedSpread < game.Spread)
						betOn = game.AwayTeam;
					else if (PredictedSpread > game.Spread)
						betOn = game.HomeTeam;
				}
				else if ( game.Spread > 0 )
				{
					//  home team favoured
					if (PredictedSpread > game.Spread)
						betOn = game.HomeTeam;
					else if (PredictedSpread > game.Spread)
						betOn = game.AwayTeam;
				}
			}

			if ( ! string.IsNullOrEmpty( betOn ) )
			{
				if (game.ResultvsSpread(betOn).Equals("Win"))
					Result = TipResult.Correct;
				else if (game.ResultvsSpread(betOn).Equals("Loss"))
					Result = TipResult.Incorrect;
				else
					Result = TipResult.Push;
			}

			return Result;
		}

		private bool HomeWinTipped()
		{
			return (HomeScore > AwayScore);
		}

	}

	public enum TipResult
	{
		None = 0,
		Correct = 1,
		Incorrect = 2,
		Push = 3
	}
}
