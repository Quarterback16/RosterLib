using System;
using System.Diagnostics;

namespace RosterLib
{
	public class BaseReport
	{
		public Action MyAction { get; set; }

		/// <summary>
		///   Execute the report
		/// </summary>
		public void Execute()
		{
			//  Check the timeing
			if ( !OkToExecute() )
			{
				SkipReport();
				return;
			}

			//  Do the report
			Utility.Announce( string.Format( "---- {0} --------------------------------", MyAction.Method.Name ), 0 );
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Utility.Announce( string.Format( "{0} ...", MyAction.Method.Name ), 0 );
			MyAction.Invoke();
			var ts = Utility.StopTheWatch( stopwatch, string.Format( "Finished: {0}", MyAction.Method.Name ) );
			Utility.Announce( "============================================", 0 );
			var runStorer = new DbfRunStorer();
			runStorer.StoreRun( MyAction.Method.Name, ts );
		}

		private void SkipReport()
		{
			Utility.Announce( string.Format( "{0} skipped.", MyAction.Method.Name ), 0 );			
		}

		public virtual bool OkToExecute()
		{
			return true;
		}
	}
}
