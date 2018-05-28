using System;
using RosterLib.Interfaces;

namespace RosterLib.Helpers
{
   public class SystemClock : IClock
   {
      public DateTime Now()
      {
         return DateTime.Now;
      }

      public int GetMonth()
      {
         var theDate = DateTime.Now;
         var m = theDate.Month;
         return m;
      }

      DateTime IClock.Now
      {
         get { return DateTime.Now; }
      }
   }
}
