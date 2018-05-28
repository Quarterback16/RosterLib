namespace RosterLib
{
   public class ClearGameMetrics
	{
		public ClearGameMetrics( PlayerGameProjectionMessage input )
		{
			Process( input );
		}

		private void Process( PlayerGameProjectionMessage input )
		{
			input.Dao = new DbfPlayerGameMetricsDao();
			input.Dao.ClearGame( input.Game.GameKey() );
		}
	}
}
