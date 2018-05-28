using System;

namespace RosterLib
{
	public class ClockWatcher
	{
		public int DaysTill( string eventCode )
		{
			var nDays = 0;
			if ( eventCode.Equals( "KickOff" ) )
			{
				var seasonStart = new DateTime( 2012, 09, 09 );
				var diff = seasonStart - DateTime.Now;
				nDays = diff.Days;
			}
			return nDays;
		}
	}
}
