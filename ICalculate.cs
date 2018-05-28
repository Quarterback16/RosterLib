namespace RosterLib
{
	/// <summary>
	/// Summary description for ICalculatee.
	/// </summary>
	public interface ICalculate
	{
		void Calculate( NflTeam team, NFLGame game );

		int Offset { get; set; }

		NFLWeek StartWeek { get; set; }

		bool AuditTrail {	get; set;	}

		NflTeam Team { get; set; }
	}
}
