using System;

namespace RosterLib
{
	public interface INLog
	{
		void Info( string message );

		void Warning( string message );

		void Error( string message );

		void Exception( Exception exception );
	}
}
