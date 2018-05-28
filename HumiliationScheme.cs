using System;
using System.Data;
using TFLLib;

namespace RosterLib
{
   /// <summary>
   /// Summary description for Revenge Scheme.
   /// </summary>
   public class HumiliationScheme : IScheme
   {
      protected DataLibrarian tflWS;
      private string schemeName;
		private int wins = 0;
		private int losses = 0;
		private int pushes = 0;

      public HumiliationScheme( DataLibrarian tflWSIn ) 
      {
         //  we have a bet if teams have played in the last year and have a revenge motive
         tflWS = tflWSIn;
         Name = "Humiliation";
      }

      public NFLBet IsBettable( NFLGame game )
      {
         NFLBet bet = null;
         string revengeTeam = "";
         string revengeGame = "";
         TimeSpan aSpan = new TimeSpan( 730, 0, 0, 0 );

         DataSet ds = tflWS.GetGamesBetween( game.HomeTeam, game.AwayTeam, DateTime.Now.Subtract( aSpan ) );
         DataTable dt = ds.Tables["SCHED"];
         dt.DefaultView.Sort = "GAMEDATE ASC";
         foreach (DataRow dr in dt.Rows)
         {
            if ( dr.RowState != DataRowState.Deleted )
            {
               NFLGame aGame = new NFLGame( dr );
               if ( aGame.WasRout() )
               {
                  revengeTeam = ( aGame.HomeWin() ) ? aGame.AwayTeam : aGame.HomeTeam;
                  revengeGame = aGame.ScoreOut( revengeTeam ) + " " + aGame.GameCodeOut();
               }
            }
         }
         
         if ( revengeTeam.Length > 0 )
            bet = new NFLBet( revengeTeam, game, Name + " - " + revengeGame, ConfidenceLevel() );

         return bet;
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
				NFLBet bet = IsBettable( game );

				if ( bet != null )
				{
					switch ( bet.Result() )
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

