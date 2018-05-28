namespace RosterLib
{

   class RunDefenceUnit : TeamUnit
	{
		public RunDefenceUnit()
		{
			UnitCode = "RD";
			UnitName = "Run Defence Unit";
		}


		public override decimal GetStat( NFLWeek week )
   	{
			int stat = 0;

			//  important stat is Tdr

			//  determine which game
			NFLGame game = week.GameFor( Team.TeamCode );
			
			if ( game != null )
			{
				//  get Tdr
				NflTeam opp = game.OpponentTeam( Team.TeamCode );
				stat = Utility.TflWs.GetTotTeamScoresFor( RosterLib.Constants.K_SCORE_TD_RUN, opp.TeamCode, week.Season, week.ZeroWeek(), game.GameCode );
			}
			else
				stat = -1;
		   return stat;
   	}

		public override string Rating()
		{
			return Team.Ratings.Substring( 4, 1 );
		}

		public override string SortDirection()
		{
			return "ASC";  //  less the better
		}

		//  logic for setting BG colour
		public override string BGPicker( int theValue )
		{
			string sColour;

			switch (theValue)
			{
            case -1:
					sColour = "LIGHTGREY";
					break;
 
				case 0:
					sColour = "YELLOW";
					break;

				case 1:
					sColour = "GREEN";
					break;

				case 2:
					sColour = "RED";
					break;

				default:
					sColour = "RED";
					break;
			}
			return sColour;
		}
	}

}


