namespace RosterLib
{
   public interface IWeekMaster
   {
      NFLWeek GetWeek(string season, int week);
      NFLWeek PreviousWeek(NFLWeek theWeek);
   }


}
