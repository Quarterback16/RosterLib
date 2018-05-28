using System;

namespace RosterLib
{
	public class DefensiveScoringCalculator : ICalculate
	{
		public NflTeam Team { get; set; }

		private NFLGame _game;

		public int Offset { get; set; }
		public NFLWeek StartWeek { get; set; }

		public bool AuditTrail { get; set; }

		public DefensiveScoringCalculator( NFLWeek startWeek, int offset )
		{
			StartWeek = startWeek;
			if (startWeek.WeekNo < 2 )
				StartWeek = startWeek.PreviousWeek(startWeek, loadgames: false, regularSeasonGamesOnly: true);
			Offset = offset;
			Team = new NflTeam("SF");
		}

		public void Calculate( NflTeam team, NFLGame game )
		{
			Team = team;
			_game = game;

			var opponentCode = DetermineOpponent();

			Team.FantasyPoints = 0;
			Team.DefensiveScores = 0;
			Team.TotInterceptions = 0;
			Team.TotSacks = 0;
			Team.PtsAgin = 0;

			CalculateDefensiveScores();  //  will increment _team.FantasyPoints

			CountStats();

			Team.FantasyPoints += Team.TotSacks;
#if DEBUG
         Utility.Announce( string.Format( "  Defense got {0} sacks for {0} FP",
            Team.TotSacks  ) );
#endif
         Team.FantasyPoints += Team.TotInterceptions * 2;
#if DEBUG
         Utility.Announce( string.Format( "  Defense got {0} intercepts for {1} FP",
            Team.TotInterceptions, Team.TotInterceptions * 2 ) );
#endif

         CountYardage( opponentCode );

			Team.PtsAgin = CountPointsAllowed();
			var defFantasyPts =  PointsForAllowing( Team.PtsAgin );
			Team.FantasyPoints += defFantasyPts;

#if DEBUG
			Utility.Announce( string.Format( "  Defense gave up {0} real points for {1} FP", 
            Team.PtsAgin, defFantasyPts ));
#endif

#if DEBUG
         Utility.Announce( string.Format( "  FP total {0}", Team.FantasyPoints ) );
#endif
         Team.Games++;
		}

		private static int PointsForAllowing( int pointsAllowed )
		{
			int points;

			if ( pointsAllowed > 34 )
				points = -4;
			else if ( pointsAllowed > 27 )
				points = -1;
			else if ( pointsAllowed > 20 )
				points = 0;
			else if ( pointsAllowed > 13 )
				points = 1;
			else if ( pointsAllowed > 6 )
				points = 4;
			else
				points = 7;

			return points;
		}

		private string DetermineOpponent()
		{
			return _game.HomeNflTeam.TeamCode == Team.TeamCode 
				? _game.AwayNflTeam.TeamCode : _game.HomeNflTeam.TeamCode;
		}

		private void CountYardage( string opponentCode )
		{
			Team.TotYdrAllowed += (int) Utility.TflWs.TeamStats( 
				 Constants.K_STATCODE_RUSHING_YARDS, _game.Season, _game.Week, _game.GameCode, opponentCode );
			Team.TotYDpAllowed += (int) Utility.TflWs.TeamStats( 
				 Constants.K_STATCODE_PASSING_YARDS, _game.Season, _game.Week, _game.GameCode, opponentCode );
		}

		private int CountPointsAllowed()
		{
			int pointsAllowed;
			if (_game.HomeNflTeam.TeamCode == Team.TeamCode)
			{
				Team.PtsAgin = _game.AwayScore;
				pointsAllowed = _game.AwayScore;
			}
			else
			{ 
				Team.PtsAgin = _game.HomeScore;
				pointsAllowed = _game.HomeScore;
			}
			return pointsAllowed;
		}

		private void CountStats()
		{
			 Team.TotInterceptions += (int) Utility.TflWs.TeamStats( 
				 Constants.K_STATCODE_INTERCEPTIONS_MADE, _game.Season, _game.Week, _game.GameCode, Team.TeamCode );
			 Team.TotSacks += Utility.TflWs.TeamStats( 
				 Constants.K_STATCODE_SACK, _game.Season, _game.Week, _game.GameCode, Team.TeamCode );
		}

		private void CalculateDefensiveScores()
		{
			//  any scores not done by an offensive player
			var ds = Utility.TflWs.GetTeamDefensiveScoresFor( 
				Team.TeamCode, _game.Season, _game.Week, _game.GameCode );
			var dt = ds.Tables[0];
			for (var i = 0; i < dt.Rows.Count; i++)
			{
				var s = new NflScore( dt.Rows[i]["WHEN"].ToString(), 
													dt.Rows[i]["SCORE"].ToString(), 
													Int32.Parse( dt.Rows[i]["DISTANCE"].ToString() ) );

				Team.FantasyPoints += s.Points();
#if DEBUG
            Utility.Announce( string.Format( "   Defensive score! :{0} {1}-{2} yds {3} FP",
               Team.DefensiveScores, s.TypeCode, s.Distance, s.Points() ) );
#endif
            if ( s.TypeCode.Equals( Constants.K_SCORE_SAFETY ) )
               continue;

				Team.DefensiveScores++;
			}
		}
	}
}
