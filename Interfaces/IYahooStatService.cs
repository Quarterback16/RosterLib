using RosterLib.Services;
using System.Collections.Generic;

namespace RosterLib.Interfaces
{
   public interface IYahooStatService
   {
      decimal GetStat( string playerId, string season, string week );
      bool IsStat( string playerId, string season, string week );
      IEnumerable<YahooStat> LoadStats( string playerId, string season, string week );
   }
}
