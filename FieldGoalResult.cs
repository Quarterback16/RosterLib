
namespace RosterLib
{
	/// <summary>
	/// A class encapsulating the output of a kicker in a particular game.
	/// </summary>
	public class FieldGoalResult
	{
		private readonly string gameCode;
      private readonly bool bLastYear;

		public FieldGoalResult( string gameCodeIn, SeasonOpposition soIn )
		{
			FgCount = soIn.Metric;
         gameCode = gameCodeIn;
         Opponent = soIn.Opponent;
         So = soIn;
         bLastYear =  ( Season() == "2004" ) ? true : false;
		}

      public string Season()
      {
         return gameCode.Substring( 0, 4 );
      }

      public string Week()
      {
         return gameCode.Substring( 4, 2 );
      }

      #region  Accessors

		public string Opponent { get; set; }

		public bool IsLastYear()
      {
         return bLastYear;
      }

		public int FgCount { get; set; }

		public SeasonOpposition So { get; set; }

		#endregion

	}
}
