namespace RosterLib.Helpers
{
	public static class StyleHelper
	{
		public static void AddStyle( SimpleTableReport str )
		{
			str.AddStyle(
				"#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 100%; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle( "#main { margin-left:1em; }" );
			str.AddStyle( "#dtStamp { font-size:0.8em; }" );
			str.AddStyle( ".end { clear: both; }" );
			str.AddStyle( ".gponame { color:white; background:black }" );
			str.AddStyle(
				"label { display:block; float:left; width:130px; padding: 3px 5px; margin: 0px 0px 5px 0px; text-align:right; }" );
			str.AddStyle(
				"value { display:block; float:left; width:100px; padding: 3px 5px; margin: 0px 0px 5px 0px; text-align:left; font-weight: bold; color:blue }" );
			str.AddStyle(
				"#notes { float:right; height:auto; width:308px; font-size: 88%; background-color: #ffffe1; border: 1px solid #666666; padding: 5px; margin: 0px 0px 10px 10px; color:#666666 }" );
			str.AddStyle(
				"div.notes H4 { background-image: url(images/icon_info.gif); background-repeat: no-repeat; background-position: top left; padding: 3px 0px 3px 27px; border-width: 0px 0px 1px 0px; border-style: solid; border-color: #666666; color: #666666; font-size: 110%;}" );
		}
	}
}
