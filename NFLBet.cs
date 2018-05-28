
using System.Collections;
using System.Text;

namespace RosterLib
{
	/// <summary>
	///   Representation of a wagering play.
	/// </summary>
	public class NFLBet
	{
		private string teamCode;  //  bet on this team
		private NFLGame game;     //  the game
		private readonly ArrayList reasonList;
		private Confidence confidence;
		private readonly double vigorish;
		private BetType betType = BetType.Spread;
		private bool under = false;
		private bool over = false;
		private bool isValid = true;

		public NFLBet( string teamCodeIn, NFLGame gameIn, string reason, Confidence confidenceIn )
		{
			TeamToBetOn = teamCodeIn;
			game = gameIn;
			reasonList = new ArrayList();
			confidence = confidenceIn;
			vigorish = 0.1D;   //  the amount that is raked off the winnings
			AddReason( reason );
		}

		public void Announce()
		{
			Utility.Announce( " Bet is on " + TeamToBetOn + " " + reasonList[ 0 ] );
		}

		public void AddReason( string reason )
		{
			reasonList.Add( reason );
		}

		public string ReasonList()
		{
			var sb = new StringBuilder();
			foreach ( string r in reasonList )
				sb.Append( r + "<br>" );
			return sb.ToString();
		}

		public string RenderasHTMLRow()
		{
			var s = "";
			return s;
		}

		public decimal Handicap()
		{
			return ( IsHome() ) ? 0 - game.Spread : game.Spread;
		}

		public bool IsHome()
		{
			return ( TeamToBetOn == game.HomeTeam );
		}

		public bool IsAway()
		{
			return ( TeamToBetOn == game.AwayTeam );
		}

		public string Opponent()
		{
			if ( TeamToBetOn == game.AwayTeam )
				return game.HomeTeam;
			else
				return game.AwayTeam;
		}

		public double Winnings( double amount )
		{
			double winnings = 0.0D;

			switch (Result())
			{
				case "Win":
					winnings = ( amount * ( 1.0D - vigorish ) ) + amount;
					break;
				case "Loss":
					winnings = 0.0D;
					break;
				case "Push":
					winnings = amount;
					break;
				case " ":
					winnings = 0.0D;
					break;
				default:
					break;
			}
			return winnings;
		}

		public string Result()
		{
			var result = "Push";

			if (Game.Played())
				result = DetermineResult(result);
			else
				result = " ";
			return result;
		}

		public bool Resolved()
		{
			return ( Game.Played() );
		}

		private string DetermineResult(string result)
		{
			if (Type == BetType.Spread)
				result = Game.ResultvsSpread(TeamToBetOn);
			else
				result = DeterminGameResult(result);

			return result;
		}

		private string DeterminGameResult(string result)
		{
			string res = Game.ResultvsTotal();
			if (Over)
				if (res == "Over")
					result = "Win";
				else
				{
					if (res == "Under")
						result = "Loss";
				}
			else
			{
				if (res == "Under")
					result = "Win";
				else
				{
					if (res == "Over")
						result = "Loss";
				}
			}
			return result;
		}

		public Outcome GetOutcome()
		{
			Outcome theOutcome;

			switch ( Game.ResultvsSpread( TeamToBetOn ) )
			{
				case "Win":
					theOutcome = Outcome.Win;
					break;
				case "Loss":
					theOutcome = Outcome.Loss;
					break;
				default:
					theOutcome = Outcome.Push;
					break;
			}
			return theOutcome;
		}
		
		#region  Accessors

		public string TeamToBetOn
		{
			get { return teamCode; }
			set { teamCode = value; }
		}

		public bool Under
		{
			get { return under; }
			set { under = value; }
		}

		public bool Over
		{
			get { return over; }
			set { over = value; }
		}

		public NFLGame Game
		{
			get { return game; }
			set { game = value; }
		}

		public Confidence ConfidenceLevel
		{
			get { return confidence; }
			set { confidence = value; }
		}

		public BetType Type
		{
			get { return betType; }
			set { betType = value; }
		}

		public bool IsValid
		{
			get { return isValid; }
			set { isValid = value; }
		}

		#endregion
	}

	public enum Outcome { Win, Loss, Push }

	public enum BetType { Spread, Total, StraightUp }

}

