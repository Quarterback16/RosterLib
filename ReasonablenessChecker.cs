using RosterLib.Interfaces;
using System.Collections.Generic;

namespace RosterLib
{
	public class ReasonablenessChecker : IReason
	{
		private Dictionary<string, HiLoRange> Rules;
		public ReasonablenessChecker()
		{
			Rules = new Dictionary<string, HiLoRange>();
			AddRules();
		}

		public bool IsNotReasonable( string ruleKey, decimal value )
		{
			if ( Rules.ContainsKey( ruleKey ) )
			{
				var rule = Rules[ ruleKey ];
				if ( value > rule.HiValue || value < rule.LoValue )
					return true;
			}
			return false;
		}

		private void AddRules()
		{
			//  Initally hard code the rules
			AddRule( Constants.K_CHECK_TOTAL_SACKS, 8, 80);
			// team sacks in a game
			AddRule( Constants.K_STATCODE_SACK, 0, 11 );
		}

		private void AddRule(string ruleOn, decimal loVal, decimal hiVal)
		{
			var hiLo = new HiLoRange
			{
				HiValue = hiVal,
				LoValue = loVal
			};
			Rules.Add( ruleOn, hiLo );
		}
	}

	public class HiLoRange
	{
		public decimal HiValue { get; set; }
		public decimal LoValue { get; set; }
	}

}
