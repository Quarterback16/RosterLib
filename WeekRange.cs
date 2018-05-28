namespace RosterLib
{


   public class WeekRange
	{
		public NFLWeek startWeek{ get; set; }
		public NFLWeek endWeek{ get; set; }

		public bool Contains( NFLGame game )
		{
			//TODO:  implement
			return true;
		}
	}
}
