using System;


namespace RosterLib
{
   /// <summary>
   ///  filter for calculating yahoo points
   /// </summary>
   public class AddYahooReceivingPoints
   {
      public AddYahooReceivingPoints( YahooProjectedPointsMessage input )
      {
#if DEBUG
         Utility.Announce( string.Format( "Calculating Receiving Points for {0} Game {1}",
            input.Player.PlayerNameShort, input.Game.GameName() ) );
#endif
         Process( input );
      }

      private void Process( YahooProjectedPointsMessage input )
      {
         input.Player.Points += input.PlayerGameMetrics.ProjTDc * 6;
#if DEBUG
         Utility.Announce( string.Format( "Projected TDc = {0} * 6 = {1}",
            input.PlayerGameMetrics.ProjTDc, input.PlayerGameMetrics.ProjTDc * 6 ) );
#endif
//         var yardagePts = Math.Floor( ( decimal ) input.PlayerGameMetrics.ProjYDc / 10 );
         var yardagePts =  (decimal) input.PlayerGameMetrics.ProjYDc / 10.0M;
#if DEBUG
         Utility.Announce( string.Format( "Projected YDc = {0} / 10 = {1}",
            input.PlayerGameMetrics.ProjYDc, input.PlayerGameMetrics.ProjYDc / 10 ) );
#endif
         input.Player.Points += yardagePts;
         //TODO:  -2 for Fumbles
         //TODO:  +2 per PAT run
#if DEBUG
         Utility.Announce( string.Format( "Projected FP = {0}", input.Player.Points ) );
#endif
      }

   }
}
