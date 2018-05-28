using System;

namespace RosterLib
{
	/// <summary>
	///   I evaluate a player using a certain scoring system over a certain week span.
	/// </summary>
	public interface IRatePlayers
	{
		Decimal RatePlayer( NFLPlayer plyr, NFLWeek week, bool takeCache = true );

		NFLWeek Week { get; set; }

		bool ScoresOnly { get; set; }

		string Name { get; set; }

		XmlCache Master { get; set; }
		bool AnnounceIt { get; set; }
	}
}