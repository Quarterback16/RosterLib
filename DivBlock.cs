using System;

namespace RosterLib
{
	/// <summary>
	/// Summary description for DivBlock.
	/// </summary>
	public class DivBlock
	{
		private readonly string _sectTitle;
		private readonly string _strClass;
		private readonly int _level;
		private string _containerHtml;

		public DivBlock( string sectIn, int levelIn, string classIn )
		{
			//RosterLib.Utility.Announce( "DivBlock:" + sectIn );
			_sectTitle = sectIn;
			_level = levelIn;
			_strClass = classIn;
			_containerHtml = String.Empty;
		}

		public string Html()
		{
			string s = "\n" + Tabs()
				+ HtmlLib.DivOpen( "class='" + _strClass + "'" ) + "\n" + Tabs() + "\t" 
				+ HtmlLib.Span( "class='sectionTitle' tabindex='0'", _sectTitle ) + "\n" + Tabs() + "\t" 
				+ HtmlLib.ALink() + "\n";

			s += Tabs() + HtmlLib.DivClose() + "\n";

			if ( _containerHtml.Length > 0 )
				s += ContainerHtml() + "\n";

			//s += Tabs() + HtmlLib.DivClose() + "\n";
			return s;
		}

		public string Tabs()
		{
			var s = String.Empty;
			int i;

			for (i = 0; i < _level; i++)
				s += "\t";
			//return s;
			return "";
		}


		public void AddContainer( string htmlIn )
		{
			_containerHtml = htmlIn;
		}


		public string ContainerHtml()
		{
			return Tabs() + "\t" + HtmlLib.DivOpen( "class='container'" ) + "\n\t" + Tabs()
				+ _containerHtml + "\n\t" + Tabs()
				+ HtmlLib.DivClose() + "\n";
		}
	}
}
