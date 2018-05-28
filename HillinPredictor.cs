using System;

namespace RosterLib
{
	/// <summary>
	///   A simple line making formula by Michael W Hillin
	/// 
	///   Based on using a power rating which is adjusted based on
	///   actual results.
	/// 
	/// </summary>
	public class HillinPredictor : IPrognosticate
	{
		public bool StorePrediction { get; set; }
		public IStorePredictions PredictionStorer { get; set; }
		public bool AuditTrail { get; set; }
		public bool TakeActuals { get; set; }

		public NFLResult PredictGame( NFLGame game, IStorePredictions persistor, DateTime predictionDate )
		{
			if ( persistor == null ) StorePrediction = false;

			var homePower = game.HomeNflTeam.GetPowerRating( game.Week );
			var awayPower = game.AwayNflTeam.GetPowerRating( game.Week );

			//Utility.Announce( string.Format( "PredictGame: Home Power {0} v Away Power {1}",
			//   homePower, awayPower ) );

			//  Add 2.5 to the Home teams power rating
			homePower += 2.5M;

			//  Compare Power Rating to get the line
			var line = homePower - awayPower;

			//  Estrapolate the score from the line using 21 point average
			var homeScore = 21.0M;
			var awayScore = 21.0M ;
			if ( line > 0 )
				homeScore += line;
			if ( line < 0 )
				awayScore -= line;

			var intHomeScore = Int32.Parse( Math.Round( homeScore, 0 ).ToString() ) ;
			var intAwayScore = Int32.Parse( Math.Round( awayScore, 0 ).ToString() );

			var res = new NFLResult( game.HomeTeam, intHomeScore, game.AwayTeam, intAwayScore ); 
			if ( StorePrediction )
				StorePredictedResult( game, res );

			//Utility.Announce( string.Format( "PredictGame: Result predicted - {0}", res.LogResult() ) );

			return res;
		}

		public void PredictWeek( NFLWeek week )
		{
			var suWins = 0;
			var suLosses = 0;
			var atsWins = 0;
			var atsLosses = 0;
			var atsTies = 0;

			foreach ( NFLGame game in week.GameList() )
			{
				var result = PredictGame( game, PredictionStorer, game.GameDate );

				Utility.Announce( result.LogResult() );

				if (game.Played())
					suWins = AnnounceResult( game, result, suWins, ref suLosses, ref atsWins, ref atsTies, ref atsLosses );
				Utility.Announce( "------------------------------" );
			}
			if (suWins + suLosses > 0)
				AnnounceTotals( suWins, suLosses, atsWins, atsLosses, atsTies );
		}

		private static void AnnounceTotals( int suWins, int suLosses, int atsWins, int atsLosses, int atsTies )
		{
			Utility.Announce( string.Format( "Straight Up ({0}-{1})", suWins, suLosses ) );
			Utility.Announce( string.Format( "ATS         ({0}-{1}-{2})", atsWins, atsLosses, atsTies ) );
		}

		private static int AnnounceResult( NFLGame game, NFLResult result, int suWins, ref int suLosses, ref int atsWins,
		                                   ref int atsTies, ref int atsLosses )
		{
			Utility.Announce( game.ScoreOut() );
			var su = game.EvaluatePrediction( result );
			Utility.Announce( su );
			if (su.IndexOf( "WIN" ) > -1) suWins++;
			else suLosses++;

			var ats = game.EvaluatePredictionAts( result, game.Spread );
			if (ats.IndexOf( "WIN" ) > -1) atsWins++;
			else if (ats.IndexOf( "PUSH" ) > -1)
				atsTies++;
			else
				atsLosses++;
			Utility.Announce( ats );
			return suWins;
		}

		public void PredictSeason( string season, DateTime projectionDate, string fileOut )
		{
			var html = new HtmlFile( fileOut,
									" Win Projections as of " + projectionDate.ToString( "ddd dd MMM yy" ) );
			html.AddToBody( Header( "Season Projections - Hillin Predictor - " + season ) );
			html.AddToBody( SeasonOut( "Spread", projectionDate, season ) );
			html.Render();
		}

		private void StorePredictedResult( NFLGame game, NFLResult nflResult )
		{
			PredictionStorer.StorePrediction( "hillin", game, nflResult );
		}

		private static string Header( string cHeading )
		{
			var htmlOut = HtmlLib.TableOpen( "class='title' cellpadding='0' cellspacing='0'" ) + "\n\t"
								  + HtmlLib.TableRowOpen() + "\n\t\t"
								  + HtmlLib.TableDataAttr( cHeading, "colspan='2' class='gponame'" ) + "\n\t"
								  + HtmlLib.TableRowClose() + "\n\t"
								  + HtmlLib.TableRowOpen() + "\n\t\t"
								  + HtmlLib.TableDataAttr( "Report Date:" + DateTime.Now.ToString( "dd MMM yy  HH:mm" )
												, "id='dtstamp'" ) + "\n\t\t"
								  + HtmlLib.TableData( HtmlLib.Div( "objshowhide", "tabindex='0'" ) ) + "\n\t"
								  + HtmlLib.TableRowClose() + "\n"
								  + HtmlLib.TableClose() + "\n";
			return htmlOut;
		}

		private string SeasonOut( string metric, DateTime projectionDate, string season )
		{
			var s = HtmlLib.TableOpen( "border=1 cellpadding='0' cellspacing='0'" );
			var nfc = LoadNfcConference( season );
			var afc = LoadAfcConference( season, nfc );
			s += nfc.SeasonProjection( metric, this, projectionDate );
			s += afc.SeasonProjection( metric, this, projectionDate );
			return s;
		}

		private static NflConference LoadAfcConference( string season, NflConference nfc )
		{
			var afc = new NflConference( "AFC", season );
			nfc.QuickAddDiv( "East", "E" );
			nfc.QuickAddDiv( "North", "F" );
			nfc.QuickAddDiv( "South", "G" );
			nfc.QuickAddDiv( "West", "H" );
			return afc;
		}

		private static NflConference LoadNfcConference( string season )
		{
			var nfc = new NflConference( "NFC", season );
			nfc.QuickAddDiv( "East", "A" );
			nfc.QuickAddDiv( "North", "B" );
			nfc.QuickAddDiv( "South", "C" );
			nfc.QuickAddDiv( "West", "D" );
			return nfc;
		}

	}
}
