using System;
using System.Data;

namespace RosterLib.Models
{
   public class UnitPerformance
	{
		public string TeamCode { get; set; }
		public string UnitCode { get; set; }
		public string Season { get; set; }
		public int WeekNo { get; set; }
		public string Opponent { get; set; }
		public string Leader { get; set; }
		public string OpponentsLeader { get; set; }
		public string UnitRating { get; set; }
		public string OpponentRating { get; set; }
		public int Yards { get; set; }
		public int Touchdowns { get; set; }
		public int Intercepts { get; set; }
		public decimal Sacks { get; set; }

		public UnitPerformance()
		{
		}

		public UnitPerformance( DataRow dr )
		{
			if ( dr != null )
			{
				TeamCode = dr[ "TEAMCODE" ].ToString();
				UnitCode = dr[ "UNIT" ].ToString();
				Season = dr[ "SEASON" ].ToString();
				WeekNo = Int32.Parse( dr[ "WEEK" ].ToString() );
				Opponent = dr[ "OPP" ].ToString();
				Leader = dr[ "LDR" ].ToString();
				OpponentsLeader = dr[ "OPPLDR" ].ToString();
				UnitRating = dr[ "UNITRATE" ].ToString();
				OpponentRating = dr[ "OPPRATE" ].ToString();
				Yards = Int32.Parse( dr[ "YDS" ].ToString() );
				Touchdowns = Int32.Parse( dr[ "TDS" ].ToString() );
				Intercepts = Int32.Parse( dr[ "INTS" ].ToString() );
				Sacks = Decimal.Parse( dr[ "SAKS" ].ToString() );
			}
		}

	}
}
