using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RosterLib
{
	/// <summary>
	///    Result of a game basically showing useful information
	/// </summary>
	public class GameSummary
	{
		public NFLGame Game { get; set; }

		public GameSummary( NFLGame game )
		{
			SummariseGame( game );
		}

		public GameSummary( List<NFLGame> gameList )
		{
			foreach ( NFLGame game in gameList )
			{
				SummariseGame( game );
				Render();
			}
		}

		private void SummariseGame( NFLGame game )
		{
			Game = game;
			Game.LoadLineups();  // sourcing data from PGMETRIC and LINEUP
			Game.LoadPgms();
		}

		public string FileName()
		{
			var fileName = Game.SummaryFile();
			return fileName;
		}

		public void Render()
		{
			var str = new SimpleTableReport( "Game Summary " + Game.ScoreOut() );
			str.AddDenisStyle();
			str.SubHeader = SubHeading();
			str.AnnounceIt = true;
			str.AddColumn( new ReportColumn( "C1", "COL01", "{0}" ) );
			str.AddColumn( new ReportColumn( "C2", "COL02", "{0}" ) );
			str.AddColumn( new ReportColumn( "C3", "COL03", "{0}" ) );
			str.AddColumn( new ReportColumn( "C4", "COL04", "{0}" ) );
			str.AddColumn( new ReportColumn( "C5", "COL05", "{0}" ) );
			str.AddColumn( new ReportColumn( "C6", "COL06", "{0}" ) );
			str.AddColumn( new ReportColumn( "C7", "COL07", "{0}" ) );
			str.AddColumn( new ReportColumn( "C8", "COL08", "{0}" ) );
			str.AddColumn( new ReportColumn( "C9", "COL09", "{0}" ) );
			str.AddColumn( new ReportColumn( "C10", "COL10", "{0}" ) );
			str.AddColumn( new ReportColumn( "C11", "COL11", "{0}" ) );

			str.CustomHeader = SummaryHeader();

			str.LoadBody( BuildTable() );
			GenerateFootNote( str );
			str.RenderAsHtml( FileName(), persist: true );
		}

		private void GenerateFootNote( SimpleTableReport str )
		{
			str.FootNote = DumpFantasyPlayers() + DumpPlayerGameMetrics() + DumpLineups();
		}

		private string DumpFantasyPlayers()
		{
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableOpen( "border='0'" ) );
			sb.Append( HtmlLib.TableRowOpen() );
			sb.Append( HtmlLib.TableData( Game.DumpFantasyPlayersAsHtml( "Home Fantasy Players", Game.HomeTeam ) ) );
			sb.Append( HtmlLib.TableData( Game.DumpFantasyPlayersAsHtml( "Away Fantasy Players", Game.AwayTeam ) ) );
			sb.Append( HtmlLib.TableRowClose() );
			sb.Append( HtmlLib.TableClose() );
			return sb.ToString();
		}

		private string DumpPlayerGameMetrics()
		{
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableOpen( "border='0'" ) );
			sb.Append( HtmlLib.TableRowOpen() );
			sb.Append( HtmlLib.TableData( Game.DumpPgmsAsHtml( "Home PGMs", Game.HomeTeam ) ) );
			sb.Append( HtmlLib.TableData( Game.DumpPgmsAsHtml( "Away PGMS", Game.AwayTeam ) ) );
			sb.Append( HtmlLib.TableRowClose() );
			sb.Append( HtmlLib.TableClose() );
			return sb.ToString();
		}

		private string DumpLineups()
		{
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableOpen( "border='0'" ) );
			sb.Append( HtmlLib.TableRowOpen() );
			sb.Append( HtmlLib.TableData( Game.HomeLineup.DumpAsHtml( "Home Lineup" ) ) );
			sb.Append( HtmlLib.TableData( Game.AwayLineup.DumpAsHtml( "Away Lineup" ) ) );
			sb.Append( HtmlLib.TableRowClose() );
			sb.Append( HtmlLib.TableClose() );
			return sb.ToString();
		}

		private static string SummaryHeader()
		{
			var htmlOut =
				HtmlLib.TableRowOpen() + "\n\t\t"
				+ HtmlLib.TableDataAttr( HtmlLib.Bold( "AWAY" ), "colspan='5' class='gponame'" ) + "\n\t"
				+ HtmlLib.TableDataAttr( HtmlLib.Bold( "---" ), "colspan='1' class='gponame'" ) + "\n\t"
				+ HtmlLib.TableDataAttr( HtmlLib.Bold( "HOME" ), "colspan='5' class='gponame'" ) + "\n\t"
				+ HtmlLib.TableRowClose() + "\n";
			return htmlOut;
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "COL01", typeof( String ) );
			cols.Add( "COL02", typeof( String ) );
			cols.Add( "COL03", typeof( String ) );
			cols.Add( "COL04", typeof( String ) );
			cols.Add( "COL05", typeof( String ) );
			cols.Add( "COL06", typeof( String ) );
			cols.Add( "COL07", typeof( String ) );
			cols.Add( "COL08", typeof( String ) );
			cols.Add( "COL09", typeof( String ) );
			cols.Add( "COL10", typeof( String ) );
			cols.Add( "COL11", typeof( String ) );

			AddTDrRow( dt );
			AddTDpRow( dt );
			AddTDdRow( dt );
			AddTDsRow( dt );
			AddFGsRow( dt );
			AddYDrRow( dt );
			AddYDpRow( dt );

			AddQB1Row( dt );

			AddRB1Row( dt );

			return dt;
		}

		private void AddRB1Row( DataTable dt )
		{
			var dr = dt.NewRow();
			if ( Game.AwayRb1 != null )
			{
				dr[ "COL01" ] = Game.AwayRb1.PlayerName;
				dr[ "COL02" ] = Output( Game.AwayRb1.CurrentGameMetrics.YDp, "YDp" );
				dr[ "COL03" ] = Output( Game.AwayRb1.CurrentGameMetrics.TDp, "TDp" );
				dr[ "COL04" ] = Output( Game.AwayRb1.CurrentGameMetrics.YDr, "YDr" );
				dr[ "COL05" ] = Output( Game.AwayRb1.CurrentGameMetrics.TDr, "TDr" );
			}
			dr[ "COL06" ] = "RB";
			if ( Game.HomeRb1 != null )
			{
				dr[ "COL07" ] = Game.HomeRb1.PlayerName;
				dr[ "COL08" ] = Output( Game.HomeRb1.CurrentGameMetrics.YDp, "YDp" );
				dr[ "COL09" ] = Output( Game.HomeRb1.CurrentGameMetrics.TDp, "TDp" );
				dr[ "COL10" ] = Output( Game.HomeRb1.CurrentGameMetrics.YDr, "YDr" );
				dr[ "COL11" ] = Output( Game.HomeRb1.CurrentGameMetrics.TDr, "TDr" );
			}
			dt.Rows.Add( dr );
		}

		private string Output( int qty, string ofWhat )
		{
			var what = string.Empty;
			if ( qty > 0 )
				what = $"{qty} {ofWhat}";
			return what;
		}

		private void AddQB1Row( DataTable dt )
		{
			var dr = dt.NewRow();
			if ( Game.AwayQb1 != null )
			{
				dr[ "COL01" ] = Game.AwayQb1.PlayerName;
				dr[ "COL02" ] = Output( Game.AwayQb1.CurrentGameMetrics.YDp, "YDp" );
				dr[ "COL03" ] = Output( Game.AwayQb1.CurrentGameMetrics.TDp, "TDp" );
				dr[ "COL04" ] = Output( Game.AwayQb1.CurrentGameMetrics.YDr, "YDr" );
				dr[ "COL05" ] = Output( Game.AwayQb1.CurrentGameMetrics.TDr, "TDr" );
			}
			dr[ "COL06" ] = "QB";
			if ( Game.HomeQb1 != null )
			{
				dr[ "COL07" ] = Game.HomeQb1.PlayerName;
				dr[ "COL08" ] = Output( Game.HomeQb1.CurrentGameMetrics.YDp, "YDp" );
				dr[ "COL09" ] = Output( Game.HomeQb1.CurrentGameMetrics.TDp, "TDp" );
				dr[ "COL10" ] = Output( Game.HomeQb1.CurrentGameMetrics.YDr, "YDr" );
				dr[ "COL11" ] = Output( Game.HomeQb1.CurrentGameMetrics.TDr, "TDr" );
			}
			dt.Rows.Add( dr );
		}

		private void AddYDpRow( DataTable dt )
		{
			var dr = dt.NewRow();
			dr[ "COL04" ] = Game.AwayYDp;
			dr[ "COL06" ] = "YDp";
			dr[ "COL08" ] = Game.HomeYDp;
			dt.Rows.Add( dr );
		}

		private void AddYDrRow( DataTable dt )
		{
			var dr = dt.NewRow();
			dr[ "COL04" ] = Game.AwayYDr;
			dr[ "COL06" ] = "YDr";
			dr[ "COL08" ] = Game.HomeYDr;
			dt.Rows.Add( dr );
		}

		private void AddTDrRow( DataTable dt )
		{
			var dr = dt.NewRow();
			dr[ "COL04" ] = Game.AwayTDr;
			dr[ "COL06" ] = "TDr";
			dr[ "COL08" ] = Game.HomeTDr;
			dt.Rows.Add( dr );
		}

		private void AddTDpRow( DataTable dt )
		{
			var dr = dt.NewRow();
			dr[ "COL04" ] = Game.AwayTDp;
			dr[ "COL06" ] = "TDp";
			dr[ "COL08" ] = Game.HomeTDp;
			dt.Rows.Add( dr );
		}

		private void AddTDdRow( DataTable dt )
		{
			var dr = dt.NewRow();
			dr[ "COL04" ] = Game.AwayTDd;
			dr[ "COL06" ] = "TDd";
			dr[ "COL08" ] = Game.HomeTDd;
			dt.Rows.Add( dr );
		}

		private void AddTDsRow( DataTable dt )
		{
			var dr = dt.NewRow();
			dr[ "COL04" ] = Game.AwayTDs;
			dr[ "COL06" ] = "TDs";
			dr[ "COL08" ] = Game.HomeTDs;
			dt.Rows.Add( dr );
		}

		private void AddFGsRow( DataTable dt )
		{
			var dr = dt.NewRow();
			dr[ "COL04" ] = Game.AwayFg;
			dr[ "COL06" ] = "FGs";
			dr[ "COL08" ] = Game.HomeFg;
			dt.Rows.Add( dr );
		}

		private string SubHeading()
		{
			var header = Legend();
			var div = HtmlLib.DivOpen( "id=\"main\"" )
			+ GameData() + EndDiv() + HtmlLib.DivClose();
			var gameDataHtml = string.Format( "{0}{1}\n", header, div );
			var html = gameDataHtml + PredictionHtml();
			return html;
		}

		#region Team prediction

		private string PredictionHtml()
		{
			var prediction = Game.GetPrediction( "unit" );
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableOpen( "border='1'" ) );
			sb.Append( PredictionHtmlHeader() );

			sb.Append( AwayLine( prediction ) );
			sb.Append( HomeLine( prediction ) );

			sb.Append( HtmlLib.TableClose() );

			return sb.ToString();
		}

		private string ScoreOut( int predictedScore, int actualScore )
		{
			var variance = actualScore - predictedScore;
			return $"<b>{actualScore}</b> ({predictedScore}) {variance}";
		}

		private string PredictionHtmlHeader()
		{
			var sb = new StringBuilder();
			HeaderCell( sb, "Team" );
			HeaderCell( sb, "Score" );
			HeaderCell( sb, "TDp" );
			HeaderCell( sb, "TDr" );
			HeaderCell( sb, "FG" );
			HeaderCell( sb, "TDd" );
			HeaderCell( sb, "TDs" );
			return sb.ToString();
		}

		private string HomeLine( NFLResult prediction )
		{
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableRowOpen() );
			sb.Append( HtmlLib.TableData( Game.HomeTeam ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.HomeScore, Game.HomeScore ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.HomeTDp, Game.HomeTDp ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.HomeTDr, Game.HomeTDr ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.HomeFg, Game.HomeFg ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.HomeTDd, Game.HomeTDd ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.HomeTDs, Game.HomeTDs ) ) );
			sb.Append( HtmlLib.TableRowClose() );
			return sb.ToString();
		}

		private string AwayLine( NFLResult prediction )
		{
			var sb = new StringBuilder();
			sb.Append( HtmlLib.TableRowOpen() );
			sb.Append( HtmlLib.TableData( Game.AwayTeam ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.AwayScore, Game.AwayScore ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.AwayTDp, Game.AwayTDp ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.AwayTDr, Game.AwayTDr ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.AwayFg, Game.AwayFg ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.AwayTDd, Game.AwayTDd ) ) );
			sb.Append( HtmlLib.TableData( ScoreOut( prediction.AwayTDs, Game.AwayTDs ) ) );
			sb.Append( HtmlLib.TableRowClose() );
			return sb.ToString();
		}

		private static void HeaderCell( StringBuilder sb, string contents )
		{
			sb.Append( HtmlLib.TableHeaderOpen() );
			sb.Append( contents );
			sb.Append( HtmlLib.TableHeaderClose() );
		}

		#endregion Team prediction

		private string Legend()
		{
			return string.Format( "\n<h1>{0}</h3>\n", Game.WordyScoreOut() );
		}

		private static string EndDiv()
		{
			return HtmlLib.DivOpen( "class=\"end\"" ) + HtmlLib.Spaces( 1 ) + HtmlLib.DivClose() + "\n";
		}

		private string GameData()
		{
			var s = String.Empty;
			s += DataOut( "Date", Game.GameDate.ToLongDateString() );
			s += DataOut( "Code", Game.GameKey() );
			s += DataOut( "Divisional", Game.IsDivisionalGame() ? "Yes" : "No" );
			s += DataOut( "Bad Weather", Game.IsBadWeather() ? "Yes" : "No" );
			s += DataOut( "Dome", Game.IsDomeGame() ? "Yes" : "No" );
			s += DataOut( "Monday Night", Game.IsMondayNight() ? "Yes" : "No" );
			s += DataOut( "TV", Game.IsOnTv ? "Yes" : "No" );
			s += DataOut( "Spread", string.Format( "{0:0.0}", Game.Spread ) );
			return s;
		}

		private static string DataOut( string label, string val )
		{
			var labelToken = string.Format( "<label>{0}:</label> <value>{1,8}</value>", label, val );
			return labelToken;
		}
	}
}