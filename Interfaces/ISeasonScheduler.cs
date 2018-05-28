using System;
namespace RosterLib.Interfaces
{
   public interface ISeasonScheduler
   {
      bool ScheduleAvailable( string season );

      DateTime SeasonStarts(string season);

      DateTime RegularSeasonEnds(string season);

      DateTime SeasonEnds(string season);

      DateTime WeekStarts( string season, string week );

      DateTime WeekEnds( string season, string week );

      string WeekKey(DateTime theDate);

   }
}
