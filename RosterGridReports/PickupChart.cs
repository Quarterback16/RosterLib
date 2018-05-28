using RosterLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterLib.RosterGridReports
{
	public class PickupChart : RosterGridReport
	{
		public int Week { get; set; }

		public SimplePreReport Report { get; set; }

		public PickupSummary PickupSummary { get; set; }
        public bool PlayerReports { get; set; }

        public PickupChart( 
            IKeepTheTime timekeeper, 
            int week,
            bool playerReports = false ) : base( timekeeper )
		{
			Name = "Pickup Chart";
			Season = timekeeper.CurrentSeason( DateTime.Now );
			Week = week;
			Report = new SimplePreReport
			{
				ReportType = "Pickup Chart",
				Folder = "Projections",
				Season = Season,
				InstanceName = $"Pickup-Chart-Week-{Week:0#}"
			};
			PickupSummary = new PickupSummary(timekeeper, week);
            PlayerReports = playerReports;
		}

		public override void RenderAsHtml()
		{
			Report.Body = GenerateBody();
			Report.RenderHtml();
			FileOut = Report.FileOut;
			PickupSummary.RenderAsHtml();
		}

		public string GenerateBody()
		{
			var bodyOut = new StringBuilder();

			var winners = GetWinners();
			var losers = GetLosers();

			var winnersList = winners.ToList().OrderByDescending( x => x.Margin );
			var losersList = losers.ToList().OrderBy( x => x.Margin );

			var c = new YahooCalculator();
			var lineNo = 0;

			foreach ( var winner in winnersList )
				lineNo = GenerateChart( bodyOut, c, lineNo, winner );

			foreach ( var loser in losersList )
				lineNo = GenerateChart( bodyOut, c, lineNo, loser );

			return bodyOut.ToString();
		}

		public int GenerateChart(
		   StringBuilder bodyOut, YahooCalculator c, int lineNo, IWinOrLose team )
		{
			team.Team.LoadKickUnit();
			team.Team.LoadRushUnit();
			team.Team.LoadPassUnit();
			var qb = GetQBBit( team, c );
			var rb = GetRunnerBit( team, c );
			var gameBit = GameBit( team );
			var timeBit = TimeBit( team );

			lineNo++;
			bodyOut.Append( $"{lineNo,2} {timeBit} {gameBit}" );
			bodyOut.Append( $" {qb}" );
			bodyOut.Append( $" {rb}" );
			//    spit out the WR1 line
			var wr1 = GetW1Bit( team, c );
			bodyOut.Append( string.Format( " {0}", wr1 ) );
			//    spit out the WR2 line
			var wr2 = GetW2Bit( team, c );
			bodyOut.Append( string.Format( " {0}", wr2 ) );
			//    spit out the TE line
			var te = GetTEBit( team, c );
			bodyOut.Append( string.Format( " {0}", te ) );
			//    spit out the PK line
			var pk = GetPKBit( team, c );
			bodyOut.AppendLine( string.Format( " {0}", pk ) );

			return lineNo;
		}

		#region Bits and Pieces

		private static string TimeBit( IWinOrLose team )
		{
			var dayName = team.Game.GameDate.ToString( "dddd" ).Substring( 0, 2 );
			var bit = string.Format( "{0}{1}", dayName, team.Game.Hour );
			return bit;
		}

		private static string GameBit( IWinOrLose team )
		{
			team.Game.CalculateSpreadResult();
			var predictedResult = team.IsWinner
			   ? team.Game.BookieTip.PredictedScore()
			   : team.Game.BookieTip.PredictedScoreFlipped();
			var theLine = team.Game.TheLine( team.Team.TeamCode );
			var url = team.Game.GameProjectionUrl();
			return $"<a href='{url}'>{predictedResult}</a> {theLine,3}";
		}

		private string GetW1Bit( IWinOrLose team, YahooCalculator c )
		{
			var bit = NoneBit( team );

			if ( team.Team.PassUnit.W1 != null )
			{
				bit = PlayerPiece( team.Team.PassUnit.W1, team.Game, c );
			}
			return string.Format( "{0,-36}", bit );
		}

		private string NoneBit( IWinOrLose team )
		{
			var bit = $" <a href='..\\Roles\\{team.Team.TeamCode}-Roles-{Week - 1:0#}.htm'>none</a>                            ";
			return bit;
		}

		public string GetW2Bit( IWinOrLose team, YahooCalculator c )
		{
			var bit = NoneBit( team );
			if ( team.Team.PassUnit.W2 != null )
			{
				bit = PlayerPiece( team.Team.PassUnit.W2, team.Game, c );
			}
			return $"{bit,-36}";
		}

		private string GetTEBit( IWinOrLose team, YahooCalculator c )
		{
			var bit = NoneBit( team );

			if ( team.Team.PassUnit.TE != null )
			{
				bit = PlayerPiece( team.Team.PassUnit.TE, team.Game, c );
			}
			return $"{bit,-36}";
		}

		private string GetPKBit( IWinOrLose team, YahooCalculator c )
		{
			var bit = NoneBit( team );
			if ( team.Team.KickUnit.PlaceKicker != null )
			{
				bit = PlayerPiece( 
                    team.Team.KickUnit.PlaceKicker, 
                    team.Game, 
                    c );
			}
			return $"{bit,-36}";
		}

		private string GetQBBit( IWinOrLose team, YahooCalculator c )
		{
			var bit = NoneBit( team );

			if ( team.Team.PassUnit.Q1 != null )
				bit = PlayerPiece( team.Team.PassUnit.Q1, team.Game, c );
			return $"{bit,-36}";
		}

		public string GetRunnerBit( IWinOrLose team, YahooCalculator c )
		{
			var bit = NoneBit( team );
			if ( team.Team.PassUnit.Q1 != null )
			{
				// get the next opponent by using the QB
				var nextOppTeam = team.Team.PassUnit.Q1.NextOpponentTeam( team.Game );

				if ( team.Team.RunUnit == null )
					team.Team.LoadRushUnit();
				else
					Logger.Trace( "   >>> Rush unit loaded {0} rushers; Ace back {1}",
					   team.Team.RunUnit.Runners.Count(), team.Team.RunUnit.AceBack );

				if ( team.Team.RunUnit.AceBack != null )
					bit = PlayerPiece( team.Team.RunUnit.AceBack, team.Game, c );
				else
				{
					var dualBacks = team.Team.RunUnit.Committee;
					var combinedPts = 0.0M;
					foreach ( NFLPlayer runner in team.Team.RunUnit.Starters )
					{
						c.Calculate( runner, team.Game );
						combinedPts += runner.Points;
					}
					if ( !string.IsNullOrWhiteSpace( dualBacks.Trim() ) )
					{
						dualBacks = dualBacks.Substring( 0, dualBacks.Length - 3 );
						if ( dualBacks.Length < 20 )
							dualBacks = dualBacks + new string( ' ', 20 - dualBacks.Length );
						if ( dualBacks.Length > 20 )
							dualBacks = dualBacks.Substring( 0, 20 );
					}
					var p = team.Team.RunUnit.R1;

					var matchupLink = "";
					if ( p != null )
					{
						var plusMatchup = PlusMatchup( p, nextOppTeam, p.CurrTeam );
						matchupLink = nextOppTeam.DefensiveUnitMatchUp( p.PlayerCat, plusMatchup );
					}
					else
						matchupLink = "?" + new String(' ', 20);

					bit = string.Format(
					   "&nbsp;<a href='..\\Roles\\{0}-Roles-{1:0#}.htm'>{3}</a> {2}  {4,2:#0}      ",
					   team.Team.TeamCode,
					   Week - 1,
					   matchupLink,
					   dualBacks,
					   (int) combinedPts
					   );
					Logger.Trace( "   >>> No Ace back for {0}", team.Team.Name );
				}
			}
			else
			{
				Logger.Trace( "   >>> No QB1 for {0}", team.Team.Name );
			}
			Logger.Trace( "   >>> bit = {0}", bit );
			return $"{bit,-36}";
		}

		public string PlayerPiece( 
            NFLPlayer p, 
            NFLGame g, 
            YahooCalculator c )
		{
			var nextOppTeam = p.NextOpponentTeam( g );
			var plusMatchup = PlusMatchup( p, nextOppTeam, p.CurrTeam );
			var matchupLink = nextOppTeam.DefensiveUnitMatchUp( 
                p.PlayerCat, 
                plusMatchup );
			var owners = p.LoadAllOwners();
			c.Calculate( p, g );
			var namePart = string.Format( "<a href='..\\Roles\\{0}-Roles-{1:0#}.htm'>{2}</a>",
			p.TeamCode, Week - 1, p.PlayerNameTo( 11 ) );
			if ( p.PlayerCat.Equals( Constants.K_KICKER_CAT ) )
			{
				AddPickup( p, g );
				return string.Format( " {0,-11}  {1}  {2,2:#0}{3} {4}",
				   namePart,
				   owners,
				   p.Points,
				   DomeBit( g, p ),
				   ActualOutput( g, p )
				   );
			}
			AddPickup( p, g );
			return string.Format( "{6}{0,-11}{7} {3}  {1}  {2,2:#0}{5} {4}",
			   namePart,
			   matchupLink,  //  defensiveRating,
			   p.Points,
			   owners,
			   ActualOutput( g, p ),
			   DomeBit( g, p ),
			   ReturnerBit( p ),
			   ShortYardageBit( p )
			   );
		}

		private void AddPickup( NFLPlayer p, NFLGame g )
		{
			p.LoadOwner( Constants.K_LEAGUE_Yahoo );
			if ( p.IsFreeAgent() || p.Owner == "77" )
			{
                var prevPts = p.Points;  // so we dont lose Points value
                var pu = new Pickup
                {
                    Season = Season,
                    Player = p,
                    Name = $"{p.PlayerNameTo( 20 )} ({p.TeamCode}) {p.PlayerPos,-10}",
                    Opp = $"{g.OpponentOut( p.TeamCode )}",
                    ProjPts = p.Points,
                    CategoryCode = p.PlayerCat,
                    Pos = p.PlayerPos,
					ActualPts = ActualOutput( g, p )
				};
                p.Points = prevPts;
				if ( p.Owner == "77" )
					pu.Name = pu.Name.ToUpper();
				PickupSummary.AddPickup( pu );
                if ( PlayerReports )
                    p.PlayerReport(forceIt:true);
			}
		}

		private string PlusMatchup( NFLPlayer p, NflTeam nextOppTeam, NflTeam pTeam )
		{
			var matchUp = "-";
			var oppRating = nextOppTeam.DefensiveRating( p.PlayerCat );
			var oppNumber = GetAsciiValue( oppRating );
			var plrRating = pTeam.OffensiveRating( p.PlayerCat );
			var plrNumber = GetAsciiValue( plrRating );
			if ( plrNumber <= oppNumber )
			{
				matchUp = "+";
				if ( oppNumber - plrNumber >= 3 )
					matchUp = "*";  //  big mismatch
			}
			return matchUp;
		}

		private static int GetAsciiValue( string rating )
		{
			byte[] value = Encoding.ASCII.GetBytes( rating );
			return value[ 0 ];
		}

		private object ShortYardageBit( NFLPlayer p )
		{
			var shortYardageBit = " ";
			if ( p.IsShortYardageBack() )
			{
				shortYardageBit = "$";
			}
			return shortYardageBit;
		}

		private string ReturnerBit( NFLPlayer p )
		{
			var returnerBit = " ";
			if ( p.IsReturner() )
			{
				returnerBit = "-";
			}
			return returnerBit;
		}

		private string DomeBit( NFLGame g, NFLPlayer p )
		{
			var bit = " ";
			if ( p.IsKicker() )
			{
				if ( g.IsDomeGame() )
					bit = "+";
				else if ( g.IsBadWeather() )
					bit = "-";
			}
			return bit;
		}

		public string ActualOutput( NFLGame g, NFLPlayer p )
		{
			if ( !g.Played(addDay:false) )
				return "____";

			Console.WriteLine( g.ScoreOut() );
			if ( g.GameWeek == null )
                g.GameWeek = new NFLWeek( g.Season, g.Week );

            var scorer = new YahooScorer( g.GameWeek )
            {
                UseProjections = false
            };
            var nScore = scorer.RatePlayer( 
                p, 
                g.GameWeek, 
                takeCache:false );

			return $" {nScore,2:#0} ";
		}

		#endregion Bits and Pieces

		public IEnumerable<Winner> GetWinners()
		{
			var week = new NFLWeek( Season, Week );
			var winners = new List<Winner>();
			foreach ( NFLGame g in week.GameList() )
			{
				g.CalculateSpreadResult();
				var teamCode = g.BookieTip.WinningTeam();
				var winner = new Winner
				{
					Team = g.Team( teamCode ),
					Margin = Math.Abs( g.Spread ),
					Home = g.IsHome( teamCode ),
					Game = g
				};
				winners.Add( winner );
			}

			return winners;
		}

		public IEnumerable<Loser> GetLosers()
		{
			var week = new NFLWeek( Season, Week );
			var losers = new List<Loser>();
			foreach ( NFLGame g in week.GameList() )
			{
				g.CalculateSpreadResult();
				var teamCode = g.BookieTip.LosingTeam();

				var loser = new Loser
				{
					Team = g.Team( teamCode ),
					Margin = Math.Abs( g.Spread ),
					Home = g.IsHome( teamCode ),
					Game = g
				};
				losers.Add( loser );
			}

			return losers;
		}
	}

	public class Winner : IComparable, IWinOrLose
	{
		public Winner()
		{
			IsWinner = true;
		}

		public decimal Margin { get; set; }

		public NflTeam Team { get; set; }

		public bool Home { get; set; }

		public bool IsWinner { get; set; }

		public NFLGame Game { get; set; }

		public int CompareTo( object obj )
		{
			var winner2 = ( Winner ) obj;
			return Margin > winner2.Margin ? 1 : 0;
		}

		public override string ToString()
		{
			return $"{Team} by {Margin,4}";
		}
	}

	public class Loser : IComparable, IWinOrLose
	{
		public Loser()
		{
			IsWinner = false;
		}

		public decimal Margin { get; set; }

		public NflTeam Team { get; set; }

		public bool Home { get; set; }

		public bool IsWinner { get; set; }

		public NFLGame Game { get; set; }

		public int CompareTo( object obj )
		{
			var winner2 = ( Winner ) obj;
			return Margin < winner2.Margin ? 1 : 0;
		}

		public override string ToString()
		{
			return string.Format( "{0,4}", Margin );
		}
	}
}