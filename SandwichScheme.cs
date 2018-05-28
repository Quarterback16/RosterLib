using System;
using TFLLib;


namespace RosterLib
{
   /// <summary>
   /// Summary description for the Sandwich Scheme.
   /// </summary>
   public class SandwichScheme : IScheme
   {
      protected DataLibrarian tflWS;
      private string schemeName;
		private int wins = 0;
		private int losses = 0;
		private int pushes = 0;
		private NFLGame game;

      public SandwichScheme( DataLibrarian tflWSIn )
      {
         //   
         //  Non Divisional Games sandwiched between
         //  4 Divisional games has the contestant
         //  in a lull
         //
         tflWS = tflWSIn;
         Name = "Sandwich";

      }


      public NFLBet IsBettable( NFLGame game )
      {
         NFLBet bet = null;
			this.game = game;
			if ( Int32.Parse( game.Week ) > 2 )
			{
				if ( ! game.IsDivisionalGame() )
				{
					RosterLib.Utility.Announce( "      Non-Divisional Game");
					if ( BetAgainst( game.HomeNflTeam.TeamCode ) )
						bet = new NFLBet( game.HomeNflTeam.TeamCode, game, Name + " - " + game.Spread, ConfidenceLevel() );
					if ( BetAgainst( game.AwayNflTeam.TeamCode ) )
						bet = new NFLBet( game.AwayNflTeam.TeamCode, game, Name + " - " + game.Spread, ConfidenceLevel() );
				}
				else
				{
					RosterLib.Utility.Announce( "      Divisional Game");
				}
			}

         return bet;
      }

      private bool BetAgainst( string teamCodeIn )
      {
         bool bBet;
         string prev1season = PreviousSeason( game.Season, game.Week );
         string prev1week   = PreviousWeek(   game.Season, game.Week );
         bBet = IsGameDivisional( teamCodeIn, prev1season, prev1week );
         if ( bBet )
         {
            string prev2season = PreviousSeason( prev1season, prev1week );
            string prev2week   = PreviousWeek(   prev1season, prev1week );
            bBet = IsGameDivisional( teamCodeIn, prev2season, prev2week );
				if ( bBet )
				{
					string next1season = NextSeason( game.Season, game.Week );
					string next1week   = NextWeek(   game.Season, game.Week );
					bBet = IsGameDivisional( teamCodeIn, next1season, next1week );

					if ( bBet )
					{
						string next2season = NextSeason( next1season, next1week );
						string next2week   = NextWeek(   next1season, next1week );
						bBet = IsGameDivisional( teamCodeIn, next2season, next2week );
					}
				}
         }
			return bBet;
      }

      private bool IsGameDivisional( string teamCodeIn, string seasonIn, string weekIn )
      {
         var dr = tflWS.GetGame( seasonIn, weekIn, teamCodeIn );
         var g = new NFLGame( dr );
         return g.IsDivisionalGame();
      }

      public Confidence ConfidenceLevel()
      {
         return Confidence.Good;
      }

   	public decimal BackTest()
   	{
			return 0.0M;
   	}

   	private static string NextWeek( string seasonIn, string weekIn )
      {

         int nextWeek = Int32.Parse( weekIn ) + 1;
			if ( Int32.Parse(weekIn) == RosterLib.Constants.K_WEEKS_IN_A_SEASON )
				nextWeek = 1;
         return string.Format( "{0:0#}", nextWeek );
      }

      private static string NextSeason( string seasonIn, string weekIn )
      {

         int nextSeason = Int32.Parse( seasonIn );
         if ( Int32.Parse(weekIn) == RosterLib.Constants.K_WEEKS_IN_A_SEASON )
            nextSeason = nextSeason + 1;
         return string.Format( "{0}", nextSeason );
      }

      private static string PreviousWeek( string seasonIn, string weekIn )
      {
         int prevWeek = Int32.Parse( weekIn ) - 1;
         if ( weekIn == "01" )
            prevWeek = RosterLib.Constants.K_WEEKS_IN_A_SEASON;
         return string.Format( "{0:0#}", prevWeek );
      }

      private static string PreviousSeason( string seasonIn, string weekIn )
      {
         int prevSeason = Int32.Parse( seasonIn );
         if ( weekIn == "01" )
            prevSeason = prevSeason - 1;
         return string.Format( "{0}", prevSeason );
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

