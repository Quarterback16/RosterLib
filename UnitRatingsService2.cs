using System;

namespace RosterLib
{
	public class UnitRatingsService2 : IRetrieveUnitRatings2
	{
		public string GetUnitRatingsFor(string teamCode, DateTime when)
		{
			var theSunday = GetSundayFor(when);
			var ratings = Utility.TflWs.GetUnitRatings(theSunday, teamCode);
			return ratings;
		}

		public DateTime GetSundayFor(DateTime when)
		{
			var theSeason = Utility.SeasonFor(when);
			var theSunday = Utility.TflWs.GetSeasonStartDate(theSeason);
			if (when <= theSunday)
				return theSunday;
			for (var i = 1; i < 16; i++)
			{
				var sunday = theSunday.AddDays(i * 7);
				if (when > sunday) continue;
				theSunday = sunday;
				break;
			}
			return theSunday;
		}

		public bool ThisSeasonOnly
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
}
