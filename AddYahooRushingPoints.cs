using System;


namespace RosterLib
{
   /// <summary>
   ///  filter for calculating yahoo points
   /// </summary>
   public class AddYahooRushingPoints
   {
      public AddYahooRushingPoints(YahooProjectedPointsMessage input)
      {
#if DEBUG
         Utility.Announce(string.Format("Calculating Rushing Points for {0} Game {1}",
            input.Player.PlayerNameShort, input.Game.GameName()));
#endif
         Process(input);
      }

      private void Process(YahooProjectedPointsMessage input)
      {
         input.Player.Points += input.PlayerGameMetrics.ProjTDr * 6;
#if DEBUG
         Utility.Announce(string.Format("Projected TDr = {0} * 6 = {1}", 
            input.PlayerGameMetrics.ProjTDp, input.PlayerGameMetrics.ProjTDp * 6 ));
#endif
//         var yardagePts = Math.Floor((decimal)input.PlayerGameMetrics.ProjYDr / 10 );
         var yardagePts = (decimal) input.PlayerGameMetrics.ProjYDr / 10.0M;
#if DEBUG
         Utility.Announce(string.Format("Projected YDr = {0} / 10 = {1}", 
            input.PlayerGameMetrics.ProjYDp, input.PlayerGameMetrics.ProjYDr / 10 ));
#endif
         input.Player.Points += yardagePts;
         //TODO:  -2 for Fumbles
         //TODO:  +2 per PAT run
#if DEBUG
         Utility.Announce(string.Format("Projected FP = {0}", input.Player.Points));
#endif
         input.PlayerGameMetrics.ProjectedFantasyPoints = input.Player.Points;
      }

   }
}
