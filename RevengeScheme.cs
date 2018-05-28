using System;
using System.Data;
using TFLLib;


namespace RosterLib
{
   /// <summary>
   /// Summary description for Revenge Scheme.
   /// </summary>
   public class RevengeScheme : IScheme
   {
      protected DataLibrarian tflWS;
      private string m_SchemeName;
		private int m_Wins = 0;
		private int losses = 0;
		private int pushes = 0;

      public RevengeScheme( DataLibrarian tflWSIn ) 
      {
         //  we have a bet if teams have played in the last year and have a revenge motive
         tflWS = tflWSIn;
         Name = "Revenge";
      }

      public NFLBet IsBettable( NFLGame game )
      {
         NFLBet bet = null;
         string revengeTeam = "";
         string revengeGame = "";
         TimeSpan aYear = new TimeSpan( 365, 0, 0, 0 );

         DataSet ds = tflWS.GetGamesBetween( game.HomeTeam, game.AwayTeam, DateTime.Now.Subtract( aYear ) );
         DataTable dt = ds.Tables["SCHED"];
         dt.DefaultView.Sort = "GAMEDATE ASC";
         foreach (DataRow dr in dt.Rows)
         {
            if ( dr.RowState != DataRowState.Deleted )
            {
               NFLGame aGame = new NFLGame( dr );
               revengeTeam = ( aGame.HomeWin() ) ? aGame.AwayTeam : aGame.HomeTeam;
               revengeGame = aGame.ScoreOut( revengeTeam ) + " " + aGame.GameCodeOut();
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
			return 0.0M;

   	}

   	#region  Accessors

      public string Name
      {
         get { return m_SchemeName; }
         set { m_SchemeName = value; }
      }

		public int M_wins
		{
			get { return m_Wins; }
			set { m_Wins = value; }
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

