using System;
using System.Collections;
using System.Text;

namespace RosterLib
{
   /// <summary>
   /// A static library of HTML specific functions.
   /// </summary>
   public class HtmlLib
   {
      private HtmlLib()
      {
         //  As this is a static library, public constructor is not necessary
      }

      public static string H1( string strData )
      {
         return "<H1>" + strData + "</H1>";
      }

      public static string H2( string strData )
      {
         return "<H2>" + strData + "</H2>";
      }

      public static string H3( string strData )
      {
         return "<H3>" + strData + "</H3>";
      }

      public static string H4( string strData )
      {
         return "<H4>" + strData + "</H4>";
      }
      
      public static string H4( string strData, string attr )
      {
         return string.Format( "<H4 {1}>{0}</H4>", strData, attr );
      }

      public static string Bold( string strData )
      {
         return "<B>" + strData + "</B>";
      }

      public static string Strikeout( string strData )
      {
         return "<S>" + strData + "</S>";
      }

      public static string Italics( string strData )
      {
         return "<I>" + strData + "</I>";
      }

      public static string Para( string strData )
      {
         return "<IP>" + strData + "</P>";
      }

      public static string Centre( string strData )
      {
         return "<CENTER>" + strData + "</CENTER>";
      }
      
      public static string Image( string filename )
      {
         return string.Format( "<img SRC=\"images\\{0}\">", filename );
      }

      public static string FixedImage(string path, string filename)
      {
         return string.Format("<img SRC=\"{1}\\images\\{0}\">", filename, path );
      }		

      public static string Font( string strFace, string strData )
      {
         return "<FONT FACE=" + strFace + ">" + strData + "</FONT>";
      }

      public static string Font( string strFace, string strData, string size )
      {
         return "<FONT FACE=" + strFace + " SIZE=" + size + ">" + strData +
                "</FONT>";
      }
      
      public static string FormOpen()
      {
         return "<FORM>";
      }

      public static string FormClose()
      {
         return "</FORM>";
      }

      public static string HTMLOpen()
      {
         return "<HTML>";
      }

      public static string HTMLOpenPlus( string strPlus )
      {
         return "<HTML " + strPlus + ">";
      }

      public static string Span( string strAttr, string strData  )
      {
         return "<SPAN " + strAttr + ">" + strData + "</SPAN>";
      }

      public static string Div( string id, string strAttr )
      {
         return "<DIV id='" + id + "' " + strAttr + "></DIV>";
      }

      public static string DivOpen( string strAttr )
      {
         return "<DIV " + strAttr + ">";
      }

      public static string DivClose()
      {
         return "</DIV>";
      }

      public static string ALink()
      {
         return "<A class='expando' href='#'></A>";
      }
      
      public static string CssLink( string filename )
      {
         return "<link rel='STYLESHEET' href='" + filename + "' MEDIA='screen,print' TYPE='text/css'>";
      }

      public static string VBScriptFile( string filename )
      {
         return "<script language='VBScript' SRC='" + filename + "'></script>";
      }

      public static string JSScriptFile( string filename )
      {
         return "<script language='JavaScript' SRC='" + filename + "'></script>";
      }

      public static string HTMLTitle( string title )
      {
         return "<TITLE>" + title + "</TITLE>";
      }

      public static string BodyOpen()
      {
         return "<BODY>";
      }

      public static string BodyClose()
      {
         return "</BODY>";
      }

      public static string HeadOpen()
      {
         return "<HEAD>";
      }

      public static string HeadClose()
      {
         return "</HEAD>";
      }


      public static string HtmlClose()
      {
         return "</HTML>";
      }

      public static string HtmlPad( string cStr, int nSpaces )
      {
         string cPad = "";
         for (int i = cStr.Trim().Length; i < nSpaces; i++ )
         {
            cPad += "&nbsp;";
         }
         return cStr.Trim() + cPad;
      }

      public static string Spaces( int nSpaces )
      {
         var cPad = "";
         for ( var i = 0; i < nSpaces; i++ )
            cPad += "&nbsp;";
         return cPad;
      }    

      public static string Style( string styles )
      {
         return string.Format("<style type=\"text/css\"> {0} </style>", styles );
      }
      
      public static string StyleOpen()
      {
         return "<style type=\"text/css\">";
      }

      public static string StyleClose()
      {
         return "</style>";
      }

      public static string HTMLPadL( string cStr, int nSpaces )
      {
         if (cStr == null) throw (new ArgumentNullException("cStr", "parameter is null"));

         var cPad = "";
         for ( var i = cStr.Trim().Length; i < nSpaces; i++ )
            cPad += "&nbsp;";

         return cPad + cStr.Trim();
      }


      public static string TableClose()
      {
         return "</TABLE>";
      }

      public static string TableOpen( string strAttr )
      {
         return "<TABLE " + strAttr + ">";
      }

      public static string TableOpen()
      {
         return "<TABLE>";
      }

      public static string TableWithBorderOpen()
      {
         return "<TABLE border='1'>";
      }

      public static string TableDataClose()
      {
         return "</TD>";
      }

      public static string TableDataAttr( string strData, string strAttr )
      {
         return "<TD " + strAttr + ">" + strData + "</TD>";
      }

      public static string TableData( string strData )
      {
         return "<TD>" + strData + "</TD>";
      }

      public static string ListOpen()
      {
         return "<ol>";
      }

      public static string ListClose()
      {
         return "</ol>";
      }

      public static string ListItem( string strData )
      {
         return "<li>" + strData + "</li>";
      }

      public static string TableData( string strData, string strColour )
      {
         return "<TD BGCOLOR=" + strColour + ">" + strData + "</TD>";
      }

      public static string TableData( string strData, string strColour, string strAttr )
      {
         return "<TD BGCOLOR=" + strColour + " " + strAttr + ">" + strData + "</TD>";
      }

      public static string TableHeader( string strHeader )
      {
         return string.Format( TableHeaderOpen() + "{0}" + TableHeaderClose(), strHeader );
      }

      public static string TableHeaderOpen()
      {
         return "<TH>";
      }
      public static string TableHeaderClose()
      {
         return "</TH>";
      }
   
      public static string TableDataOpen()
      {
         return "<TD>";
      }
      public static string TableDataOpen( string cAttr )
      {
         return "<TD " + cAttr + ">";
      }
      public static string TableRowClose()
      {
         return "</TR>";
      }

      public static string TableRowOpen()
      {
         return "<TR>";
      }

      public static string TableRowOpen( string cAttr )
      {
         return "<TR " + cAttr + ">";
      }

      public static string TableRowHeaderOpen( string cAttr )
      {
         return "<TH " + cAttr + ">";
      }

      public static string TableRowHeaderOpen()
      {
         return "<TH>";
      }

      public static string TableRowHeaderClose()
      {
         return "</TH>";
      }

      public class HtmCol
      {
         private string _attr;
         private string _data;

         public string Render()
         {
            return TableDataAttr( Data, Attr );
         }

         public string Attr
         {
            get { return _attr; }
            set { _attr = value; }
         }
         public string Data
         {
            get { return _data; }
            set { _data = value; }
         }
      }

      public class HtmRow
      {
         private readonly ArrayList colList;

         public HtmRow()
         {
            colList = new ArrayList();
         }

         public void AddCol( HtmCol col )
         {
            colList.Add( col );
         }

         public string Render()
         {
            var sb = new StringBuilder();
            sb.Append(TableRowOpen() );
            foreach (var t in colList)
            {
               var item = (HtmCol) t;
               sb.Append( item.Render() );
            }
            sb.Append( TableRowClose() );
            return sb.ToString();
         }

      }

      public class HtmTable
      {
         private readonly ArrayList _rowList;
         private readonly string _attr;

         public HtmTable()
         {
            //  Constructor
            _rowList = new ArrayList();
            _attr = "";
         }

         public HtmTable( string attrIn )
         {
            //  Constructor
            _rowList = new ArrayList();
            _attr = attrIn;
         }

         public void AddRow( HtmRow row )
         {
            _rowList.Add( row );
         }

         public string Render()
         {
            var sb = new StringBuilder();
            sb.Append( TableOpen( _attr ) );
            foreach (var t in _rowList)
            {
               var item = (HtmRow) t;
               sb.Append( item.Render() );
            }           
            sb.Append( TableClose() );
            return sb.ToString();
         }
      }

      public static string TableRow(string[] pgmArray)
      {
         var html = string.Empty;
         html += TableRowOpen();
         for (int i = 0; i < pgmArray.Length; i++)
         {
            html += TableDataAttr( pgmArray[i], "align='RIGHT'" );
         }
         html += TableRowClose() + Environment.NewLine;
         return html;
      }

      internal static string TableHeaderRow(string[] pgmArray)
      {
         var html = string.Empty;
         html += TableRowOpen();
         for (int i = 0; i < pgmArray.Length; i++)
         {
            html += TableRowHeaderOpen();
            html += pgmArray[i];
            html += TableRowHeaderClose();

         }
         html += TableRowClose() + Environment.NewLine;
         return html;
      }
   }
}
