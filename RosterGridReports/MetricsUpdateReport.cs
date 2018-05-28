using RosterLib.Interfaces;
using System.Collections.Generic;
using System.Text;
using System;

namespace RosterLib.RosterGridReports
{
	public class MetricsUpdateReport : RosterGridReport
	{
		public NFLWeek Week { get; set; }

		public YahooScorer Scorer { get; set; }

		public IPlayerGameMetricsDao Dao { get; set; }

		public MetricsUpdateReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Metrics Update Report";
			Season = timekeeper.CurrentSeason();
			Week = new NFLWeek( Season, timekeeper.PreviousWeek() );
			Scorer = new YahooScorer( Week );
			Dao = new DbfPlayerGameMetricsDao();
		}

		public override string OutputFilename()
		{
			return $"{Utility.OutputDirectory()}{Season}/{Name}.htm";
		}

		public override void RenderAsHtml()
		{
			var body = new StringBuilder();
			var gameList = Week.GameList();
			foreach ( NFLGame g in gameList )
			{
				body.AppendLine( g.GameName() + "  " + g.ScoreOut() );
				g.LoadAllFantasyAwayPlayers( null, string.Empty );
				g.LoadAllFantasyHomePlayers( null, string.Empty );
				ProcessPlayerList( g.HomePlayers, body );
				ProcessPlayerList( g.AwayPlayers, body );
#if DEBUG2
		      break;  // to speed things up
#endif
			}
			//For each game in the last week
			//  for each player
			//     get actuals
			//     save them
			OutputReport( body.ToString() );
			Finish();
		}

		public void ProcessPlayerList( IEnumerable<NFLPlayer> plist, StringBuilder body )
		{
			foreach ( var p in plist )
			{
				ProcessPlayer( body, p );
			}
		}

		public void ProcessPlayer( StringBuilder body, NFLPlayer p )
		{
			var pts = Scorer.RatePlayer( p, Week, takeCache: false );
			// By product is that the players Game Metrics are updated
			p.Points = pts;

#if DEBUG
			if ( p.PlayerCode.Equals( "RIVEPH01" ) )
				p.DumpMetrics();
#endif
			var line = $"   {p.PlayerNameShort,25} : {pts,2} > {p.ActualStats(),8}";
			if ( pts > 0 )
			{
				Announce( line );
				body.AppendLine( line );
			}
			p.UpdateActuals( Dao );
		}

		private void Announce( string line )
		{
			Logger.Trace( line );
			Console.WriteLine(line);
		}

		private void OutputReport( string body )
		{
			var PreReport = new SimplePreReport
			{
				ReportType = Name,
				Folder = "Metrics",
				Season = Season,
				InstanceName = $"MetricsUpdates-{Week:0#}",
				Body = body
			};
			PreReport.RenderHtml();
			FileOut = PreReport.FileOut;
		}
	}
}