using System.Collections;

namespace RosterLib
{
	/// <summary>
	///   I render data to a simple report.
	/// </summary>
	public interface IRenderWeekly
	{
		string RenderData( ArrayList playerList, string header, NFLWeek week );
	}
}

