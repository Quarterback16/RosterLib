using System;

namespace RosterLib
{
   /// <summary>
   ///  A tipping algorithm based on a 1970s Nibble magazine.
   /// </summary>
   public class NibblePredictor : IPrognosticate
	{
		public bool StorePrediction { get; set; }
		public IRetrieveNibbleRatings RatingsService { get; set; }
		public IStorePredictions PredictionStorer { get; set; }
		public bool AuditTrail { get; set; }
		public bool TakeActuals { get; set; }

		public NibblePredictor()
		{
		}

		public NFLResult PredictGame(NFLGame game, IStorePredictions persistor, DateTime predictionDate)
		{
			const int avgScore = 21;

			var homeRating = RatingsService.GetNibbleRatingFor( game.HomeNflTeam, predictionDate );
			var awayRating = RatingsService.GetNibbleRatingFor( game.AwayNflTeam, predictionDate );

			var homeOff = homeRating.Offence;
			var homeDef = homeRating.Defence;
			var awayOff = awayRating.Offence;
			var awayDef = awayRating.Defence;

			var homeScore = avgScore + ((homeOff + awayDef) / 2) + 3;  //  3pts home field advantage
			var awayScore = avgScore + ((awayOff + homeDef) / 2);

			homeScore = Utility.PickAScore(homeScore);
			awayScore = Utility.PickAScore(awayScore);

			if (homeScore == awayScore) homeScore++;  //  no ties
			var res = new NFLResult( game.HomeTeam, homeScore, game.AwayTeam, awayScore );

			//TODO:  Nibble predictor does not predict Tdp or Tdr

			if (AuditTrail)
			{
				AuditPrediction(game, awayDef, awayOff, homeOff, res, homeDef);
				if ( StorePrediction )
					StorePredictedResult( game, res );
			}
			return res;
		}

		private void StorePredictedResult( NFLGame game, NFLResult nflResult )
		{
			PredictionStorer.StorePrediction( "nibble", game, nflResult );
		}


		private static void AuditPrediction(NFLGame game, int awayDef, int awayOff, int homeOff, NFLResult res, int homeDef)
		{
			const string debugTeamCode = "GB";
			var debugTeamRank = "(0-0)";
			var strVenue = "unknown";
			var oppTeamCode = "YY";
			var oppRank = "(0-0)";
			const string rankFormat = "({0}.{1}={2})";
			var bAudit = false;
			if (game.HomeTeam.Equals(debugTeamCode))
			{
				bAudit = true;
				strVenue = "Home";
				oppTeamCode = game.AwayTeam;
				oppRank = string.Format(rankFormat, awayOff, awayDef, Rating(awayOff, awayDef));
				debugTeamRank = string.Format(rankFormat, homeOff, homeDef, Rating(homeOff, homeDef));
			}
			if (game.AwayTeam.Equals(debugTeamCode))
			{
				bAudit = true;
				strVenue = "Away";
				oppTeamCode = game.HomeTeam;
				oppRank = string.Format(rankFormat, homeOff, homeDef, Rating(homeOff, homeDef));
				debugTeamRank = string.Format(rankFormat, awayOff, awayDef, Rating(awayOff, awayDef));
			}
			if (!bAudit) return;
			Utility.Announce(string.Format(" {5} Debug Team {0} {1}, is {2} vs {3} {4}",
			                               debugTeamCode, debugTeamRank, strVenue, oppTeamCode, oppRank, game.GameCodeOut()));

			Utility.Announce(res.LogResult());
		}

		static private int Rating( int off, int def )
		{
			return ( off - def );
		}

		#region  Self testing code

		public void PredictSeason( string season, DateTime projectionDate, string fileOut )
		{
			var html = new HtmlFile( fileOut,
									" Win Projections as of " + projectionDate.ToString( "dd MMM yy" ) );
			html.AddToBody( Header( "Season Projections - Nibble Predictor - "  + season ) );
			html.AddToBody( SeasonOut( "Spread", projectionDate, season ) );
			html.Render();			
		}

		private string SeasonOut( string metric, DateTime projectionDate, string season )
		{
			var s = HtmlLib.TableOpen( "border=1 cellpadding='0' cellspacing='0'" );
			var nfc = LoadNfcConference(season);
			var afc = LoadAfcConference(season, nfc);
			s += nfc.SeasonProjection( metric, this, projectionDate );
			s += afc.SeasonProjection( metric, this, projectionDate );
			return s;
		}

		private static NflConference LoadAfcConference(string season, NflConference nfc)
		{
			var afc = new NflConference("AFC", season);
			nfc.QuickAddDiv("East", "E");
			nfc.QuickAddDiv("North", "F");
			nfc.QuickAddDiv("South", "G");
			nfc.QuickAddDiv("West", "H");
			return afc;
		}

		private static NflConference LoadNfcConference(string season)
		{
			var nfc = new NflConference("NFC", season);
			nfc.QuickAddDiv("East", "A");
			nfc.QuickAddDiv("North", "B");
			nfc.QuickAddDiv("South", "C");
			nfc.QuickAddDiv("West", "D");
			return nfc;
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

		#endregion

	}

}
