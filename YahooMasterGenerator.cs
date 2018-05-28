using NLog;
using RosterLib.Interfaces;

namespace RosterLib
{
	public class YahooMasterGenerator : RosterGridReport
	{
		public YahooMaster YahooMaster { get; set; }

		public bool FullSeason { get; set; }

		public YahooMasterGenerator( 
			bool fullSeason, 
			IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Yahoo Master Generator";
			YahooMaster = new YahooMaster( "Yahoo", "YahooOutput.xml" );
			Logger = LogManager.GetCurrentClassLogger();
			FullSeason = fullSeason;
			TimeKeeper = timekeeper;
		}

		public override void RenderFullAsHtml()
		{
			YahooMaster.Calculate( TimeKeeper.Season );
			YahooMaster.Dump2Xml( Logger );
		}

		public override void RenderAsHtml()
		{
			if ( FullSeason )
			{
				YahooMaster.Calculate( TimeKeeper.Season );
				Logger.Info( "  Generating Yahoo xml for full Season}" );
			}
			else
			{
				Logger.Info(
					"  Generating Yahoo xml for Season {0} Week {1}",
				   TimeKeeper.Season, TimeKeeper.Week );
				YahooMaster.Calculate( 
					TimeKeeper.Season, 
					TimeKeeper.PreviousWeek() );
			}
			YahooMaster.Dump2Xml( Logger );
		}

		public override string OutputFilename()
		{
			return YahooMaster.Filename;
		}
	}
}