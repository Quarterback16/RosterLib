using System.Collections.Generic;

namespace RosterLib
{
	public class FantasyLeague
	{
		public List<LineupSlot> LineupSlots { get; set; }

		public FantasyLeague( string leagueId )
		{
			LineupSlots = new List<LineupSlot>();

			LineupSlot slot;
			LineupSlot slot2;
			LineupSlot slot3;
			LineupSlot slot4;
			LineupSlot slot5;
			LineupSlot slot6;
			LineupSlot slot7;
			LineupSlot slot8;

			if (leagueId.Equals(Constants.K_LEAGUE_Yahoo )  )
			{
				slot = new LineupSlot {SlotNumber = 1, SlotType = new string[1]};
				slot.SlotType[0] = "1";  // QB
				slot.SlotCode = "QB";
				LineupSlots.Add( slot );

				slot2 = new LineupSlot {SlotNumber = 2, SlotType = new string[1]};
				slot2.SlotType[0] = "2";  // RB1
				slot2.SlotCode = "RB1";
				LineupSlots.Add( slot2 );

				slot3 = new LineupSlot {SlotNumber = 3, SlotType = new string[1]};
				slot3.SlotType[0] = "2";  // RB2
				slot3.SlotCode = "RB2";
				slot3.Rank = 2;
				LineupSlots.Add( slot3 );

				slot4 = new LineupSlot {SlotNumber = 4, SlotType = new string[1]};
				slot4.SlotType[0] = "W";  // WR
				slot4.SlotCode = "WR1";
				LineupSlots.Add( slot4 );

				slot5 = new LineupSlot {SlotNumber = 5, SlotType = new string[1]};
				slot5.SlotType[0] = "W";  // WR
				slot5.SlotCode = "WR2";
				slot5.Rank = 2;
				LineupSlots.Add( slot5 );

				slot6 = new LineupSlot {SlotNumber = 6, SlotType = new string[1]};
				slot6.SlotType[0] = "W";  // WR
				slot6.SlotCode = "WR3";
				slot6.Rank = 3;
				LineupSlots.Add( slot6 );

				slot7 = new LineupSlot {SlotNumber = 7, SlotType = new string[1]};
				slot7.SlotType[0] = "T";  // TE
				slot7.SlotCode = "TE";
				LineupSlots.Add( slot7 );

				slot8 = new LineupSlot {SlotNumber = 8, SlotType = new string[1]};
				slot8.SlotType[0] = "4";  // PK
				slot8.SlotCode = "PK";
				LineupSlots.Add( slot8 );
			}

			if ( leagueId.Equals( Constants.K_LEAGUE_PerfectChallenge ) )
			{
#if ! DEBUG2
				slot = new LineupSlot { SlotNumber = 1, SlotType = new string[ 1 ] };
				slot.SlotType[ 0 ] = "1";  // QB
				slot.SlotCode = "QB";
				LineupSlots.Add( slot );

				slot2 = new LineupSlot { SlotNumber = 2, SlotType = new string[ 1 ] };
				slot2.SlotType[ 0 ] = "2";  // RB1
				slot2.SlotCode = "RB1";
				LineupSlots.Add( slot2 );

				slot3 = new LineupSlot { SlotNumber = 3, SlotType = new string[ 1 ] };
				slot3.SlotType[ 0 ] = "2";  // RB2
				slot3.SlotCode = "RB2";
				slot3.Rank = 2;
				LineupSlots.Add( slot3 );

				slot4 = new LineupSlot { SlotNumber = 4, SlotType = new string[ 1 ] };
				slot4.SlotType[ 0 ] = "W";  // WR
				slot4.SlotCode = "WR1";
				LineupSlots.Add( slot4 );

				slot5 = new LineupSlot { SlotNumber = 5, SlotType = new string[ 1 ] };
				slot5.SlotType[ 0 ] = "W";  // WR
				slot5.SlotCode = "WR2";
				slot5.Rank = 2;
				LineupSlots.Add( slot5 );

				slot6 = new LineupSlot { SlotNumber = 6, SlotType = new string[ 1 ] };
				slot6.SlotType[ 0 ] = "T";  // TE
				slot6.SlotCode = "TE";
				LineupSlots.Add( slot6 );
#endif
				slot7 = new LineupSlot { SlotNumber = 7, SlotType = new string[ 1 ] };
				slot7.SlotType[ 0 ] = "4";  // PK
				slot7.SlotCode = "PK";
				LineupSlots.Add( slot7 );

			}

			if (!leagueId.Equals( Constants.K_LEAGUE_Gridstats_NFL1 )) return;

			slot = new LineupSlot {SlotNumber = 1, SlotType = new string[1]};
			slot.SlotType[0] = "1";  // QB
			slot.SlotCode = "Q1";
			LineupSlots.Add( slot );

			slot2 = new LineupSlot {SlotNumber = 2, SlotType = new string[1]};
			slot2.SlotType[0] = "1";  // QB
			slot2.Rank = 2;  //  take second best QB
			slot2.SlotCode = "Q2";
			LineupSlots.Add( slot2 );

			slot3 = new LineupSlot {SlotNumber = 3, SlotType = new string[1]};
			slot3.SlotType[0] = "2";  // RB1
			slot3.SlotCode = "R1";
			LineupSlots.Add( slot3 );

			slot4 = new LineupSlot {SlotNumber = 4, SlotType = new string[1]};
			slot4.SlotType[0] = "2";  // RB1 backup
			slot4.Rank = 4;
			slot4.SlotCode = "R4";
			LineupSlots.Add( slot4 );

			slot5 = new LineupSlot {SlotNumber = 5, SlotType = new string[1]};
			slot5.SlotType[0] = "2";  // RB2
			slot5.Rank = 2;
			slot5.SlotCode = "R2";
			LineupSlots.Add( slot5 );

			slot6 = new LineupSlot {SlotNumber = 6, SlotType = new string[1]};
			slot6.SlotType[0] = "2";  // RB2 backup
			slot6.Rank = 3;
			slot6.SlotCode = "R3";
			LineupSlots.Add( slot6 );

			slot7 = new LineupSlot {SlotNumber = 7, SlotType = new string[1]};
			slot7.SlotType[0] = "T";  // TE
			slot7.SlotCode = "T1";
			LineupSlots.Add( slot7 );

			slot8 = new LineupSlot {SlotNumber = 8, SlotType = new string[1]};
			slot8.SlotType[0] = "T";  // TE backup
			slot8.Rank = 2;
			slot8.SlotCode = "T2";
			LineupSlots.Add( slot8 );

			var slot9 = new LineupSlot {SlotNumber = 9, SlotType = new string[1]};
			slot9.SlotType[0] = "W";  // WR
			slot9.Rank = 1;
			slot9.SlotCode = "W1";
			LineupSlots.Add( slot9 );

			var slot10 = new LineupSlot {SlotNumber = 10, SlotType = new string[1]};
			slot10.SlotType[0] = "W";  // WR
			slot10.Rank = 4;
			slot10.SlotCode = "W4";
			LineupSlots.Add( slot10 );

			var slot11 = new LineupSlot {SlotNumber = 11, SlotType = new string[1]};
			slot11.SlotType[0] = "W";  // WR
			slot11.Rank = 2;
			slot11.SlotCode = "W2";
			LineupSlots.Add( slot11 );

			var slot12 = new LineupSlot {SlotNumber = 12, SlotType = new string[1]};
			slot12.SlotType[0] = "W";  // WR
			slot12.Rank = 3;
			slot12.SlotCode = "W3";
			LineupSlots.Add( slot12 );

			var slot13 = new LineupSlot {SlotNumber = 13, SlotType = new string[1]};
			slot13.SlotType[0] = "4";  // PK
			slot13.SlotCode = "K1";
			LineupSlots.Add( slot13 );

			var slot14 = new LineupSlot {SlotNumber = 14, SlotType = new string[1]};
			slot14.SlotType[0] = "4";  // PK backup
			slot14.Rank = 2;
			slot14.SlotCode = "K2";
			LineupSlots.Add( slot14 );
		}
	}
}
