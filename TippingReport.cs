using System;
using System.Collections;

namespace RosterLib
{
   public class TippingReport
   {
      private readonly string m_Season;
      private readonly ArrayList weekList;
      
      public TippingReport()
      {
         m_Season = Utility.CurrentSeason();
         weekList = new ArrayList();
         for (var i = 1; i <= Constants.K_WEEKS_IN_REGULAR_SEASON; i++)
            weekList.Add(new NFLWeek(Int32.Parse(m_Season), i));
      }

      public void Render()
      {
         var str = new SimpleTableReport("Tipping Report " + m_Season );
         str.AddStyle("#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }");
         str.AddStyle("#main { margin-left:1em; }");
         str.AddStyle("#dtStamp { font-size:0.8em; }");
         str.AddStyle(".end { clear: both; }");
         str.AddStyle(".gponame { color:white; background:black }");
         str.AddStyle("label { display:block; float:left; width:130px; padding: 3px 5px; margin: 0px 0px 5px 0px; text-align:right; }");
         str.AddStyle("value { display:block; float:left; width:100px; padding: 3px 5px; margin: 0px 0px 5px 0px; text-align:left; font-weight: bold; color:blue }");
         str.AddStyle("#notes { float:right; height:auto; width:308px; font-size: 88%; background-color: #ffffe1; border: 1px solid #666666; padding: 5px; margin: 0px 0px 10px 10px; color:#666666 }");
         str.AddStyle("div.notes H4 { background-image: url(images/icon_info.gif); background-repeat: no-repeat; background-position: top left; padding: 3px 0px 3px 27px; border-width: 0px 0px 1px 0px; border-style: solid; border-color: #666666; color: #666666; font-size: 110%;}");
         str.ColumnHeadings = true;
         str.DoRowNumbers = false;
         str.ShowElapsedTime = false;
         str.IsFooter = false;
         str.AddColumn(new ReportColumn("Wk", "WEEK", "{0}", typeof(String) ));
         str.AddColumn(new ReportColumn("G", "GAMES", "{0}", typeof(Int32), true ));
         str.AddColumn(new ReportColumn("Spr", "SPREAD", "{0}", typeof(Int32), true ));
         str.AddColumn(new ReportColumn("MyT", "MYTIPS", "{0}", typeof(Int32), true ));
         str.AddColumn(new ReportColumn("ATS", "ATS", "{0}", typeof(Int32), true));
         str.AddColumn(new ReportColumn("H", "HOME", "{0}", typeof(Int32), true));
         BuildTable(str);
         str.RenderAsHtml(string.Format("{0}//tipping{1}.htm", Utility.OutputDirectory(), m_Season), true);

      }

      private void BuildTable(SimpleTableReport str)
      {
         if (weekList != null)
         {
            foreach (NFLWeek w in weekList)
            {
            	if (w == null) continue;
            	if (!w.HasPassed()) continue;
            	var dr = str.Body.NewRow();
            	dr["WEEK"] = w.Week;                 
            	dr[ "GAMES" ] = w.GameList().Count;
            	dr[ "SPREAD" ] = w.SpreadTotalCorrect();
            	dr[ "MYTIPS" ] = w.MyTipsCorrect();
            	dr["ATS"] = w.MyAtsCorrect();                     
            	dr["HOME"] = w.TotalHomeWins();

            	str.Body.Rows.Add(dr);
            }
         }
         return;
      }
      
   }
}
