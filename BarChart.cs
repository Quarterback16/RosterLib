//using Excel = Microsoft.Office.Interop.Excel;
//using Word = Microsoft.Office.Interop.Word;

namespace RosterLib
{
   public class BarChart
    {
        public string Title { get; set; }


        //public void Plot()
        //{
        //    object missing = System.Reflection.Missing.Value;
        //    Word.Application word = null;
        //    word = new Word.Application();
        //    word.Visible = true;
        //    Word.Document doc = word.Documents.Add( ref missing, ref missing, ref missing, ref missing);

        //    //Word.Chart wdChart = doc.InlineShapes.AddChart(
        //    //    Microsoft.Office.Core.XlChartType.xl3DColumn, ref missing ).Chart;

        //    Word.Chart wdChart = doc.InlineShapes.AddChart(
        //       Microsoft.Office.Core.XlChartType.xlBarStacked, ref missing).Chart;

        //    Word.ChartData chartData = wdChart.ChartData;
        //    //  corresponds to the Excel Workbook containing the data of your chart
        //    Excel.Workbook dataWorkbook = (Excel.Workbook) chartData.Workbook;   
        //    Excel.Worksheet dataSheet = (Excel.Worksheet) dataWorkbook.Worksheets[1];

        //    // set the data
        //    Excel.Range tRange = dataSheet.Cells.get_Range( "A1", "B5" );
        //    ((Excel.Range)dataSheet.Cells.get_Range("A2", missing)).FormulaR1C1 = "Bikes";
        //    ((Excel.Range)dataSheet.Cells.get_Range("A3", missing)).FormulaR1C1 = "Accessories";
        //    ((Excel.Range)dataSheet.Cells.get_Range("A4", missing)).FormulaR1C1 = "Repairs";
        //    ((Excel.Range)dataSheet.Cells.get_Range("A5", missing)).FormulaR1C1 = "Clothing";
        //    ((Excel.Range)dataSheet.Cells.get_Range("B2", missing)).FormulaR1C1 = "1000";
        //    ((Excel.Range)dataSheet.Cells.get_Range("B3", missing)).FormulaR1C1 = "2500";
        //    ((Excel.Range)dataSheet.Cells.get_Range("B4", missing)).FormulaR1C1 = "4000";
        //    ((Excel.Range)dataSheet.Cells.get_Range("B5", missing)).FormulaR1C1 = "3000";

        //    wdChart.HasLegend = false;
        //    wdChart.HasTitle = true;
        //    wdChart.ChartTitle.Text = Title;

        //    // font
        //    wdChart.ChartTitle.Font.Italic = true;
        //    wdChart.ChartTitle.Font.Size = 18;  
        //    wdChart.ChartTitle.Font.Color = Color.Black.ToArgb();

        //    //wdChart.ApplyDataLabels( 
        //    //    Word.XlDataLabelsType.xlDataLabelsShowLabel, 
        //    //    missing, missing, missing, missing, missing, missing, missing, missing, missing);

        //    dataWorkbook.Application.Quit();

        //}
    }
}
