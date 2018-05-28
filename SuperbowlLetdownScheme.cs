using TFLLib;


namespace RosterLib
{
	/// <summary>
	/// Summary description for SuperbowlLetdownScheme.
	/// </summary>
	public class SuperbowlLetdownScheme : IScheme
	{
      protected DataLibrarian tflWS;
      private string schemeName;
		private int wins = 0;
		private int losses = 0;
		private int pushes = 0;

		public SuperbowlLetdownScheme( DataLibrarian tflWSIn ) 
		{
			//  we have a bet if either of the teams are the superbowl winner and the week is one or 2
         tflWS = tflWSIn;
         Name = "SuperbowlLetdown";
		}

      public NFLBet IsBettable( NFLGame game )
      {
         NFLBet bet = null;

         if ( ( game.Week == "01" ) || ( game.Week == "02" ) )
         {
            if ( IsSuperbowlWinner( game.AwayTeam, game.Season ) )
               //  bet against
               bet = new NFLBet( game.HomeTeam, game, Name, ConfidenceLevel() );
            else
               if ( IsSuperbowlWinner( game.HomeTeam, game.Season ) )
                  bet = new NFLBet( game.AwayTeam, game, Name, ConfidenceLevel() );
         }
         return bet;
      }

      public Confidence ConfidenceLevel()
      {
         return Confidence.High;
      }

		public decimal BackTest()
		{
			return 0.0M;

		}

		private bool IsSuperbowlWinner( string teamCode, string season )
      {
         if ( teamCode == tflWS.GetSuperbowlWinner( season ) )
            return true;
         else
            return false;
      }

      #region  Accessors

      public string Name
      {
         get { return schemeName; }
         set { schemeName = value; }
      }

		public int M_wins
		{
			get { return wins; }
			set { wins = value; }
		}

		public int Losses
		{
			get { return losses; }
			set { losses = value; }
		}

		public int Pushes
		{
			get { return pushes; }
			set { pushes = value; }
		}


		#endregion
	}
}
