using System;
using System.Data;
using TFLLib;


namespace RosterLib
{
   /// <summary>
   /// Summary description for the Home Dog Scheme.
   /// </summary>
   public class HomeDogScheme : IScheme
   {
      protected DataLibrarian tflWS;
      private string schemeName;
		private int wins = 0;
		private int losses = 0;
		private int pushes = 0;

      public HomeDogScheme( DataLibrarian tflWSIn ) 
      {
         //   
         //  Root for the home team when they are under dogs
         //
         tflWS = tflWSIn;
         Name = "Home Dog";
      }


      public NFLBet IsBettable( NFLGame game )
      {
         NFLBet bet = null;

         if ( HomeDog( game ) )
            bet = new NFLBet( game.Dog(), game, Name + " - " + game.Spread, ConfidenceLevel() );

         return bet;
      }

		private static bool HomeDog( NFLGame game )
		{
			if ( Int32.Parse(game.Week) == RosterLib.Constants.K_WEEKS_IN_A_SEASON )
				//Superbowl
				return false;
			else
			{
				if ( game.Dog() == game.HomeTeam )
					return true;
				else
					return false;
			}
		}

		
      public Confidence ConfidenceLevel()
      {
         return Confidence.Good;
      }

   	public decimal BackTest()
   	{
			//  for each instance that has a line
 
#if DEBUG
			DataSet ds = tflWS.GetGames( 2005, 13 );
#else
			DataSet ds = tflWS.GetAllGames();
#endif

			DataTable dt = ds.Tables["sched"];
			foreach (DataRow dr in dt.Rows)
			{
				NFLGame game = new NFLGame(dr);
				if ( HomeDog( game ) )
				{
					switch ( game.ResultvsSpread( game.HomeTeam ) )
					{
						case "Win":
							M_wins++;
							break;
						case "Loss":
							Losses++;
							break;
						case "Push":
							Pushes++;
							break;
					}
				}
			}
			return Utility.Clip( M_wins, Losses, Pushes );
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

