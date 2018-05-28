using System;

namespace RosterLib
{
	public class PerformanceReport
	{
		public string Season { get; set; }
		public int WeekNo { get; set; }
		public IRatePlayers Scorer { get; set; }
		public string FileOut { get; set; }
		public PlayerLister PlayerLister { get; set; }
		public bool StartersOnly { get; set; }

		public PerformanceReport( string season, int week )
		{
			Season = season;
			WeekNo = week;
			PlayerLister = new PlayerLister();
		}

		public PerformanceReport( string season, int week, IRatePlayers scorer )
		{
			Season = season;
			WeekNo = week;
			Scorer = scorer;
			PlayerLister = new PlayerLister();
		}

		public void Render(string catCode, string sPos, string leagueId, bool startersOnly )
		{
			var currentWeek = new NFLWeek( seasonIn:Int32.Parse( Season ), weekIn:17, loadGames:false );
			if ( currentWeek.WeekNo > 0 )
			{
				var gs = leagueId.Equals( Constants.K_LEAGUE_Yahoo )
				                  	? (IRatePlayers) new YahooScorer( currentWeek )
				                  	: new GS4Scorer( currentWeek ) {ScoresOnly = true};

				if ( Scorer.Master == null )
					Scorer.Master = new YahooMaster( "Yahoo", "YahooOutput.xml" );

				PlayerLister.SetScorer( Scorer );
				PlayerLister.SetFormat( "weekly" );
				PlayerLister.AllWeeks = false; //  dont go back into last year anymore
				PlayerLister.StartersOnly = startersOnly;
            PlayerLister.Clear();
				PlayerLister.Collect( catCode, sPos, leagueId );

				var targetFile = string.Format( leagueId.Equals( Constants.K_LEAGUE_Yahoo ) 
					? "ESPN {1} Performance {0}" : "GS {1} Performance {0}", currentWeek.Season, sPos );

				var destinationFile = string.Format( "{3}{2}//Performance//{0}-{1:0#}.htm",
					targetFile, WeekNo, Season, Utility.OutputDirectory() );

				FileOut = PlayerLister.Render( destinationFile );
			}
			else
				Utility.Announce( "Season has not started yet" );

		}
	}
}
