using RosterLib.Interfaces;

namespace RosterLib
{
	public class CommitteeAllocateYDrStrategy : IAllocateYDrStrategy
	{
		public void Allocate( RushUnit ru, int nYDr, PlayerGameMetricsCollection pgms )
		{
			var nStarters = 0;
			//  45% 45%
			foreach ( var starter in ru.Starters )
			{
				var projYDr = ( int ) ( 0.45M * nYDr );
				var pgm = pgms.GetPgmFor( starter.PlayerCode );
				pgm.ProjYDr += ( int ) ( projYDr * starter.HealthFactor() );
				pgms.Update( pgm );
				nStarters++;
				if ( nStarters > 2 ) break;
			}
		}
	}
}
