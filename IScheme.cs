
namespace RosterLib
{

	/// <summary>
	/// Summary description for IScheme.
	/// </summary>
	public interface IScheme
	{
		NFLBet IsBettable( NFLGame game );

		Confidence ConfidenceLevel();

		//  Test the scheme against known history, returns clip
		decimal BackTest();

		string Name
		{
			get;
			set;
		}

		int M_wins
		{
			get;
			set;
		}

		int Losses
		{
			get;
			set;
		}

		int Pushes
		{
			get;
			set;
		}

	}


}