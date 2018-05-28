namespace RosterLib
{
	public interface IBreakdown
	{
		void AddLine(string breakdownKey, string line);

		void Dump(string breakdownKey, string outputFileName);
	}
}
