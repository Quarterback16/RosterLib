using System.Collections;
using System.Data;

namespace RosterLib
{
	/// <summary>
	///   Iterate through the teams and spit out their lineup cards.
	/// </summary>
	public class LineupSlate
	{
		private readonly ArrayList _teamList;

		public LineupSlate( string season )
		{
			var ds = Utility.TflWs.TeamsDs( season );
			var dt = ds.Tables["Team"];
			_teamList = new ArrayList();
			foreach (DataRow dr in dt.Rows)
			{
				var t = Masters.Tm.GetTeam( season, dr[ "TEAMID" ].ToString() );
				
				_teamList.Add( t );
			}
		}

		public void RenderAsHtml()
		{
			foreach ( NflTeam t in _teamList )
			{
				t.SpitLineups( true );
#if DEBUG
				//break;
#endif
			}

			return;
		}
	}
}
