using System.Collections;
using System.Data;

namespace RosterLib
{
	public class FakeTeamMetricsLister : IGetTeams
	{
		public System.Collections.ArrayList GetTeams( string season )
		{
			var teamList = new ArrayList();
			var ds = Utility.TflWs.GetTeams( season, "" );
			var teams = ds.Tables[ "teams" ];
			foreach ( DataRow dr in teams.Rows )
			{
				var teamCode = dr[ "TEAMID" ].ToString();
				var team = Utility.GetTeam( teamCode );
				team.MetricsHt = new Hashtable();
				team.SetRecord( season, skipPostseason:false );
				teamList.Add( team );
			}
			return teamList;
		}
	}
}
