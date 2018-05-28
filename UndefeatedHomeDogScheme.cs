using TFLLib;

namespace RosterLib
{
   /// <summary>
   /// Summary description for the Home Dog Scheme.
   /// </summary>
   public class UndefeatedHomeDogScheme : IScheme
   {
      protected DataLibrarian tflWS;
      private string schemeName;
		private int wins = 0;
		private int losses = 0;
		private int pushes = 0;

      public UndefeatedHomeDogScheme( DataLibrarian tflWSIn ) 
      {
         //   
         //  An Undefeated home dog is bound to lose
         //
         tflWS = tflWSIn;
         Name = "Undefeated Home Dog";
      }


      public NFLBet IsBettable( NFLGame game )
      {
         NFLBet bet = null;

         if ( game.Dog() == game.HomeTeam )
         {
            //  Have they lost yet

            bet = new NFLBet( game.Dog(), game, Name + " - " + game.Spread, ConfidenceLevel() );
         }

         return bet;
      }

      public Confidence ConfidenceLevel()
      {
         return Confidence.Good;
      }

   	public decimal BackTest()
   	{
			return 0.0M;

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

