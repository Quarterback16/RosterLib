using System;
using System.Collections.Generic;

namespace RosterLib
{
	public class TippingComp
	{
		public Dictionary<string, WinLossRecord> Tipsters { get; set; }

		public string OutputFilename { get; set; }

		public TippingComp( Dictionary<string, WinLossRecord> tipsters )
		{
			Tipsters = tipsters;
		}

		public void Render( string season, string tipType )
		{
			OutputFilename = string.Format( "{0}{1}//Tipping//TippingComp-{2}.htm", 
				Utility.OutputDirectory(), season, tipType );
			Render();		
		}

		public void Render()
		{
			var str = new SimpleTableReport( string.Format( "Tipping Comp - {0:d}", DateTime.Now ) );
			str.AddStyle( "#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 761px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle( "#main { margin-left:1em; }" );
			str.AddStyle( "#dtStamp { font-size:0.8em; }" );
			str.AddStyle( ".end { clear: both; }" );
			str.AddStyle( ".gponame { color:white; background:black }" );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Method", "METHOD", "{0}", typeof( String ), false ) );
			str.AddColumn( new ReportColumn( "Predictions", "GUESSES", "{0}", typeof( Int32 ), tally: true ) );
			str.AddColumn( new ReportColumn( "Wins", "WINS", "{0}", typeof( Int32 ), tally: true ) );
			str.AddColumn( new ReportColumn( "Losses", "LOSSES", "{0}", typeof( Int32 ), tally:true ) );
			str.AddColumn( new ReportColumn( "Ties", "TIES", "{0}", typeof( Int32 ), tally: true ) );
			str.AddColumn( new ReportColumn( "Clip", "CLIP", "{0:#.##0}", typeof( Decimal ), false ) );
			BuildTable( str );
			str.SetSortOrder( "Clip DESC" );
			str.RenderAsHtml( OutputFilename, true );
		}

		private void BuildTable( SimpleTableReport str )
		{
			foreach (var tipster in Tipsters)
			{
				var dr = str.Body.NewRow();
				dr["METHOD"] = tipster.Key;
				dr[ "GUESSES" ] = tipster.Value.Total;
				dr[ "WINS" ] = tipster.Value.Wins;
				dr[ "LOSSES" ] = tipster.Value.Losses;
				dr[ "TIES" ] = tipster.Value.Ties;
				dr[ "CLIP" ] = tipster.Value.Clip();

				str.Body.Rows.Add( dr );
			}
			return;
		}
	}
}
