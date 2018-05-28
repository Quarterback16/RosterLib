using System;
using System.Xml;

namespace RosterLib.Models
{
	public class YahooOutput
	{
		public string Season { get; set; }
		public string Week { get; set; }
		public string PlayerId { get; set; }
		public decimal Quantity { get; set; }
		public string Opponent { get; set; }

		internal string FormatKey()
		{
			return $"{Season}:{Week}:{PlayerId}";
		}

		public YahooOutput()
		{
		}

		public YahooOutput( 
			string season, 
			string week, 
			string playerId, 
			decimal quantity, 
			string opponent )
		{
			Season = season;
			Week = week;
			PlayerId = playerId;
			Quantity = quantity;
			Opponent = opponent;
		}

		public YahooOutput( XmlNode node )
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
			return $"{Quantity}:{Opponent}";
		}
		public override string ToString()
		{
			return StatOut();
		}
	}
}
