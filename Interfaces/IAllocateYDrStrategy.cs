namespace RosterLib.Interfaces
{
	public interface IAllocateYDrStrategy
	{
		void Allocate( RushUnit ru, int nYDr, PlayerGameMetricsCollection pgms );
	}
}
