using System.Collections.Generic;

namespace RosterLib.Interfaces
{
	public interface ILoadRunners
	{
		List<NFLPlayer> Load( string teamCode );
	}
}
