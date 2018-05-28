using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace RosterLib
{
	/// <summary>
	/// Summary description for HTMLFile.
	/// </summary>
	public class HtmlFile
	{
		public Logger Logger { get; set; }

		public List<string> TopScripts { get; private set; }
		public List<string> Scripts { get; private set; }
		private string _filename;
		private readonly string _title;
		private readonly string _header;
		private readonly string _cssFile;
		private readonly string _script1;
		private readonly string _script2;
		private readonly ArrayList _bodyList;

		public bool AnnounceIt { get; set; }

		public HtmlFile( string filenameIn, string titleIn )
		{
			_filename = filenameIn;
			_title = titleIn;
			_header = String.Empty;
			_bodyList = new ArrayList();
			StyleList = new ArrayList();
			AnnounceIt = true;
		}

		public HtmlFile( string filenameIn, string titleIn, string headerIn,
							  string cssFileIn, string script1In, string script2In )
		{
			_filename = filenameIn;
			_title = titleIn;
			_header = headerIn;
			_cssFile = cssFileIn;
			_script1 = script1In;
			_script2 = script2In;
			_bodyList = new ArrayList();
			StyleList = new ArrayList();
			AnnounceIt = true;
		}

		public ArrayList StyleList { get; set; }

		public void AddTopScript( string scriptText )
		{
			if ( TopScripts == null ) TopScripts = new List<string>();

			TopScripts.Add( scriptText );
		}

		public void AddScript( string scriptText )
		{
			if ( Scripts == null ) Scripts = new List<string>();

			Scripts.Add( scriptText );
		}

		public void AddToBody( string strBody )
		{
			_bodyList.Add( strBody );
		}

		public void Render( string newFile )
		{
			_filename = newFile;
			Render();
		}

		public void Render()
		{
			Utility.EnsureDirectory( _filename );
			var sw = new StreamWriter( _filename, false );
			sw.WriteLine( _header.Length > 0 ? HtmlLib.HTMLOpenPlus( _header ) : HtmlLib.HTMLOpen() );
			sw.WriteLine( HtmlLib.HeadOpen() );
			sw.WriteLine( "\t" + HtmlLib.HTMLTitle( _title ) );
			if ( _cssFile != null )
			{
				if ( _cssFile.Length > 0 )
					sw.WriteLine( "\t" + HtmlLib.CssLink( _cssFile ) );
			}
			if ( _script1 != null )
			{
				if ( _script1.Length > 0 )
					sw.WriteLine( "\t" + HtmlLib.VBScriptFile( _script1 ) );
			}
			if ( _script2 != null )
			{
				if ( _script2.Length > 0 )
					sw.WriteLine( "\t" + HtmlLib.JSScriptFile( _script2 ) );
			}
			if ( TopScripts != null )
				RenderTopScripts( sw );

			if ( StyleList.Count > 0 )
			{
				var styleEnumerator = StyleList.GetEnumerator();
				sw.WriteLine( "\t" + HtmlLib.StyleOpen() );
				while ( styleEnumerator.MoveNext() )
					sw.WriteLine( "\t" + styleEnumerator.Current );
				sw.WriteLine( "\t" + HtmlLib.StyleClose() );
			}

			sw.WriteLine( HtmlLib.HeadClose() );
			sw.WriteLine( HtmlLib.BodyOpen() );

			sw.WriteLine( HtmlLib.DivOpen( "id=\"container\"" ) );

			//  Add the body parts
			var myEnumerator = _bodyList.GetEnumerator();
			while ( myEnumerator.MoveNext() )
				sw.WriteLine( myEnumerator.Current );

			sw.WriteLine( HtmlLib.DivClose() );

			if ( Scripts != null )
			{
				WriteScripts( sw );
			}
			sw.WriteLine( HtmlLib.BodyClose() );
			sw.WriteLine( HtmlLib.HtmlClose() );
			sw.Close();

			if ( AnnounceIt )
				Announce( string.Format( "   {0} has been rendered", _filename ) );
		}

		public void Announce( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Trace( "   " + message );
		}

		private void WriteScripts( StreamWriter sw )
		{
			foreach ( var item in Scripts )
			{
				sw.WriteLine( "    {0}", item );
			}
		}

		private void RenderTopScripts( StreamWriter sw )
		{
			foreach ( var item in TopScripts )
			{
				sw.WriteLine( "    <script type='text / javascript' {0}></script>", item );
			}
		}

		public void AddStyle( string style )
		{
			StyleList.Add( style );
		}
	}
}