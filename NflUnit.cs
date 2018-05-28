using System;
using System.Collections.Generic;
using NLog;

namespace RosterLib
{
   public class NflUnit
   {
      public List<String> ErrorMessages { get; set; }

      public Logger Logger { get; set; }

      public NflUnit()
      {
         ErrorMessages = new List<string>();         
      }

      public void DumpErrors()
      {
         foreach ( var err in ErrorMessages )
         {
            Console.WriteLine( err );
            Utility.Announce( err );
         }
      }

      public void AddError( string msg )
      {
         ErrorMessages.Add( msg );
      }

      public void Announce( string message )
      {
         if ( Logger == null )
            Logger = NLog.LogManager.GetCurrentClassLogger();

         //Logger.Info( "   " + message );
#if DEBUG
         Utility.Announce( message );
#endif
      }
   }
}
