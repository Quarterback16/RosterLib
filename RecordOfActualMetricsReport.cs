using RosterLib.Interfaces;
using System;
using System.Text;

namespace RosterLib
{
	public class RecordOfActualMetricsReport : RosterGridReport
	{
		public string Week { get; set; }

		public DbfPlayerGameMetricsDao Dao { get; set; }

		public RecordOfActualMetricsReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Recording Actual Player Metrics Log";
			Season = timekeeper.CurrentSeason( DateTime.Now );
			Week = string.Format( "{0:00}", timekeeper.CurrentWeek( DateTime.Now ) );

			Dao = new DbfPlayerGameMetricsDao();
		}

		public override void RenderAsHtml()
		{
			var PreReport = new SimplePreReport
			{
				ReportType = "Actual Metrics",
				Folder = "GameSummaries",
				Season = Season,
				InstanceName = Week,
				Body = GenerateBody()
			};
			PreReport.RenderHtml();
			FileOut = PreReport.FileOut;
		}

		private string GenerateBody()
		{
			var bodyOut = new StringBuilder();
			bodyOut.AppendLine( string.Format( "Recording actual metrics for Week {0}:{1}", Season, Week ) );
			bodyOut.AppendLine();

			// Foreach game in the week
			var theWeek = new NFLWeek( Season, Week );
			theWeek.LoadGameList();
			foreach ( NFLGame game in theWeek.GameList() )
			{
				var homeList = game.LoadLineupPlayers( game.HomeTeam );
				TallyMetrics( bodyOut, game, homeList );

				var awayList = game.LoadLineupPlayers( game.AwayTeam );
				TallyMetrics( bodyOut, game, awayList );
			}

			return bodyOut.ToString();
		}

		private void TallyMetrics( StringBuilder bodyOut, NFLGame game, System.Collections.Generic.List<NFLPlayer> list )
		{
			TallyPlayers( list );
			foreach ( var player in list )
			{
				//  record metrics
				bodyOut.AppendLine( RecordMetrics( player, game ) );
			}
		}

		public string RecordMetrics( NFLPlayer player, NFLGame game )
		{
			var gameKey = game.GameKey();
			var pgm = Dao.Get( player.PlayerCode, gameKey );

			if ( pgm.GameKey != null )
			{
				pgm.TDp = player.CurrentGameMetrics.TDp;
				pgm.TDr = player.CurrentGameMetrics.TDr;
				pgm.TDc = player.CurrentGameMetrics.TDc;
				pgm.YDp = player.CurrentGameMetrics.YDp;
				pgm.YDr = player.CurrentGameMetrics.YDr;
				pgm.YDc = player.CurrentGameMetrics.YDc;
				pgm.FG = player.CurrentGameMetrics.FG;
				pgm.Pat = player.CurrentGameMetrics.Pat;

				Dao.Save( pgm );
				return string.Format( "Game:{0} Player:{1} metrics:{2}", game.GameName(), player.PlayerName, pgm );
			}
			else
				return string.Format( "Failed to find pgm for {0}:{1}", game.GameName(), player.PlayerName );
		}

		private void TallyPlayers( System.Collections.Generic.List<NFLPlayer> playerList )
		{
			foreach ( var player in playerList )
			{
				player.TallyScores( Season, Int32.Parse( Week ) );
				player.TallyStats( Season, Int32.Parse( Week ) );
				//    write out the metrics
			}
		}

		public override string OutputFilename()
		{
			var fileName =
			   string.Format( "{0}{2}\\logs\\ActualMetris-W{1}.log", Utility.OutputDirectory(), Week, Season );
			return fileName;
		}
	}
}