using RosterLib.Interfaces;

namespace RosterLib
{
	public class CommitteeAllocateTDrStrategy : IAllocateTDrStrategy
	{
		public void Allocate( RushUnit ru, int nTDr, PlayerGameMetricsCollection pgms )
		{
			foreach ( var starter in ru.Starters )
			{
				var pgm = pgms.GetPgmFor( starter.PlayerCode );
				pgm.ProjTDr += nTDr / ru.Starters.Count;
				pgms.Update( pgm );
			}
		}
	}
}