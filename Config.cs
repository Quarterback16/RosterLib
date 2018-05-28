using System;

namespace RosterLib
{
	public static class Config
	{
		public static bool AllGames { get; set; }

		#region Config file Options

		/// <summary>
		///    Produces Roster grids for QB, RB, WR, PK
		///    also Injury grids for the same positions.
		/// </summary>
		/// <returns></returns>
		public static bool OldFormat()
		{
			return ConfigMaster.GetInstance().OldMatrix;
		}

		/// <summary>
		///    Produces a *large* NFL roster page.
		/// </summary>
		/// <returns></returns>
		public static bool NewFormat()
		{
			return ConfigMaster.GetInstance().NewFormat;
		}

		#region  New Format options

		/// <summary>
		///    Controls whether to put the game Scores on for a team
		/// </summary>
		/// <returns>go or no go</returns>
		public static bool ShowGameLog()
		{
			return ConfigMaster.GetInstance().GameLog;
		}

		/// <summary>
		///    Shows the Player Experience points on the list.
		/// </summary>
		/// <returns></returns>
		public static bool ShowEp()
		{
			return false;
		}

		public static bool HideBackups()
		{
			return ConfigMaster.GetInstance().HideBackups;
		}

		public static bool HideReserves()
		{
			return ConfigMaster.GetInstance().HideReserves;
		}

		public static bool ShowAllPlayerGames()
		{
			AllGames = ConfigMaster.GetInstance().AllPlayerGames;
			return AllGames;
		}

		public static bool HideInjuries()
		{
			return ConfigMaster.GetInstance().HideInjuries;
		}

		#endregion

		#region  New format sub-options

		/// <summary>
		///    Controls whether to predict outcome of scheduled games.
		/// </summary>
		/// <returns></returns>
		public static bool DoProjections()
		{
			return ConfigMaster.GetInstance().Projections;
		}

		/// <summary>
		///    Controls whether to dump out the New NFL roster.
		/// </summary>
		/// <returns></returns>
		public static bool DoRoster()
		{
			return ConfigMaster.GetInstance().Roster;
		}

		/// <summary>
		///    Predicts ouput for all the starting kickers.
		/// </summary>
		/// <returns></returns>
		public static bool DoKickers()
		{
			return ConfigMaster.GetInstance().Kickers;
		}

		/// <summary>
		///    Draws the starting Defence and offence for each team.
		/// </summary>
		/// <returns></returns>
		public static bool DoTeamcards()
		{
			return ConfigMaster.GetInstance().TeamCards;
		}

		public static bool DoPlayerReports()
		{
			return ConfigMaster.GetInstance().PlayerReports;
		}

		public static bool DoPlayerCsv()
		{
			return ConfigMaster.GetInstance().PlayerCsv;
		}

		#endregion

		/// <summary>
		///    Produces lists of all players by Average season Scores.
		/// </summary>
		/// <returns></returns>
		public static bool DoRankings()
		{
			return ConfigMaster.GetInstance().Rankings;
		}

		public static bool DoOffensiveLine()
		{
			return ConfigMaster.GetInstance().OffensiveLine;
		}

		public static bool DoStarters()
		{
			return ConfigMaster.GetInstance().Starters;
		}

		public static bool DoReturners()
		{
			return ConfigMaster.GetInstance().Returners;
		}

		public static bool DoDepthCharts()
		{
			return ConfigMaster.GetInstance().DepthCharts;
		}

		/// <summary>
		///    Controls whether to produce lists of scorers for the current season.
		///    *Not* controlled by the config file
		/// </summary>
		/// <returns></returns>
		public static bool DoCurrentScorers()
		{
			if (DoScorers())
				if (Int32.Parse( Utility.CurrentWeek() ) > 0)
					return true;
			return false;
		}

		/// <summary>
		///    Produces ladders for Defence, Offence and total based on
		///    the experience points method.
		/// </summary>
		/// <returns></returns>
		public static bool DoScorers()
		{
			return ConfigMaster.GetInstance().Scorers;
		}

		/// <summary>
		///    Produces ladders for Defence, Offence and total based on
		///    the experience points method.
		/// </summary>
		/// <returns></returns>
		public static bool DoExperience()
		{
			return ConfigMaster.GetInstance().Experience;
		}

		public static bool DoVictoryPoints()
		{
			return ConfigMaster.GetInstance().VictoryPoints;
		}

		public static bool DoFaMarket()
		{
			return ConfigMaster.GetInstance().FaMarket;
		}

		public static bool DoStarRatings()
		{
			return ConfigMaster.GetInstance().StarRatings;
		}

		public static bool DoGsPerformance()
		{
			return ConfigMaster.GetInstance().GsPerformance;
		}

		public static bool DoEspnPerformance()
		{
			return ConfigMaster.GetInstance().EspnPerformance;
		}

		public static bool DoYahooProjections()
		{
			return ConfigMaster.GetInstance().YahooProjections;
		}

		public static bool DoFpProjections()
		{
			return ConfigMaster.GetInstance().FpProjections;
		}

		public static bool DoBalanceReport()
		{
			return ConfigMaster.GetInstance().BalanceReport;
		}

		public static bool DoDefensiveScoring()
		{
			return ConfigMaster.GetInstance().DefensiveScoring;
		}

		public static bool DoUnitReports()
		{
			return ConfigMaster.GetInstance().UnitReports;
		}

		public static bool DoUnitsByWeek()
		{
			return ConfigMaster.GetInstance().UnitsByWeek;
		}

		public static bool DoFreeAgents()
		{
			return ConfigMaster.GetInstance().FreeAgents;
		}

		public static bool DoNflukRatings()
		{
			return ConfigMaster.GetInstance().NflUk;
		}

		public static bool DoStrengthOfSchedule()
		{
			return ConfigMaster.GetInstance().SoS;
		}

		public static bool DoFrequencyTables()
		{
			return ConfigMaster.GetInstance().FrequencyTables;
		}

		public static bool DoTeamMetrics()
		{
			return ConfigMaster.GetInstance().TeamMetrics;
		}

		public static bool DoGridStatsReport()
		{
			return ConfigMaster.GetInstance().GridStatsReport;
		}

		public static bool DoEspn()
		{
			return ConfigMaster.GetInstance().Espn;
		}

		public static bool DoLineupCards()
		{
			return ConfigMaster.GetInstance().LineupCards;
		}

		public static bool DoGs4WrRanks()
		{
			return ConfigMaster.GetInstance().Gs4WrRanks;
		}

		public static bool DoHotLists()
		{
			return ConfigMaster.GetInstance().HotLists;
		}

		public static bool DoStatsGrids()
		{
			return ConfigMaster.GetInstance().StatsGrids;
		}

		public static bool DoHillenTips()
		{
			return ConfigMaster.GetInstance().HillenTips;
		}

		public static bool DoSuggestedLineups()
		{
			return ConfigMaster.GetInstance().SuggestedLineups;
		}

		public static bool DoProjectedLineups()
		{
			return ConfigMaster.GetInstance().ProjectedLineups;
		}

		/// <summary>
		///    Controls whether to print matchup reports for all the upcoming games
		/// </summary>
		/// <returns>do it or not</returns>
		public static bool DoMatchups()
		{
			return ConfigMaster.GetInstance().MatchUps;
		}

		/// <summary>
		///    Controls whether to print out the best plays of the week.
		/// </summary>
		/// <returns>do it or not.</returns>
		public static bool DoPlays()
		{
			return ConfigMaster.GetInstance().Plays;
		}

		/// <summary>
		///    Controls whether to back test all the schemes.
		/// </summary>
		/// <returns>do it or not.</returns>
		public static bool DoBackTest()
		{
			return ConfigMaster.GetInstance().BackTest;
		}

		public static bool ShowPerformance
		{
			get { return ConfigMaster.GetInstance().ShowPerformance; }
		}

		public static string ReportType { get; set; }

		public static decimal LeagueAvgTDp()
		{
			return 1.3M;
		}

		public static decimal LeagueAvgTDr()
		{
			return 0.8M;
		}

		public static decimal LeagueAvgSak()
		{
			return 2.3M;
		}

		public static string NflConnectionString()
		{
			return ConfigMaster.GetInstance().NflConnectionString;
		}

		public static bool GenerateYahooXml()
		{
			return ConfigMaster.GetInstance().GenerateYahooXml;
		}

		public static bool GenerateStatsXml()
		{
			return ConfigMaster.GetInstance().GenerateStatsXml;
		}

		#endregion

		internal static string OutputDirectory()
		{
			return ConfigMaster.GetInstance().OutputDirectory;
		}

		internal static string XmlDirectory()
		{
			return ConfigMaster.GetInstance().XmlDirectory;
		}

		internal static string PrimaryDrive()
		{
			return ConfigMaster.GetInstance().PrimaryDrive;
		}

		internal static string TflFolder()
		{
			return ConfigMaster.GetInstance().TflFolder;
		}
	}
}