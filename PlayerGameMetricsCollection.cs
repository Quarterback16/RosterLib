using System.Collections.Generic;

namespace RosterLib
{
	public class PlayerGameMetricsCollection
	{
		public List<PlayerGameMetrics> Pgms { get; private set; }

		public string GameKey { get; set; }

		public PlayerGameMetricsCollection( NFLGame game )
		{
			GameKey = game.GameKey();
			var pgms = game.PlayerGameMetrics;
			if ( pgms == null )
				pgms = new List<PlayerGameMetrics>();
			Pgms = pgms;
		}

		public PlayerGameMetrics GetPgmFor( string playerId  )
		{
			var pgm = new PlayerGameMetrics();
			foreach ( var m in Pgms )
			{
				if ( m.PlayerId.Equals( playerId ) )
				{
					pgm = m;
					break;
				}
			}
			pgm.PlayerId = playerId;
			return pgm;
		}

		public void Update( PlayerGameMetrics pgm )
		{
			var index = Pgms.FindIndex( i => i.PlayerId == pgm.PlayerId );
			if ( index == -1 )
			{
				pgm.GameKey = GameKey;
				Pgms.Add( pgm );
			}
			else
				Pgms[ index ] = pgm;
		}

		public int NumberOfPgms()
		{
			return Pgms.Count;
		}

	}
}
