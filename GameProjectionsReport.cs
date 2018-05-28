using RosterLib.Interfaces;

namespace RosterLib
{
	public class GameProjectionsReport : RosterGridReport
	{
		public NflSeason NflSeason { get; private set; }

		public GameProjectionsReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Game Projections Report";
			TimeKeeper = timekeeper;
		}

		public override void RenderAsHtml()
		{
			NflSeason = new NflSeason( 
				TimeKeeper.CurrentSeason(), 
				loadGames: true, 
				loadDivisions: false );
			foreach ( var game in NflSeason.GameList )
			{
				game.WriteProjection();
#if DEBUG
				if ( game.WeekNo > 1 ) break;
#endif
			}
		}
	}
}