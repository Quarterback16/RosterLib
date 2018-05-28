using RosterLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class PointsAllowedReport : TeamReport
	{
		public string Week { get; set; }
		public string RootFolder { get; set; }
		public IBreakdown TeamBreakdowns { get; set; }

        public List<FptsAllowed> TotalFpAllowedList { get; set; }
        public PointsAllowedReport( IKeepTheTime timekeeper ) : base( timekeeper )
		{
			Week = timekeeper.Week;
			if ( timekeeper.IsItPostSeason() )
			{
				Week = Constants.K_GAMES_IN_REGULAR_SEASON.ToString();
			}
			TeamBreakdowns = new PreStyleBreakdown
			{
				NumberLines = false
			};
            TotalFpAllowedList = new List<FptsAllowed>();
        }

        public override void RenderAsHtml()
		{
			Name = "Points Allowed Report";
			Heading = $"{Name} Week {Season}:{Week}";
			RootFolder = $"{Utility.OutputDirectory()}{Season}//Scores//";
			FileOut = string.Format( "{0}Points-Allowed-{1}.htm", RootFolder, Week );
			RenderSingle();
		}

		private void RenderSingle()
		{
			Ste = DefineSte();
			Data = BuildDataTable();
			LoadDataTable();
			Render();
			Finish();
		}

        private ReportColumn.ColourDelegate PickTotalColourDelegate( 
            string positionAbbr )
		{
			ReportColumn.ColourDelegate theDelegate;
			switch ( positionAbbr )
			{
				case "RB":
					theDelegate = TotRbBgPicker;
					break;

				case "WR":
					theDelegate = TotWrBgPicker;
					break;

				case "QB":
					theDelegate = TotQbBgPicker;
					break;

				case "PK":
					theDelegate = TotPkBgPicker;
					break;

				case "TE":
					theDelegate = TotTeBgPicker;
					break;

				default:
					theDelegate = TotBgPicker;
					break;
			}
			return theDelegate;
		}

		private SimpleTableReport DefineSte()
		{
			var str = new SimpleTableReport( Heading )
			{
				ColumnHeadings = true,
				DoRowNumbers = true
			};
			str.AddColumn( new ReportColumn( "Team", "TEAM", "{0,-20}" ) );
			str.AddColumn(
			   new ReportColumn( 
                   "Total", 
                   "TOTAL", 
                   "{0:0.00}", 
                   typeof( decimal ), 
                   tally: true,
			       colourDelegateIn: PickTotalColourDelegate( "TOT" ) ) );
			str.AddColumn(
			   new ReportColumn( 
                   "QB", 
                   "QB", 
                   "{0:0.00}", 
                   typeof( decimal ), 
                   tally: true,
			       colourDelegateIn: Simple32BgPicker ) );
			str.AddColumn(
			   new ReportColumn( 
                   "RB", 
                   "RB", 
                   "{0:0.00}", 
                   typeof( decimal ), 
                   tally: true,
			       colourDelegateIn: Simple32BgPicker ) );
			str.AddColumn(
			   new ReportColumn( 
                   "WR", 
                   "WR", 
                   "{0:0.00}", 
                   typeof( decimal ), 
                   tally: true,
			       colourDelegateIn: Simple32BgPicker ) );
			str.AddColumn(
			   new ReportColumn( 
                   "TE", 
                   "TE", 
                   "{0:0.00}", 
                   typeof( decimal ), 
                   tally: true,
			       colourDelegateIn: Simple32BgPicker ) );
			str.AddColumn(
			   new ReportColumn( 
                   "PK", 
                   "PK", 
                   "{0:0.00}", 
                   typeof( decimal ), 
                   tally: true,
			       colourDelegateIn: Simple32BgPicker ) );
			return str;
		}

		private DataTable BuildDataTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "TEAM", typeof( String ) );
			cols.Add( "TOTAL", typeof( decimal ) );
			cols.Add( "QB", typeof( string ) );
			cols.Add( "RB", typeof( string ) );
			cols.Add( "WR", typeof( string ) );
			cols.Add( "TE", typeof( string ) );
			cols.Add( "PK", typeof( string ) );
			dt.DefaultView.Sort = "TOTAL DESC";
			return dt;
		}

		private void LoadDataTable()
        {
#if DEBUG2
         var tCount = 0;
#endif
            var asOfWeek = int.Parse( Week );
            foreach ( KeyValuePair<string, NflTeam> teamPair in TeamList )
            {
                var team = teamPair.Value;

                FptsAllowed fptsAllowed = new FptsAllowed( team.TeamCode );
                FptsAllowed totalFptsAllowed = new FptsAllowed( team.TeamCode );

                for ( var w = Constants.K_WEEKS_IN_REGULAR_SEASON; w > 0; w-- )
                {
                    if ( w > asOfWeek ) continue;
                    string theWeek = string.Format( "{0:0#}", w );

                    var ds = Utility.TflWs.GameForTeam( 
                        Season, 
                        theWeek, 
                        team.TeamCode );
                    if ( ds.Tables[ 0 ].Rows.Count != 1 )
                        continue;

                    fptsAllowed = CalculateFptsAllowed( team, theWeek, ds );

                    totalFptsAllowed.Add( fptsAllowed );
                    AccumulateFptsAllowed( fptsAllowed );
                }
                DumpBreakdowns( team.TeamCode );

#if DEBUG2
            tCount++;
            if ( tCount > 0 )
               break;
#endif
            }

            RankTotalPoints();
            RankPointsToQbs();
            RankPointsToRbs();
            RankPointsToWrs();
            RankPointsToTes();
            RankPointsToPks();

            //  Build the output table
            foreach ( FptsAllowed item in TotalFpAllowedList )
            {
                DataRow teamRow = Data.NewRow();
                teamRow[ "TEAM" ] = item.TeamCode;
                teamRow[ "TOTAL" ] = 0;
                teamRow[ "QB" ] = LinkFor( item.TeamCode, "QB", item.ToQbs, item.ToQbsRank );
                teamRow[ "RB" ] = LinkFor( item.TeamCode, "RB", item.ToRbs, item.ToRbsRank );
                teamRow[ "WR" ] = LinkFor( item.TeamCode, "WR", item.ToWrs, item.ToWrsRank );
                teamRow[ "TE" ] = LinkFor( item.TeamCode, "TE", item.ToTes, item.ToTesRank );
                teamRow[ "PK" ] = LinkFor( item.TeamCode, "PK", item.ToPks, item.ToPksRank );

                teamRow[ "TOTAL" ] = item.TotPtsAllowed();
                Data.Rows.Add( teamRow );
            }
        }

        private void AccumulateFptsAllowed( FptsAllowed fptsAllowed )
        {
            // get the right object
            var fpts = GetFptsFor( fptsAllowed.TeamCode );
            if ( fpts == null )
            {
                TotalFpAllowedList.Add( fptsAllowed );
            }
            else
            {
                fpts.IncrementBy( fptsAllowed );
            }
        }

        private FptsAllowed GetFptsFor( string teamCode )
        {
            FptsAllowed fpts = null;
            foreach ( FptsAllowed item in TotalFpAllowedList )
            {
                if ( item.TeamCode.Equals(teamCode))
                {
                    fpts = item;
                    break;
                }
            }
            return fpts;
        }

        private void RankPointsToQbs()
        {
            TotalFpAllowedList.Sort(
                ( x, y ) => Decimal.Compare(
                    x.ToQbs,
                    y.ToQbs ) );

            var i = 32;
            foreach ( FptsAllowed item in TotalFpAllowedList )
            {
                item.ToQbsRank = i;
                Console.WriteLine( $@"{item.TotalRank:0#}: {
                    item.TeamCode
                    } QB:{item.ToQbs}" );
                i--;
            }
        }

        private void RankPointsToRbs()
        {
            TotalFpAllowedList.Sort(
                ( x, y ) => Decimal.Compare(
                    x.ToRbs,
                    y.ToRbs ) );

            var i = 32;
            foreach ( FptsAllowed item in TotalFpAllowedList )
            {
                item.ToRbsRank = i;
                Console.WriteLine( $@"{item.TotalRank:0#}: {
                    item.TeamCode
                    } RB:{item.ToRbs}" );
                i--;
            }
        }

        private void RankPointsToWrs()
        {
            TotalFpAllowedList.Sort(
                ( x, y ) => Decimal.Compare(
                    x.ToWrs,
                    y.ToWrs ) );

            var i = 32;
            foreach ( FptsAllowed item in TotalFpAllowedList )
            {
                item.ToWrsRank = i;
                Console.WriteLine( $@"{item.TotalRank:0#}: {
                    item.TeamCode
                    } WR:{item.ToWrs}" );
                i--;
            }
        }

        private void RankPointsToTes()
        {
            TotalFpAllowedList.Sort(
                ( x, y ) => Decimal.Compare(
                    x.ToTes,
                    y.ToTes ) );

            var i = 32;
            foreach ( FptsAllowed item in TotalFpAllowedList )
            {
                item.ToTesRank = i;
                Console.WriteLine( $@"{item.TotalRank:0#}: {
                    item.TeamCode
                    } WR:{item.ToTes}" );
                i--;
            }
        }

        private void RankPointsToPks()
        {
            TotalFpAllowedList.Sort(
                ( x, y ) => Decimal.Compare(
                    x.ToPks,
                    y.ToPks ) );

            var i = 32;
            foreach ( FptsAllowed item in TotalFpAllowedList )
            {
                item.ToPksRank = i;
                Console.WriteLine( $@"{item.TotalRank:0#}: {
                    item.TeamCode
                    } PK:{item.ToPks}" );
                i--;
            }
        }

        private void RankTotalPoints()
        {
            TotalFpAllowedList.Sort(
                ( x, y ) => Decimal.Compare(
                    x.TotPtsAllowed(),
                    y.TotPtsAllowed() ) );

            var i = 0;
            foreach ( FptsAllowed item in TotalFpAllowedList )
            {
                i++;
                item.TotalRank = i;
                Console.WriteLine( $@"{item.TotalRank:0#}: {
                    item.TeamCode
                    } {
                    item.TotPtsAllowed():0.00} QB:{item.ToQbs} RB:{item.ToRbs}" );
            }
        }

        private void DumpBreakdowns( string teamCode )
		{
			DumpBreakdown( teamCode, "QB" );
			DumpBreakdown( teamCode, "RB" );
			DumpBreakdown( teamCode, "WR" );
			DumpBreakdown( teamCode, "TE" );
			DumpBreakdown( teamCode, "PK" );
		}

		private void DumpBreakdown( string teamCode, string positionAbbr )
		{
			var breakdownKey = $"{teamCode}-{positionAbbr}-{Week}";
			TeamBreakdowns.Dump( breakdownKey,
			   $"{RootFolder}\\pts-allowed\\{breakdownKey}.htm" );
		}

		private string LinkFor( 
            string teamCode, 
            string positionAbbr, 
            decimal pts,
            int rank)
		{
			var link = $"<a href='.//pts-allowed//{teamCode}-{positionAbbr}-{Week}.htm'>{pts} ({rank})";
			return link;
		}

		private FptsAllowed CalculateFptsAllowed(
		   NflTeam team, 
		   string theWeek, 
		   DataSet gameDs )
		{
			// Process Stats and Scores for the week
			// save the calculations
			var ftpsAllowed = new FptsAllowed(team.TeamCode);
			var game = new NFLGame( gameDs.Tables[ 0 ].Rows[ 0 ] );

			List<NFLPlayer> playerList = new List<NFLPlayer>();
			if ( game.IsAway( team.TeamCode ) )
				playerList = game.LoadAllFantasyHomePlayers( 
					(DateTime?) game.GameDate,
					String.Empty );
			else
				playerList = game.LoadAllFantasyAwayPlayers(
					( DateTime? ) game.GameDate,
					String.Empty );

			var week = new NFLWeek( Season, theWeek );

			var scorer = new YahooXmlScorer( week );
			foreach ( var p in playerList )
			{
				var plyrPts = scorer.RatePlayer( p, week );

				if ( p.IsQuarterback() )
				{
					ftpsAllowed.ToQbs += plyrPts;
					AddBreakdownLine( team, theWeek, p, plyrPts, "QB" );
				}
				else if ( p.IsRb() )
				{
					ftpsAllowed.ToRbs += plyrPts;
					AddBreakdownLine( team, theWeek, p, plyrPts, "RB" );
				}
				else if ( p.IsWideout() )
				{
					ftpsAllowed.ToWrs += plyrPts;
					AddBreakdownLine( team, theWeek, p, plyrPts, "WR" );
				}
				else if ( p.IsTe() )
				{
					ftpsAllowed.ToTes += plyrPts;
					AddBreakdownLine( team, theWeek, p, plyrPts, "TE" );
				}
				else if ( p.IsKicker() )
				{
					ftpsAllowed.ToPks += plyrPts;
					AddBreakdownLine( team, theWeek, p, plyrPts, "PK" );
				}
			}

			return ftpsAllowed;
		}

		private void AddBreakdownLine(
		   NflTeam team, string theWeek, NFLPlayer p, decimal plyrPts, string abbr )
		{
			if ( plyrPts == 0 ) return;

			var strPts = string.Format( "{0:0.0}", plyrPts );
			strPts = strPts.PadLeft( 5 );
			strPts = strPts.Substring( strPts.Length - 5 );
			TeamBreakdowns.AddLine(
			   breakdownKey: $"{ team.TeamCode}-{abbr}-{Week}",
			   line: $@"Wk:{
				  theWeek
				  } {p.PlayerName,-25} Pts : {strPts}"
				);
		}

		public decimal FractionOfTheSeason()
		{
			var multiplier = ( decimal.Parse( Week ) ) / Constants.K_WEEKS_IN_REGULAR_SEASON;
			return multiplier;
		}

		private string TotQbBgPicker( int theValue )
		{
			const int Excellent_Target_For_A_Season = 170;  //  giving up 10 a week
			const int Good_Target_For_A_Season = 255;  //  15 a week
			const int Average_Target_For_A_Season = 325;  //  19 a week

			string sColour;

			if ( theValue < ( Excellent_Target_For_A_Season * FractionOfTheSeason() ) )
				sColour = Constants.Colour.Excellent;
			else if ( theValue < ( Good_Target_For_A_Season * FractionOfTheSeason() ) )
				sColour = Constants.Colour.Good;
			else if ( theValue < ( Average_Target_For_A_Season * FractionOfTheSeason() ) )
				sColour = Constants.Colour.Average;
			else
				sColour = Constants.Colour.Bad;
			return sColour;
		}

		public string TotBgPicker( int theValue )
		{
			const int Excellent_Target_For_A_Season = 680;  //  giving up 40 a week
			const int Good_Target_For_A_Season = 850;  //  50 a week
			const int Average_Target_For_A_Season = 1350;  //  80 a week
			string sColour;
			var fraction = FractionOfTheSeason();
			//Utility.Announce( $"Fraction:{fraction:0.00}" );
			if ( theValue < ( Excellent_Target_For_A_Season * fraction ) )
			{
				//DebugMessage( Excellent_Target_For_A_Season, fraction, "excellent" );
				sColour = Constants.Colour.Bad;
			}
			else if ( theValue < ( Good_Target_For_A_Season * fraction ) )
			{
				//DebugMessage( Good_Target_For_A_Season, fraction, "good" );
				sColour = Constants.Colour.Average;
			}
			else if ( theValue < ( Average_Target_For_A_Season * fraction ) )
			{
				//DebugMessage( Average_Target_For_A_Season, fraction, "average" );
				sColour = Constants.Colour.Good;
			}
			else
			{
				//Utility.Announce( $"{theValue * fraction } is bad" );
				sColour = Constants.Colour.Excellent;
			}
			return sColour;
		}

		private static void DebugMessage( int target, decimal fraction, string theWord )
		{
			var theValue = target * fraction;
			Utility.Announce( $"Less than {theValue:0.00} is {theWord}" );
		}

		private string TotPkBgPicker( int theValue )
		{
			const int Excellent_Target_For_A_Game = 4;
			const int Good_Target_For_A_Game = 6;
			const int Average_Target_For_A_Game = 8;

			string sColour;

			if ( theValue < ( Excellent_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Excellent;
			else if ( theValue < ( Good_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Good;
			else if ( theValue < ( Average_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Average;
			else
				sColour = Constants.Colour.Bad;
			return sColour;
		}

		private string TotTeBgPicker( int theValue )
		{
			const int Excellent_Target_For_A_Game = 3;
			const int Good_Target_For_A_Game = 5;
			const int Average_Target_For_A_Game = 10;

			string sColour;

			if ( theValue < ( Excellent_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Excellent;
			else if ( theValue < ( Good_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Good;
			else if ( theValue < ( Average_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Average;
			else
				sColour = Constants.Colour.Bad;
			return sColour;
		}

		private string TotWrBgPicker( int theValue )
		{
			const int Excellent_Target_For_A_Game = 10;  
			const int Good_Target_For_A_Game = 13;
			const int Average_Target_For_A_Game = 22; 

			string sColour;

			if ( theValue < ( Excellent_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Excellent;
			else if ( theValue < ( Good_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Good;
			else if ( theValue < ( Average_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Average;
			else
				sColour = Constants.Colour.Bad;
			return sColour;
		}

		public string TotRbBgPicker( int theValue )
		{
			const int Excellent_Target_For_A_Game = 5; 
			const int Good_Target_For_A_Game = 8; 
			const int Average_Target_For_A_Game = 19; 

			string sColour;
			if ( theValue < ( Excellent_Target_For_A_Game * Int32.Parse(Week) ) )
				sColour = Constants.Colour.Excellent;
			else if ( theValue < ( Good_Target_For_A_Game * Int32.Parse( Week ) ) )
				sColour = Constants.Colour.Good;
			else if ( theValue < ( Average_Target_For_A_Game * Int32.Parse( Week ) ) )
			{
				sColour = Constants.Colour.Average;
			}
			else
			{
				Utility.Announce( $"{theValue * Int32.Parse( Week )  } is bad" );
				sColour = Constants.Colour.Bad;
			}
			return sColour;
		}

        public string Simple32BgPicker( int theValue )
        {
            string sColour;
            if ( theValue < 4 )
                sColour = Constants.Colour.Excellent;
            else if ( theValue < 10 )
                sColour = Constants.Colour.Good;
            else if ( theValue < 30 )
                sColour = Constants.Colour.Average;
            else
                sColour = Constants.Colour.Bad;
            return sColour;
        }

        public override string OutputFilename()
		{
			return FileOut;
		}
	}

	public class FptsAllowed
	{
        public string TeamCode { get; set; }
        public decimal ToQbs { get; set; }
		public decimal ToRbs { get; set; }
		public decimal ToWrs { get; set; }
		public decimal ToTes { get; set; }
		public decimal ToPks { get; set; }
        public int ToQbsRank { get; set; }
        public int ToRbsRank { get; set; }
        public int ToWrsRank { get; set; }
        public int ToTesRank { get; set; }
        public int ToPksRank { get; set; }
        public int TotalRank { get; set; }

        public FptsAllowed(string teamCode)
        {
            TeamCode = teamCode;
        }
		internal decimal TotPtsAllowed()
		{
			return ToQbs + ToRbs + ToWrs + ToTes + ToPks;
		}

		internal void Add( FptsAllowed fptsAllowed )
		{
			ToQbs += fptsAllowed.ToQbs;
			ToRbs += fptsAllowed.ToRbs;
			ToWrs += fptsAllowed.ToWrs;
			ToTes += fptsAllowed.ToTes;
			ToPks += fptsAllowed.ToPks;
		}

        internal void IncrementBy( FptsAllowed fptsAllowed )
        {
            ToQbs += fptsAllowed.ToQbs;
            ToRbs += fptsAllowed.ToRbs;
            ToWrs += fptsAllowed.ToWrs;
            ToTes += fptsAllowed.ToTes;
            ToPks += fptsAllowed.ToPks;
        }
    }
}