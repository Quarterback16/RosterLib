namespace RosterLib
{
	public interface ICache
	{
		int CacheHits { get; set; }

		int CacheMisses { get; set; }

		decimal GetStat( string theKey );

		string StatsMessage();
	}
}