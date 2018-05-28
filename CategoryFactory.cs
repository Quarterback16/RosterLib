namespace RosterLib
{
	/// <summary>
	///  The CategoryFactory is used to create Position objects.
	/// </summary>
	public class CategoryFactory
	{
		public PlayerPos CreatePos( string categoryIn, NFLPlayer p )
		{
			PlayerPos pp;
			switch( categoryIn )
			{
				case RosterLib.Constants.K_QUARTERBACK_CAT: 
					pp = new QuarterbackCategory();
					break;
                case RosterLib.Constants.K_RUNNINGBACK_CAT: 
					pp = new RunningbackCategory();
					break;
                case RosterLib.Constants.K_RECEIVER_CAT: 
					pp = new ReceiverCategory();
					break;
                case RosterLib.Constants.K_KICKER_CAT: 
					pp = new KickerCategory();
					break;
                case RosterLib.Constants.K_LINEMAN_CAT: 
					pp = new DefensiveLineCategory();
					break;
                case RosterLib.Constants.K_DEFENSIVEBACK_CAT: 
					pp = new DefensiveBackCategory();
					break;
                case RosterLib.Constants.K_OFFENSIVELINE_CAT: 
					pp = new OffensiveLineCategory();
					break;
                case RosterLib.Constants.K_DEFENSIVETEAM_CAT:
               pp = new DefensiveTeamCategory();
               break;
            default:
               RosterLib.Utility.Announce( string.Format( "Unknown Category>{0} player - {1}", categoryIn, p.PlayerName ));
			      pp = new DefensiveLineCategory();
			      break;
            				      
			}
			pp.Category = categoryIn;
			return pp;
		}
	}
}
