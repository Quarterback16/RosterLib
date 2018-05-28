using System.Collections.Generic;

namespace RosterLib.Interfaces
{
	public interface ILoadPassUnit
	{
		List<NFLPlayer> Load( string teamCode, string playerCat );
	}
}
