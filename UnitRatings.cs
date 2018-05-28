using System;

namespace RosterLib
{
	public class UnitRatings
	{
		public string PassOffence { get; set; }
		public string RushOffence { get; set; }
		public string PassProtection { get; set; }
		public string PassRush { get; set; }
		public string RunDefence { get; set; }
		public string PassDefence { get; set; }

		public enum UnitRating
		{
			Po = 0,
			Ro = 1,
			Pp = 2,
			Pr = 3,
			Rd = 4,
			Pd = 5
		}

		public UnitRatings()
		{
			PassOffence = "C";
			RushOffence = "C";
			PassProtection = "C";
			PassRush = "C";
			RunDefence = "C";
			PassDefence = "C";
		}

		public UnitRatings( string ratings )
		{
			PassOffence = ratings.Substring(0,1);
			RushOffence = ratings.Substring( 1, 1 );
			PassProtection = ratings.Substring( 2, 1 );
			PassRush = ratings.Substring( 3, 1 );
			RunDefence = ratings.Substring( 4, 1 );
			PassDefence = ratings.Substring( 5, 1 );
		}

		public string RatingFor( UnitRating ur)
		{
			if ( ur.Equals( UnitRating.Po ) )
				return PassOffence;
			if ( ur.Equals( UnitRating.Ro ) )
				return RushOffence;
			if ( ur.Equals( UnitRating.Pp ) )
				return PassProtection;
			if ( ur.Equals( UnitRating.Pr ) )
				return PassRush;
			if ( ur.Equals( UnitRating.Rd ) )
				return RunDefence;
			if ( ur.Equals( UnitRating.Pd ) )
				return PassDefence;
			return "?";
		}

		public override string ToString()
		{
			return String.Format("{0}{1}{2}{3}{4}{5}", 
				PassOffence, RushOffence, PassProtection, PassRush, RunDefence, PassDefence);
		}
	}
}
