namespace RosterLib
{

   class PassDefenceUnit : TeamUnit
	{
		public PassDefenceUnit()
		{
			UnitCode = "PD";
			UnitName = "Pass Defence Unit";
		}


		public override decimal GetStat( NFLWeek week )
   	{
			int stat = 0;

			//  important stat is Tdp

			//  determine which game
			NFLGame game = week.GameFor( Team.TeamCode );
			
			if ( game != null )
			{
				//  get Tdp
				NflTeam opp = game.OpponentTeam( Team.TeamCode );
				stat = Utility.TflWs.GetTotTeamScoresFor( RosterLib.Constants.K_SCORE_TD_PASS, opp.TeamCode, week.Season, week.ZeroWeek(), game.GameCode );
			}
			else
				stat = -1;
		   return stat;
   	}

		public override string Rating()
		{
			return Team.Ratings.Substring( 5, 1 );
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
					sColour = "YELLOW";
					break;

				case 2:
					sColour = "GREEN";
					break;

				default:
					sColour = "RED";
					break;
			}
			return sColour;
		}
	}

}



