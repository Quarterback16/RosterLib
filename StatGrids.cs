using RosterLib.Interfaces;
using System.Collections.Generic;

namespace RosterLib
{
	public class StatGrids : RosterGridReport
	{
		public List<StatGridConfig> Configs { get; set; }

		public StatGrid StatGrid { get; set; }

		public StatGrids( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Stat Grids";
			Configure();
		}

		private void Configure()
		{
			Configs = new List<StatGridConfig>
			{
			   new StatGridConfig {Season = Season, StatType = "FG"},
			   new StatGridConfig {Season = Season, StatType = "TDr"},
			   new StatGridConfig {Season = Season, StatType = "TDp"},
			   new StatGridConfig {Season = Season, StatType = "YDr"},
			   new StatGridConfig {Season = Season, StatType = "YDp"},
			   new StatGridConfig {Season = Season, StatType = "Sacks"},
			   new StatGridConfig {Season = Season, StatType = "SacksAllowed"},
			   new StatGridConfig {Season = Season, StatType = "YDrAllowed"},
			   new StatGridConfig {Season = Season, StatType = "YDpAllowed"},
			   new StatGridConfig {Season = Season, StatType = "TDpAllowed"},
			   new StatGridConfig {Season = Season, StatType = "TDrAllowed"},
			   new StatGridConfig {Season = Season, StatType = "INTs"},
			   new StatGridConfig {Season = Season, StatType = "INTsThrown"}
			};
		}

		public override void RenderAsHtml()
		{
			StatGrid = new StatGrid( Season );
			//  Force a reCalc
			StatGrid.ReGenAll();
			foreach ( var rpt in Configs )
				GenerateReport( rpt );
			DumpXml();
		}

		//  for testing
		public void DumpXml()
		{
			StatGrid.DumpXml();
		}

		public override string OutputFilename()
		{
			return StatGrid.FileName();
		}

		private void GenerateReport( StatGridConfig rpt )
		{
			StatGrid.StatInFocus = rpt.StatType;
			StatGrid.Render();
		}
	}

	public class StatGridConfig
	{
		public string Season { get; set; }

		public string StatType { get; set; }
	}
}