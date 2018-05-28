namespace RosterLib
{

   class ProtectionUnit : TeamUnit
	{
		public ProtectionUnit()
		{
			UnitCode = "PP";
			UnitName = "Pass Protection Unit";
		}


		public override decimal GetStat( NFLWeek week )
   	{
			decimal stat = 0.0M;

			//  important stat is sacks allowed

			//  determine which game
			NFLGame game = week.GameFor( Team.TeamCode );
			
			if ( game != null )
			{
				//  get opponents sacks
				NflTeam opp = game.OpponentTeam( Team.TeamCode );
				stat = Utility.TflWs.GetTotStats( opp.TeamCode, RosterLib.Constants.K_QUARTERBACK_SACKS,  week.Season, week.ZeroWeek(), game.GameCode );
			}
			else
				stat = -1.0M;
		   return stat;
   	}

		public override string Rating()
		{
			return Team.Ratings.Substring( 2, 1 );
		}

		public override string SortDirection()
		{
			return "ASC";  //  smaller the better
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
