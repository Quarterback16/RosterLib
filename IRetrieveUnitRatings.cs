using System;

namespace RosterLib
{
	public interface IRetrieveUnitRatings
	{
		string GetUnitRatingsFor(NflTeam team, DateTime when);

		bool ThisSeasonOnly { get; set; }

	}
}
