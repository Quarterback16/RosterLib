namespace RosterLib
{
   /// <summary>
   /// Applys the GS4 scoring rules to a player.
   /// </summary>
   public class NFLUKTeamScorer : IRateTeams
	{
		private NFLWeek week;

		public NFLUKTeamScorer()
		{
			Name = "NflUk.com Scorer";
			Week = new NFLWeek(Utility.CurrentWeek(), Utility.CurrentSeason());
		}

		#region IRateTeams Members

		public int RateTeam(NflTeam team)
		{
			//  Defensive team ratings
			team.ProjectNextWeek();
			//  Sacks are worth 3 points each
			int sackPoints = team.ProjectedSacks * 3;
			//  Interceptions are worth 6 points each
			int intPoints = team.ProjectedSteals * 6;
			team.Points = sackPoints + intPoints;

			return team.Points;
		}

		public string Name
		{
			get
			{
				// TODO:  Add GS4Scorer.Name getter implementation
				return Name;
			}
			set
			{
				// TODO:  Add GS4Scorer.Name setter implementation
			}
		}

		public NFLWeek Week
		{
			get { return week; }
			set { week = value; }
		}

		#endregion
	}
}


