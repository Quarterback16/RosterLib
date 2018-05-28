using RosterLib.Interfaces;

namespace RosterLib
{
	public class StandardAllocateTDrStrategy : IAllocateTDrStrategy
	{
		public void Allocate( RushUnit ru, int nTDr, PlayerGameMetricsCollection pgms )
		{
			if ( ru.Starters.Count > 0 )
			{
				var pgm = pgms.GetPgmFor( ru.R1.PlayerCode );
				pgm.PlayerId = ru.R1.PlayerCode;  // incase its a new one
				pgm.ProjTDr += R1TdsFrom( nTDr );
				pgms.Update( pgm );
				var pgm2 = pgms.GetPgmFor( ru.R2.PlayerCode );
				pgm2.PlayerId = ru.R2.PlayerCode;  // incase its a new one
				pgm2.ProjTDr += R2TdsFrom( nTDr );
				pgms.Update( pgm2 );
			}
		}

		private static int R1TdsFrom( int totalTdr )
		{
			var tdrs = 0;
			switch ( totalTdr )
			{
				case 1:
					tdrs = 1;
					break;

				case 2:
					tdrs = 1;
					break;

				case 3:
					tdrs = 2;
					break;

				case 4:
					tdrs = 3;
					break;

				case 5:
					tdrs = 3;
					break;

				case 6:
					tdrs = 3;
					break;
			}
			return tdrs;
		}

		private static int R2TdsFrom( int totalTdr )
		{
			var tdrs = 0;
			switch ( totalTdr )
			{
				case 1:
					tdrs = 0;
					break;

				case 2:
					tdrs = 1;
					break;

				case 3:
					tdrs = 1;
					break;

				case 4:
					tdrs = 1;
					break;

				case 5:
					tdrs = 2;
					break;

				case 6:
					tdrs = 2;
					break;
			}
			return tdrs;
		}
	}
}