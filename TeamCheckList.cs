using System.Collections;
using System.Data;
using System.Text;

namespace RosterLib
{
   public class TeamCheckList
   {
      public Hashtable HtTeam { get; set; }
      public Hashtable HtDone { get; set; }

      public TeamCheckList()
      {
         HtTeam = new Hashtable();
         HtDone = new Hashtable();
         var ds = Utility.TflWs.GetTeams( Utility.CurrentSeason() );
         var dt = ds.Tables[ 0 ];
         foreach ( DataRow dr in dt.Rows )
         {
            var theKey = dr[ "TEAMID" ].ToString();
            HtTeam.Add( theKey, dr );
         }
      }

      public void TickOff( string theKey, string sPos )
      {
         if (HtTeam.ContainsKey( theKey ))
            HtTeam.Remove( theKey );

#if DEBUG
         if (HtDone.ContainsKey( theKey ))
         {
            Utility.Announce( "We have a duplicate " + sPos + " for " + theKey );
            //DumpHt();
         }
         else
            HtDone.Add( theKey, theKey );
#endif
      }

      public string TeamsLeft()
      {
         var sb = new StringBuilder();
         var myEnumerator = HtTeam.GetEnumerator();
         while (myEnumerator.MoveNext())
            sb.Append( myEnumerator.Key + "," );
         return sb.ToString();
      }

      public void DumpHt()
      {
         var myEnumerator = HtTeam.GetEnumerator();
         var i = 0;
         Utility.Announce( "\t-INDEX-\t-KEY-\t-VALUE-" );
         while ( myEnumerator.MoveNext() )
            Utility.Announce( string.Format( "\t[{0}]:\t{1}\t{2}", i++, myEnumerator.Key, myEnumerator.Value ) );
      }
   }
}