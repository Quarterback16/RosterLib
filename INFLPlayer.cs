using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;

namespace RosterLib
{
	public interface INFLPlayer
	{
		string PlayerCode { get; set; }
		string PlayerRole { get; set; }
		string PlayerName { get; set; }
		string Bio { get; set; }
		string College { get; set; }
		PlayerGameMetrics CurrentGameMetrics { get; set; }
		List<NFLGame> CurrentGames { get; set; }
		INflTeam CurrTeam { get; set; }
		string DBirth { get; set; }
		string Drafted { get; set; }
		bool EpDone { get; set; }
		decimal ExperiencePoints { get; set; }
		string FirstName { get; set; }
		Dictionary<string, PlayerGameMetrics> GameMetrics { get; set; }
		bool GamesLoaded { get; set; }
		int HeightFeet { get; set; }
		int HeightInches { get; set; }
		string Injury { get; set; }
		bool IsRetired { get; set; }
		string JerseyNo { get; set; }
		string LastSeason { get; set; }
		string LineupPos { get; set; }
		string Opponent { get; set; }
		string OppRate { get; set; }
		string PlayerSpread { get; set; }
		decimal Points { get; set; }
		int ProjectedFg { get; set; }
		int ProjectedPat { get; set; }
		int ProjectedReceptions { get; set; }
		decimal ProjectedTDc { get; set; }
		decimal ProjectedTDp { get; set; }
		decimal ProjectedTDr { get; set; }
		int ProjectedYDc { get; set; }
		int ProjectedYDp { get; set; }
		int ProjectedYDr { get; set; }
		string StarRating { get; set; }
		DateTime StartDate { get; set; }
		string Surname { get; set; }
		string TeamCode { get; }
		string TeamLastYear { get; set; }
		int Weight { get; set; }

		string ActiveStatus();
		decimal ActualFpts(NFLGame g);
		string ActualStats();
		string ActualStatsFor(NFLGame g);
		void AddMetric(string metricType, string gameKey, decimal nScores);
		void AddMetric(string metricType, string gameKey, decimal nScores, int fp);
		void CalculateEp(string season);
		string CatRow(string cat);
		int Compare(object x, object y);
		int CompareTo(object obj);
		void ConsoleOut();
		bool Contains(string subString, string mainString);
		string DetailLine();
		int DraftRound();
		void DumpHistory();
		void DumpMetrics();
		string GameFor(string season, int week);
		string GameKeyFor(string season, string week);
		bool HasSpecRole(string specRole);
		string Injuries();
		bool IsAce();
		bool IsActive();
		bool IsAtHome(NFLWeek week);
		bool IsBackup();
		bool IsDefence();
		bool IsFantasyOffence();
		bool IsFantasyPlayer();
		bool IsFreeAgent();
		bool IsInjured();
		bool IsInPrime();
		bool IsInUnit(string unitCode);
		bool IsItalic();
		bool IsKicker();
		bool IsNewbie();
		bool IsOffence();
		bool IsOneOrTwo();
		bool IsPlayerReport();
		bool IsPlayoffBound();
		bool IsRb();
		bool IsReturner();
		bool IsRookie();
		bool IsRusher();
		bool IsShortYardageBack();
		bool IsStar();
		bool IsStarter();
		bool IsSuspended();
		bool IsTandemBack();
		bool IsTe();
		DataSet LastScores(string scoreType, int weekFrom, int weekTo, string season, string id);
		DataSet LastStats(string statType, int weekFrom, int weekTo, string season);
		string LoadAllOwners();
		void LoadOwner([Optional] string fantasyLeague);
		void LoadPerformances(bool allGames, bool currSeasonOnly, string whichSeason);
		void LoadPlayer(DataRow dr);
		void LoadProjections(PlayerGameMetrics pgm);
		void LoadSeason(string season);
		string MetricsOut(string gameKey);
		bool NextGameIsBye();
		string NextGameSpread(NFLGame nextGame);
		string NextOpponentOut(NFLGame nextGame);
		NflTeam NextOpponentTeam(NFLGame nextGame);
		string NextResult(NFLGame nextGame);
		int NoOfSeasons();
		string OpponentRating(string ratings);
		string PlayerAge();
		string PlayerBox(bool isBold);
		string PlayerDiv();
		string PlayerHeader();
		string PlayerHeaderRow(string perfCol1, string perfCol2);
		string PlayerNameTo(int len);
		string PlayerOut();
		string PlayerProjection(string season);
		string PlayerReport([Optional] bool forceIt);
		string PlayerRow(bool addAvg);
		decimal PointsForWeek(NFLWeek week, IRatePlayers rater);
		string ProjectionLink();
		string ProjectionLink(int padding);
		string ProjectionLink(string season);
		string ProjectionLink(string season, int padding);
		void ProjectNextWeek();
		string RoleOut();
		void Save();
		string ScoresOut();
		decimal ScoresPerYear();
		string Seasons();
		string SetColour(string takenColour);
		void SetCurrentGame();
		void SetDraftRound();
		void SetHiWeek(string hiWeekIn);
		void SetLoWeek(string loWeekIn);
		string Status();
		void StoreProjection(int nProjected);
		void TallyPerformance(bool allGames, bool currSeasonOnly, string whichSeason);
		void TallyProjections(bool allGames, bool currSeasonOnly, string whichSeason);
		void TallyScores(string season, int weekNo);
		void TallyStats(string season, int weekNo);
		string TeamRating(string season);
		string ToString();
		string Unit();
		string UnitRating();
		void UpdateActuals(IPlayerGameMetricsDao dao);
		string Url(string text, [Optional] bool forceReport);
		string ValidatePosDesc(string candidatePos);
		int Value();
	}
}