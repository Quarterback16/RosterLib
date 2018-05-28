using System;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class KickUnit : NflUnit
	{
		public NFLPlayer PlaceKicker { get; set; }

		public List<NFLPlayer> Kickers { get; set; }

		public string TeamCode { get; set; }

      public KickUnit()
      {
         Kickers = new List<NFLPlayer>();;
      }

		public List<string> Load(string teamCode)
		{
			TeamCode = teamCode;
			var ds = Utility.TflWs.GetTeamPlayers(teamCode, Constants.K_KICKER_CAT);
			var dt = ds.Tables["player"];
			if (dt.Rows.Count == 0) return DumpUnit();
			foreach (DataRow dr in dt.Rows)
			{
				var newPlayer = new NFLPlayer(dr["PLAYERID"].ToString());
				Add(newPlayer);
				if (newPlayer.IsStarter())
					PlaceKicker = newPlayer;
			}

			return DumpUnit();
		}

		public void Add(NFLPlayer player)
		{
			Kickers.Add(player);
		}

		public List<string> DumpUnit()
		{
			var output = new List<string>();
			var unit = string.Empty;

			foreach (var kicker in Kickers)
			{
				var pk = string.Format("{0,-25} : {1} : {2}", kicker.ProjectionLink(25), kicker.PlayerRole, kicker.PlayerPos);
				Utility.Announce(pk);
				unit += pk + Environment.NewLine;
			}
			output.Add(unit + Environment.NewLine);
			return output;
		}
	}
}
