using RosterLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RosterLib.RosterGridReports
{
    public class LineupSummary : RosterGridReport
    {
        public string Week { get; private set; }
        public SimplePreReport Report { get; private set; }

        public List<Lineup> Lineups { get; set; }

        public LineupSummary( IKeepTheTime timekeeper, int week ) : base( timekeeper )
        {
            Name = "Lineup Summary";
            Season = timekeeper.Season;
            Week = $"{week:0#}";
            Report = new SimplePreReport
            {
                ReportType = "Lineup Summary",
                Folder = "Starters",
                Season = Season,
                InstanceName = $"Lineup-Summary-Week-{Week:0#}"
            };
        }

        public override void RenderAsHtml()
        {
            Report.Body = GenerateBody();
            Report.RenderHtml();
            FileOut = Report.FileOut;
        }

        private string GenerateBody()
        {
            var simple = new StringBuilder();
            simple.Append( LoadLineups() );
            return simple.ToString();
        }

        private string LoadLineups()
        {
            var output = new StringBuilder();
            if ( Lineups == null ) Lineups = new List<Lineup>();
            Lineups.Clear();
            var nByeTeams = 0;
            var nGames = 0;
            var teamDs = Utility.TflWs.TeamsDs( Season );
            DataTable dt = teamDs.Tables[ 0 ];
            foreach ( DataRow dr in dt.Rows )
            {
                var teamCode = dr[ "TEAMID" ].ToString();
                var lu = new Lineup( teamCode, Season, Week );
                lu.UpdateJerseysAndDefensiveRoles(teamCode);
                Lineups.Add( lu );
                if ( lu.TeamCode.Equals( "SF" ) )
                    lu.DumpLineup();
                if ( lu.PlayerCount() == 0 )
                    nByeTeams++;
                else
                {
                    nGames++;
                    output.AppendLine( $@"{
                        lu.TeamCode
                        } has {
                        lu.PlayerCount(),3} Starters:{
                        lu.Starters,3
                        } off {
                        lu.OffStarters,3
                        } def {
                        lu.DefStarters,3
                        } Backups: {lu.Backups}" );
                }
            }
            Console.WriteLine($"{Lineups.Count} lineups loaded");
            Console.WriteLine( $"{nByeTeams} teams on Bye" );
            Console.WriteLine( $"{nGames/2} games" );

            return output.ToString();
        }
    }
}
