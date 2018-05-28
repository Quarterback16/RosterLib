using RosterLib.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class LoadRunners : ILoadRunners
	{
		public List<NFLPlayer> Load( string teamCode )
		{
			var playerList = new List<NFLPlayer>();
			var ds = Utility.TflWs.GetTeamPlayers( teamCode, Constants.K_RUNNINGBACK_CAT );
			var dt = ds.Tables[ "player" ];
			if ( dt.Rows.Count != 0 )
			{
				foreach ( DataRow dr in dt.Rows )
				{
					if ( dr[ "POSDESC" ].ToString().Trim().Contains( "FB" ) ) continue;

					var newPlayer = new NFLPlayer( dr[ "PLAYERID" ].ToString() );
					playerList.Add( newPlayer );
				}
			}
			return playerList;
		}
	}
}
