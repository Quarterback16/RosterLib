using System;
using System.Collections.Generic;
using System.Data;


namespace RosterLib
{
   public class OffLineReport
   {
      private GenericList< NflTeam > m_TeamList;
      private string m_Season;

      public OffLineReport( string season )
      {
         //  Part 1 - Get the Teams for the season
         m_Season = season;
         DataSet ds = Utility.TflWs.TeamsDs( season );
         DataTable dt = ds.Tables[ "Team" ];
         m_TeamList = new GenericList< NflTeam >();

         //  Part 2 - Iterate through the teams
         foreach ( DataRow dr in dt.Rows )
            m_TeamList.Add( new NflTeam( dr[ "TEAMID" ].ToString() ) );
      }

      public void RenderAsHTML()
      {
         SimpleTableReport str = new SimpleTableReport( 
            string.Format( "Offensive Lines : Season {0} Week {1}", m_Season, Utility.CurrentWeek() ) );
         str.ColumnHeadings = true;
         str.DoRowNumbers = false;
         
         str.AddColumn(new ReportColumn("Div", "DIV", "{0,-6}"));
         str.AddColumn(new ReportColumn("Team", "TEAM", "{0,-20}"));
         str.AddColumn(new ReportColumn("LT", "LT", "{0,-20}"));
         str.AddColumn(new ReportColumn("LG", "LG", "{0,-20}"));
         str.AddColumn(new ReportColumn("C", "C", "{0,-20}"));
         str.AddColumn(new ReportColumn("RG", "RG", "{0,-20}"));
         str.AddColumn(new ReportColumn("RT", "RT", "{0,-20}"));
         str.AddColumn(new ReportColumn("Rating", "Rating", "{0}"));
         str.AddColumn(new ReportColumn("SacksAlld", "SAKALLD", "{0}"));
         str.LoadBody(BuildTable());
         str.RenderAsHtml(Utility.OutputDirectory() + "ol" +
                           m_Season + Utility.CurrentWeek() + ".htm", true );
      }

      private DataTable BuildTable()
      {
         DataTable dt = new DataTable();
         DataColumnCollection cols = dt.Columns;
         cols.Add( "SORTKEY", typeof ( String ) );
         cols.Add( "DIV", typeof ( String ) );
         cols.Add( "TEAM", typeof ( String ) );
         cols.Add( "LT", typeof ( String ) );
         cols.Add( "LG", typeof ( String ) );
         cols.Add( "C", typeof ( String ) );
         cols.Add( "RG", typeof ( String ) );
         cols.Add( "RT", typeof ( String ) );
         cols.Add( "Rating", typeof ( String ) );
         cols.Add( "SAKALLD", typeof( Decimal ));

         foreach ( NflTeam t in m_TeamList )
         {
            t.PpUnit( m_Season );
            DataRow dr = dt.NewRow();
            dr[ "SORTKEY" ] = t.Division();
            dr[ "DIV" ] = NFLDivision.ShortNameOut( t.Division() );
            dr[ "TEAM" ] = t.TeamCode;
            dr[ "LT" ] = PlayerOut( t.GetPlayerAt( "LT", 1, false, false ) );
            dr[ "LG" ] = PlayerOut(t.GetPlayerAt( "LG", 1, false, false ) );
            dr[ "C" ] = PlayerOut(t.GetPlayerAt( "C", 1, false, false ) );
            dr[ "RG" ] = PlayerOut(t.GetPlayerAt( "RG", 1, false, false ) );
            dr[ "RT" ] = PlayerOut(t.GetPlayerAt( "RT", 1, false, false ) );
            dr[ "Rating" ] = t.PpRating();
            dr["SAKALLD"] = t.TotSacksAllowed;
            dt.Rows.Add(dr);
         }
         dt.DefaultView.Sort = "SORTKEY";
         dt.DefaultView.Sort = "Rating";
         return dt;
      }

      private static string PlayerOut( NFLPlayer p )
      {
         string name;
         name = p == null ? "" : p.PlayerName;
         if ( string.IsNullOrEmpty( name ))
            return "<unknown>";
         else
            return name;
      }
      
   }


   public class GenericList< T >
   {
      // The nested class is also generic on T
      private class Node
      {
         // T used in non-generic constructor
         public Node( T t )
         {
            next = null;
            data = t;
         }

         private Node next;

         public Node Next
         {
            get { return next; }
            set { next = value; }
         }

         // T as private member data type
         private T data;

         // T as return type of property
         public T Data
         {
            get { return data; }
            set { data = value; }
         }
      }

      private Node head;

      // constructor
      public GenericList()
      {
         head = null;
      }

      // T as method parameter type:
      public void Add( T t )
      {
         Node n = new Node( t );
         n.Next = head;
         head = n;
      }

      public IEnumerator< T > GetEnumerator()
      {
         Node current = head;

         while ( current != null )
         {
            yield return current.Data;
            current = current.Next;
         }
      }
   }
}