using RosterLib.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class LoadPassUnit : ILoadPassUnit
	{
		public List<NFLPlayer> Load( string teamCode, string playerCat )
		{
			var playerList = new List<NFLPlayer>();
			var ds = Utility.TflWs.GetTeamPlayers( teamCode, playerCat );
			var dt = ds.Tables[ "player" ];
			if ( dt.Rows.Count != 0 )
				foreach ( DataRow dr in dt.Rows )
					playerList.Add( new NFLPlayer( dr[ "PLAYERID" ].ToString() ) );
			return playerList;
		}

	}
}
