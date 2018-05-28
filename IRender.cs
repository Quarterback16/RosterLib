using System.Collections;
using System.Runtime.InteropServices;

namespace RosterLib
{
	/// <summary>
	///   I render data to a simple report.
	/// </summary>
	public interface IRender
	{
		string RenderData( ArrayList playerList, string header, [Optional] string sortOrder, IRatePlayers scorer );
	}
}
