namespace RosterLib
{

   class RunOffenceUnit : TeamUnit
	{
		public RunOffenceUnit()
		{
			UnitCode = "RO";
			UnitName = "Run Offence Unit";
		}


		public override decimal GetStat( NFLWeek week )
   	{
			int stat = 0;

			//  important stat is Tdr

			//  determine which game
			NFLGame game = week.GameFor( Team.TeamCode );
			
			if ( game != null )
			{
				//  get Tdp
                stat = Utility.TflWs.GetTotTeamScoresFor(RosterLib.Constants.K_SCORE_TD_RUN, Team.TeamCode, week.Season, week.ZeroWeek(), game.GameCode);
			}
			else
				stat = -1;
		   return stat;
   	}

		public override string Rating()
		{
			return Team.Ratings.Substring( 1, 1 );
		}

		public override string SortDirection()
		{
			return "DESC";  //  more the better
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
					sColour = "GREEN";
					break;

				default:
					sColour = "YELLOW";
					break;
			}
			return sColour;
		}
	}

}

