using RosterLib.Models;

namespace RosterLib
{
   public interface IUnitPerformanceRepository
	{
		bool Add( UnitPerformance unitPerformance );
		bool Delete( UnitPerformance unitPerformance );
		UnitPerformance GetByKey( string teamCode, string season, int week, string unitCode );
	}
}
