namespace RosterLib
{
	public interface ILeague
	{
		string FileOut { get; set; }

      GsTeam GetTeam( string ownerCode );

		void RosterReport();

		void GameRatings( NFLWeek week, string fTeamOwner );

		void LoadTeams();

		string Season { get; set; }

		int SeasonNo	{ get; }

		int WeekNo { get; set; }

		string CompCode { get; set; }

	}
}