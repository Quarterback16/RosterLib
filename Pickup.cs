using System;

namespace RosterLib
{
	public class Pickup
	{
        public string Season { get; set; }
        public NFLPlayer Player { get; set; }
        public string Name { get; set; }

		public string Opp { get; set; }

		public decimal ProjPts { get; set; }

		public string ActualPts { get; set; }

		public string CategoryCode { get; set; }

		public string  Pos { get; set; }

		public string Category()
		{
			var s = "Unknown";
			switch ( RealCatCode() )
			{
				case "1":
					s = "QUARTERBACKS";
					break;

				case "2":
					s = "RUNNING BACKS";
					break;

				case "3":
					s = "RECEIVERS";
					break;

				case "4":
					s = "TIGHT ENDS";
					break;

				case "5":
					s = "KICKERS";
					break;

				default:
					s = "Not defined";
					break;
			}
			return s;
		}

		public string RealCatCode()
		{
			var catOut = "5";
			switch ( CategoryCode )
			{
				case Constants.K_QUARTERBACK_CAT:
					catOut = "1";
					break;

				case "2":
					catOut = "2";
					break;

				case "3":
					if ( Pos.Contains( "TE" ) )
						catOut = "4";
					else
						catOut = "3";
					break;

			}
			return catOut;
		}

		public decimal SortPoints
		{
			get
			{
				return ( ( 10.0M - Decimal.Parse( RealCatCode() ) ) * 100.0M ) + ProjPts;
			}
		}

		public override string ToString()
		{
			return $"{ProjectionLink(),-36}    {Opp,-10} {ProjPts,5}  {ActualPts}";
		}

        private string ProjectionLink()
        {
            string url = Name.PadRight( 36, ' ' ).Substring( 0, 36 );
            if ( Player.IsPlayerProjection(Season) )
            {
                url = $"<a href =\"..//..//PlayerProjections/{Player.PlayerCode}.htm\">{Name}</a>";
            }
            return url;
        }
    }
}
