namespace RosterLib
{
    public class AddYahooKickingPoints
    {
        public AddYahooKickingPoints( YahooProjectedPointsMessage input )
        {
#if DEBUG
            Utility.Announce(
                $@"Calculating Kicking Points for {
                    input.Player.PlayerNameShort
                    } Game {input.Game.GameName()}" );
#endif
            if ( input.Player.PlayerCat.Equals(Constants.K_KICKER_CAT) )
            {
                Process( input );
            }
        }

        private static void Process( YahooProjectedPointsMessage input )
        {
            input.Player.Points += input.PlayerGameMetrics.ProjFG * 3;
#if DEBUG
            Utility.Announce( $"Projected FG = {input.PlayerGameMetrics.ProjFG} * 3 = {input.PlayerGameMetrics.ProjFG * 3}" );
#endif
            input.Player.Points += input.PlayerGameMetrics.ProjPat * 1;
#if DEBUG
            Utility.Announce( $"Projected Pat = {input.PlayerGameMetrics.ProjPat} * 1 = {input.PlayerGameMetrics.ProjPat * 1}" );
#endif
#if DEBUG
            Utility.Announce( $"Projected FP = {input.Player.Points}" );
#endif
        }
    }
}