using RosterLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace RosterLib
{
	public class DepthChartReport : RosterGridReport
	{
		public string TeamCode { get; set; }

		public string LeagueInFocus { get; set; }

		public NflTeam NflTeam { get; set; }

		public int PlayerCount { get; set; }

		public List<String> Errors { get; set; }

		public DepthChartReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Depth Charts";
			TeamCode = "SF";
			Errors = new List<String>();
			Season = timekeeper.CurrentSeason();
		}

		public DepthChartReport( IKeepTheTime timekeeper, string teamCode ) : base( timekeeper )
		{
			Season = timekeeper.CurrentSeason();
			TeamCode = teamCode;
			Errors = new List<String>();
		}

		public override void RenderAsHtml()
		{
			LeagueInFocus = Constants.K_LEAGUE_Yahoo;
            if (TimeKeeper.CurrentWeek(DateTime.Now) > 16  )
                LeagueInFocus = Constants.K_LEAGUE_Gridstats_NFL1;

            var totalPlayers = 0;
			var season = new NflSeason( Season, teamsOnly: true );
			foreach ( var team in season.TeamList )
			{
				TeamCode = team.TeamCode;
				Execute();
				totalPlayers += PlayerCount;
#if DEBUG2
            break;
#endif
			}
			DumpErrors();
			TraceIt( $"   {TeamCode} Player Count : {totalPlayers}" );
		}

		public void Execute()
		{
			var PreReport = new SimplePreReport
			{
				ReportType = "Depth Chart",
				Folder = "DepthCharts",
				Season = Season,
				InstanceName = TeamCode,
				Body = GenerateBody()
			};
			PreReport.RenderHtml();
			FileOut = PreReport.FileOut;
		}

		public override string OutputFilename()
		{
			return string.Format( "{0}{1}/DepthCharts/Errors.htm",
				Utility.OutputDirectory(), Season );
		}

		private string GenerateBody()
		{
			var bodyOut = new StringBuilder();
			NflTeam = new NflTeam( TeamCode );
			NflTeam.LoadTeam();
			PlayerCount = NflTeam.PlayerList.Count;

			TraceIt( $"   {TeamCode} Roster Count : {PlayerCount}" );

			bodyOut.AppendLine( NflTeam.RatingsOut() + "    " + NflTeam.SeasonProjectionOut() );
			bodyOut.AppendLine();
			bodyOut.AppendLine( NflTeam.ScheduleHeaderOut() );
			bodyOut.AppendLine( NflTeam.ScheduleOut() );
			bodyOut.AppendLine( NflTeam.ScoreCountsOut() );

			bodyOut.AppendLine();
			bodyOut.AppendLine();
			AppendPlayerLine( bodyOut, "QB" );
			AppendPlayerLine( bodyOut, "Q2" );
			AppendPlayerLine( bodyOut, "RB" );
			AppendPlayerLine( bodyOut, "R2" );
			AppendPlayerLine( bodyOut, "RR" );
			AppendPlayerLine( bodyOut, "3D" );
			AppendPlayerLine( bodyOut, "SH" );
			AppendPlayerLine( bodyOut, "FB" );
			AppendPlayerLine( bodyOut, "W1" );
			AppendPlayerLine( bodyOut, "W2" );
			AppendPlayerLine( bodyOut, "W3" );
			AppendPlayerLine( bodyOut, "TE" );
			AppendPlayerLine( bodyOut, "T2" );
			AppendPlayerLine( bodyOut, "PR" );
			AppendPlayerLine( bodyOut, "KR" );
			AppendPlayerLine( bodyOut, "PK" );
			AppendPlayerLine( bodyOut, "INJ" );
			AppendPlayerLine( bodyOut, "SUS" );
			AppendPlayerLine( bodyOut, "HO" );

			bodyOut.AppendLine();
			bodyOut.AppendLine( string.Format( "   {0} Roster Count : {1}", TeamCode, PlayerCount ) );
			bodyOut.AppendLine();
			bodyOut.AppendLine();
			NflTeam.LoadPassUnit();
			bodyOut.AppendLine( string.Format( "Pass Unit : ({0})", NflTeam.PoRating() ) );
			var lines = NflTeam.PassUnit.DumpUnit();
			foreach ( var line in lines )
				bodyOut.AppendLine( line );

			NflTeam.LoadRushUnit();
			bodyOut.AppendLine( string.Format( "Rush Unit : ({0})", NflTeam.RoRating() ) );
			lines = NflTeam.RunUnit.DumpUnit();
			foreach ( var line in lines )
				bodyOut.AppendLine( line );

			return bodyOut.ToString();
		}

		private void AppendPlayerLine( StringBuilder bodyOut, string posDesc )
		{
			bodyOut.AppendLine( PlayerLine( posDesc ) );
			bodyOut.AppendLine();
		}

		private string PlayerLine( string posDesc )
		{
			var playerList = GetPlayers( posDesc );
			if ( playerList.Count != 1 )
			{
				if ( playerList.Count == 0 )
					return string.Format( "{0}: ", posDesc );
				var playerKeys = GetPlayerKeys( playerList, posDesc );
				return string.Format( "{1}: {0}", playerKeys, posDesc );
			}
			var player = playerList[ 0 ];
			if ( posDesc == "INJ" )
				return string.Format( "{2}: {0}-{1} {4} {5} ({3})", player.JerseyNo, ProjectionLink( player ), posDesc,
									  player.Owner, player.PlayerPos, player.ScoresOut() );
			return string.Format( "{2}: {0}-{1} {4} ({3})", player.JerseyNo, ProjectionLink( player ), posDesc,
								  player.Owner, player.ScoresOut() );
		}

		public string ProjectionLink( NFLPlayer player )
		{
			return string.Format( "<a href='..\\playerprojections\\{0}.htm'>{1}</a>", player.PlayerCode, player.PlayerName );
		}

		private static object GetPlayerKeys( IEnumerable<NFLPlayer> playerList, string posDesc )
		{
			var playerKeys = new StringBuilder();
			foreach ( var player in playerList )
			{
				if ( posDesc == "INJ" )
					playerKeys.Append( string.Format( "{0}-{1} {3} {4} ({2}), ",
					   player.JerseyNo, player.PlayerName, player.Owner, player.PlayerPos, player.ScoresOut() ) );
				else
				{
					var injStatus = string.Empty;
					if ( player.PlayerRole == Constants.K_ROLE_INJURED )
						injStatus = "-INJ";
					playerKeys.Append( string.Format( "{0}-{1}{3} {4} ({2}), ",
					   player.JerseyNo, player.PlayerName, player.Owner, injStatus, player.ScoresOut() ) );
				}
			};
			var playersOut = playerKeys.ToString();
			if ( playersOut.Length > 0 )
				playersOut = playersOut.Substring( 0, playersOut.Length - 2 );
			return playersOut;
		}

		public List<NFLPlayer> GetPlayers( string posDesc )
		{
			var desiredRole = "S";
			if ( posDesc == "R2" || posDesc == "Q2" || posDesc == "T2" )
				desiredRole = "B";
			if ( posDesc == "RR" )
				desiredRole = "R";
			if ( posDesc == "INJ" ) desiredRole = "I";
			if ( posDesc == "SUS" ) desiredRole = "X";
			if ( posDesc == "HO" ) desiredRole = "H";
			var playerList = new List<NFLPlayer>();
			if ( NflTeam == null )
			{
				NflTeam = new NflTeam( TeamCode );
				NflTeam.LoadTeam();
			}
			foreach ( var p in NflTeam.PlayerList )
			{
				var player = ( NFLPlayer ) p;
				if ( HasPos( posDesc, player, desiredRole ) && RoleMatches( posDesc, desiredRole, player ) )
				{
					player.LoadOwner( LeagueInFocus );
					playerList.Add( player );
				}
			}
			ApplyRules( posDesc, playerList );
			return playerList;
		}

		/// <summary>
		///   Will apply rules oon Roles
		/// </summary>
		/// <param name="posDesc"></param>
		/// <param name="playerList"></param>
		private void ApplyRules( string posDesc, List<NFLPlayer> playerList )
		{
			//  missing spots rules:-
			//  must have Q1, R1, W1, W2, TE, PK
			if ( posDesc == "Q1" || posDesc == "R1" || posDesc == "W1" || posDesc == "W2" || posDesc == "TE" || posDesc == "PK" )
			{
				if ( playerList.Count == 0 )
				{
					var errMsg = string.Format( "No one found for {0} - {1}", posDesc, TeamCode );
					Announce( errMsg );
					Errors.Add( errMsg );
				}
			}
			//  only 1 rules
			if ( posDesc != "Q1" && posDesc != "W1" && posDesc != "W2" && posDesc != "PK" ) return;
			if ( ActiveCount( playerList ) <= 1 ) return;
			var errMsg2 = string.Format( "Too many found for {0} - {1} - {2}",
									   posDesc, GetPlayerKeys( playerList, posDesc ), TeamCode );
			Announce( errMsg2 );
			Errors.Add( errMsg2 );
		}

		public void Announce( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Info( "   " + message );
		}

		public void TraceIt( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Info( "   " + message );
		}

		private static int ActiveCount( IEnumerable<NFLPlayer> playerList )
		{
			return playerList.Count( p => !p.IsInjured() && !p.IsSuspended() );
		}

		private static bool RoleMatches( string posDesc, string desiredRole, NFLPlayer player )
		{
			if ( posDesc == "SH" ) return true;
			if ( posDesc == "3D" ) return true;
			// backups can be given these monikers otherwise I would have to remove them manually
			//if ( posDesc == "W1" ) return true;
			//if ( posDesc == "W2" ) return true;
			//if ( posDesc == "W3" ) return true;
			if ( posDesc == "PR" ) return true;
			if ( posDesc == "KR" ) return true;
			if ( desiredRole == "I" ) return true;
			if ( desiredRole == "X" ) return true;
			if ( desiredRole == "H" ) return true;
			return player.PlayerRole.Equals( desiredRole );
		}

		private static bool HasPos( string posDesc, NFLPlayer player, string desiredRole )
		{
			if ( desiredRole == "I" || desiredRole == "X" || desiredRole == "H" )
				return ( player.PlayerRole == desiredRole );

			if ( posDesc == "RB" || posDesc == "R2" )
			{
				return player.Contains( "RB", player.PlayerPos ) || player.Contains( "HB", player.PlayerPos );
			}
			else if ( posDesc == "SH" )
			{
				return player.Contains( "SH", player.PlayerPos );
			}
			else if ( posDesc == "RR" )
			{
				return player.Contains( "RB", player.PlayerPos ) || player.Contains( "HB", player.PlayerPos );
			}
			else if ( posDesc == "3D" )
			{
				return player.Contains( "3D", player.PlayerPos );
			}
			else if ( posDesc == "PR" )
			{
				return player.Contains( "PR", player.PlayerPos );
			}
			else if ( posDesc == "KR" )
			{
				return player.Contains( "KR", player.PlayerPos );
			}
			else if ( posDesc == "Q2" )
			{
				return player.Contains( "QB", player.PlayerPos );
			}
			else if ( posDesc == "T2" )
			{
				return player.Contains( "TE", player.PlayerPos );
			}
			else if ( posDesc == "WR" )
			{
				return player.Contains( "WR", player.PlayerPos ) || player.Contains( "SE", player.PlayerPos ) || player.Contains( "FL", player.PlayerPos );
			}
			else if ( posDesc == "PK" )
			{
				return player.Contains( "PK", player.PlayerPos ) || ( player.Contains( "K", player.PlayerPos ) && player.PlayerCat == "4" );
			}
			return player.Contains( posDesc, player.PlayerPos );
		}

		public bool HasIntegrityError()
		{
			return Errors.Any();
		}

		public void DumpErrors()
		{
			var PreReport = new SimplePreReport
			{
				ReportType = "Depth Chart",
				Folder = "DepthCharts",
				Season = Season,
				InstanceName = "Errors",
				Body = GenerateErrorBody()
			};
			PreReport.RenderHtml();
			FileOut = PreReport.FileOut;
		}

		private string GenerateErrorBody()
		{
			var bodyOut = new StringBuilder();
			foreach ( var err in Errors )
			{
				Utility.Announce( err );
				bodyOut.AppendLine( err );
			}

			bodyOut.AppendLine();

			return bodyOut.ToString();
		}
	}
}