namespace RosterLib
{
	public class UnitFactory
	{
		public TeamUnit CreateUnit( string unitCode )
		{
		   TeamUnit tu;

		   switch ( unitCode )
		   {
		      case "PO":
		         tu = new PassOffenceUnit();
		         break;

		      case "RO":
		         tu = new RunOffenceUnit();
		         break;

		      case "PP":
		         tu = new ProtectionUnit();
		         break;

		      case "PR":
		         tu = new PassRushUnit();
		         break;

		      case "RD":
		         tu = new RunDefenceUnit();
		         break;

		      case "PD":
		         tu = new PassDefenceUnit();
		         break;

		      case "FG":
		         tu = new FGDefenceUnit();
		         break;

		      default:
		         tu = new TeamUnit();
		         break;
		   }
		   return tu;
		}
	}


}
