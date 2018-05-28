using System;
using System.Data;
using RosterLib.Helpers;

namespace RosterLib
{
	public class HillenTips
	{
		public NflSeason Season { get; set; }
		public NFLWeek Week { get; set; }
		public HillinPredictor HillenPredictor { get; set; }

		public HillenTips( string season, string week )
		{
			Season = new NflSeason( season );
			Week = new NFLWeek( season, week );
			HillenPredictor = new HillinPredictor();
		}

		public void Render()
		{
			var report = string.Format( "Hillen Tips {0}-{1}", Season.Year, Week.Week );
			var str = new SimpleTableReport( report ) { ReportHeader = report };
			StyleHelper.AddStyle( str );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Away", "AWAY", "{0}" ) );
			str.AddColumn( new ReportColumn( "AwayTip", "AWAYTIP", "{0}" ) );
			str.AddColumn( new ReportColumn( "Home", "HOME", "{0}" ) );
			str.AddColumn( new ReportColumn( "HomeTip", "HOMETIP", "{0}" ) );
			str.AddColumn( new ReportColumn( "HLine", "HLINE", "{0}" ) );
			str.AddColumn( new ReportColumn( "Spr", "SPREAD", "{0}" ) );
			str.AddColumn( new ReportColumn( "Result", "RESULT", "{0}" ) );
			str.AddColumn( new ReportColumn( "SuResult", "SURESULT", "{0}" ) );
			str.AddColumn( new ReportColumn( "SuWins", "SUWINS", "{0}" ) );
			str.AddColumn( new ReportColumn( "SuLoses", "SULOSES", "{0}" ) );
			str.AddColumn( new ReportColumn( "SprResult", "SPRRESULT", "{0}" ) );
			str.AddColumn( new ReportColumn( "Wins", "WINS", "{0}" ) );
			str.AddColumn( new ReportColumn( "Loses", "LOSES", "{0}" ) );
			str.AddColumn( new ReportColumn( "Ties", "TIES", "{0}" ) );
			str.LoadBody( BuildTable() );
			//str.SubHeader = SubHeading();
			str.RenderAsHtml( FileName(), true );
		}

		public string FileName()
		{
			return string.Format( "{0}{1}//Tips//Hillen//Tips-{2:0#}.htm",
				Utility.OutputDirectory(), Season.Year, Week.WeekNo );
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "GAMECODE", typeof( String ) );
			cols.Add( "AWAY", typeof( String ) );
			cols.Add( "AWAYTIP", typeof( Int32 ) );
			cols.Add( "HOME", typeof( String ) );
			cols.Add( "HOMETIP", typeof( Int32 ) );
			cols.Add( "HLINE", typeof( Int32 ) );
			cols.Add( "SPREAD", typeof( Int32 ) );
			cols.Add( "RESULT", typeof( String ) );
			cols.Add( "SURESULT", typeof( String ) );
			cols.Add( "SUWINS", typeof( Int32 ) );
			cols.Add( "SULOSES", typeof( Int32 ) );
			cols.Add( "SPRRESULT", typeof( String ) );
			cols.Add( "WINS", typeof( Int32 ) );
			cols.Add( "LOSES", typeof( Int32 ) );
			cols.Add( "TIES", typeof( Int32 ) );

			var suWins = 0;
			var suLosses = 0;
			var atsWins = 0;
			var atsLosses = 0;
			var atsTies = 0;

			foreach ( NFLGame game in Week.GameList() )
			{
				var result = HillenPredictor.PredictGame( game, null, game.GameDate );
				var dr = dt.NewRow();
				dr[ "GAMECODE" ] = game.GameCode;
				dr[ "AWAY" ] = game.AwayNflTeam.NameOut();
				dr[ "AWAYTIP" ] = result.AwayScore;
				dr[ "HOME" ] = game.HomeNflTeam.NameOut();
				dr[ "HOMETIP" ] = result.HomeScore;
				dr[ "HLINE" ] = result.Margin();
				dr[ "SPREAD" ] = game.Spread;
				dr[ "RESULT" ] = game.ScoreOut3();
				var su = game.EvaluatePrediction( result );
				if ( su.IndexOf( "WIN" ) > -1 ) suWins++;
				else suLosses++;
				dr[ "SURESULT" ] = su;
				dr[ "SUWINS" ] = suWins;
				dr[ "SULOSES" ] = suLosses;

				var ats = game.EvaluatePredictionAts( result, game.Spread );
				dr[ "SPRRESULT" ] = ats;
				if ( ats.IndexOf( "WIN" ) > -1 ) atsWins++;
				else if ( ats.IndexOf( "PUSH" ) > -1 )
					atsTies++;
				else
					atsLosses++;
				dr[ "WINS" ] = atsWins;
				dr[ "LOSES" ] = atsLosses;
				dr[ "TIES" ] = atsTies;

				dt.Rows.Add( dr );
	
			}
			dt.DefaultView.Sort = "GAMECODE ASC";
			return dt;			
		}
	}
}
