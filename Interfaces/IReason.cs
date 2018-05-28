namespace RosterLib.Interfaces
{
	public interface IReason
	{
		bool IsNotReasonable( string ruleKey, decimal value );
	}
}