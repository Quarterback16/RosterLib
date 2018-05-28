using System.Collections;
using System.Data;

namespace RosterLib
{
   /// <summary>
   /// Summary description for UnitLister.
   /// </summary>
   public class UnitLister
   {
      private readonly ArrayList mUnitList;
		private string mFormat;

		public string SortOrder { get; set; }

      public UnitLister( string unitCode )
      {
         mUnitList = new ArrayList();
			mFormat = "weekly";
			UnitFactory uf = new UnitFactory();
         DataSet ds = Utility.TflWs.GetTeams( Utility.CurrentSeason() );
         DataTable dt = ds.Tables[ 0 ];
         foreach ( DataRow dr in dt.Rows )
         {
            NflTeam t = new NflTeam( dr["TEAMID"].ToString() );
			   TeamUnit myUnit = uf.CreateUnit( unitCode );
            myUnit.Team = t;
            mUnitList.Add( myUnit );
         }
      }


		public void SetFormat( string theFormat )
		{
			mFormat = theFormat;
		}

		public void Render( string header )
		{
			RenderUnitStatsToHtml html = new RenderUnitStatsToHtml();
			html.RenderData( mUnitList, header, Utility.CurrentNFLWeek() );
		}

   }
}
