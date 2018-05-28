using System;
using System.Globalization;

namespace RosterLib
{
	/// <summary>
	/// A class to hold and display NFL Output for a particular Metric.
	/// Handles spread (wins) and Tdp.
	/// </summary>
	public class NFLOutputMetric
	{
		private string _metricName;
		private readonly NFLPlayer _player;
		private readonly NflTeam _team;
		private int _total;
		public bool AddBlankRow;   //  Adds a blank line where I can pencil in my tip

		private const int KOpponents = 21;

		/// <summary>
		/// Upcoming/current year projections
		/// </summary>
		private readonly SeasonOpposition[] _opps;

		/// <summary>
		/// Last years results
		/// </summary>
		private readonly SeasonOpposition[] _prevOpp;

		public NFLOutputMetric( string metricIn, NFLPlayer playerIn, NflTeam teamIn )
		{
			_metricName = metricIn;
			_player = playerIn;
			_team = teamIn;
			_team.Wins = 0;
			_team.Losses = 0;
			_prevOpp = new SeasonOpposition[ KOpponents ];
			_opps = new SeasonOpposition[ KOpponents ];
			for ( var i = 0; i < KOpponents; i++ )
			{
				_opps[ i ] = new SeasonOpposition( "BYE", true, 0 );
				_prevOpp[ i ] = new SeasonOpposition( "   ", true, 0 );
			}
		}

		#region Accessors

		public string Name
		{
			get { return _metricName; }
			set { _metricName = value; }
		}

		public int Total
		{
			get { return _total; }
			set { _total = value; }
		}

		#endregion Accessors

		public void AddWeeklyOutput( int week, int output, string opponent, bool bHome )
		{
			if ( week > 0 )
			{
				_total += output;
				var so = new SeasonOpposition( opponent, bHome, output );
				_opps[ week - 1 ] = so;
			}
		}

		public void AddPrevWeeklyOutput( int week, SeasonOpposition soIn )
		{
			if ( week > 0 )
			{
				if ( week < 18 ) _prevOpp[ week - 1 ] = soIn;
			}
		}

		private int TotalPrev()
		{
			var tot = 0;
			for ( var i = 1; i < 17; i++ ) tot += _prevOpp[ i ].Metric;
			return tot;
		}

		private string TotalColour()
		{
			if ( _metricName == "Spread" )
			{
				if ( _total > TotalPrev() )
					return "LIME";
				if ( _total == TotalPrev() )
					return "WHITE";

				return "PINK";
			}
			return "WHITE";
		}

		private string MetricColour( int metric )
		{
			var sColour = "WHITE";

			if ( _metricName == "Spread" )
			{
				if ( _player != null )
				{
					if ( metric == 0 )
						sColour = "PINK";
					else
					   if ( metric > 1 ) sColour = "LIME";
				}
				else
				{
					if ( metric > 0 ) sColour = "LIME";
					if ( metric < 0 ) sColour = "PINK";
					if ( metric == 0 ) sColour = "TOMATO";
				}
			}
			else
			{
				if ( _metricName == "Tdp" )
				{
					if ( metric > 1 ) sColour = "CYAN";
					if ( metric > 2 ) sColour = "LIME";
					if ( metric < 2 ) sColour = "PINK";
				}
				else
				{
					if ( metric == 0 ) sColour = "TOMATO";
					if ( metric > 0 ) sColour = "CYAN";
					if ( metric > 1 ) sColour = "LIME";
					if ( metric < 1 ) sColour = "PINK";
				}
			}
			return sColour;
		}

		private static string FormatWeek( string weekIn )
		{
			var sWeek = "";
			if ( weekIn != null )
				sWeek = string.Format( "wk {0} {1}", weekIn.Substring( 4, 2 ), weekIn.Substring( 0, 4 ) );
			return sWeek;
		}

		private string Label1Out()
		{
			var s = "";
			if ( _player != null )
				s = _player.PlayerName;
			else
			{
				if ( _metricName == "Spread" )
				{
					s = string.Format( "{0}-{1}", _team.Ratings, _team.RatingPts() );
					s = UnitUrl( _team.TeamCode, s );
				}
			}
			return s;
		}

		private string UnitUrl( string teamCode, string ratings )
		{
			return string.Format( "<a href =\"./html/UnitMenu_{0}.htm\">{1}</a>", teamCode, ratings );
		}

		private string Colour1Out()
		{
			var s = "";
			if ( _player != null ) s = string.Format( "BGCOLOR={0}", _player.SetColour( "PINK" ) );
			return s;
		}

		private string Stats1Out()
		{
			var s = "";
			if ( _player != null )
				s = string.Format( "{0:0.0}  {1}", _player.ScoresPerYear(), _player.Seasons() );
			else
				if ( _metricName == "Spread" ) s = string.Format( "(<b>{0}</b>-{1})", _team.Wins, _team.Losses );
			return s;
		}

		private string Stats2Out()
		{
			var s = "";
			if ( _player != null ) s = FormatWeek( _player.LoWeek );
			return s;
		}

		private string Stats3Out()
		{
			var s = "";
			if ( _player != null )
				s = FormatWeek( _player.HiWeek );
			else
			{
				if ( _metricName == "Spread" ) s = string.Format( "({0}-{1})", _team.PrevWins, _team.PrevLosses );
			}

			return s;
		}

		private string OpponentColour( string opponentCode )
		{
			var sColour = "WHITE";
			if ( _team.IsDivisionalOpponent( opponentCode ) ) sColour = "CYAN";
			return string.Format( " BGCOLOR='{0}'", sColour );
		}

		public string RenderAsHtml()
		{
			var s = HtmlLib.TableRowOpen();
			string sOpp;

			//  three rows
			s += HtmlLib.TableData( _team.TeamUrl() ) + HtmlLib.TableData( "" );
			for ( var i = 1; i < 18; i++ )
				s += HtmlLib.TableDataAttr( HtmlLib.Font( "ARIAL", i.ToString(), "-2" ),
				   "ALIGN='CENTER' BGCOLOR='MOCCASIN'" );  //  week number row
			s += HtmlLib.TableRowClose();

			//  Row 2  Schedule this year
			s += HtmlLib.TableRowOpen() +
			   HtmlLib.TableDataAttr( Label1Out(), Colour1Out() + " ALIGN='CENTER'" );
			s += HtmlLib.TableData( _metricName );
			for ( var j = 0; j < Constants.K_WEEKS_IN_REGULAR_SEASON; j++ )
			{
				if ( ( _opps[ j ].Opponent == "BYE" ) || ( _opps[ j ].Opponent.Trim().Length == 0 ) )
					sOpp = _opps[ j ].Opponent;
				else
					sOpp = ( _opps[ j ].IsHome() ? "v" : "@" ) + TeamUrl( _opps[ j ].Opponent );
				s += HtmlLib.TableDataAttr( sOpp, "ALIGN='CENTER'" + OpponentColour( _opps[ j ].Opponent ) );
			}
			s += HtmlLib.TableRowClose();

			if ( AddBlankRow )
			{
				s += HtmlLib.TableRowOpen() + HtmlLib.TableDataAttr( HtmlLib.HtmlPad( "", 2 ), "ALIGN='CENTER'" );
				s += HtmlLib.TableDataAttr( HtmlLib.HtmlPad( "", 2 ), "ALIGN='RIGHT' BGCOLOR='WHITE'" );

				for ( var k = 0; k < Constants.K_WEEKS_IN_REGULAR_SEASON; k++ )
					s += HtmlLib.TableDataAttr( HtmlLib.HtmlPad( "", 2 ), string.Format( "ALIGN='CENTER' BGCOLOR='{0}'", "WHITE" ) );
				s += HtmlLib.TableRowClose();
			}
			//  Row 3 Total  (W-L) tot spread
			s += HtmlLib.TableRowOpen() +
			   HtmlLib.TableDataAttr( Stats1Out(), "ALIGN='CENTER'" );
			s += HtmlLib.TableDataAttr(
			   HtmlLib.Bold( _total.ToString() ),
			   string.Format( "ALIGN='RIGHT' BGCOLOR='{0}'", TotalColour() ) );

			for ( var k = 0; k < Constants.K_WEEKS_IN_REGULAR_SEASON; k++ )
			{
				var mBit = LinkedCellContent( _opps[k], k, _team.TeamCode);
				s += mBit;
			}
			if ( _metricName == "Spread" )
			{
				//  Additional Row 4 last year's spreads
				s += HtmlLib.TableRowOpen() + HtmlLib.TableDataAttr( Stats2Out(), "ALIGN='CENTER'" );
				s += HtmlLib.TableData( "prev" );
				for ( var l = 0; l < Constants.K_WEEKS_IN_REGULAR_SEASON; l++ )
				{
					if ( ( _prevOpp[ l ].Opponent == "BYE" ) || ( _prevOpp[ l ].Opponent.Length == 0 ) )
						sOpp = _prevOpp[ l ].Opponent;
					else
						sOpp = ( _prevOpp[ l ].IsHome() ? "v" : "@" )
								  + MatchupUrl( l + 1, _team.TeamCode, _prevOpp[ l ].Opponent, _prevOpp[ l ].IsHome() );
					s += HtmlLib.TableDataAttr( sOpp, "ALIGN='CENTER'" +
					   OpponentColour( _prevOpp[ l ].Opponent ) );
				}
				s += HtmlLib.TableRowClose();
				//  Row 5 last years results
				s += HtmlLib.TableRowOpen() + HtmlLib.TableDataAttr( Stats3Out(), "ALIGN='CENTER'" );  // W-L
				s += HtmlLib.TableDataAttr( TotalPrev().ToString(), "ALIGN='RIGHT'" );
				for ( var m = 0; m < Constants.K_WEEKS_IN_REGULAR_SEASON; m++ )
				{
					var mBit = CellContent( _prevOpp[ m ] );
					s += mBit;
				}
				s += HtmlLib.TableRowClose();
			}

			return s;
		}

		private string LinkedCellContent( SeasonOpposition opp, int k, string teamCode )
		{
			if ( opp.Opponent.Equals("BYE") )
				return HtmlLib.TableDataAttr( "   ", "ALIGN='CENTER' BGCOLOR='WHITE'" );

			var rawdata = ProjectionUrl( opp.Metric, opp.Opponent, opp.IsHome(), teamCode, k + 1 );
			var formatting = $"ALIGN='CENTER' BGCOLOR='{MetricColour( opp.Metric )}'";
			return HtmlLib.TableDataAttr( rawdata, formatting );
		}

		private string CellContent(SeasonOpposition opp )
		{
			if (string.IsNullOrEmpty( opp.Opponent.Trim() ))
				return HtmlLib.TableDataAttr( "BYE", "ALIGN='CENTER' BGCOLOR='WHITE'" );

			if ( opp.Metric == 0 )
				return HtmlLib.TableDataAttr( "tie", "ALIGN='CENTER' BGCOLOR='TOMATO'" );

			var rawdata = opp.Metric.ToString( CultureInfo.InvariantCulture );
			var formatting = $"ALIGN='CENTER' BGCOLOR='{MetricColour( opp.Metric )}'";
			return	HtmlLib.TableDataAttr( rawdata, formatting );
		}

		private static string MatchupUrl( int week, string teamCode, string opponentTeamCode, bool isHome )
		{
			string homeTeam;
			string awayTeam;

			if ( isHome )
			{
				homeTeam = teamCode;
				awayTeam = opponentTeamCode;
			}
			else
			{
				homeTeam = opponentTeamCode;
				awayTeam = teamCode;
			}
			return string.Format( "<a href =\"./Matchups/Week{0:0#}/Matchup_Wk{0:0#}_{1}v{2}.htm\">{3}</a>",
				   week, homeTeam, awayTeam, opponentTeamCode );
		}

		private static string SummaryUrl( int week, string teamCode, string opponentTeamCode, bool isHome )
		{
			string homeTeam;
			string awayTeam;

			if ( isHome )
			{
				homeTeam = teamCode;
				awayTeam = opponentTeamCode;
			}
			else
			{
				homeTeam = opponentTeamCode;
				awayTeam = teamCode;
			}
			return string.Format( "<a href =\"./Matchups/Week{0:0#}/Matchup_Wk{0:0#}_{1}v{2}.htm\">{3}</a>",
			   week, homeTeam, awayTeam, opponentTeamCode );
		}

		private static string TeamUrl( string teamCode )
		{
			return string.Format( "<a href =\"./html/teams/TeamCard_{0}.htm\">{0}</a>", teamCode );
		}

		private static string ProjectionUrl( 
			int projectedSpread, 
			string opponent, 
			bool isHome, 
			string teamCode,
			int week )
		{
			var url = string.Empty;

			string homeTeam;
			string awayTeam;

			if ( isHome )
			{
				homeTeam = teamCode;
				awayTeam = opponent;
			}
			else
			{
				homeTeam = opponent;
				awayTeam = teamCode;
			}

			var theNumber = (projectedSpread==0) ? "tie" : $"{projectedSpread}";

			url = string.Format( "<a href =\"./gameprojections/Week {4:0#}/{1}@{2}.htm\">{3}</a>",
				Utility.CurrentSeason(), awayTeam, homeTeam, theNumber, week );

			return url;
		}
	}
}