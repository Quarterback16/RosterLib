namespace RosterLib.RosterGridReports
{
   public interface IWinOrLose
   {
      RosterLib.NFLGame Game { get; set; }
      bool Home { get; set; }

      bool IsWinner { get; set; }

      decimal Margin { get; set; }
      RosterLib.NflTeam Team { get; set; }
   }
}
