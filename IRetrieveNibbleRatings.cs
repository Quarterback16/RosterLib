using System;

namespace RosterLib
{
	public interface IRetrieveNibbleRatings
	{
		NibbleTeamRating GetNibbleRatingFor(NflTeam team, DateTime when);
	}
}
