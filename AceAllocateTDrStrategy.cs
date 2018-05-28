using RosterLib.Interfaces;

namespace RosterLib
{
	public class AceAllocateTDrStrategy : IAllocateTDrStrategy
	{
		public void Allocate( RushUnit ru, int nTDr, PlayerGameMetricsCollection pgms )
		{
			var pgm = pgms.GetPgmFor( ru.R1.PlayerCode );
			pgm.PlayerId = ru.R1.PlayerCode;
			pgm.ProjTDr += nTDr;
			pgms.Update( pgm );
		}

	}
}
