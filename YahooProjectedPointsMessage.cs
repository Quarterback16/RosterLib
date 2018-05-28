namespace RosterLib
{
	public class YahooProjectedPointsMessage
	{
		public NFLPlayer Player { get; set; }
		public NFLGame Game { get; set; }
		public PlayerGameMetrics PlayerGameMetrics { get; set; }

      public string StatLine()
      {
         var statLine = "???";
         switch ( Player.PlayerCat )
         {
            case Constants.K_QUARTERBACK_CAT:
               statLine = string.Format( "{0}({1})", 
                  PlayerGameMetrics.ProjYDp, PlayerGameMetrics.ProjTDp );
               break;

            case Constants.K_RUNNINGBACK_CAT:
               statLine = string.Format( "{0}({1})",
                  PlayerGameMetrics.ProjYDr, PlayerGameMetrics.ProjTDr );
               break;

            case Constants.K_RECEIVER_CAT:
               statLine = string.Format( "{0}({1})",
                  PlayerGameMetrics.ProjYDc, PlayerGameMetrics.ProjTDc );
               break;

            case Constants.K_KICKER_CAT:
               statLine = string.Format( "{0}({1})",
                  PlayerGameMetrics.ProjFG, PlayerGameMetrics.ProjPat );
               break;

            default:
               break;
         }
         return statLine;
      }

      internal bool TestPlayer()
      {
         if ( Player == null ) return false;
         return Player.PlayerCode.Equals("XXXXXX01");
      }
   }
}
