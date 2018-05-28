namespace RosterLib
{
	public class NflScore
	{
		public string When { get; set; }
		public string TypeCode { get; set; }
		public int Distance { get; set; }

		public NflScore( string when, string typeCode, int dist )
		{
			When = when;
			TypeCode = typeCode;
			Distance = dist;
		}

		public int Points()
		{
			return TypeCode.Equals( Constants.K_SCORE_SAFETY ) ? 2 : 6;
		}
	}
}
