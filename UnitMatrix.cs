using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	/// A data structure for a teams experience points.
	/// </summary>
	public class UnitMatrix
	{
		#region  Privates

		private readonly string [] _opponent;

      public SimpleTableReport St;

      DataTable _dt;

		#endregion
		
		#region  Constants
		public const string KUnitNamePassRush       = "Pass Rush";
		public const string KUnitNamePassingDefence = "Passing Defence";
		public const string KUnitNameRushingDefence = "Rushing Defence";
		public const string KUnitNamePassingOffence = "Passing Offence";
		public const string KUnitNameRushingOffence = "Rushing Offence";
		public const string KUnitNamePassProtection = "Pass Protection";

		//  3 columns ( 0 = 1 * 2 )
		public const int KCol0ExperiencePts = 0;
		public const int KCol1Metric = 1;
		public const int KCol2Multiplier = 2;
		//  total row
		public const int KRowTotal = Constants.K_WEEKS_IN_A_SEASON;

		#endregion
		
		#region  Constructors
		
		public UnitMatrix( NflTeam teamIn )
		{
         Team = teamIn;
			TeamCode = teamIn.TeamCode;
			//  Create Arrays
         PoExp = new decimal[ KRowTotal+1, 3 ];   //  3 numbers kept each week, total kept in cell offset 17
         RoExp = new decimal[ KRowTotal+1, 3 ];
         PpExp = new decimal[ KRowTotal+1, 3 ];
         PrExp = new decimal[ KRowTotal+1, 3 ];
         RdExp = new decimal[ KRowTotal+1, 3 ];
         PdExp = new decimal[ KRowTotal+1, 3 ];

         _opponent = new String[ KRowTotal ];

         BuildTable();
      }

		#endregion
		
      private void BuildTable()
      {
         _dt = new DataTable();
         var cols = _dt.Columns;
         cols.Add( "UnitName", typeof(String ) );
         cols.Add( "Week01", typeof(Decimal) );
         cols.Add( "Week02", typeof(Decimal) );
         cols.Add( "Week03", typeof(Decimal) );
         cols.Add( "Week04", typeof(Decimal) );
         cols.Add( "Week05", typeof(Decimal) );
         cols.Add( "Week06", typeof(Decimal) );
         cols.Add( "Week07", typeof(Decimal) );
         cols.Add( "Week08", typeof(Decimal) );
         cols.Add( "Week09", typeof(Decimal) );
         cols.Add( "Week10", typeof(Decimal) );
         cols.Add( "Week11", typeof(Decimal) );
         cols.Add( "Week12", typeof(Decimal) );
         cols.Add( "Week13", typeof(Decimal) );
         cols.Add( "Week14", typeof(Decimal) );
         cols.Add( "Week15", typeof(Decimal) );
         cols.Add( "Week16", typeof(Decimal) );
         cols.Add( "Week17", typeof(Decimal) );
			cols.Add( "Week18", typeof(Decimal) );
			cols.Add( "Week19", typeof(Decimal) );
			cols.Add( "Week20", typeof(Decimal) );
			cols.Add( "Week21", typeof(Decimal) );
			cols.Add( "Total",  typeof(Decimal) );
			cols.Add( "Rank",   typeof(String ) );
		}


      public void ClearTable()
      {
         _dt.Clear();
      }

      
		#region  incrementors

		public void AddMetrics( GameStats game, bool bHome )
		{
			GamesPlayed++;
			AddPoMetric( game, bHome );
			AddRoMetric( game, bHome );
			AddPpMetric( game, bHome );
			AddPrMetric( game, bHome );
			AddRdMetric( game, bHome );
			AddPdMetric( game, bHome );
		}
			
		public void AddPoPoints( decimal points, decimal multiplier, GameStats game )
      {
         PoExp[ game.Week-1, KCol0ExperiencePts ] += ( points * multiplier );
         PoExp[ KRowTotal, KCol0ExperiencePts ] += ( points * multiplier );
         PoExp[ game.Week-1, KCol2Multiplier ] += multiplier;
      }
		
		public void AddPoMetric( GameStats game, bool bHome )
		{
			var metric = ( bHome ) ? Decimal.Parse( game.HomeTDpasses.ToString() ):
					                       Decimal.Parse( game.AwayTDpasses.ToString() );

			PoExp[ game.Week-1, KCol1Metric ] += metric;
			PoExp[ KRowTotal, KCol1Metric ] += metric;
			
			//  Also count Tdp and Fg
			if ( bHome )
			{
				TotTDp += Int32.Parse( game.HomeTDpasses.ToString() );
            TotYDp += Decimal.Parse(game.HomePassingYds.ToString());
            TotYDr += Decimal.Parse(game.HomeRushingYds.ToString());
            TotFGs += Int32.Parse(game.HomeFGs.ToString());
			}
			else
			{
				TotTDp += Int32.Parse( game.AwayTDpasses.ToString() );
            TotYDp += Decimal.Parse(game.AwayPassingYds.ToString());
            TotYDr += Decimal.Parse(game.AwayRushingYds.ToString());
            TotFGs += Int32.Parse(game.AwayFGs.ToString());				
			}
		}

      public void AddPoBonus( decimal points, GameStats game )
      {
         PoExp[ game.Week-1, KCol0ExperiencePts ] += points;
         PoExp[ KRowTotal, KCol0ExperiencePts ] += points;
      }

      public void AddPdPoints( decimal points, decimal multiplier, GameStats game )
      {
         PdExp[ game.Week-1, KCol0ExperiencePts ] += ( points * multiplier );
         PdExp[ KRowTotal, KCol0ExperiencePts ] += ( points * multiplier );
         PdExp[ game.Week-1, KCol2Multiplier ] += multiplier;
      }

		public void AddPdMetric( GameStats game, bool bHome )
		{
			var metric = ( bHome ) ? decimal.Parse( game.AwayTDpasses.ToString() ):
				decimal.Parse( game.HomeTDpasses.ToString() );

			PdExp[ game.Week-1, KCol1Metric ] += metric;
			PdExp[ KRowTotal, KCol1Metric ] += metric;
		}

      public void AddPdBonus( decimal points, GameStats game )
      {
         PdExp[ game.Week-1, KCol0ExperiencePts ] += points;
         PdExp[ KRowTotal, KCol0ExperiencePts ] += points;
      }

      public void AddRoPoints( decimal points, decimal multiplier, GameStats game )
      {
         RoExp[ game.Week-1, KCol0ExperiencePts ] += ( points * multiplier );
         RoExp[ KRowTotal, KCol0ExperiencePts ] += ( points * multiplier );
         RoExp[ game.Week-1, KCol2Multiplier ] += multiplier;
      }

		public void AddRoMetric( GameStats game, bool bHome )
		{
			var metric = ( bHome ) ? Decimal.Parse( game.HomeTDruns.ToString() ):
				Decimal.Parse( game.AwayTDruns.ToString() );

			RoExp[ game.Week-1, KCol1Metric ] += metric;
			RoExp[ KRowTotal, KCol1Metric ] += metric;
			
			//  Also count Tdr
			if ( bHome )
				TotTDr += Int32.Parse( game.HomeTDruns.ToString() );
			else
				TotTDr += Int32.Parse( game.AwayTDruns.ToString() );
		}

      public void AddRoBonus( decimal points, GameStats game )
      {
         RoExp[ game.Week-1, KCol0ExperiencePts ] += points;
         RoExp[ KRowTotal, KCol0ExperiencePts ] += points;
      }

      public void AddRdPoints( decimal points, decimal multiplier, GameStats game )
      {
         RdExp[ game.Week-1, KCol0ExperiencePts ] +=  ( points * multiplier );
         RdExp[ KRowTotal, KCol0ExperiencePts ] +=  ( points * multiplier );
         RdExp[ game.Week-1, KCol2Multiplier ] += multiplier;
      }

		public void AddRdMetric( GameStats game, bool bHome )
		{
			var metric = ( bHome ) ? decimal.Parse( game.AwayTDruns.ToString() ):
				decimal.Parse( game.HomeTDruns.ToString() );

			RdExp[ game.Week-1, KCol1Metric ] += metric;
			RdExp[ KRowTotal, KCol1Metric ] += metric;
		}

      public void AddRdBonus( decimal points, GameStats game )
      {
         RdExp[ game.Week-1, KCol0ExperiencePts ] += points;
         RdExp[ KRowTotal, KCol0ExperiencePts ] += points;
      }

      public void AddPpPoints( decimal points, decimal multiplier, GameStats game )
      {
         PpExp[ game.Week-1, KCol0ExperiencePts ] += ( points * multiplier );
         PpExp[ KRowTotal, KCol0ExperiencePts ] += ( points * multiplier );
         PpExp[ game.Week-1, KCol2Multiplier ] += multiplier;
      }

		public void AddPpMetric( GameStats game, bool bHome )
		{
			var metric = ( bHome ) ? Decimal.Parse( game.HomeSacksAllowed.ToString() ):
				Decimal.Parse( game.AwaySacksAllowed.ToString() );

			PpExp[ game.Week-1, KCol1Metric ] += Int32.Parse( string.Format( "{0:0}", metric ) );
			PpExp[ KRowTotal, KCol1Metric ] += Int32.Parse( string.Format( "{0:0}", metric ) );
		}

      public void AddPpBonus( decimal points, GameStats game )
      {
         PpExp[ game.Week-1, KCol0ExperiencePts ] += points;
         PpExp[ KRowTotal, KCol0ExperiencePts ] += points;
      }

      public void AddPrPoints( decimal points, decimal multiplier, GameStats game )
      {
         PrExp[ game.Week-1, KCol0ExperiencePts ] += ( points * multiplier );
         PrExp[ KRowTotal, KCol0ExperiencePts ] += ( points * multiplier );
         PrExp[ game.Week-1, KCol2Multiplier ] += multiplier;
      }

		public void AddPrMetric( GameStats game, bool bHome )
		{
			var metric = ( bHome ) ? Decimal.Parse( game.HomeSacks.ToString() ):
				Decimal.Parse( game.AwaySacks.ToString() );

			PrExp[ game.Week-1, KCol1Metric ] += Decimal.Parse( string.Format( "{0:0.0}", metric ) );
			PrExp[ KRowTotal, KCol1Metric ] += Decimal.Parse( string.Format( "{0:0.0}", metric ) );
		}

      public void AddPrBonus( decimal points, GameStats game )
      {
         PrExp[ game.Week-1, KCol0ExperiencePts ] += points;
         PrExp[ KRowTotal, KCol0ExperiencePts ] += points;
      }

      #endregion

      #region  Accessors

      #region  Exp Points
      public decimal PoPoints
      {
         get { return PoExp[ KRowTotal, KCol0ExperiencePts ]; }
         set { PoExp[ KRowTotal, KCol0ExperiencePts] = value; }
      }

      public decimal RoPoints
      {
         get { return RoExp[ KRowTotal, KCol0ExperiencePts ]; }
         set { RoExp[ KRowTotal, KCol0ExperiencePts ] = value; }
      }

      public decimal PpPoints
      {
         get { return PpExp[ KRowTotal, KCol0ExperiencePts ]; }
         set { PpExp[ KRowTotal, KCol0ExperiencePts ] = value; }
      }

      public decimal PrPoints
      {
         get { return PrExp[ KRowTotal, KCol0ExperiencePts ]; }
         set { PrExp[ KRowTotal, KCol0ExperiencePts ] = value; }
      }
      
		public decimal RdPoints
      {
         get { return RdExp[ KRowTotal, KCol0ExperiencePts ]; }
         set { RdExp[ KRowTotal, KCol0ExperiencePts ] = value; }
      }
      
		public decimal PdPoints
      {
         get { return PdExp[ KRowTotal, KCol0ExperiencePts ]; }
         set { PdExp[ KRowTotal, KCol0ExperiencePts ] = value; }
      }

      #endregion

      #region  Metrics
      
		public decimal PoMetrics
      {
         get { return PoExp[KRowTotal, KCol1Metric ]; }
         set { PoExp[KRowTotal, KCol1Metric ] = value; }
      }
	   
      public decimal PoMetricsWeek( int week )
      {
         return PoExp[week, KCol1Metric ];
      }	   

      public decimal RoMetrics
      {
         get { return RoExp[ KRowTotal, 1]; }
         set { RoExp[ KRowTotal, 1] = value; }
      }

      public decimal PpMetrics
      {
         get { return PpExp[ KRowTotal, 1]; }
         set { PpExp[ KRowTotal, 1] = value; }
      }

      public decimal PrMetrics
      {
         get { return PrExp[ KRowTotal, 1]; }
         set { PrExp[ KRowTotal, 1] = value; }
      }
      public decimal RdMetrics
      {
         get { return RdExp[ KRowTotal, 1]; }
         set { RdExp[ KRowTotal, 1] = value; }
      }
      public decimal PdMetrics
      {
         get { return PdExp[ KRowTotal, 1]; }
         set { PdExp[ KRowTotal, 1] = value; }
      }

      #endregion          
      
      public decimal TotalPoints
      {
         get { return PoPoints + RoPoints + PpPoints + PrPoints + RdPoints + PdPoints; }
      }
      public decimal TotalOffPoints
      {
			get { return PoPoints + RoPoints + PpPoints; }
      }
      public decimal TotalDefPoints
      {
			get { return PrPoints + RdPoints + PdPoints; }
      }

		public decimal AveragePoMetric
		{
			get { return ( PoMetrics / 32 ); }
		}

		public decimal AverageRoMetric
		{
			get { return ( RoMetrics / 32 ); }
		}

		public decimal AveragePpMetric
		{
			get { return ( PpMetrics / 32 ); }
		}

		public decimal AveragePrMetric
		{
			get { return ( PrMetrics / 32 ); }
		}

		public decimal AverageRdMetric
		{
			get { return ( RdMetrics / 32 ); }
		}

		public decimal AveragePdMetric
		{
			get { return ( PdMetrics / 32 ); }
		}

		public decimal PassingDvoa
		{
			get { return 1.0M - (AveragePoMetric / 1.2M ); }
		}
		
		public decimal RushingDvoa
		{
			get { return 1.0M - (AverageRoMetric / 0.8M ); }
		}

		public NflTeam Team { get; set; }

		public decimal GamesPlayed { get; set; }

		public string TeamCode { get; set; }

		public int TotTDp { get; set; }

		public int TotTDr { get; set; }

		public int TotFGs { get; set; }

		public decimal[,] PoExp { get; set; }

		public decimal[,] RoExp { get; set; }

		public decimal[,] PpExp { get; set; }

		public decimal[,] PrExp { get; set; }

		public decimal[,] RdExp { get; set; }

		public decimal[,] PdExp { get; set; }

		public decimal TotYDp { get; set; }

		public decimal TotYDr { get; set; }

		public decimal[,] GetUnit( string unitCode )
		{
			return GetExp( unitCode );
		}

		private decimal[,] GetExp( string unitCode )
		{
			decimal[,] exp = null;
			switch (unitCode)
			{
				case "PO":
					exp = PoExp;
					break;
				case "RO":
					exp = RoExp;
					break;
				case "PP":
					exp = PpExp;
					break;
				case "PR":
					exp = PrExp;
					break;
				case "RD":
					exp = RdExp;
					break;
				case "PD":
					exp = PdExp;
					break;					

			}
			return exp;
		}

#endregion
		
		public void AddUnit( string unitCode, decimal ep )
		{
			var exp = GetExp( unitCode );
			exp[ KRowTotal, KCol0ExperiencePts ] = ep;
		}
		
      private void LoadWeeks( string unitName, decimal[,] exp )
      {
         var dr = _dt.NewRow();
         dr[ "UnitName" ] = unitName;
			var gotoWeek = Utility.CurrentWeek() == "00" ? KRowTotal : Int32.Parse( Utility.CurrentWeek() );
			for (var i = 0; i < gotoWeek; i++ )
			{
            var fieldName = string.Format( "WEEK{0:0#}", i+1 );
            dr[ fieldName ] = exp[ i, KCol0ExperiencePts ];
         }
         dr[ "TOTAL" ] = exp[ KRowTotal, KCol0ExperiencePts ];
			if ( Utility.Wz == null )
				dr[ "RANK"  ] = " ";
			else
			   dr[ "RANK"  ] = Utility.Wz.GetRank( Team.TeamCode, unitName );
			_dt.Rows.Add( dr );
      }

      private void LoadMetric( string unitName, decimal[,] exp )
      {
         var dr = _dt.NewRow();
         dr[ "UnitName" ] = HtmlLib.HTMLPadL( unitName, 3 );
      	var gotoWeek = Utility.CurrentWeek() == "00" ? KRowTotal : Int32.Parse( Utility.CurrentWeek() );
      	for (var i = 0; i < gotoWeek; i++ )
         {
            string fieldName = string.Format( "WEEK{0:0#}", i+1 );
            dr[ fieldName ] = exp[ i, KCol1Metric ];
         }
         dr[ "TOTAL" ] = exp[ KRowTotal, KCol1Metric ];
         _dt.Rows.Add( dr );
      }

      private void LoadMultiplier( string name, decimal[,] exp )
      {
         DataRow dr = _dt.NewRow();
         dr[ "UnitName" ] = HtmlLib.HTMLPadL( name, 3 );
			int gotoWeek = Utility.CurrentWeek() == "00" ? KRowTotal : Int32.Parse( Utility.CurrentWeek() );
			for (int i = 0; i < gotoWeek; i++ )
			{
            string fieldName = string.Format( "WEEK{0:0#}", i+1 );
            dr[ fieldName ] = exp[ i, KCol2Multiplier ];
         }
         dr[ "TOTAL" ] = exp[ KRowTotal, KCol2Multiplier ];
         _dt.Rows.Add( dr );
      }

      private void TotalLine()
      {
         var dr = _dt.NewRow();
         dr[ "UnitName" ] = "Totals";
			int gotoWeek = Utility.CurrentWeek() == "00" ? KRowTotal : Int32.Parse( Utility.CurrentWeek() );
			for (int i = 0; i < gotoWeek; i++ )
			{
            string fieldName = string.Format( "WEEK{0:0#}", i+1 );
            dr[ fieldName ] = 
               PoExp[ i, KCol0ExperiencePts ] + RoExp[ i, KCol0ExperiencePts ] + PpExp[ i, KCol0ExperiencePts ] +
               PrExp[ i, KCol0ExperiencePts ] + RdExp[ i, KCol0ExperiencePts ] + PdExp[ i, KCol0ExperiencePts ];
         }
         dr[ "TOTAL" ] = 
            PoExp[ KRowTotal, KCol0ExperiencePts ] + RoExp[ KRowTotal, KCol0ExperiencePts ] + PpExp[ KRowTotal, KCol0ExperiencePts ] +
            PrExp[ KRowTotal, KCol0ExperiencePts ] + RdExp[ KRowTotal, KCol0ExperiencePts ] + PdExp[ KRowTotal, KCol0ExperiencePts ];
         _dt.Rows.Add( dr );
      }

      public void SetOpponent( int week, string oppCode )
      {
         _opponent[ week-1 ] = oppCode;
      }

		/// <summary>
		/// Renders the Matrix as HTML.
		/// </summary>
		/// <param name="header1">The header.</param>
		/// <param name="defence">if set to <c>true</c> [defence].</param>
		/// <param name="offence">if set to <c>true</c> [offence].</param>
		/// <param name="persist">if set to <c>true</c> [persist].</param>
		/// <param name="showRank">if set to <c>true</c> [show rank].</param>
		/// <returns></returns>
      public string RenderAsHtml( string header1, bool defence, bool offence, bool persist, bool showRank )
      {
         //  load table
			_dt.Clear();
         if ( offence )
         {
            LoadWeeks( KUnitNamePassingOffence, PoExp );
            //  for debug show multiplier
            LoadMultiplier( "PD multiplier", PoExp );
            LoadMetric( "Tdp", PoExp );
         
            LoadWeeks( KUnitNameRushingOffence, RoExp );
            LoadMultiplier( "RD multiplier", RoExp );
            LoadMetric( "Tdr", RoExp );

            LoadWeeks( KUnitNamePassProtection, PpExp );
            LoadMultiplier( "PR multiplier", PpExp );
            LoadMetric( "SAK allowed", PpExp );
         }

         if ( defence )
         {
            LoadWeeks( KUnitNamePassRush, PrExp );
            LoadMultiplier( "PP multiplier", PrExp );
            LoadMetric( "SAK", PrExp );
         
            LoadWeeks( KUnitNameRushingDefence, RdExp );
            LoadMultiplier( "RO multiplier", RdExp );
            LoadMetric( "Tdr", RdExp );
         
            LoadWeeks( KUnitNamePassingDefence, PdExp );
            LoadMultiplier( "PO multiplier", PdExp );
            LoadMetric( "Tdp", PdExp );
         }

         if ( defence && offence ) TotalLine();

			St = new SimpleTableReport("Unit Matrix " + Team.TeamCode, "") {ColumnHeadings = true};

			St.AddColumn( new ReportColumn( header1,   "UnitName",        "{0}"   ) ); 
         St.AddColumn( new ReportColumn( "01<br>" + _opponent[0], "Week01",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "02<br>" + _opponent[1], "Week02",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "03<br>" + _opponent[2], "Week03",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "04<br>" + _opponent[3], "Week04",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "05<br>" + _opponent[4], "Week05",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "06<br>" + _opponent[5], "Week06",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "07<br>" + _opponent[6], "Week07",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "08<br>" + _opponent[7], "Week08",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "09<br>" + _opponent[8], "Week09",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "10<br>" + _opponent[9], "Week10",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "11<br>" + _opponent[10], "Week11",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "12<br>" + _opponent[11], "Week12",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "13<br>" + _opponent[12], "Week13",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "14<br>" + _opponent[13], "Week14",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "15<br>" + _opponent[14], "Week15",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "16<br>" + _opponent[15], "Week16",          "{0:#0.0}"   ) ); 
         St.AddColumn( new ReportColumn( "17<br>" + _opponent[16], "Week17",          "{0:#0.0}"   ) ); 
			St.AddColumn( new ReportColumn( "18<br>" + _opponent[16], "Week18",          "{0:#0.0}"   ) ); 
			St.AddColumn( new ReportColumn( "19<br>" + _opponent[16], "Week18",          "{0:#0.0}"   ) ); 
			St.AddColumn( new ReportColumn( "20<br>" + _opponent[16], "Week18",          "{0:#0.0}"   ) ); 
			St.AddColumn( new ReportColumn( "21<br>" + _opponent[16], "Week18",          "{0:#0.0}"   ) ); 
			St.AddColumn( new ReportColumn( "Tot",    "TOTAL",           "{0:#0.0}"   ) ); 
			if ( showRank )
				St.AddColumn( new ReportColumn( "Rank", "RANK", "{0}" ) ); 

         St.LoadBody( _dt );
         St.DoRowNumbers = false;
         St.ShowElapsedTime = false;
         if ( persist )
             St.RenderAsHtml(
					 string.Format("{0}Experience Points\\unitMatrix\\Unit_{1}.htm", 
							Utility.OutputDirectory(), Team.TeamCode), true);

         return St.BodyOut();
      }

		/// <summary>
		///   Formats the CSV line for output to ListPro or Excel.
		/// </summary>
		/// <returns></returns>
		public string CsvLine()
		{
         Team.SetRecord(Utility.LastSeason(), skipPostseason: false);
			return string.Format( "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", 
			   Utility.DivisionOut( Team.Division() ), 
				Team.NameOut(), 
				GamesPlayed,
				Convert.ToInt32(TotYDp),   //  YDp
				Convert.ToInt32( TotYDr ),   //  YDr
			   TotTDp + TotTDr,        //  TDs           
            TotTDp,                 //  Tdp
			   TotTDr,                 //  Tdr
			   TotFGs,                 //  FGs
			   Team.Wins
				);
		}
		
		public void DumpToLog()
		{
			Utility.Announce( string.Format( "Unit Matrix for {0}", Team.NameOut() ) );
			Utility.Announce( string.Format( "   GP:{0} PO:{1} RO:{2} Tdp:{3} Tdr:{4} PDVOA:{5:#0.00} RDVOA:{6:#0.00}",
			                                    GamesPlayed, PoMetrics, RoMetrics, TotTDp, TotTDr,
			                     	            PassingDvoa, RushingDvoa ) );
		}
	}
}
