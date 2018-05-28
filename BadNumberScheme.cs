using System;
using System.Data;
using TFLLib;


namespace RosterLib
{
   /// <summary>
   /// Summary description for Nibble Predictor Lock Scheme.
   /// </summary>
   public class BadNumberScheme : IScheme
   {
      private readonly DataLibrarian _tflWs;

   	public BadNumberScheme( DataLibrarian tflWsIn ) 
      {
         //   
         //  Bad numbers are numbers that show a statistical trend of 60% or more
         //
         //   eg Home favourites by 9.5 point fail to cover > 60% of the time
         //
         _tflWs = tflWsIn;
         Name = "Bad Number";
      }

      private static bool BadNumber( decimal spreadIn )
      {
         bool isBad = ( Decimal.Compare( spreadIn, -2M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn, -3.5M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn,   -4M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn,   -7M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn,    5M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn,  5.5M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn,  6.5M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn,  9.5M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn,   10M ) == 0 ) ? true : false;
         if ( ! isBad ) isBad = ( Decimal.Compare( spreadIn, 13.5M )  > 0 ) ? true : false;

         return isBad;
      }

      public NFLBet IsBettable( NFLGame game )
      {
         NFLBet bet = null;

         if (game == null) throw (new ArgumentNullException("game", "parameter is null"));

      	if ( BadNumber( game.Spread ) )

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
			DataSet ds = _tflWs.GetGames( 2005, 13 );
#else
			DataSet ds = _tflWs.GetAllGames();
#endif

			DataTable dt = ds.Tables["sched"];
			foreach (DataRow dr in dt.Rows)
			{
				NFLGame game = new NFLGame(dr);
				NFLBet bet = IsBettable( game );

				if ( bet == null )
				{}
				else
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

   	public string Name { get; set; }

   	public int M_wins { get; set; }

   	public int Losses { get; set; }

   	public int Pushes { get; set; }

   	#endregion
   }
}

