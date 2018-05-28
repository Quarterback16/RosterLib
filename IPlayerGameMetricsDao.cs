using System.Collections.Generic;

namespace RosterLib
{
   public interface IPlayerGameMetricsDao
   {
      /// <summary>
      ///   Get a projection for a player for a game
      /// </summary>
      /// <param name="playerCode"></param>
      /// <param name="gameCode">SSSS:WW-X</param>
      /// <returns></returns>
      PlayerGameMetrics Get( string playerCode, string gameCode );

      List<PlayerGameMetrics> GetGame( string gameCode );

      List<PlayerGameMetrics> GetWeek( string season, string week );

      List<PlayerGameMetrics> GetSeason( string season, string playerCode );

      void Save( PlayerGameMetrics pgm );

		void SaveActuals(PlayerGameMetrics pgm, decimal fpts);

      void ClearGame( string gameKey );

      PlayerGameMetrics GetPlayerWeek( string gameCode, string playerCode );

      decimal ProjectedStatsOfType( string forStatType, PlayerGameMetrics pgm );
   }
}