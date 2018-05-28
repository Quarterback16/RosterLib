using System;

namespace RosterLib
{
	public interface IRetrieveUnitRatings2
	{
		string GetUnitRatingsFor(string teamCode, DateTime when);

		bool ThisSeasonOnly { get; set; }

	}
}
