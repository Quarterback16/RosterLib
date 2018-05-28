using System;
using System.Data;

namespace RosterLib
{
   public class GordanRanks
   {
      private NflSeason s;
      private DataTable dt;

      public GordanRanks( string season )
      {
         //  set up data
         s = Masters.Sm.GetSeason( season );

         DefineData();
         LoadData();
         Render();
      }

      private void Render()
      {
         SimpleTableReport str = new SimpleTableReport( "Gordan Rankings : Week " + Utility.CurrentWeek() );
         str.ColumnHeadings = true;
         str.DoRowNumbers = true;
         str.AddColumn( new ReportColumn( "Team", "TEAM", "{0,-20}" ) );
         str.AddColumn( new ReportColumn( "Rating", "RANK", "{0}" ) );
         for ( int i = Int32.Parse( Utility.CurrentWeek() ); i > -1; i-- )
         {
            string colName = string.Format( "Week{0:0#}", i );
            str.AddColumn( new ReportColumn( colName, colName, "{0}" ) );
         }

         str.LoadBody( dt );
         str.RenderAsHtml(
            string.Format("{0}Gordan{1}{2}.htm", Utility.OutputDirectory(), s.Year, Utility.CurrentWeek()),
            true );
      }

      private void LoadData()
      {
         NflTeam t;

         foreach ( string key in s.TeamKeyList )
         {
            RosterLib.Utility.Announce( string.Format( "GordanRanks.LoadData \t[{0}]", key ) );
            t = Masters.Tm.GetTeam( key );
            DataRow dr = dt.NewRow();
            dr[ "TEAM" ] = t.Name;
            for ( int i = Int32.Parse( Utility.CurrentWeek() ); i > -1; i-- )
            {
               dr[ string.Format( "WEEK{0:0#}", i ) ] = t.LetterRating[ i ];
               if ( Int32.Parse( Utility.CurrentWeek() ).Equals( i ) )
                  dr[ "RANK" ] = t.LetterRating[ i ];
            }
            dt.Rows.Add( dr );
         }
      }

      private void DefineData()
      {
         dt = new DataTable();
         DataColumnCollection cols = dt.Columns;
         cols.Add( "TEAM", typeof ( String ) );
         cols.Add( "RANK", typeof ( String ) );

         for ( int i = 0; i < RosterLib.Constants.K_WEEKS_IN_REGULAR_SEASON; i++ )
            cols.Add( string.Format( "WEEK{0:0#}", i ), typeof ( String ) );
      }
   }
}