using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace RosterLib
{
	/// <summary>
	///  Represents a seasons worth (16 games) of regualr _season games for a *team*.
	/// </summary>
	public class NFLSchedule
	{
		public ArrayList GameList;
		public NflTeam Team;
		private readonly string _season;

		public NFLSchedule( string season, NflTeam team )
		{
#if DEBUG
			Utility.Announce( string.Format( "NFLSchedule:Loading {0} Schedule for {1}", season, team.Name ) );
#endif
			Team = team;
			_season = season;
			//  load schedule
			GameList = new ArrayList();
			Load();
		}

		public void Load()
		{
			var ds = Utility.TflWs.TeamSchedDs( _season, Team.TeamCode );
			var dt = ds.Tables["sched"];
#if DEBUG
			Utility.Announce( string.Format( "Schedule has {0} games for {1} in {2}", 
			   dt.Rows.Count, Team.TeamCode, _season ) );
#endif
			//  For each game on their schedule
			foreach ( DataRow dr in dt.Rows )
			{
				var dGame = DateTime.Parse(dr["GameDate"].ToString());
				var nHomeScore = Int32.Parse(dr["HOMESCORE"].ToString());
				var nAwayScore = Int32.Parse(dr["AWAYSCORE"].ToString());
				var gameWeek = dr[ "WEEK" ].ToString();
				var gameCode = dr[ "GAMENO" ].ToString();
				
				var strHomeTeam = dr["HOMETEAM"].ToString();
				var strAwayTeam = dr["AWAYTEAM"].ToString();
				var nSpread = Decimal.Parse(dr["SPREAD"].ToString());
				var nTotal = Decimal.Parse(dr["TOTAL"].ToString());
				//  use this old constructor to avoid a call stack explosion
				var game = new NFLGame(gameWeek, dGame, strHomeTeam, strAwayTeam,
				                       nHomeScore, nAwayScore, nSpread, nTotal, _season, gameCode)
				           	{Hour = dr["GAMEHOUR"].ToString()};

				if (dGame > DateTime.Now)
				{
					game.ProjectedHomeScore = game.HomeScore;
					game.ProjectedAwayScore = game.AwayScore;
				}

				if ( game.HomeNflTeam == null ) game.HomeNflTeam = Masters.Tm.GetTeam( game.Season + game.HomeTeam );
				if ( game.AwayNflTeam == null ) game.AwayNflTeam = Masters.Tm.GetTeam( game.Season + game.AwayTeam );

				GameList.Add( game );
			}
		}

		public NFLGame Game(int week)
		{
			return GameList.Cast< NFLGame >().FirstOrDefault( g => string.Format( "{0:0#}", week ) == g.Week );
		}
		
		public NflTeam Opponent( int week )
		{
			return ( from NFLGame g in GameList
						where string.Format( "{0:0#}", week ) == g.Week
						select g.OpponentTeam( Team.TeamCode ) ).FirstOrDefault();
		}
	}
}
