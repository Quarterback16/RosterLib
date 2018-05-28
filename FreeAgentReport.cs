using RosterLib.Interfaces;
using System;

namespace RosterLib
{
	/// <summary>
	///   Dumps out a file for each teams current fantasy free agents.
	/// </summary>
    public class FreeAgentReport : RosterGridReport
	{
		public FreeAgentReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{

		}
		public int TeamCount;

		public void Render()
		{
			var s = Masters.Sm.GetSeason(Utility.CurrentSeason());
			foreach (string teamKey in s.TeamKeyList)
			{
				var t = Masters.Tm.GetTeam(teamKey);
				var fileOut = string.Format("{0}FreeAgents\\FA-{1}.htm", Utility.OutputDirectory(), t.TeamCode);
				var h = new HtmlFile(fileOut, " Free Agents as of " + 
               DateTime.Now.ToString("ddd dd MMM yy") +
				                              "  Week " + Utility.CurrentWeek());
				h.AddToBody(Header(t));
				h.AddToBody(t.FreeAgents(true, true, true));
				h.Render();
				TeamCount++;
#if DEBUG
				break;
#endif
			}
		}

		private static string Header(NflTeam t)
		{
			return HtmlLib.H2(string.Format("Free Agents for {0}", t.NameOut()));
		}
	}
}