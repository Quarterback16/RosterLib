using RosterLib.Models;

namespace RosterLib
{
   public class DbfUnitPerformanceRepository : IUnitPerformanceRepository
	{
		public bool Add( UnitPerformance unitPerformance )
		{
			Utility.TflWs.InsertUnitPerformance(
 				unitPerformance.TeamCode,
				unitPerformance.UnitCode,
				unitPerformance.Season,
				unitPerformance.WeekNo,
				unitPerformance.Opponent,
				unitPerformance.Leader,
				unitPerformance.OpponentsLeader,
				unitPerformance.UnitRating,
				unitPerformance.OpponentRating,
				unitPerformance.Yards,
				unitPerformance.Touchdowns,
				unitPerformance.Intercepts,
				unitPerformance.Sacks
				);
			return true;
		}

		public bool Delete( UnitPerformance unitPerformance )
		{
			if ( unitPerformance != null )
			{
				Utility.TflWs.DeleteUnitPerformance(
					unitPerformance.TeamCode,
					unitPerformance.Season,
					unitPerformance.WeekNo,
					unitPerformance.UnitCode
				);
				return true;
			}
			return false;
		}

		public UnitPerformance GetByKey( string teamCode, string season, int week, string unitCode )
		{
			var dr = Utility.TflWs.GetUnitPerformance(teamCode, season, week, unitCode );
			var up = new UnitPerformance( dr );			
			return up;
		}
	}
}
