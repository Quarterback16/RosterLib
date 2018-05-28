using RosterLib.Interfaces;

namespace RosterLib
{
	public class StandardAllocateYDrStrategy : IAllocateYDrStrategy
	{
		public void Allocate( RushUnit ru, int nYDr, PlayerGameMetricsCollection pgms )
		{
			if ( ru.Runners.Count == 0 ) return;
			if ( ru.Starters.Count > 0 )
			{
				//  70% 20%
				var projYDr = ( int ) ( 0.7M * nYDr );
				var pgm = pgms.GetPgmFor( ru.R1.PlayerCode );
				pgm.ProjYDr += ( int ) ( projYDr * ru.R1.HealthFactor() );
				pgms.Update( pgm );
			}

			if ( ru.R2 != null )
			{
				var projYDr2 = ( int ) ( 0.2M * nYDr );
				var pgm2 = pgms.GetPgmFor( ru.R2.PlayerCode );
				pgm2.ProjYDr += projYDr2;
				pgms.Update( pgm2 );
			}
		}
	}
}
