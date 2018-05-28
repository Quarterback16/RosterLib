using System.Collections.Generic;
using System.Data;

namespace RosterLib
{

   public class PlayerOutput
	{
		private FrequencyTable _ft;

		public IRatePlayers Scorer { get; set; }

		public WeekRange wRange { get; set; }

		List<NFLPlayer> playerList;

		public string PlayerType { get; set; }

		public string ScoreType { get; set; }

		public string ScoreSlot{ get; set; }

		public string SinglePlayer { get; set; }

		public string FileName { get; set; }

		public PlayerOutput()
		{
			Utility.Announce( "PlayerOutput..." );
			playerList = new List< NFLPlayer >();
			wRange = new WeekRange();
		}

		public void Load()
		{
			_ft = new FrequencyTable( $@"{PlayerType} output week {
				wRange.startWeek.WeekKey( "-" )
				} to {
				wRange.endWeek.WeekKey( "-" )
				}" );

			if (string.IsNullOrEmpty(SinglePlayer))
			{
				//  get players who appeared in this range
				var ds = Utility.TflWs.GetPlayersScoring(
					ScoreType, wRange.startWeek.Season, wRange.endWeek.Season, ScoreSlot);
				var dt = ds.Tables["score"];
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						var player = new NFLPlayer(dr[string.Format("PLAYERID{0}", ScoreSlot)].ToString());
						if (player.IsStarter())
						{
							playerList.Add(player);
							Utility.Announce($@"  Adding {
								player.PlayerNameShort,-15
								} to the list of {PlayerType}");
						}
					}
				}
			}
			else
			{
				//  single player
				var player = new NFLPlayer( SinglePlayer );
				playerList.Add( player );
				Utility.Announce(string.Format("  Adding {0,-15} to the list of {1}",
						player.PlayerNameShort, PlayerType));
			}
			Utility.Announce($"    Examining {playerList.Count:#0} players");

			//  do the counting, so get all the performances for 
			foreach (NFLPlayer p in playerList)
			{
				var week = wRange.startWeek;
				while ( wRange.endWeek.IsBefore( week ) )
				{
					var amount = Scorer.RatePlayer( p, week );
					_ft.Add( amount );
					Utility.Announce( string.Format( "  {0,-15} got {1,2} in {2}",
						p.PlayerNameShort, amount, week.WeekKey() ) );
					week = week.NextWeek( week );
				}
			}
			_ft.Calculate();
		}


		public void Render( string fileName, string season )
		{
			_ft.RenderAsHtml( fileName, season );
			FileName = _ft.FileName;
		}

	}
}
