using System;

namespace RosterLib.Interfaces
{
   public interface IClock
   {
      DateTime Now { get; }

      int GetMonth();
   }
}
