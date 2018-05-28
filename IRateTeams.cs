using System;

namespace RosterLib
{

	/// <summary>
	/// I evaluate a team using a certain scoring system over a certain week span.
	/// </summary>
	public interface IRateTeams
	{

		Int32 RateTeam(NflTeam team);

		NFLWeek Week
		{
			get;
			set;
		}

		string Name
		{
			get;
			set;
		}

	}


}
