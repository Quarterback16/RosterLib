using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using RosterLib.Interfaces;

namespace RosterLib
{
   public class TeamReport : RosterGridReport, IHtmlReport
   {
      public SimpleTableReport Ste { get; set; }

      public string Heading { get; set; }

      public Dictionary<string, NflTeam> TeamList { get; set; }

      public DataTable Data { get; set; }

      public TeamReport( IKeepTheTime timekeeper ) : base( timekeeper )
	  {
         LoadTeams();
      }

      public void Render()
      {
         Render( Ste, Heading );
      }

      public void Render( SimpleTableReport r, string header )
      {
         r.LoadBody( Data );
         r.RenderAsHtml(FileOut, persist:true);
      }

      private void LoadTeams()
      {
         var ds = Utility.TflWs.TeamsDs( Season );
         DataTable dt = ds.Tables[ "Team" ];
         TeamList = new Dictionary<string, NflTeam>();

         foreach ( DataRow dr in dt.Rows )
         {
#if DEBUG2
				if ( dr[ "TEAMID" ].ToString() != "SF" )
					continue;
#endif
				var t = new NflTeam( dr[ "TEAMID" ].ToString(), Season,
                                     Int32.Parse( dr[ "WINS" ].ToString() ),
                                     dr[ "TEAMNAME" ].ToString() );
            t.MetricsHt = new Hashtable();
            TeamList.Add( t.TeamCode, t );
         }
      }
   }
}