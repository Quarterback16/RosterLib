using System;
using System.Data;
using TFLLib;

namespace RosterLib
{
	/// <summary>
	/// Summary description for Nibble Predictor Lock Scheme.
	/// Has to work after the fact ie independant of time
	/// </summary>
	public class NibbleLockScheme : IScheme
	{
		protected DataLibrarian TflWs;
		readonly NibblePredictor _pred;

		#region  Accessors

		public string Name { get; set; }

		public int M_wins { get; set; }

		public int Losses { get; set; }

		public int Pushes { get; set; }

		#endregion

		public NibbleLockScheme( DataLibrarian tflWsIn ) 
		{
			Pushes = 0;
			Losses = 0;
			//  we have a bet if teams have played in the last year and have a revenge motive
			TflWs = tflWsIn;
			Name = "Nibble LOCK";
			_pred = new NibblePredictor();
		}

		public NFLBet IsBettable( NFLGame game )
		{
			NFLBet bet = null;
			if ( Decimal.Compare( game.Spread, 0M ) != 0 )
			{
				//  Predict game
				var res = _pred.PredictGame( game, new FakePredictionStorer(), DateTime.Now);
				//  if differs from spread by 10 or more its a lock
				var diff = Math.Abs( game.Spread - res.Margin() );
				if ( Decimal.Compare( diff, 10 ) > 0 )
				{
					bet = new NFLBet( res.WinningTeam(), game, Name + " - " + diff + " " +
						res.PredictedScore(), ConfidenceLevel() );
					bet.Announce();
				}
			}  //  else its off the board
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
			var ds = TflWs.GetGames( 2005, 13 );
#else
			DataSet ds = TflWs.GetAllGames();
#endif

			var dt = ds.Tables["sched"];
			foreach (DataRow dr in dt.Rows)
			{
				var game = new NFLGame(dr);
				//  TODO:  cant do this for past games as the results are already built into the current ratings

				var bet = IsBettable( game );

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
			//return DataLibrarian.Clip( M_wins, Losses, Pushes );
			return 0.0M;
		}

	}
}

