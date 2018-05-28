using System;
using System.Collections;
using System.Data;
using System.Linq;


namespace RosterLib
{
	/// <summary>
	/// A Frequency Table is a statistical representation of a set of numbers.
	/// </summary>
	public class FrequencyTable
	{
		private string name;
		private ArrayList dataList;
		private DataTable dt;
		private int cumTotal = 0;
		private decimal totalMetric = 0.0M;
		private decimal min = 999999.9M;
		private decimal max = 0.0M;
		private decimal percentile33 = 0.0M;
		private decimal percentile67 = 0.0M;

		public string FileName { get; set; }

		public FrequencyTable( string name )
		{
			this.name = name;
			this.dataList = new ArrayList();
		}

		public FrequencyTable( string name, ArrayList dataList )
		{
			this.name = name;
			this.dataList = dataList;
		}

		public void Add( Object obj )
		{
			this.dataList.Add( obj );
		}

		public decimal Average()
		{
			return Average( CumulativeTotal, Count );
		}

		public decimal Variance()
		{
			if ( Count == 0 ) return 0.0M;  //  avoids the divide by zero situation

			var av = Average();
			var sum = (from decimal item in dataList 
						  select Math.Abs(item - av) into diff select diff*diff).Sum();

			return ( sum / Count );
		}

		public double StandardDeviation()
		{
			return Math.Sqrt( Double.Parse( Variance().ToString() ) );
		}

		
		#region  Calculate

		public void Calculate()
		{
			int nCount = 0;

			if (dataList.Count > 0)
			{ 
				dataList.Sort();
				decimal lastItem = (decimal) dataList[0];
				decimal item = 0.0M;

				#region  build a data table
				dt = new DataTable();
				DataColumnCollection cols = dt.Columns;
				DataColumn col = cols.Add( "Occurence", typeof(System.Decimal ) );
				cols.Add( "Frequency", typeof(System.Int32) );
				cols.Add( "Cumulative", typeof(System.Int32) );
				cols.Add( "Percent", typeof(System.Decimal) );
				cols.Add( "TotPercent", typeof(System.Decimal) );
				#endregion

				#region  fill the table

				for ( int i = 0; i < dataList.Count; i++ )
				{
					item = (decimal) dataList[i];
					if ( item != lastItem )
					{
						AddRow( dt, lastItem, nCount, cumTotal );
						nCount = 0;
						lastItem = item;
					}
					nCount++;
					cumTotal++;
					totalMetric += item;
					if ( item < min ) min = item;
					if ( item > max ) max = item;
					if ( percentile33 == 0.0M )
					{
						if ( Percent( cumTotal, Count ) > 33 )  percentile33 = item;
					}
					if ( percentile67 == 0.0M )
					{
						if ( Percent( cumTotal, Count ) > 67 )  percentile67 = item;
					}
				} 
				AddRow( dt, lastItem, nCount, cumTotal );

			#endregion
			}


		}

		private void AddRow( DataTable dt, decimal item, int count, int cumulative )
		{
			DataRow dr = dt.NewRow();
			dr[ "Occurence"  ] = item;
			dr[ "Frequency"  ] = count;
			dr[ "Cumulative" ] = cumulative;
			dr[ "Percent"    ] = Percent( count, Count );
			dr[ "TotPercent" ] = Percent( cumulative, Count );
			dt.Rows.Add( dr );
		}

		private decimal Percent( int quotient, int divisor )
		{
			return 100 * Average( quotient, divisor );
		}

		private decimal Average( int quotient, int divisor )
		{
			//  need to do decimal other wise INT() will occur
			return ( System.Decimal.Parse( quotient.ToString() )  / 
				System.Decimal.Parse( divisor.ToString() ) );
		}

		private decimal Average( decimal quotient, int divisor )
		{
			if ( divisor == 0 ) return 0.0M;
			//  need to do decimal other wise INT() will occur
			return ( quotient  / System.Decimal.Parse( divisor.ToString() ) );
		}

		#endregion

		public void RenderAsHtml( string fileName, string season )
		{
			var st = new SimpleTableReport( "Frequency Table " + name, StdFooter()) {ColumnHeadings = true};

			st.AddColumn( new ReportColumn( "Occurence",    "Occurence",  "{0:##0}"   ) ); 
			st.AddColumn( new ReportColumn( "Freq.",        "Frequency",  "{0}"       ) ); 
			st.AddColumn( new ReportColumn( "Cum. Freq.",   "Cumulative", "{0}"       ) ); 
			st.AddColumn( new ReportColumn( "Percent ",     "Percent",    "{0:##0.0}" ) ); 
			st.AddColumn( new ReportColumn( "Cum. %",       "TotPercent", "{0:##0.0}" ) ); 

			st.LoadBody( dt );
			st.DoRowNumbers = false;
			st.ShowElapsedTime = false;
			FileName = string.Format("{0}{2}\\Frequency\\{1}.htm", Utility.OutputDirectory(), name, season);
         st.RenderAsHtml( FileName, true);
		}

		private string StdFooter()
		{
			//  a table with some magic numbers
			string footer = "";
			footer = HtmlLib.TableOpen( "border='0' cellpadding='1' cellspacing='1'" );
			footer += AddFooterLine( "Mean", Average() );
			footer += AddFooterLine( "Max", Int32.Parse( string.Format( "{0:0}", Maximum ) ) );
			footer += AddFooterLine( "Min", Int32.Parse( string.Format( "{0:0}", Minimum ) ) );
			footer += AddFooterLine( "Variance", Variance() );
			footer += AddFooterLine( "Std Deviation", Decimal.Parse( StandardDeviation().ToString() ) );
			footer += AddFooterLine( "Percentile 33", Int32.Parse( string.Format( "{0:0}", Percentile33 ) ) );
			footer += AddFooterLine( "Percentile 67", Int32.Parse( string.Format( "{0:0}", Percentile67 ) ) );
			footer += AddFooterLine( "Total", CumulativeTotal );
			footer += AddFooterLine( "Team average", ( CumulativeTotal / 32 ) );
			return footer + HtmlLib.TableClose();
		}

		private string AddFooterLine( string label, decimal amount )
		{
			return HtmlLib.TableRowOpen() + HtmlLib.TableData( label ) + 
				HtmlLib.TableDataAttr( string.Format( "{0:##0.0}", amount ), "ALIGN='RIGHT'" ) +
				HtmlLib.TableRowClose();
		}

		private string AddFooterLine( string label, int amount )
		{
			return HtmlLib.TableRowOpen() + HtmlLib.TableData( label ) + 
				HtmlLib.TableDataAttr( amount.ToString(), "ALIGN='RIGHT'" ) +
				HtmlLib.TableRowClose();
		}


		#region  Accessors

		public int Count
		{
			get { return dataList.Count; }
		}

		public decimal Minimum
		{
			get { return min; }
		}

		public decimal Maximum
		{
			get { return max; }
		}

		public decimal Percentile33
		{
			get { return percentile33; }
		}

		public decimal Percentile67
		{
			get { return percentile67; }
		}

		public decimal CumulativeTotal
		{
			get { return totalMetric; }
			set { totalMetric = value; }
		}

		#endregion

	}
}
