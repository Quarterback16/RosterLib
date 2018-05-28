using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	///   A generic ranking of items.
	/// </summary>
	public class Ladder
	{
		string title;
		string[] participant;
		decimal[] metric;

		public Ladder( string titleIn, int places )
		{
			title = titleIn;
			participant = new string[ places ];
			metric = new decimal[ places ];
			for ( var i = 0; i < places -1; i++ )
			{
				participant[i] = "";
				metric[i] = 0.0M;
			}
		}

		public void Place( string participantIn, int position, decimal metricIn )
		{
         //RosterLib.Utility.Announce( 
         //   string.Format( "   {3} - Placing {0} in position {1} - {2:0.0}", 
         //   participantIn, position, metricIn, title  ) );

			participant[ position - 1 ] = participantIn;
			metric[ position - 1 ] = metricIn;
		}

		public string PosFor( string participantIn )
		{
			var nPos = Array.IndexOf( participant, participantIn ) + 1;
			var pos = nPos.ToString();
			var suffix = "th";
			if ( pos.EndsWith( "1" ) )	suffix = "st";
			if ( pos.EndsWith( "2" ) )	suffix = "nd";
			if ( pos.EndsWith( "3" ) )	suffix = "rd";
			if ( pos == "11" ) suffix = "th";
			return pos + suffix;
		}

		public void Load( DataTable dt, string fieldName, string descriptor )
		{
			RosterLib.Utility.Announce( descriptor );
			DataRowView drv;
			for (int j=0; j < dt.DefaultView.Count; j++)
			{
				drv = dt.DefaultView[ j ];
				Place( drv[ fieldName ].ToString(), j+1, Decimal.Parse(drv[ "metric" ].ToString() ) );
			}
		}

	}
}
