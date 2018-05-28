using System;
using System.Xml;

namespace RosterLib
{
	public class GridStatsOutput
	{
		public string Season { get; set; }
		public string Week { get; set; }
		public string PlayerId { get; set; }
		public decimal Quantity { get; set; }
		public string Opponent { get; set; }

		internal string FormatKey()
		{
			return string.Format( "{0}:{1}:{2}", Season, Week, PlayerId );
		}

		public GridStatsOutput()
		{
		}

		public GridStatsOutput( string season, string week, string playerId, decimal quantity, string opponent )
		{
			Season = season;
			Week = week;
			PlayerId = playerId;
			Quantity = quantity;
			Opponent = opponent;
		}

		public GridStatsOutput( XmlNode node )
		{
			if ( node.Attributes == null ) return;

			Season = node.Attributes[ "season" ].Value;
			Week = node.Attributes[ "week" ].Value.Trim();
			PlayerId = node.Attributes[ "id" ].Value;
			Quantity = Decimal.Parse( node.Attributes[ "qty" ].Value );
			Opponent = node.Attributes[ "opp" ].Value;
		}

		public string StatOut()
		{
			return string.Format( "{0}:{1}", Quantity, Opponent );
		}
	}
}
