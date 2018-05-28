namespace RosterLib
{

   class FGDefenceUnit : TeamUnit
	{
		public FGDefenceUnit()
		{
			UnitCode = "FG";
			UnitName = "FG Defence Unit";
		}


		public override decimal GetStat( NFLWeek week )
   	{
			int stat = 0;

			//  important stat is FG

			//  determine which game
			NFLGame game = week.GameFor( Team.TeamCode );
			
			if ( game != null )
			{
				//  get FG allowed
				NflTeam opp = game.OpponentTeam( Team.TeamCode );
				stat = Utility.TflWs.GetTotTeamScoresFor( RosterLib.Constants.K_SCORE_FIELD_GOAL, opp.TeamCode, week.Season, week.ZeroWeek(), game.GameCode );
			}
			else
				stat = -1;
		   return stat;
   	}

		public override string Rating()
		{
			return "X";
		}

		public override string SortDirection()
		{
			return "DESC";  //  put teams that give up more FGs at the top
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
					sColour = "RED";
					break;

				case 1:
					sColour = "GREEN";
					break;

				case 2:
					sColour = "YELLOW";
					break;

				default:
					sColour = "YELLOW";
					break;
			}
			return sColour;
		}
	}

}



