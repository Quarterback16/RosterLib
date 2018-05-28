using System;

namespace RosterLib
{
	public class DbfPredictionQueryMaster : IGetPredictions
	{
		public Prediction Get( string method, string season, string week, string gameCode )
		{
			var prediction = new Prediction();

			var ds = Utility.TflWs.GetPrediction(method, season, week, gameCode);

			if (ds.Tables[ 0 ].Rows.Count != 1) return prediction;

			var game = new NFLGame( string.Format( "{0}:{1}-{2}", season, week, gameCode ) );
			prediction.Method = method;
			prediction.Season = season;
			prediction.Week = week;
			prediction.GameCode = gameCode;
			prediction.HomeScore = IntValue( ds, "HomeScore" );
			prediction.AwayScore = IntValue( ds, "AwayScore" );
			//  also do a result
			var result = new NFLResult( home: game.HomeTeam, homePts: prediction.HomeScore,
				away: game.AwayTeam, awayPts: prediction.AwayScore )
			{
				HomeTDp = IntValue( ds, "htdp" ), HomeTDr = IntValue( ds, "htdr" ),
				HomeTDd = IntValue( ds, "htdd" ), HomeTDs = IntValue( ds, "htds" ), 
				AwayTDp = IntValue( ds, "atdp" ), AwayTDr = IntValue( ds, "atdr" ), 
				AwayTDd = IntValue( ds, "atdd" ), AwayTDs = IntValue( ds, "atds" ), 
				HomeYDp = IntValue( ds, "hydp" ), AwayYDp = IntValue( ds, "aydp" ), 
				HomeYDr = IntValue( ds, "hydr" ), AwayYDr = IntValue( ds, "aydr" ), 
				AwayFg = IntValue( ds, "afg" ), HomeFg = IntValue( ds, "hfg" )
			};
			prediction.NflResult = result;
			return prediction;
		}

		private static int IntValue( System.Data.DataSet ds, string fieldName )
		{
			return Int32.Parse( ds.Tables[ 0 ].Rows[ 0 ][ fieldName ].ToString() );
		}
	}
}
