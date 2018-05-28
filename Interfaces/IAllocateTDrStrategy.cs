namespace RosterLib.Interfaces
{
	public interface IAllocateTDrStrategy
	{
		void Allocate( RushUnit ru, int nTDr, PlayerGameMetricsCollection pgms );
	}
}
