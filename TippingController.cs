using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RosterLib
{
	public class TippingController
	{
		public List<Prediction> Predictions { get; set; }

		public Dictionary<string,WinLossRecord> Tipsters { get; set; }

		public string OutputFilename { get; set; }

		public TippingController()
		{
			Predictions = new List<Prediction>();
			Tipsters = new Dictionary<string, WinLossRecord>();
		}

		public void Index( string season )
		{
			LoadAllMethods(season);
			var tc = new TippingComp( Tipsters );
			tc.Render( season, "SU" );
			OutputFilename = tc.OutputFilename;
		}

		private void LoadAllMethods(string season)
		{
			LoadPredictions(season, "unit");
			//TODO:  Generate these
			//LoadAtsPredictions(season, "hillin");
			//LoadPredictions(season, "nibble");
			//LoadPredictions(season, "Miller");
			LoadPredictions( season, "bookie" );
		}

		public void IndexAts( string season )
		{
			LoadAllMethods(season);
			var tc = new TippingComp( Tipsters );
			OutputFilename = tc.OutputFilename;
			tc.Render( season, "ATS" );
		}

		private void LoadPredictions(string season, string method )
		{
			var ds = Utility.TflWs.GetAllPredictions(season, method );
			var totPredictions = ds.Tables["prediction"].Rows.Count;
			Utility.Announce( string.Format( "{0} predictions loaded for method {1}", totPredictions, method ) );
			if ( totPredictions > 0)
			{
				var winLoss = new WinLossRecord(0, 0, 0);
				foreach ( var result in from DataRow dr in ds.Tables["prediction"].Rows 
							select new Prediction(dr) into prediction select prediction.CheckResult() )
				{
					switch (result)
					{
						case TipResult.Correct:
							winLoss.Wins++;
							break;
						case TipResult.Incorrect:
							winLoss.Losses++;
							break;
						default:
							winLoss.Ties++;
							break;
					}
					winLoss.Total++;
				}
				Tipsters.Add( method, winLoss );	
			}
		}

		private void LoadAtsPredictions( string season, string method )
		{
			var ds = Utility.TflWs.GetAllPredictions( season, method );
			var totPredictions = ds.Tables["prediction"].Rows.Count;
			Utility.Announce( string.Format( "{0} predictions loaded for method {1}", totPredictions, method ) );
			if ( totPredictions > 0)
			{
				var winLoss = new WinLossRecord( 0, 0, 0 );
				foreach ( var result in from DataRow dr in ds.Tables[ "prediction" ].Rows
												select new Prediction( dr ) into prediction
												select prediction.CheckResultAts() )
				{
					switch ( result )
					{
						case TipResult.Correct:
							winLoss.Wins++;
							break;
						case TipResult.Incorrect:
							winLoss.Losses++;
							break;
						default:
							winLoss.Ties++;
							break;
					}
				}
				Tipsters.Add( method, winLoss );
			}
		}

	}
}
