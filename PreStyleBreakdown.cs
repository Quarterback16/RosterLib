using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace RosterLib
{
	public class PreStyleBreakdown : IBreakdown
	{
		public Hashtable Breakdowns { get; set; }

		public bool NumberLines { get; set; }

		public PreStyleBreakdown()
		{
			Breakdowns = new Hashtable();
			NumberLines = true;
		}

		public void AddLine(string breakdownKey, string line)
		{
			if (!Breakdowns.ContainsKey(breakdownKey))
				Breakdowns.Add(breakdownKey, new List<String>());

			var theLines = (List<String>) Breakdowns[breakdownKey];
			theLines.Add(line);
			Breakdowns[breakdownKey] = theLines;
		}

		public void Dump(string breakdownKey, string outputFileName)
		{
			if (!Breakdowns.ContainsKey(breakdownKey)) return;

			var theLines = (List<String>) Breakdowns[ breakdownKey ];

			Utility.EnsureDirectory( outputFileName );

			var sw = new StreamWriter( outputFileName, false );

			sw.WriteLine( "<pre>" );

			var i = 1;
			var myEnumerator = theLines.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				var numPart = "  ";
				if ( NumberLines ) numPart = $"{i++:0#}";
				sw.WriteLine( $"{numPart}  {myEnumerator.Current}" );
			}

			sw.WriteLine( "</pre>" );
			sw.Close();

			//Utility.Announce( $"   {outputFileName} has been rendered" );
		}
	}
}
