using System;
using System.Collections;

namespace RosterLib
{
	/// <summary>
	/// Summary description for NFLRosterGrid.
	/// </summary>
	public class NFLRosterGrid 
	{
		public string FileOut = "";
		public string StrCat  = "2";
		public ArrayList DivList;
		public bool ShowBackups = true;
		public string CurrentLeague { get; set; }
		public string Season { get; set; }

        public NflConference Nfc;
        public NflConference Afc;

		public string Focus { get; set; }

		public NFLRosterGrid()
		{
			DivList =  new ArrayList();
		}

		public void LoadDivisions()
		{
			DivList.Clear();
			var div1 = new NFLDivision( "East", "NFC", "A", Season, StrCat );
			DivList.Add( div1 );
			var div2 = new NFLDivision( "North", "NFC", "B", Season, StrCat );
			DivList.Add( div2 );
			var div3 = new NFLDivision( "South", "NFC", "C", Season, StrCat );
			DivList.Add( div3 );
			var div4 = new NFLDivision( "West", "NFC", "D", Season, StrCat );
			DivList.Add( div4 );
			var div5 = new NFLDivision( "East", "AFC", "E", Season, StrCat );
			DivList.Add( div5 );
			var div6 = new NFLDivision( "North", "AFC", "F", Season, StrCat );
			DivList.Add( div6 );
			var div7 = new NFLDivision( "South", "AFC", "G", Season, StrCat );
			DivList.Add( div7 );
			var div8 = new NFLDivision( "West", "AFC", "H", Season, StrCat );
			DivList.Add( div8 );
		}

		public string CurrentRoster()
		{
			FileOut = string.Format( "{0}\\{3}\\Rosters\\{1}\\Roster-{2}-{4}.htm",
				Utility.OutputDirectory(), CurrentLeague, Focus, Season, Utility.CurrentWeek() );
			var h = new HtmlFile( FileOut, 
				Utility.CategoryOut( StrCat ) + " Roster as of " + DateTime.Now.ToString("dd MMM yy") +
				                        "  Week " + Utility.CurrentWeek() );
												
			h.AddToBody( Header() );
			h.AddToBody( TeamListOut() );
			h.AddToBody( Footer() );
			h.Render();

			//  copy to a current report (ignoring week)
			var currentFile = CurrentReportFilename();
			h.Render( currentFile );
			return currentFile;
		}

		public string CurrentReportFilename()
		{
			return string.Format("{0}{3}\\Rosters\\{1}\\Roster-{2}.htm",
					 Utility.OutputDirectory(), CurrentLeague, Focus, Season);
		}

		public void CurrentInjuries()
		{
         FileOut = string.Format("{0}Rosters\\{2}\\{3}\\Injuries{1}.htm", Utility.OutputDirectory(), StrCat,
				Season, CurrentLeague );
			var h = new HtmlFile( FileOut, Utility.CategoryOut( StrCat ) + " Injuries as of " + 
				                                 DateTime.Now.ToString("dd MMM yy") + 
															"  Week " + Utility.CurrentWeek() );
			h.AddToBody( Header() );
			h.AddToBody( InjuryListOut() );
			h.AddToBody( Footer() );
			h.Render();
		}

		private string TeamListOut()
		{
			var s = HtmlLib.TableOpen("BORDER=1 BORDERCOLOR=BLACK") + "\n" + HtmlLib.TableRowOpen() + "\n";
			var nCount = 0;
			var myEnumerator = DivList.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				nCount++;
				if ( nCount == 5 )
				{
					s += HtmlLib.TableRowClose() + "\n" + HtmlLib.TableRowOpen();
					nCount = 0;
				}

				var d = (NFLDivision) myEnumerator.Current;
				var strPos = Focus ?? string.Empty;
				s += d.DivHtml( ShowBackups, StrCat, strPos );				 
			}
			
			return s + HtmlLib.TableRowClose() + "\n" + HtmlLib.TableClose() + "\n";
		}

		private string InjuryListOut()
		{
			var s = HtmlLib.TableOpen("BORDER=1 BORDERCOLOR=BLACK") + "\n" +
				HtmlLib.TableRowOpen() + "\n";
			var nCount = 0;
			var myEnumerator = DivList.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				nCount++;
				if ( nCount == 5 )
				{
					s += HtmlLib.TableRowClose() + "\n" + HtmlLib.TableRowOpen();
					nCount = 0;
				}

				var d = (NFLDivision) myEnumerator.Current;
				s += d.DivInjuries();				 
			}
			
			return s + HtmlLib.TableRowClose() + "\n" +
				HtmlLib.TableClose() + "\n";
		}

		private static string Header()
		{
			return "<Date>";
		}

		private static string Footer()
		{
			const string strFooter = "";

			return strFooter;
		}

        public void TeamCards()
        {
            //  for each conference, spit out the team cards
            Utility.Announce("Team Cards...");
            if (Nfc != null) Nfc.TeamCards();
            if (Afc != null) Afc.TeamCards();
        }        
	}
}
