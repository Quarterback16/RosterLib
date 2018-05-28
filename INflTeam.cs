using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;

namespace RosterLib
{
	public interface INflTeam
	{
		string ApCode { get; set; }
		decimal CurrentClip { get; set; }
		int DefensiveScores { get; set; }
		string Div { get; set; }
		decimal ExperiencePoints { get; set; }
		int ExpLosses { get; set; }
		int ExpWins { get; set; }
		decimal FantasyPoints { get; set; }
		int FaPoints { get; set; }
		int FieldGoals { get; set; }
		ArrayList GameList { get; set; }
		int Games { get; set; }
		bool IsPlayoffBound { get; set; }
		NFLPlayer Kicker { get; set; }
		KickUnit KickUnit { get; set; }
		DataSet LineupDs { get; set; }
		int Losses { get; set; }
		Hashtable MetricsHt { get; set; }
		int MTies { get; set; }
		string Name { get; set; }
		int[,] NibbleRating { get; set; }
		string NickName { get; set; }
		int Passes { get; set; }
		PassUnit PassUnit { get; set; }
		ICachePlayers PlayerCache { get; set; }
		string PlayersGot { get; set; }
		int PlayersIn { get; set; }
		string PlayersLost { get; set; }
		int PlayersOut { get; set; }
		int Points { get; set; }
		decimal PowerRating { get; set; }
		int ProjectedSacks { get; set; }
		int ProjectedSteals { get; set; }
		string Ratings { get; set; }
		int Runs { get; set; }
		RushUnit RushUnit { get; set; }
		string Season { get; set; }
		double SoS { get; set; }
		decimal StartingPowerRating { get; set; }
		TeamCard TeamCard { get; set; }
		string TeamCode { get; set; }
		int Ties { get; set; }
		int TotInterceptions { get; set; }
		int TotIntercepts { get; set; }
		decimal TotSacks { get; set; }
		decimal TotSacksAllowed { get; set; }
		int TotTDf { get; set; }
		int TotTDi { get; set; }
		int TotTDk { get; set; }
		int TotTDp { get; set; }
		int TotTDpAllowed { get; set; }
		int TotTDr { get; set; }
		int TotTDrAllowed { get; set; }
		int TotTDs { get; set; }
		int TotTDt { get; set; }
		int TotYdp { get; set; }
		int TotYDpAllowed { get; set; }
		int TotYdr { get; set; }
		int TotYdrAllowed { get; set; }
		bool Uses34Defence { get; set; }
		int VictoryPoints { get; set; }
		int Wins { get; set; }

		bool Above500();
		string AdjustedRatings(string ratings);
		decimal AvgDefSak(int weekSeed);
		decimal AvgDefTDp(int weekSeed);
		decimal AvgDefTDr(int weekSeed);
		decimal AvgOffSaka(int weekSeed);
		decimal AvgOffTDp(int weekSeed);
		decimal AvgOffTDr(int weekSeed);
		string BoxHtml(bool showBackups, string catIn, string strPos);
		void CalculateDefensiveScoring(ICalculate myCalculator, [Optional] bool doOpponent);
		string ChangeRating(int startPos, int diff, string ratings);
		string City();
		decimal Clip();
		int CompareTo(object obj);
		void CountFaPoints(string seasonIn);
		void CountVictoryPoints();
		string DefensiveRating();
		string DefensiveRating(string catCode);
		string DefensiveUnit(string catCode);
		string DefensiveUnitMatchUp(string catCode, string matchUp);
		string DefUnits();
		bool DetermineDefenceAlignment();
		void DistributeExperience(string gameCode, string unitCode, decimal expPoints);
		string Division();
		string DueDownWeek();
		string DueUpWeek();
		void DumpLineup(string season, string week);
		int DumpMissingKickers();
		int DumpMissingQuarterBacks();
		int DumpMissingRunningBacks();
		int DumpMissingTightEnds();
		void DumpStarters();
		string FreeAgents(bool isOffence, bool showHeader, bool includeStarters);
		NFLGame GameFor(string seasonIn, int week);
		int GamesPlayed();
		NFLPlayer GetCurrentStarter(string positionIn);
		NFLGame GetGame(string gameSeason, string gameWeek, string gameNo);
		UnitMatrix GetMatrix();
		decimal GetMetric(string seasonIn, string week, string metric);
		NFLPlayer GetPlayerAt(string lineupSpot, int occurence, bool bDef, bool usedVal);
		decimal GetPowerRating(string week);
		bool HasGoodProtection();
		string HtmlList(ArrayList plyrList, bool isOffence, bool showHeader);
		string InjuredList(bool isOffence);
		string InjuryHtml();
		bool IsDivisionalOpponent(string opponentCode);
		bool IsGoodDefence();
		bool IsGoodOffence();
		bool IsOutOfContention();
		bool IsRival(string opponentTeamCode);
		string KeyPlayer(string pos, string seasonIn, string week);
		string KickerProjection();
		Lineup Lineup(string season, string week);
		string LineupFile();
		void LoadCurrentStarters();
		void LoadGames(string sTeam, string sSeason);
		ArrayList LoadGamesFrom(string sStartSeason, string sStartWeek, int offset);
		void LoadKicker();
		List<string> LoadKickUnit();
		void LoadOldGrid(string catIn, string strPos);
		void LoadPassReceivers(string roleFilter);
		List<string> LoadPassUnit();
		void LoadPlayerUnits();
		void LoadPreviousGames(int nGames, DateTime theDate);
		void LoadPreviousGames(string sTeam, string sSeason);
		void LoadPreviousRegularSeasonGames(int nGames, DateTime theDate);
		void LoadPreviousRegularSeasonGames(string sTeam, string sSeason, DateTime focusDate);
		void LoadQuarterbacks(string roleFilter);
		List<string> LoadRushUnit();
		void LoadSchedule();
		void LoadStarters(string catIn, string strPos);
		void LoadTeam();
		void LoadTeamCard();
		string NameOut();
		NFLGame NextGame();
		NFLGame NextGame(DateTime when);
		NFLGame NextGameOrBye();
		string NextOpponent(DateTime when);
		string Nick();
		string OffensiveRating();
		string OffensiveRating(string catCode);
		string OffUnits();
		NflTeam OpponentFor(string seasonIn, int week);
		string PdList();
		decimal PdMultiplierAt(int weekSeed);
		string PdRating();
		string PdReport();
		string PdUnit(string seasonIn);
		string PoList();
		decimal PoMultiplierAt(int weekSeed);
		string PoRating();
		string PoReport();
		string PoUnit(string seasonIn);
		decimal PpMultiplierAt(int weekSeed);
		string PpRating();
		string PpReport();
		string PpUnit(string seasonIn);
		NFLGame PreviousGame(DateTime when);
		decimal PrMultiplierAt(int weekSeed);
		void ProjectNextWeek();
		string ProperName();
		string ProperNickName();
		string PrRating();
		string PrReport();
		string PrUnit(string seasonIn);
		string RatingPts();
		string RatingPtsOut();
		string RatingsOut();
		string RdList();
		decimal RdMultiplierAt(int weekSeed);
		string RdRating();
		string RdReport();
		string RdUnit(string seasonIn);
		decimal RecordAfterWin(DateTime since);
		string RecordOut();
		string RenderTeamCard();
		string RoList();
		decimal RoMultiplierAt(int weekSeed);
		string RoRating();
		string RoReport();
		string RoUnit(string seasonIn);
		string ScheduleHeaderOut();
		string ScheduleOut();
		string ScoreCountsOut();
		string SeasonProjection(IPrognosticate strategy, string metricName, DateTime projectionDate);
		string SeasonProjectionOut();
		void SetDefence();
		NFLPlayer SetKicker();
		void SetMetrics(Hashtable ht);
		void SetRecord(string seasonInFocus, bool skipPostseason);
		string SpitLineups(bool bPersist);
		string SpreadoutRatings();
		decimal SpreadRange(string seasonIn, string week);
		decimal SpreadRecordAfterLoss(DateTime since);
		decimal SpreadRecordAfterWin(DateTime since);
		string StarDueDownWeek();
		string StarDueUpWeek();
		string Starter(string seasonIn, int weekNo, string pos, int slot);
		double StrengthOfSchedule();
		void TallyPlays(string seasonIn, bool skipPostseason);
		void TallyStats();
		string TdpBreakdownLink();
		string TdrBreakdownLink();
		string TeamDiv();
		string TeamUrl();
		string ToString();
		string UnitRating(string unitCode);
		string UnitReport();
		string UnitTypeDiv();
		bool WinningRecord();
	}
}