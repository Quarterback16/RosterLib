namespace RosterLib.Interfaces
{
	public interface IGetGamebooks
	{
		int DownloadWeek( NFLWeek week );

		string Seed( NFLWeek week );
	}
}