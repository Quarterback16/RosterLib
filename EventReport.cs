
using System;
using System.Data;
using TFLLib;

namespace RosterLib
{
	/// <summary>
	/// Summary description for EventReport.
	/// </summary>
	public class EventReport
	{
		private readonly DataLibrarian tflWS;

		public enum GameEvent
		{
			Sack = 0,
			Interception = 1
		}

		public EventReport( string teamCode, string statCode, DataLibrarian tflWS)
		{
			Team = new NflTeam( teamCode );
			Team.LoadGames( teamCode, Utility.LastSeason() );
			this.tflWS = tflWS;
			StatCode = statCode;
		}

		#region  Acessors

		public NflTeam Team { get; set; }

		public string StatCode { get; set; }

		#endregion
		

		/// <summary>
		///   Creates the output.
		/// </summary>
		public void Render()
		{
			SimpleTableReport str = new SimpleTableReport( string.Format( "{0}-{1}", 
			                                                              StatCode, Team.TeamCode ) );
			str.AddStyle(  "#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle(  "#main { margin-left:1em; }" );
			str.AddStyle(  "#dtStamp { font-size:0.8em; }" );
			str.AddStyle(  ".end { clear: both; }" );
			str.AddStyle(  ".gponame { color:white; background:black }" );
			str.ColumnHeadings = true;
			str.DoRowNumbers = false;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Week",      "WEEK",   "{0}", typeof( String )       ) ); 
			str.AddColumn( new ReportColumn( "Opp",       "OPP",    "{0}", typeof( String )       ) ); 
			str.AddColumn( new ReportColumn( "Count",     "COUNT",  "{0}", typeof( String ), true ) ); 
			str.AddColumn( new ReportColumn( "Who",       "WHO",    "{0}", typeof( String )       ) ); 
			BuildTable( str );
			//str.SubHeader = SubHeading();
            str.RenderAsHtml(string.Format("{0}{1}-{2}.htm", Utility.OutputDirectory(), Team.TeamCode, StatCode), true);
			
		}
		
		private void BuildTable( SimpleTableReport str )
		{
			if ( Team.GameList != null )
			{
				foreach ( NFLGame g in Team.GameList )
				{
					if ( g != null )
					{
						DataRow dr = str.Body.NewRow();
						g.LoadStats( tflWS );
						dr[ "WEEK" ]  = g.GameCodeOut();
						dr[ "OPP" ]   = g.Opponent( Team.TeamCode );
						dr[ "COUNT" ] = g.StatFor( Team.TeamCode, StatCode );
						dr[ "WHO" ] = g.PlayersFor( Team.TeamCode, StatCode );
						str.Body.Rows.Add( dr );
					}
				}
			}
			return;
		}
		
		
	}
	
}
