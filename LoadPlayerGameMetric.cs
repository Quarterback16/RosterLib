using NLog;
using System;

namespace RosterLib
{
   /// <summary>
   ///   This is a "Filter"
   /// </summary>
   public class LoadPlayerGameMetric
   {
      public Logger Logger { get; set; }

      public LoadPlayerGameMetric()
      {
         Logger = LogManager.GetCurrentClassLogger();
      }

      public LoadPlayerGameMetric( YahooProjectedPointsMessage input )
      {
         if ( input.Game != null && input.Player != null )
            Process( input, new DbfPlayerGameMetricsDao() );
         else
         {
            if (input.Game == null )
               Logger.Error( "Input missing Game" );
            else
               Logger.Error( "Input missing Player" );
         }
      }

      private void Process( 
          YahooProjectedPointsMessage input, 
          IPlayerGameMetricsDao dao )
      {
         if ( dao == null )
                throw new ArgumentNullException( 
                    "dao", 
                    "parameter is null" );

         if ( input != null )
         {
            input.PlayerGameMetrics = dao.Get( 
                input.Player.PlayerCode, 
                input.Game.GameKey() );

            if ( input.TestPlayer() )
            {
               Logger.Info( "PGM got {0}", input.PlayerGameMetrics );
            }
         }
         else
         {
            Logger.Info( "input is null" );
         }
      }
   }
}