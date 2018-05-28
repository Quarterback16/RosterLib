
using System.Collections;

namespace RosterLib
{
	/// <summary>
	/// Summary description for TeamUnit.
	/// </summary>
	public class TeamUnit
	{
		private string unitCode;
		private string unitName;
		private ArrayList positionList;

		public NflTeam Team { get; set; }

		public TeamUnit()
		{
		}

		public TeamUnit( string code, string name )
		{
			UnitCode = code;
			UnitName = name;
			PositionList = new ArrayList();
		}

		public void AddPosition( string posCode )
		{
			PositionList.Add( posCode );
		}
		
		#region  Accessors
		
		public string UnitCode
		{
			get { return unitCode; }
			set { unitCode = value; }
		}

		public string UnitName
		{
			get { return unitName; }
			set { unitName = value; }
		}

		public ArrayList PositionList
		{
			get { return positionList; }
			set { positionList = value; }
		}
	
		#endregion
		
		public virtual decimal GetStat( NFLWeek week )
		{
			return 0.0M;
		}

		public virtual string SortDirection()
		{
			return "DESC";
		}

		public virtual string BGPicker( int theValue )
		{
			return "LIGHTGREY";
		}

		public virtual string Rating()
		{
			return "X";
		}

	}
}
