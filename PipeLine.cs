using System;
using System.Collections.Generic;

namespace RosterLib
{
   public class PipeLine<T>
   {
      private readonly List<Action<T>> _actions = new List<Action<T>>();

      public void Execute( T input )
      {
#if DEBUG
         var _et = new ElapsedTimer();
         var stepNo = 0;
#endif

         foreach ( var action in _actions )
         {
#if DEBUG
            _et.Start( DateTime.Now );
            stepNo++;
#endif

            action.Invoke( input );

#if DEBUG
            _et.Stop( DateTime.Now );
            Utility.Announce( string.Format( "Step {1} took {0} ",
            _et.TimeOut(), stepNo ) );
#endif
         }
      }

      public PipeLine<T> Register( Action<T> action )
      {
         _actions.Add( action );
         return this;
      }
   }
}