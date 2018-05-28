using System;
using System.Data;
using System.Collections;

namespace RosterLib
{
   /// <summary>
   /// Summary description for NFLGambler.
   ///  Has a list of schemes it uses regularly
   /// </summary>
   public class NFLGambler
   {
      private ArrayList m_SchemeList;
      private string focusWeek;
      private string focusSeason = "";
      private double bankRoll;
      private double totalInvestment = 0.0D;
      private double totalReturn = 0.0D;
      private int wins = 0;
      private int losses = 0;
      private int pushes = 0;
      private NFLWeek theWeek;

      /// <summary>
      ///   All the known schemes will be registered once on construction.
      /// </summary>
      public NFLGambler( double bankRollIn )
      {
         bankRoll = bankRollIn;
         m_SchemeList = new ArrayList();
         m_SchemeList.Add( new SuperbowlLetdownScheme( Utility.TflWs ) );
         m_SchemeList.Add( new RevengeScheme( Utility.TflWs  ) );
         m_SchemeList.Add( new HumiliationScheme( Utility.TflWs  ) );
         m_SchemeList.Add( new NibbleLockScheme( Utility.TflWs ) );
         m_SchemeList.Add( new BadNumberScheme( Utility.TflWs  ) );
         m_SchemeList.Add( new HomeDogScheme( Utility.TflWs  ) );
         m_SchemeList.Add( new OverUnderScheme( Utility.TflWs  ) );
         m_SchemeList.Add( new BigNumberScheme() );         
         //TODO:this.m_SchemeList.Add( new SandwichScheme( RosterGrid.tflWs  ) );
      }

      /// <summary>
      ///   Have a look at a week and let us know what the best bets are
      /// </summary>
      /// <param name="week">  The NFL week to look at</param>
      /// <returns> A collection of plays </returns>
      public ArrayList Consider( NFLWeek week )
      {
         Week = week.Week;
         theWeek = week;
         
         ArrayList betList = new ArrayList();

         foreach ( NFLGame g in week.GameList() ) 
            ConsiderGame( g, ref betList );

         foreach ( NFLGame g in week.GameList() ) 
            RemoveGameOnConflict( g, ref betList );
         
         return betList;
      }
      
      private static void RemoveGameOnConflict( NFLGame g, ref ArrayList betList )
      {
         string team1 = g.HomeTeam;
         string team2 = g.AwayTeam;
         
         if ( ( BettingOn( team1, betList ) ) && (BettingOn( team2, betList ) ) )
         {
            RemoveBetsOn( team1, ref betList );
            RemoveBetsOn( team2, ref betList );
         }
      }
      
      private static bool BettingOn( string teamCode, ArrayList betList )
      {
         bool areBetting = false;
         foreach ( NFLBet b in betList )
         {
            if ( b.TeamToBetOn == teamCode )
            {
               areBetting = true;
               break;
            }
         }
         return areBetting;
      }
      
      private static void RemoveBetsOn( string teamCode, ref ArrayList betList )
      {
         foreach ( NFLBet b in betList )
         {
            if ( b.TeamToBetOn == teamCode )
            {
               //RosterLib.Utility.Announce( "Removing a bet on " + teamCode );
               b.IsValid = false;
            }
         }
      }
            
      /// <summary>
      ///   Examines a particular game and fills an ArrayList with possible bets
      /// </summary>
      /// <param name="g">NFL Game</param>
      /// <param name="betList">Collection of NFLBets</param>
      public void ConsiderGame( NFLGame g, ref ArrayList betList )
      {
         if ( theWeek == null )
            theWeek = new NFLWeek( Int32.Parse( g.Season ), Int32.Parse( g.Week ) );

         //RosterLib.Utility.Announce( string.Format( "NFLGambler.ConsiderGame: {0}:{1} {2} @ {3} {4}-{5}", 
         //                                    g.Season, g.Week, g.AwayTeam, g.HomeTeam, g.AwayScore, g.HomeScore ) );

         foreach ( IScheme s in m_SchemeList )
         {
            //RosterLib.Utility.Announce( string.Format( "  trying {0}", s.Name ) );
            var bet = s.IsBettable( g );
            if ( bet != null ) 
            {
               RosterLib.Utility.Announce( string.Format( "   Its a bet {0}", s.Name ) );
               betList.Add( bet );
            }
         }
      }

      public double WagerAmount()
      {
         return ( bankRoll * .01D );
      }

      public string EvaluateBets( ArrayList betList )
      {
         //TODO:  tally wins and losses 
         return RecordOut;
      }


      #region Back Testing

      public void BackTestSchemes()
      {
         //  go through all the schemes you know and tell me which is the best
         //  BackTest each scheme and put results into a DataTable
         DataTable dt = BuildTable();
         foreach ( IScheme s in m_SchemeList )
         {
            RosterLib.Utility.Announce( "BackTesting " + s.Name );
            s.BackTest();
            DataRow dr = dt.NewRow();
            dr[ "Name" ] = s.Name;
            dr[ "Bets" ] = s.M_wins + s.Losses + s.Pushes;
            dr[ "M_wins" ] = s.M_wins;
            dr[ "Losses" ] = s.Losses;
            dr[ "Pushes" ] = s.Pushes;
            dr[ "Clip" ]   = Utility.Clip( s.M_wins, s.Losses, s.Pushes );
            dt.Rows.Add( dr );		
         }
         DumpResults( dt );
      }

      private static DataTable BuildTable()
      {
         var dt = new DataTable();
         var cols = dt.Columns;
         cols.Add( "Name", typeof(String) );
         cols.Add( "Bets", typeof(Int32) );
         cols.Add( "M_wins", typeof(Int32) );
         cols.Add( "Losses", typeof(Int32) );
         cols.Add( "Pushes", typeof(Int32) );
         cols.Add( "Clip", typeof(Decimal) );
         return dt;
      }

      private void DumpResults( DataTable dt )
      {
         var st =
            new SimpleTableReport("Back Testing Results " + Week + ":" + Season) {ColumnHeadings = true};
         st.AddColumn( new ReportColumn( "Scheme", "Name", "{0,-20}"   ) ); 
         st.AddColumn( new ReportColumn( "Wagers",  "Bets", "{0}"       ) ); 
         st.AddColumn( new ReportColumn( "M_wins",  "M_wins", "{0}"       ) ); 
         st.AddColumn( new ReportColumn( "Losses",  "Losses", "{0}"       ) ); 
         st.AddColumn( new ReportColumn( "Pushes",  "Pushes", "{0}"       ) ); 
         st.AddColumn( new ReportColumn( "Clip",  "Clip", "{0:0.000}"       ) ); 
         st.LoadBody( dt );
         st.RenderAsHtml( string.Format( "{0}Back Testing Results {1} {2}.htm",
                                                      Utility.OutputDirectory(), Week, Utility.CurrentSeason()), true);
      }

      #endregion
      
      #region  Output section

      public string RenderBets( ArrayList betList, bool showResults, bool persist )
      {
         RosterLib.Utility.Announce( "Producing Best Bets" );

         string resultsSuffix = String.Empty;
         if (theWeek.HasPassed())
            resultsSuffix = "-results";
               
         DataTable dt = LoadBets( betList );
         SimpleTableReport st = 
            new SimpleTableReport( "Best Bets Week " + Week + ":" + Season, Footer(showResults) );
         st.ColumnHeadings = true;
         st.AddColumn( new ReportColumn( "Team", "TEAM", "{0,-20}"   ) ); 
         st.AddColumn( new ReportColumn( "H/A",  "TURF", "{0}"       ) ); 
         st.AddColumn( new ReportColumn( "Opp",  "OPPONENT", "{0,-20}"   ) ); 
         st.AddColumn( new ReportColumn( "Spread",  "SPREAD", "{0}"       ) ); 
         st.AddColumn( new ReportColumn( "Date (au)",  "DATE", "{0}"   ) );
         st.AddColumn( new ReportColumn( "Time (au)",  "TIME", "{0}"   ) );
         st.AddColumn( new ReportColumn( "Reason(s)",  "REASONS", "{0}"   ) );
         st.AddColumn( new ReportColumn( "Amount",  "AMOUNT", "{0}"   ) );
         st.AddColumn( new ReportColumn( "Result",  "RESULT", "{0}"   ) );
         st.AddColumn( new ReportColumn( "Return",  "WINNINGS", "{0}"   ) );

         dt.DefaultView.Sort = "GAMECODE ASC";
         st.LoadBody( dt );
         st.CarryRow = false;
         st.ShowElapsedTime = false;
         string betHTML = st.RenderAsHtml( 
            string.Format( "{0}bets//BestBets Week {1:0#} {2}{3}.htm",
                                    Utility.OutputDirectory(), Int32.Parse(Week), Utility.CurrentSeason(), resultsSuffix), persist);
      return betHTML;
      }

      private string Footer( bool showResults )
      {
         string s = HtmlLib.TableOpen();

         s += FooterRow( "Investment", "${0:###0.00}", TotalInvestment );            

         if ( showResults )
         {
            s += FooterRow( "Return",     "${0:###0.00}", TotalReturn );
            s += FooterRow( "ROI",        "{0:###0.0}%", ((TotalReturn/TotalInvestment) - 1.0D) * 100.0D  );
            s += FooterRow( "Total P/L" , "${0:###0.00}", (TotalReturn - TotalInvestment) );
            s += FooterRow( "Record " ,   "{0}",           RecordOut );
         }

         return s + HtmlLib.TableClose();
      }

      private static string FooterRow( string label, string fmt, double amt )
      {
         string row = 
            HtmlLib.TableRowOpen() + HtmlLib.TableData( label ) +
            HtmlLib.TableDataAttr( string.Format( fmt, amt ), "ALIGN='RIGHT'" ) 
            + HtmlLib.TableRowClose();
         return row;
      }

      private static string FooterRow(string label, string fmt, string msg)
      {
         string row = 
            HtmlLib.TableRowOpen() + HtmlLib.TableData( label ) +
            HtmlLib.TableData( string.Format( fmt, msg ) ) 
            + HtmlLib.TableRowClose();
         return row;
      }
      
      private DataTable LoadBets( ArrayList betList )
      {
         DataTable dt = new DataTable();
         DataColumnCollection cols = dt.Columns;
         cols.Add( "TEAM",      typeof( String) );
         cols.Add( "GAMECODE",  typeof( String) );
         cols.Add( "TURF",      typeof( String) );
         cols.Add( "SPREAD",    typeof( String) );
         cols.Add( "OPPONENT",  typeof( String) );
         cols.Add( "DATE",      typeof( String) );
         cols.Add( "TIME",      typeof( String) );
         cols.Add( "REASONS",   typeof( String) );
         cols.Add( "AMOUNT",    typeof( String) );
         cols.Add( "RESULT",    typeof( String) );
         cols.Add( "WINNINGS",  typeof( String) );

         if ( betList != null )
         {
            foreach ( NFLBet b in betList )
            {
               if ( b.IsValid )
               {
                  double amount =  ( WagerAmount() * (int) b.ConfidenceLevel );
                  DataRow dr = dt.NewRow();
                  dr[ "TEAM"     ] = HtmlLib.Bold( b.TeamToBetOn);
                  dr[ "GAMECODE" ] = b.Game.GameCode;
                  dr[ "TURF"     ] = ( b.IsHome() ) ? "v" : "@";
                  dr[ "SPREAD"   ] = HtmlLib.Bold( SpreadOut( b.Handicap() ) );
                  dr[ "OPPONENT" ] = b.Opponent();
                  dr[ "DATE"     ] = b.Game.GameDate.AddDays(1D).ToShortDateString();
                  if ( b.Game.Played() )
                     dr[ "TIME"  ] = b.Game.ScoreOut3();
                  else
                     dr[ "TIME"  ] = MilitaryTime( theWeek.AusHour( b.Game.Hour, b.Game.GameDate.Month ).ToString() );
                  dr[ "REASONS"  ] = b.ReasonList();
                  dr[ "AMOUNT"   ] = string.Format( "{0:#0.00}", amount );
                  dr[ "RESULT"   ] = Embelish( b.Result() );
                  switch ( b.Result() )
                  {
                     case "Win":
                        Wins++;
                        break;
                     case "Loss":
                        Losses++;
                        break;
                     case "Push":
                        Losses++;
                        break;
                     default:
                        //  not played yet
                        break;
                  }
                  if ( b.Resolved() )
                     dr[ "WINNINGS" ] = string.Format( "{0:#0.00}", b.Winnings( amount ) );
                  dt.Rows.Add( dr );
                  TotalInvestment += amount;
                  TotalReturn += b.Winnings( amount );
               }
            }
         }

         return dt;
      }

      private static string Embelish(string result)
      {
         if ( result == "Win" )
            result = HtmlLib.Bold( result );
         return result; 
      }

      private static string MilitaryTime(string hour)
      {
         if ( hour.Length == 1 ) hour = "0" + hour;
         return hour + ":00";
      }

      private static string SpreadOut(decimal spread)
      {
         string s = spread.ToString();
         if ( s == "0" )
            s = "OTB";
         else
         {
            if ( s == ".5" )
               s = "Pickem";
            else
            {
               if ( Decimal.Compare( spread, 0M ) > 0 )
                  s = string.Format( "PLUS {0}", spread );
               else
                  s = string.Format( "MINUS {0}", Math.Abs( spread ) );
            }
         }
         return s;
      }
      
      #endregion

      #region  Accessors

      public string Week
      {
         get { return focusWeek; }
         set { focusWeek = value; }
      }

      public string Season
      {
         get { return focusSeason; }
         set { focusSeason = value; }
      }

      public double TotalInvestment
      {
         get { return totalInvestment; }
         set { totalInvestment = value; }
      }

      public double TotalReturn
      {
         get { return totalReturn; }
         set { totalReturn = value; }
      }

      public int Losses
      {
         get { return losses; }
         set { losses = value; }
      }

      public int Pushes
      {
         get { return pushes; }
         set { pushes = value; }
      }

      public string RecordOut
      {
         get { return string.Format( "({0}-{1}-{2})", Wins, Losses, Pushes ); }
      }

      public int Wins
      {
         get { return wins; }
         set { wins = value; }
      }

      #endregion
   }

   public enum Confidence
   {
      None = 0,
      Good = 1,
      VeryGood = 2,
      High = 3
   }

}
