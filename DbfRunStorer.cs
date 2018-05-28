using System;

namespace RosterLib
{
	public class DbfRunStorer : IRunStorer
	{
		public void StoreRun( string stepName, TimeSpan ts )
		{
			Utility.TflWs.InsertRun( stepName, ts, "Report" );
		}

		public void StoreRun(string stepName, TimeSpan ts, string category )
		{
			Utility.TflWs.InsertRun(stepName, ts, category);
		}
	}
}