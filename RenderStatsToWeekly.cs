using RosterLib.Interfaces;
using System;
using System.Collections;
using System.Data;

namespace RosterLib
{
   public class RenderStatsToWeekly : IRenderWeekly
   {
      private readonly IRatePlayers _scorer;
      private readonly IWeekMaster WeekMaster;
      private readonly IKeepTheTime TimeKeeper;

      private const string FieldFormat = "Wk{0:0#}";

      public bool CurrentSeasonOnly { get; set; }

      public string FileOut { get; set; }

      public int WeeksToGoBack { get; set; }

      public RenderStatsToWeekly(
         IRatePlayers scorerIn, 
         IWeekMaster weekMasterIn,
         IKeepTheTime timekeeper
         )
      {
         _scorer = scorerIn;
         WeekMaster = weekMasterIn;
         TimeKeeper = timekeeper;
      }

      public RenderStatsToWeekly(IRatePlayers scorerIn )
      {
         _scorer = scorerIn;
      }

      public bool FullStart { get; set; }

      public string RenderData(ArrayList playerList, string sHead, NFLWeek week)
      {
         //  Output the list
         var r = new SimpleTableReport {ReportHeader = sHead, ReportFooter = "", DoRowNumbers = true};

         var ds = LoadData(playerList, week);

         r.AddColumn(new ReportColumn("Name", "NAME", "{0,-15}"));
         r.AddColumn(new ReportColumn("Team", "CURRTEAM", "{0,2}"));
         r.AddColumn(new ReportColumn("Role", "ROLE", "{0,1}"));
         r.AddColumn(new ReportColumn("Owner", "FT", "{0,2}"));
         r.AddColumn(new ReportColumn("Total", "tot", "{0,5}"));

         var startAt = FullStart ? Constants.K_WEEKS_IN_A_SEASON : Constants.K_WEEKS_IN_REGULAR_SEASON;

         for (var w = startAt; w > 0; w--)
         {
            var header = string.Format("Week {0}", w);
            var fieldName = string.Format(FieldFormat, w);

            if ( IsGridStatsQBReport( sHead ) )
                r.AddColumn( new ReportColumn(header, fieldName, "{0,5}", QbBgPicker) );
            if ( IsYahooQbReport( sHead) )
                r.AddColumn(new ReportColumn(header, fieldName, "{0,5}", EspnQbBgPicker));
            if (  IsYahooRbReport( sHead ) )
               r.AddColumn( new ReportColumn(header, fieldName, "{0,5}", EspnRbBgPicker));
            if ( IsYahooWrReport( sHead ) ) 
               r.AddColumn(new ReportColumn(header, fieldName, "{0,5}", EspnWrBgPicker));
            if ( IsYahooTeReport( sHead ) )
               r.AddColumn(new ReportColumn(header, fieldName, "{0,5}", EspnTeBgPicker));
            if ( IsYahooPkReport( sHead ) )
               r.AddColumn( new ReportColumn( header, fieldName, "{0,5}", EspnPkBgPicker ) );

         }

         var dt = ds.Tables[0];
         dt.DefaultView.Sort = "tot DESC";
         r.LoadBody(dt);
         FileOut =  sHead;
         r.RenderAsHtml( FileOut, true);
         return FileOut;
      }

      private static bool IsYahooPkReport(string sHead )
      {
         return ( IsYahooReport( sHead ) ) && ( ( sHead.IndexOf( "PK" ) > -1 ) );
      }

      private static bool IsYahooTeReport(string sHead )
      {
         return ( IsYahooReport( sHead ) ) && ( ( sHead.IndexOf( "TE" ) > -1 ) );
      }

      private static bool IsYahooWrReport(string sHead)
      {
         return ( IsYahooReport( sHead ) ) && ( ( sHead.IndexOf( "WR" ) > -1 ) );
      }

      private static bool IsYahooRbReport(string sHead)
      {
         return ( IsYahooReport( sHead ) ) && ( ( sHead.IndexOf( "RB" ) > -1 ) );
      }

      private static bool IsYahooQbReport(string sHead)
      {
         return ( IsYahooReport( sHead ) ) && ( ( sHead.IndexOf( "QB" ) > -1 ) );
      }

      private static bool IsGridStatsQBReport( string sHead )
      {
         return ( sHead.IndexOf( "GS" ) > -1 ) && ( ( sHead.IndexOf( "QB" ) > -1 ) );
      }

      private static bool IsYahooReport( string sHead )
      {
         return ( ( sHead.IndexOf( "ESPN" ) > -1 ) || ( sHead.IndexOf( "Yahoo" ) > -1 ) );
      }

      public DataSet LoadData(ArrayList plyrList, NFLWeek startWeek)
      {
         var ds = new DataSet();
         var dt = new DataTable();
         var cols = dt.Columns;
         cols.Add("Name", typeof (String));
         cols.Add("Currteam", typeof (String));
         cols.Add("ROLE", typeof (String));
         cols.Add("FT", typeof (String));
         cols.Add("tot", typeof (Int32));

         var currentWeek = new NFLWeek(
            Int32.Parse(TimeKeeper.Season), 
            Int32.Parse(TimeKeeper.Week), 
            loadGames:false);

         for (var w = Constants.K_WEEKS_IN_A_SEASON; w > 0; w--)
         {
            var fieldName = string.Format(FieldFormat, currentWeek.WeekNo);
            Utility.Announce( string.Format( "Adding field: {0}", fieldName ) );
            cols.Add(fieldName, typeof (String));
            currentWeek = currentWeek.PreviousWeek(currentWeek,loadgames:false, regularSeasonGamesOnly: false);
         }
         foreach (NFLPlayer p in plyrList)
         {
            decimal nTot = 0;
            var dr = dt.NewRow();
            dr["Name"] = p.PlayerName;
            dr["CurrTeam"] = p.TeamCode;
            dr["ROLE"] = p.PlayerRole;
            dr["FT"] = p.Owner;

            var weekCounter = Constants.K_WEEKS_IN_A_SEASON;
            var weeksDone = 0;
            var scoreWeek = WeekMaster != null ? WeekMaster.GetWeek(startWeek.Season, 17) : new NFLWeek( startWeek.Season, Constants.K_WEEKS_IN_REGULAR_SEASON );
            do
            {
               var game = scoreWeek.GameFor(p.TeamCode);
               var cOp = game == null ? string.Empty : game.OpponentOut(p.TeamCode);
               var nScore = 0M;
               if ( game != null )
               {
                  if ( game.Played() )
                  {
                     nScore = _scorer.RatePlayer( p, scoreWeek );
                     nTot += nScore;
                     weeksDone++;
                  }
               }
	            if (game != null && game.Played())
	            {
		            dr[ string.Format(FieldFormat, scoreWeek.WeekNo) ] 
                     = string.Format("{0:0}:{1}", nScore, cOp) + "<br>"
		                 + OpponentDefence(p, game);
	            }
	            else
	            {
						dr[ string.Format( FieldFormat, scoreWeek.WeekNo ) ] 
                     = string.Format( "{0:#}:{1}", nScore, cOp ) + "<br>"
					        + OpponentDefence( p, game );		            
	            }

	            scoreWeek = WeekMaster != null ? WeekMaster.PreviousWeek(scoreWeek) : scoreWeek.PreviousWeek(scoreWeek,false,false);

               if (CurrentSeasonOnly)
                  if (scoreWeek.Season != Utility.CurrentSeason())
                     weekCounter = 0;  //  breaks you out of the loop

               if ( weeksDone >= WeeksToGoBack )
                  break;

               weekCounter--;
            } while (weekCounter > 0);

            dr["tot"] = nTot;
            dt.Rows.Add(dr);
         }
         ds.Tables.Add(dt);
         return ds;
      }

      private static string OpponentDefence( NFLPlayer p, NFLGame game )
      {
         if (game == null) return "BYE";
         var opponent = game.OpponentTeam( p.TeamCode );
         return opponent == null ? "BYE" : opponent.DefensiveRating( p.PlayerCat );
      }

      //  logic for setting BG colour

      private static string QbBgPicker(int theValue)
      {
         string sColour;

         switch (theValue)
         {
            case 0:
               sColour = "RED";
               break;
            case 1:
               sColour = "RED";
               break;
            case 2:
               sColour = "GREEN";
               break;

            default:
               sColour = "YELLOW";
               break;
         }
         return sColour;
      }

      private static string OtherBgPicker(int theValue)
      {
         string sColour;

         switch (theValue)
         {
            case 0:
               sColour = "RED";
               break;

            case 1:
               sColour = "GREEN";
               break;

            default:
               sColour = "YELLOW";
               break;
         }
         return sColour;
      }

      #region  Espn colours

      private static string EspnQbBgPicker(int theValue)
      {
         string sColour = "RED";

         if (theValue > 19)
            sColour = "YELLOW";
         else if (theValue > 9)
            sColour = "GREEN";

         return sColour;
      }

      private static string EspnRbBgPicker(int theValue)
      {
         var sColour = "RED";

         if (theValue > 19)
            sColour = "YELLOW";
         else if (theValue > 9)
            sColour = "GREEN";

         return sColour;
      }

      private static string EspnWrBgPicker(int theValue)
      {
         string sColour = "RED";

         if (theValue > 10)
            sColour = "YELLOW";
         else if (theValue > 5)
            sColour = "GREEN";

         return sColour;
      }

      private static string EspnTeBgPicker(int theValue)
      {
         var sColour = "RED";

         if (theValue > 8)
            sColour = "YELLOW";
         else if (theValue > 3)
            sColour = "GREEN";

         return sColour;
      }

      private static string EspnPkBgPicker(int theValue)
      {
         string sColour = "RED";

         if (theValue > 9)
            sColour = "YELLOW";
         else if (theValue > 3)
            sColour = "GREEN";

         return sColour;
      }

      #endregion
   }
}