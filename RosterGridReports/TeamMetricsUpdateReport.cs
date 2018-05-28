using System;
using System.Collections.Generic;
using System.Text;
using RosterLib.Interfaces;

namespace RosterLib.RosterGridReports
{
   public class TeamMetricsUpdateReport : RosterGridReport
   {
      public NFLWeek Week { get; set; }

      public TeamMetricsUpdateReport( IKeepTheTime timekeeper ) : base( timekeeper )
	  {
         Name = "Team Metrics Update Report";
         Season = timekeeper.CurrentSeason();
         Week = new NFLWeek( Season, timekeeper.PreviousWeek() );
      }

      public override string OutputFilename()
      {
         return string.Format( "{0}{1}/{2}.htm", Utility.OutputDirectory(), Season, Name );
      }

      public override void RenderAsHtml()
      {
         var body = new StringBuilder();
         var gameList = Week.GameList();
         foreach ( NFLGame g in gameList )
         {
            body.AppendLine( g.GameName() + "  " + g.ScoreOut() );
            g.RefreshTotals();
         }
         OutputReport( body.ToString() );
         Finish();
      }

      private void OutputReport( string body )
      {
         var PreReport = new SimplePreReport
         {
            ReportType = Name,
            Folder = "Metrics",
            Season = Season,
            InstanceName = string.Format( "TeamMetricsUpdates-{0:0#}", Week ),
            Body = body
         };
         PreReport.RenderHtml();
         FileOut = PreReport.FileOut;
      }
   }
}
