using System;
using System.Xml;

namespace RosterLib.Models
{
	public class NflStat
	{
		public string Season { get; set; }
		public string Week { get; set; }
		public string TeamCode { get; set; }
		public string StatType { get; set; }
		public decimal Quantity { get; set; }
		public string Opponent { get; set; }

		internal string FormatKey()
		{
			return string.Format( "{0}:{1}:{2}:{3}", Season, Week, TeamCode, StatType  );
		}

		public NflStat()
		{
		}

		public NflStat( string season, string week, string teamCode, string statType, decimal quantity, string opponent )
		{
			Season = season;
			TeamCode = teamCode;
			Week = week;
			StatType = statType;
			Quantity = quantity;
			Opponent = opponent;
		}

      public NflStat( XmlNode node )
      {
      	if (node.Attributes == null) return;

      	Season = node.Attributes[ "season" ].Value;
      	TeamCode = node.Attributes[ "team" ].Value;
      	Week = node.Attributes[ "week" ].Value.Trim();
      	StatType = node.Attributes[ "statType" ].Value;
      	Quantity = Decimal.Parse( node.Attributes[ "qty" ].Value );
			Opponent = node.Attributes[ "opp" ].Value;
      }

		public string StatOut()
		{
			return string.Format( "{0}:{1}", Quantity, Opponent );
		}
	}
}
