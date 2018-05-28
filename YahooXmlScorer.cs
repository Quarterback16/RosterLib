using RosterLib.Interfaces;
using RosterLib.Services;
using System;

namespace RosterLib
{
	public class YahooXmlScorer : IRatePlayers
	{
		public IYahooStatService YahooStatService { get; set; }

		public string Name { get; set; }

		public bool ScoresOnly { get; set; }

		public XmlCache Master
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public NFLWeek Week { get; set; }
		public bool AnnounceIt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public YahooXmlScorer( NFLWeek week )
		{
			Name = "Yahoo XML Scorer";
			Week = week;
			YahooStatService = new YahooStatService();
		}

		public decimal RatePlayer( NFLPlayer plyr, NFLWeek week, bool takeCache = true )
		{
			if ( week.WeekNo.Equals( 0 ) ) return 0;

			//  Check the stats service first
			if ( YahooStatService.IsStat( plyr.PlayerCode, week.Season, week.Week ) )
				return YahooStatService.GetStat( plyr.PlayerCode, week.Season, week.Week );
			else
				return 0.0M;
		}
	}
}