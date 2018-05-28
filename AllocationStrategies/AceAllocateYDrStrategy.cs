using RosterLib.Interfaces;

namespace RosterLib
{
	public class AceAllocateYDrStrategy : IAllocateYDrStrategy
	{
		public void Allocate( RushUnit ru, int nYDr, PlayerGameMetricsCollection pgms )
		{
			//  aces get 90%
			var projYDr = ( int ) ( 0.9M * nYDr );
			var pgm = pgms.GetPgmFor( ru.R1.PlayerCode );
			pgm.ProjYDr += ( int ) ( projYDr * ru.R1.HealthFactor() );
			pgms.Update( pgm );
		}
	}
}
