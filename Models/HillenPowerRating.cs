using System;
using System.Xml;

namespace RosterLib.Models
{
	public class HillenPowerRating
	{
		public string Season { get; set; }
		public string Week { get; set; }
		public string TeamCode { get; set; }
		public decimal Quantity { get; set; }

		internal string FormatKey()
		{
			return string.Format( "{0}:{1}:{2}", Season, Week, TeamCode );
		}

		public HillenPowerRating()
		{
		}

		public HillenPowerRating( string season, string week, string teamCode, decimal quantity )
		{
			Season = season;
			Week = week;
			TeamCode = teamCode;
			Quantity = quantity;
		}

		public HillenPowerRating( XmlNode node )
		{
			if ( node.Attributes == null ) return;

			Season = node.Attributes[ "season" ].Value;
			Week = node.Attributes[ "week" ].Value.Trim();
			TeamCode = node.Attributes[ "team" ].Value;
			Quantity = Decimal.Parse( node.Attributes[ "qty" ].Value );
		}

		public string StatOut()
		{
			return string.Format( "{0}", Quantity );
		}
	}
}
