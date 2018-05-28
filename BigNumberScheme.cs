using System;
using System.Data;

namespace RosterLib
{
   /// <summary>
   /// Summary description for Big Number Scheme.
   /// </summary>
   public class BigNumberScheme : IScheme
   {
   	public BigNumberScheme()
      {
         //   
         //  Go the dog if they get more than 2 TDs start
         //
         Name = "Big Number";
      }

      private static bool BigNumber(decimal spreadIn)
      {
         bool isBig = false;

         if ( spreadIn > 14.0M )
            isBig = true;
         else if ( spreadIn < -14.0M )
            isBig = true;

         return isBig;
      }

      public NFLBet IsBettable(NFLGame game)
      {
         if (game == null) throw (new ArgumentNullException("game", "parameter is null"));

         NFLBet bet = null;

         if ( BigNumber( game.Spread ) )
            bet = new NFLBet( game.Dog(), game, Name + " - " + game.Spread, ConfidenceLevel() );


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
         DataSet ds = Utility.TflWs.GetGames(2005, 13);
#else
         DataSet ds = Utility.TflWs.GetAllGames();
#endif

         DataTable dt = ds.Tables["sched"];
         foreach (DataRow dr in dt.Rows)
         {
            NFLGame game = new NFLGame(dr);
            NFLBet bet = IsBettable(game);

            if (bet != null)
            {
               switch (bet.Result())
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
         return Utility.Clip(M_wins, Losses, Pushes);
      }


      #region  Accessors

   	public string Name { get; set; }

   	public int M_wins { get; set; }

   	public int Losses { get; set; }

   	public int Pushes { get; set; }

   	#endregion
   }
}

