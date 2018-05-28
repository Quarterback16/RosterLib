using System;

namespace RosterLib
{
	public class YahooProjector
	{
		public string FileOut { get; set; }

		public PlayerLister Lister { get; set; }

		public YahooProjector()
		{
			Lister = new PlayerLister();
		}

		public void AllProjections( NFLWeek week )
		{
			ProjectYahooPerformance( Constants.K_QUARTERBACK_CAT, week.WeekNo, "QB" );
			ProjectYahooPerformance( Constants.K_RECEIVER_CAT, week.WeekNo, "WR" );
			ProjectYahooPerformance( Constants.K_RECEIVER_CAT, week.WeekNo, "TE" );
			ProjectYahooPerformance( Constants.K_RUNNINGBACK_CAT, week.WeekNo, "RB" );
			ProjectYahooPerformance( Constants.K_KICKER_CAT, week.WeekNo, "PK" );			
		}

		public string ProjectYahooPerformance( string catCode, int weekNo, 
			[System.Runtime.InteropServices.Optional] string sPos )
		{
			if ( String.IsNullOrEmpty( sPos ) || sPos.Equals( "PK" ) )
				sPos = "*";

			var nextWeek = new NFLWeek( Int32.Parse( Utility.CurrentSeason() ), weekNo, false );
			var gs = new StatProjector( nextWeek );

			Lister.SetScorer( gs );
			Lister.StartersOnly = true;
			Lister.SetFormat( "weekly" );
			Lister.Week = nextWeek.WeekNo;
			Lister.AllWeeks = false; //  just the regular saeason
			Lister.Season = nextWeek.Season;
			Lister.Collect( catCode, sPos, Constants.K_LEAGUE_Yahoo );

			if ( String.IsNullOrEmpty( sPos ) || sPos.Equals("*") ) sPos = "All";

			FileOut = String.Format( "{0}\\Yahoo\\{1}\\{1} Projection W{2:00}", Lister.Season, sPos, Lister.Week );
         var weekMaster = new WeekMaster();
			FileOut = Lister.RenderProjection( FileOut, weekMaster );
			FileOut = Lister.FileOut;
			return Lister.FileOut;
		}
	}
}
