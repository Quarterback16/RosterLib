namespace RosterLib
{

   class PassRushUnit : TeamUnit
	{
		public PassRushUnit()
		{
			UnitCode = "PR";
			UnitName = "Pass Rush Unit";
		}


		public override decimal GetStat( NFLWeek week )
   	{
			decimal stat = 0.0M;
			//  important stat is sacks made
			//  determine which game
			NFLGame game = week.GameFor( Team.TeamCode );
			
			if ( game != null )
			{
				//  get sacks
				stat = Utility.TflWs.GetTotStats( Team.TeamCode, RosterLib.Constants.K_QUARTERBACK_SACKS,  week.Season, week.ZeroWeek(), game.GameCode );
			}
			else
				stat = -1.0M;
		   return stat;
   	}

		public override string Rating()
		{
			return Team.Ratings.Substring( 3, 1 );
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
					sColour = "RED";
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

