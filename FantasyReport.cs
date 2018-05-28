using RosterLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class FantasyReport : TeamReport
	{
		public string Week { get; set; }

		public YahooScorer Scorer { get; set; }

		public FantasyReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Name = "Fantasy Report";
			Week = timekeeper.PreviousWeek();

			Heading = Name;

			FileOut = string.Format( "{0}{1}//Scores//FantasyScores-{2}.htm",
						Utility.OutputDirectory(), Season, Week );
		}

		public override string OutputFilename()
		{
			return FileOut;
		}

		public override void RenderAsHtml()
		{
			Ste = DefineSte();
			Data = BuildDataTable();

			LoadDataTable();

			Render();

			Finish();
		}

		private void LoadDataTable()
		{
#if DEBUG2
         var tCount = 0;
#endif
			foreach ( KeyValuePair<string, NflTeam> team in TeamList )
			{
				CalculateFpts( team.Value );
#if DEBUG2
            tCount++;
            if (tCount >2)
               break;
#endif
			}
		}

		private void CalculateFpts( NflTeam team )
		{
			// Process Stats and Scores for the week
			// save the calculations
			var ds = Utility.TflWs.GameForTeam( Season, Week, team.TeamCode );
			if ( ds.Tables[ 0 ].Rows.Count != 1 )
				return;

			DataRow teamRow = Data.NewRow();
			teamRow[ "TEAM" ] = team.NameOut();
			teamRow[ "TOTAL" ] = 0;
			teamRow[ "SCORE" ] = 0;
			var game = new NFLGame( ds.Tables[ 0 ].Rows[ 0 ] );
			teamRow[ "RESULT" ] = game.ResultFor( team.TeamCode, abbreviate: true, barIt: false );
			teamRow[ "SCORE" ] = game.ScoreFor( team );

			List<NFLPlayer> playerList = new List<NFLPlayer>();
			if ( game.IsAway( team.TeamCode ) )
				playerList = game.LoadAllFantasyAwayPlayers();
			else
				playerList = game.LoadAllFantasyHomePlayers();

			var totPts = 0.0M;
			var qbPts = 0.0M;
			var rbPts = 0.0M;
			var wrPts = 0.0M;
			var tePts = 0.0M;
			var pkPts = 0.0M;
			var week = new NFLWeek( Season, Week );

			TallyPts( playerList, ref totPts, ref qbPts, ref rbPts, ref wrPts, ref tePts, ref pkPts, week );

			teamRow[ "TOTAL" ] = totPts;
			teamRow[ "QB" ] = qbPts;
			teamRow[ "RB" ] = rbPts;
			teamRow[ "WR" ] = wrPts;
			teamRow[ "TE" ] = tePts;
			teamRow[ "PK" ] = pkPts;
			teamRow[ "DEF" ] = game.DefensiveFantasyPtsFor( team.TeamCode );

			Data.Rows.Add( teamRow );
		}

		private static void TallyPts( List<NFLPlayer> playerList, ref decimal totPts,
		   ref decimal qbPts, ref decimal rbPts, ref decimal wrPts,
		   ref decimal tePts, ref decimal pkPts, NFLWeek week )
		{
			var scorer = new YahooScorer( week );
			foreach ( var p in playerList )
			{
				var fpts = scorer.RatePlayer( p, week );
				totPts += fpts;

				switch ( p.PlayerCat )
				{
					case Constants.K_QUARTERBACK_CAT:
						qbPts += fpts;
						break;

					case Constants.K_RUNNINGBACK_CAT:
						rbPts += fpts;
						break;

					case Constants.K_RECEIVER_CAT:
						if ( p.IsTe() )
							tePts += fpts;
						else
							wrPts += fpts;
						break;

					case Constants.K_KICKER_CAT:
						pkPts += fpts;
						break;

					default:
						break;
				}
			}
		}

		private DataTable BuildDataTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "TEAM", typeof( String ) );
			cols.Add( "RESULT", typeof( String ) );
			cols.Add( "SCORE", typeof( Int32 ) );
			cols.Add( "TOTAL", typeof( decimal ) );
			cols.Add( "QB", typeof( decimal ) );
			cols.Add( "RB", typeof( decimal ) );
			cols.Add( "WR", typeof( decimal ) );
			cols.Add( "TE", typeof( decimal ) );
			cols.Add( "PK", typeof( decimal ) );
			cols.Add( "DEF", typeof( decimal ) );
			dt.DefaultView.Sort = "TOTAL DESC";
			return dt;
		}

		private SimpleTableReport DefineSte()
		{
			var str = new SimpleTableReport( Heading )
			{
				ColumnHeadings = true,
				DoRowNumbers = true
			};
			str.AddColumn( new ReportColumn( "Team", "TEAM", "{0,-20}" ) );
			str.AddColumn( new ReportColumn( "Result", "RESULT", "{0}" ) );
			str.AddColumn( new ReportColumn( "Pts", "SCORE", "{0:0.0}", typeof( int ), tally: true ) );
			str.AddColumn( new ReportColumn( "Total", "TOTAL", "{0:0.00}", typeof( decimal ), tally: true,
			   colourDelegateIn: TotalBgPicker ) );
			str.AddColumn( new ReportColumn( "QB", "QB", "{0:0.00}", typeof( decimal ), tally: true,
			   colourDelegateIn: QbBgPicker ) );
			str.AddColumn( new ReportColumn( "RB", "RB", "{0:0.0}", typeof( decimal ), tally: true,
			   colourDelegateIn: RbBgPicker ) );
			str.AddColumn( new ReportColumn( "WR", "WR", "{0:0.0}", typeof( decimal ), tally: true,
			   colourDelegateIn: WrBgPicker ) );
			str.AddColumn( new ReportColumn( "TE", "TE", "{0:0.0}", typeof( decimal ), tally: true,
			   colourDelegateIn: TeBgPicker ) );
			str.AddColumn( new ReportColumn( "PK", "PK", "{0:0.0}", typeof( decimal ), tally: true,
			   colourDelegateIn: PkBgPicker ) );
			str.AddColumn( new ReportColumn( "Defense", "DEF", "{0:0.0}", typeof( decimal ), tally: true,
			   colourDelegateIn: DefBgPicker ) );
			return str;
		}

		private static string TotalBgPicker( int theValue )
		{
			string sColour;

			if ( theValue < 60 )
				sColour = "RED";
			else if ( theValue < 80 )
				sColour = "GREEN";
			else
				sColour = "YELLOW";
			return sColour;
		}

		private static string QbBgPicker( int theValue )
		{
			string sColour;

			if ( theValue < 10 )
				sColour = "RED";
			else if ( theValue < 20 )
				sColour = "GREEN";
			else
				sColour = "YELLOW";
			return sColour;
		}

		private static string RbBgPicker( int theValue )
		{
			string sColour;

			if ( theValue < 10 )
				sColour = "RED";
			else if ( theValue < 20 )
				sColour = "GREEN";
			else
				sColour = "YELLOW";
			return sColour;
		}

		private static string WrBgPicker( int theValue )
		{
			string sColour;

			if ( theValue < 10 )
				sColour = "RED";
			else if ( theValue < 30 )
				sColour = "GREEN";
			else
				sColour = "YELLOW";
			return sColour;
		}

		private static string TeBgPicker( int theValue )
		{
			string sColour;

			if ( theValue < 5 )
				sColour = "RED";
			else if ( theValue < 10 )
				sColour = "GREEN";
			else
				sColour = "YELLOW";
			return sColour;
		}

		private static string PkBgPicker( int theValue )
		{
			string sColour;

			if ( theValue < 5 )
				sColour = "RED";
			else if ( theValue < 10 )
				sColour = "GREEN";
			else
				sColour = "YELLOW";
			return sColour;
		}

		private static string DefBgPicker( int theValue )
		{
			string sColour;

			if ( theValue < 6 )
				sColour = "RED";
			else if ( theValue < 10 )
				sColour = "GREEN";
			else
				sColour = "YELLOW";
			return sColour;
		}
	}
}