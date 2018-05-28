using NLog;

namespace RosterLib
{
   /// <summary>
   ///  filter for calculating yahoo points
   /// </summary>
   public class AddYahooPassingPoints
   {
      public Logger Logger { get; set; }

      public AddYahooPassingPoints()
      {
         Logger = NLog.LogManager.GetCurrentClassLogger();
      }

      public AddYahooPassingPoints( YahooProjectedPointsMessage input )
      {
         Logger = NLog.LogManager.GetCurrentClassLogger();
         if ( input.TestPlayer() )
         {
            Logger.Info( string.Format( "Calculating Passing Points for {0} Game {1}",
               input.Player.PlayerNameShort, input.Game.GameName() ) );
         }

         Process( input );
      }

      private void Process( YahooProjectedPointsMessage input )
      {
			input.Player.Points += input.PlayerGameMetrics.ProjTDp * 4;
         if ( input.TestPlayer() )
            Logger.Info( string.Format("Projected TDp = {0} * 4 = {1}", input.PlayerGameMetrics.ProjTDp, input.PlayerGameMetrics.ProjTDp * 4));

//         var yardagePts = Math.Floor( (decimal) input.PlayerGameMetrics.ProjYDp / 25 );
         var yardagePts = (decimal) input.PlayerGameMetrics.ProjYDp / 25.0M;
         if ( input.TestPlayer() )
            Logger.Info( string.Format("Projected YDp = {0} / 25 = {1}", input.PlayerGameMetrics.ProjYDp, input.PlayerGameMetrics.ProjYDp / 25 ));

         input.Player.Points += yardagePts;
         //TODO:  -1 for Interceptions
         //TODO:  +2 per PAT pass
         if ( input.TestPlayer() )
            Logger.Info( string.Format("Projected FP = {0}", input.Player.Points ));

      }

   }
}
