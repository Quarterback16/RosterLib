using System;
using System.Globalization;

namespace RosterLib
{
	public class PlayerGameMetrics
	{
		public string PlayerId { get; set; }
		public string GameKey { get; set; }

		public int ProjTDp { get; set; }
		public int TDp { get; set; }
		public decimal ProjTDr { get; set; }
		public int TDr { get; set; }
		public int ProjTDc { get; set; }
		public int TDc { get; set; }
		public int ProjYDp { get; set; }
		public int YDp { get; set; }
		public int ProjYDr { get; set; }
		public int YDr { get; set; }
		public int ProjYDc { get; set; }
		public int YDc { get; set; }

		public int ProjRec { get; set; }
		public int Rec { get; set; }

		public int ProjFG { get; set; }
		public int FG { get; set; }
		public int ProjPat { get; set; }
		public int Pat { get; set; }

		public decimal FantasyPoints { get; set; }

		public decimal ProjectedFantasyPoints { get; set; }

		public bool IsEmpty { get; set; }

		public IRatePlayers Scorer { get; set; }

		public PlayerGameMetrics()
		{
			IsEmpty = true;
		}

		public override string ToString()
		{
			if ( YDp + TDp + YDr + TDr + YDc + TDc + FG + Pat > 0 )
				return string.Format(
				"{0} in {1} actuals: passing>{2,3}-({3})  running>{4,3}-({5})  catch>{6,3}-({7})  kick>{8}-{9}",
				PlayerId, GameKey, YDp, TDp, YDr, TDr, YDc, TDc, FG, Pat );

			return string.Format(
			   "{0} in {1} projected: passing>{2,3}-({3})  running>{4,3}-({5:0.0})  catch>{6,3}-({7})  kick>{8}-{9}",
			   PlayerId, GameKey, ProjYDp, ProjTDp, ProjYDr, ProjTDr, ProjYDc, ProjTDc, ProjFG, ProjPat );
		}

		public string ActualStatsOut( string playerCat )
		{
			var output = string.Empty;
			switch ( playerCat )
			{
				case Constants.K_QUARTERBACK_CAT:
					output = string.Format( "{0} ({1})", YDp, TDp + TDr );
					break;

				case Constants.K_RUNNINGBACK_CAT:
					output = string.Format( "{0} ({1})", YDr + YDc, TDr + TDc );
					break;

				case Constants.K_RECEIVER_CAT:
					output = string.Format( "{0} ({1})", YDc + YDr, TDc + TDr );
					break;

				case Constants.K_KICKER_CAT:
					output = string.Format( "{0} ({1})", Pat, FG );
					break;

				default:
					break;
			}
			return output;
		}

		public void Save( IPlayerGameMetricsDao dao )
		{
			dao.Save( this );
		}

		public void UpdateAcuals( IPlayerGameMetricsDao dao )
		{
			dao.SaveActuals( this, fpts: this.FantasyPoints );
		}

		public string Season()
		{
			return GameKey.Substring( 1, 4 );
		}

		public string Week()
		{
			return GameKey.Substring( 5, 2 );
		}

		public decimal ProjectedScoresOfType( string forScoreType, string id )
		{
			decimal metric = 0M;
			switch ( forScoreType )
			{
				case Constants.K_SCORE_TD_PASS:
					if ( id == "2" ) metric = ProjTDp;
					if ( id == "1" ) metric = ProjTDc;
					break;

				case Constants.K_SCORE_TD_RUN:
					metric = ProjTDr;
					break;

				case Constants.K_SCORE_FIELD_GOAL:
					metric = ProjFG;
					break;

				case Constants.K_SCORE_PAT:
					metric = ProjPat;
					break;

				default:
					Utility.Announce( string.Format( "PlayerGameMetrics: Unknown score type {0}", forScoreType ) );
					break;
			}
			return metric;
		}

		public decimal ProjectedStatsOfType( string forStatType )
		{
			var metric = 0.0M;
			switch ( forStatType )
			{
				case Constants.K_STATCODE_PASSING_YARDS:
					metric = ProjYDp;
					break;

				case Constants.K_STATCODE_PASSES_CAUGHT:
					metric = ProjRec;
					break;

				case Constants.K_STATCODE_RUSHING_YARDS:
					metric = ProjYDr;
					break;

				case Constants.K_STATCODE_INTERCEPTIONS_THROWN:
					break;

				case Constants.K_STATCODE_RECEPTION_YARDS:
					metric = ProjYDc;
					break;

				default:
					Utility.Announce( string.Format( "Unknown stat type {0}", forStatType ) );
					break;
			}
			return metric;
		}

		public string FormatAsTableRow( string playerName, string role, decimal pts )
		{
			string[] pgmArray = {
								 playerName,
								 role,
								 ProjYDp.ToString( CultureInfo.InvariantCulture ),
								 ProjTDp.ToString( CultureInfo.InvariantCulture ),
								 ProjYDr.ToString( CultureInfo.InvariantCulture ),
								 ProjTDr.ToString( CultureInfo.InvariantCulture ),
								 ProjYDc.ToString( CultureInfo.InvariantCulture),
								 ProjTDc.ToString(CultureInfo.InvariantCulture),
								 ProjFG.ToString( CultureInfo.InvariantCulture),
								 ProjPat.ToString( CultureInfo.InvariantCulture),
								 YDp.ToString( CultureInfo.InvariantCulture ),
								 TDp.ToString( CultureInfo.InvariantCulture ),
								 YDr.ToString( CultureInfo.InvariantCulture ),
								 TDr.ToString( CultureInfo.InvariantCulture ),
								 YDc.ToString( CultureInfo.InvariantCulture),
								 TDc.ToString(CultureInfo.InvariantCulture),
								 FG.ToString( CultureInfo.InvariantCulture),
								 Pat.ToString( CultureInfo.InvariantCulture),
								 pts.ToString(CultureInfo.InvariantCulture)
						};
			var html = HtmlLib.TableRow( pgmArray );
			return html;
		}

		public string FormatProjectionsAsTableRow( NFLPlayer player )
		{
			if ( player.IsKicker() )
			{
				string[] pgmArray = {
											player.PlayerName,
											player.RoleOut(),
											string.Empty,
											string.Empty,
											string.Empty,
											string.Empty,
											string.Empty,
											string.Empty,
											ProjFG.ToString( CultureInfo.InvariantCulture),
											ProjPat.ToString( CultureInfo.InvariantCulture),
											ProjectedFantasyPoints.ToString()
								};
				var html = HtmlLib.TableRow( pgmArray );
				return html;
			}
			else
			{
				string[] pgmArray = {
											player.PlayerName,
											player.RoleOut(),
											ProjYDp.ToString( CultureInfo.InvariantCulture ),
											ProjTDp.ToString( CultureInfo.InvariantCulture ),
											ProjYDr.ToString( CultureInfo.InvariantCulture ),
											ProjTDr.ToString( CultureInfo.InvariantCulture ),
											ProjYDc.ToString( CultureInfo.InvariantCulture),
											ProjTDc.ToString( CultureInfo.InvariantCulture),
											string.Empty,
											string.Empty,
											ProjectedFantasyPoints.ToString()
								};
				var html = HtmlLib.TableRow( pgmArray );
				return html;
			}
		}

		public string FormatActualsAsTableRow( NFLPlayer player )
		{
			if ( player.IsKicker() )
			{
				string[] pgmArray = {
											string.Empty,
											string.Empty,
											string.Empty,
											string.Empty,
											string.Empty,
											string.Empty,
											string.Empty,
											string.Empty,
											FG.ToString( CultureInfo.InvariantCulture),
											Pat.ToString( CultureInfo.InvariantCulture),
											FantasyPoints.ToString(),
											VarianceOut()
								};
				var html = HtmlLib.TableRow( pgmArray );
				return html;
			}
			else
			{
				string[] pgmArray = {
											string.Empty,
											string.Empty,
											YDp.ToString( CultureInfo.InvariantCulture ),
											TDp.ToString( CultureInfo.InvariantCulture ),
											YDr.ToString( CultureInfo.InvariantCulture ),
											TDr.ToString( CultureInfo.InvariantCulture ),
											YDc.ToString( CultureInfo.InvariantCulture),
											TDc.ToString( CultureInfo.InvariantCulture),
											string.Empty,
											string.Empty,
											FantasyPoints.ToString(),
											VarianceOut()
								};
				var html = HtmlLib.TableRow( pgmArray );
				return html;
			}
		}

		private string VarianceOut()
		{
			var variance = CalculateVariance();
			if ( variance > 0 )
				return $"{variance}";
			else if ( variance < 0 )
				return $"+{Math.Abs( variance )}";
			else
				return "---";
		}

		private string ActualPlusVariance()
		{
			var variance = ProjectedFantasyPoints - FantasyPoints;
			return $"{FantasyPoints} {variance}";
		}

		public string PgmHeaderRow()
		{
			string[] pgmArray = {
						   "Starter",
						   "Role",
						   "Proj YDp",
						   "Proj TDp",
						   "Proj YDr",
						   "Proj TDr",
						   "Proj YDc",
						   "Proj TDc",
						   "Proj FG",
						   "Proj Pat",
						   "YDp",
						   "TDp",
						   "YDr",
						   "TDr",
						   "YDc",
						   "TDc",
						   "FG",
						   "Pat",
						   "YH FP"
						};
			var html = HtmlLib.TableHeaderRow( pgmArray );
			return html;
		}

		public string PgmHeaderVerticalRow()
		{
			string[] pgmArray = {
									"Starter",
									"Role",
									"YDp",
									"TDp",
									"YDr",
									"TDr",
									"YDc",
									"TDc",
									"FG",
									"Pat",
									"F pts",
									"Var"
								};
			var html = HtmlLib.TableHeaderRow( pgmArray );
			return html;
		}

		public bool HasNumbers()
		{
			var checkSum =
							  ProjYDp +
							  ProjTDp +
							  ProjYDr +
							  ProjTDr +
							  ProjYDc +
							  ProjTDc +
							  ProjFG +
							  ProjPat;
			return ( checkSum > 0 );
		}

		public decimal CalculateProjectedFantasyPoints( NFLPlayer p )
		{
			var scorer = new YahooProjectionScorer();
			p.ProjectedTDp = ProjTDp;
			p.ProjectedYDp = ProjYDp;
			p.ProjectedYDr = ProjYDr;
			p.ProjectedTDr = ProjTDr;
			p.ProjectedYDc = ProjYDc;
			p.ProjectedTDc = ProjTDc;
			p.ProjectedFg = ProjFG;
			p.ProjectedPat = ProjPat;

			var pts = scorer.RatePlayer( p, new NFLWeek( Season(), 99, loadGames: false ) );
			return pts;
		}

		public decimal CalculateActualFantasyPoints( NFLPlayer p )
		{
			var scorer = new YahooProjectionScorer();
			p.ProjectedTDp = TDp;
			p.ProjectedYDp = YDp;
			p.ProjectedYDr = YDr;
			p.ProjectedTDr = TDr;
			p.ProjectedYDc = YDc;
			p.ProjectedTDc = TDc;
			p.ProjectedFg = FG;
			p.ProjectedPat = Pat;

			var pts = scorer.RatePlayer( p, new NFLWeek( Season(), 99, loadGames: false ) );
			return pts;
		}

		public decimal CalculateVariance()
		{
			return ProjectedFantasyPoints - FantasyPoints;
		}
	}
}