using System;

namespace RosterLib
{
   /// <summary>
   /// Summary description for ElapsedTimer.
   /// </summary>
   public class ElapsedTimer
   {
      private DateTime startTime;
      private DateTime endTime;
      private TimeSpan elapsedTime;

      const string K_dataFmt = "{0,-12}{1,8}";    // left justify 12 and right justify 8

      public void Start( DateTime startTimeIn )
      {
         startTime = startTimeIn;
      }

      public void Stop( DateTime endTimeIn )
      {
         endTime = endTimeIn;
         elapsedTime = endTime - startTime;
      }

      public TimeSpan ElapsedTime()
      {
         return elapsedTime;
      }

      public string DaysElapsed()
      {
         return string.Format( K_dataFmt, "Days", elapsedTime.Days );
      }

      public string HoursElapsed()
      {
         return string.Format( K_dataFmt, "Hours", elapsedTime.Hours );
      }

      public string MinutesElapsed()
      {
         return string.Format( K_dataFmt, "Minutes", elapsedTime.Minutes );
      }

      public string SecondsElapsed()
      {
         return string.Format( K_dataFmt, "Seconds", elapsedTime.Seconds );
      }

      public string MillisecondsElapsed()
      {
         return string.Format( K_dataFmt, "Milliseconds", elapsedTime.Milliseconds);
      }

      public string TimeOut()
      {
         return String.Format("{1}:{0:0#}:{2:0#}.{3:0#}", 
            elapsedTime.Minutes, elapsedTime.Hours, elapsedTime.Seconds, elapsedTime.Milliseconds );
      }
   }
}
