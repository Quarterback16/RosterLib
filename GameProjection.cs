using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	///   GameProjection outputs an expected result for a future game
	/// </summary>
	public class GameProjection
	{
		public NFLGame Game { get; set; }

		public string Season { get; set; }

		public GameProjection( NFLGame game )
		{
			Game = game;
		}

		public bool AnnounceIt { get; set; }

		/// <summary>
		///   Creates the output.
		/// </summary>
		public void Render()
		{
			var homeRatings = Game.HomeNflTeam.Ratings;
			var awayRatings = Game.AwayNflTeam.Ratings;

			Game.LoadPrediction();  // ratings get overwritten!!

			Game.HomeNflTeam.Ratings = homeRatings;
			Game.AwayNflTeam.Ratings = awayRatings;

			var str = new SimpleTableReport( "GameProjection " + Game.GameName() );
			str.AddDenisStyle();
			str.ColumnHeadings = true;
			str.DoRowNumbers = false;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AnnounceIt = AnnounceIt;
			str.AddColumn( new ReportColumn( "Team", "TEAM", "{0}" ) );
			str.AddColumn( new ReportColumn( "Rating", "RATING", "{0}" ) );
			str.AddColumn( new ReportColumn( "Score", "SCORE", "{0}", ScoreTotalDelegate ) );
			str.AddColumn( new ReportColumn( "YDp", "YDP", "{0}", true ) );
			str.AddColumn( new ReportColumn( "Tdp", "TDP", "{0}", true ) );
			str.AddColumn( new ReportColumn( "YDr", "YDR", "{0}", true ) );
			str.AddColumn( new ReportColumn( "Tdr", "TDR", "{0}", true ) );
			str.AddColumn( new ReportColumn( "TDd", "TDD", "{0}", true ) );
			str.AddColumn( new ReportColumn( "TDs", "TDS", "{0}", true ) );
			str.AddColumn( new ReportColumn( "FGs", "FG", "{0}", true ) );
			str.LoadBody( BuildTable() );
			str.SubHeader = SubHeading();
			str.FootNote = Game.PlayerProjectionsHtml();  //  these projections are old
			str.RenderAsHtml( FileName(), true );
		}

		private string ScoreTotalDelegate()
		{
			var calculatedSpread = Game.GetFormattedSpread();
			return calculatedSpread;
		}

		public string FileName()
		{
			return $@"{Utility.OutputDirectory()}{
				Game.Season
				}//Projections//gameprojections//Week {
				Game.Week
				}//{Game.AwayTeam}@{Game.HomeTeam}.htm";
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "TEAM", typeof( String ) );
			cols.Add( "SCORE", typeof( String ) );
			cols.Add( "RATING", typeof( String ) );
			cols.Add( "YDP", typeof( Int16 ) );
			cols.Add( "TDP", typeof( Int16 ) );
			cols.Add( "YDR", typeof( Int16 ) );
			cols.Add( "TDR", typeof( Int16 ) );
			cols.Add( "TDD", typeof( Int16 ) );
			cols.Add( "TDS", typeof( Int16 ) );
			cols.Add( "FG", typeof( Int16 ) );

			var dr = dt.NewRow();
			if ( Game.AwayNflTeam == null )
				dr[ "TEAM" ] = Game.AwayTeam;
			else
			{
				dr[ "TEAM" ] = Game.AwayNflTeam.Name;
				dr[ "RATING" ] = Game.AwayNflTeam.RatingPtsOut();
			}
			dr[ "SCORE" ] = Game.Result.AwayScore;
			dr[ "YDP" ] = Game.Result.AwayYDp;
			dr[ "TDP" ] = Game.Result.AwayTDp;
			dr[ "YDR" ] = Game.Result.AwayYDr;
			dr[ "TDR" ] = Game.Result.AwayTDr;
			dr[ "TDD" ] = Game.Result.AwayTDd;
			dr[ "TDS" ] = Game.Result.AwayTDs;
			dr[ "FG" ] = Game.Result.AwayFg;
			dt.Rows.Add( dr );

			dr = dt.NewRow();
			if ( Game.HomeNflTeam == null )
				dr[ "TEAM" ] = Game.HomeTeam;
			else
			{
				dr[ "TEAM" ] = Game.HomeNflTeam.Name;
				dr[ "RATING" ] = Game.HomeNflTeam.RatingPtsOut();
			}
			dr[ "SCORE" ] = Game.Result.HomeScore;
			dr[ "YDP" ] = Game.Result.HomeYDp;
			dr[ "TDP" ] = Game.Result.HomeTDp;
			dr[ "YDR" ] = Game.Result.HomeYDr;
			dr[ "TDR" ] = Game.Result.HomeTDr;
			dr[ "TDD" ] = Game.Result.HomeTDd;
			dr[ "TDS" ] = Game.Result.HomeTDs;
			dr[ "FG" ] = Game.Result.HomeFg;
			dt.Rows.Add( dr );
			return dt;
		}

		private string SubHeading()
		{
			var header = Legend();
			var div = HtmlLib.DivOpen( "id=\"main\"" ) 
				+ GameData() + EndDiv() + HtmlLib.DivClose();
			return $"{header}{div}\n";
		}

		private string Legend()
		{
			return string.Format( "\n<h3>{0}</h3>\n", Game.GameName(linkToDepthChart:true) );
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
			return string.Format( "<label>{0}:</label> <value>{1,8}</value>", label, val );
		}
	}
}