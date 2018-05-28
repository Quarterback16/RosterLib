namespace RosterLib
{
   public class LineupSlot
	{
		/// <summary>
		///   Just to get the order
		/// </summary>
		public int  SlotNumber { get; set; }

		/// <summary>
		///   Which ranked player goes in this slot
		/// </summary>
		public int  Rank { get; set; }

		/// <summary>
		///   What types of players can be used in this slot
		/// </summary>
		public string[] SlotType { get; set; }		

		public string SlotCode { get; set; }

		public NFLPlayer PlayerSelected { get; set; }

		public NflTeam Opponent { get; set; }

		public LineupSlot()
		{
			Rank = 1;
			SlotCode = "X";
		}
	}
}
