using System;

namespace RosterLib
{
   /// <summary>
   /// PlayerStats is really just a struct at the moment.
   /// </summary>
   public class PlayerStats
   {
      // Scores
      public int Tdp;
      public decimal Tdr;
      public int Tdc;
      public int Fg;
      public int Pat;
      public int PatPass;
      public int PatCatch;
      public int PatRun;
      public int KickRet;
      public int FumRet;
      public int IntRet;
      public int Safety;
      //  Stats
      public int Touches;
      public decimal TouchLoad;
      public int YDr;
      public int Rushes;
      public int Completions;
      public int PassAtts;
      public int YDp;
      public int PassInt;
      public int Catches;
      public int YDc;
      public int Ints;
      public Decimal Sacks;

      public string FourStats( int com, int att, int ydp, int pInt, int tdpIn )
      {
         var s = String.Empty;
         if ( ( com + att + ydp + pInt ) > 0 )
            s = HtmlLib.HTMLPadL( String.Format( "{0}", com ), 2 ) + "-" +
                HtmlLib.HTMLPadL( String.Format( "{0}", att ), 2 ) + "-" +
                HtmlLib.HTMLPadL( String.Format( "{0}", ydp ), 3 ) + "-" +
                HtmlLib.HTMLPadL( String.Format( "{0}", pInt ), 1 ) +
                TdsOut( tdpIn );
         return s;
      }

      public void Zeroise()
      {
         Tdp = 0;
         Tdr = 0;
         Tdc = 0;
         Fg = 0;
         Pat = 0;
         PatPass = 0;
         PatCatch = 0;
         PatRun = 0;
         KickRet = 0;
         FumRet = 0;
         IntRet = 0;
         Safety = 0;
         //  Stats
         YDr = 0;
         Rushes = 0;
         Completions = 0;
         PassAtts = 0;
         YDp = 0;
         PassInt = 0;
         Catches = 0;
         YDc = 0;
         Ints = 0;
         Sacks = 0.0M;
      }

      public string TwoStats( int rushesIn, int ydrIn, decimal tdrIn, bool addAvg )
      {
         string s = String.Empty;
         if ( ( rushesIn + ydrIn + tdrIn > 0 ) )
         {
            s = HtmlLib.HTMLPadL( String.Format( "{0}", rushesIn ), 4 ) + "-" +
                HtmlLib.HTMLPadL( String.Format( "{0}", ydrIn ), 2 ) + " " + TdsOut( tdrIn );
            if ( addAvg ) s += String.Format( " {0:0.0}", Average( ydrIn, rushesIn ) );
         }
         return s;
      }

      private static decimal Average( decimal quotient, int divisor )
      {
         //  need to do decimal other wise INT() will occur
         if ( divisor == 0 ) return 0.0M;
         return ( quotient/Decimal.Parse( divisor.ToString() ) );
      }

      public string TwoStats( decimal sacksIn, int intsIn, int td )
      {
         string s = String.Empty;
         if ( ( sacksIn + intsIn + td > 0 ) )
            s = HtmlLib.HTMLPadL( String.Format( "{0:0.#}", sacksIn ), 4 ) + "-" +
                HtmlLib.HTMLPadL( String.Format( "{0}", intsIn ), 2 ) + " " + TdsOut( td );
         return s;
      }

      public string TdsOut( decimal nTd )
      {
         string s = String.Empty;
         if ( nTd > 0 )
            s = string.Format( " ({0:#})", nTd );
         return s;
      }

      public string TdsOut( int nTd )
      {
         string s = String.Empty;
         if ( nTd > 0 )
            s = string.Format( " ({0:#})", nTd );
         return s;
      }

      #region Stat formatter according to Category

      public string Stat1( string playerCat, bool addAvg )
      {
         string s = String.Empty;

         switch ( playerCat )
         {
            case RosterLib.Constants.K_QUARTERBACK_CAT:
               s = FourStats( Completions, PassAtts, YDp, PassInt, Tdp );
               break;
            case RosterLib.Constants.K_RUNNINGBACK_CAT:
               s = TwoStats( Rushes, YDr, Tdr, addAvg );
               break;
            case RosterLib.Constants.K_RECEIVER_CAT:
               s = TwoStats( Catches, YDc, Tdc );
               break;
            case RosterLib.Constants.K_KICKER_CAT:
               s = TwoStats( Fg, Pat, TotOtherTDs() );
               break;
            case RosterLib.Constants.K_LINEMAN_CAT:
               s = TwoStats( Sacks, Ints, TotOtherTDs() );
               break;
            case RosterLib.Constants.K_DEFENSIVEBACK_CAT:
               s = TwoStats( Sacks, Ints, TotOtherTDs() );
               break;
         }
         return s;
      }

      public string Stat2( string playerCat )
      {
         string s = String.Empty;

         switch ( playerCat )
         {
            case "1":
               s = TwoStats( Rushes, YDr, Tdr, false );
               break;
            case "2":
               s = TwoStats( Catches, YDc, Tdc );
               break;
            case "3":
               s = TwoStats( Rushes, YDr, Tdr, false );
               break;
            case "4":
               s = TwoStats( Fg*3, Pat, ( Fg*3 ) + Pat );
               break;
         }
         return s;
      }

      private int TotOtherTDs()
      {
         return Tdc + KickRet + FumRet + IntRet;
      }

      #endregion
   }
}