using RosterLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RosterLib.RosterGridReports
{
	public class PickupSummary : RosterGridReport
	{
		public string Week { get; private set; }
		public SimplePreReport Report { get; private set; }

		public List<Pickup> Pickups { get; set; }

		public PickupSummary( IKeepTheTime timekeeper, int week ) : base( timekeeper )
		{
			Name = "Pickup Summary";
			Season = timekeeper.Season;
			Week = week.ToString();
			Report = new SimplePreReport
			{
				ReportType = "Pickup Summary",
				Folder = "Projections",
				Season = Season,
				InstanceName = $"Pickup-Summary-Week-{Week:0#}"
			};
			Pickups = new List<Pickup>();
		}

		public override void RenderAsHtml()
		{
			Report.Body = GenerateBody();
			Report.RenderHtml();
			FileOut = Report.FileOut;
			Pickups.Clear();
		}

		private string GenerateBody()
		{
			var simple = new StringBuilder();
			var compareByProjPts = new Comparison<Pickup>( CompareByProjPts );
			Pickups.Sort( compareByProjPts );
			var lastCategory = "X";
			foreach ( Pickup pickup in Pickups )
			{
				if ( pickup.RealCatCode() != lastCategory)
				{
					simple.AppendLine( string.Empty );
					simple.AppendLine( pickup.Category() );
					simple.AppendLine( string.Empty );
					lastCategory = pickup.RealCatCode();
				}
				simple.AppendLine($"   {pickup.ToString()}");
			}
			return simple.ToString();
		}

		private static int CompareByProjPts( Pickup x, Pickup y)
		{
			if ( x == null )
			{
				if ( y == null )
					return 0;
			}
			return y == null ? 1 : y.SortPoints.CompareTo( x.SortPoints );
		}

		public void AddPickup( Pickup pickup )
		{
			Pickups.Add( pickup );
		}
	}
}
