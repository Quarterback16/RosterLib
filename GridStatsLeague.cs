using System;
using System.Collections;
using System.Data;
using System.Linq;
using TFLLib;

namespace RosterLib
{
	/// <summary>
	///  An abstraction of a Grid Stats League.
	/// </summary>
	public class GridStatsLeague : ILeague
	{
		private string _compCode;

		private readonly DataLibrarian _tflWs;
		public ArrayList _teamList;  //  A collection of GSTeams

		public string FileOut { get; set; }
				
		public GridStatsLeague( string nameIn, string compCode, string season, int weekNo )
		{
			Name = nameIn;
			CompCode = compCode;
			Season = Utility.CurrentSeason();
			WeekNo = weekNo;
			_tflWs = Utility.TflWs;
		}

		public GsTeam GetTeam( string ownerCode )
		{
			GsTeam selectedTeam = null;
			if ( _teamList != null )
			{
				foreach ( var t in from GsTeam t in _teamList where t.OwnerId.Equals( ownerCode ) select t )
				{
					selectedTeam = t;
					break;
				}
			}
			return selectedTeam;
		}

		#region Reports
		
		#region  Roster Report
		/// <summary>
		///   This is a model SimpleTableReport, using expanded ReportColumn constructors
		/// </summary>
		public void RosterReport()
		{
			var week = Int32.Parse( Utility.CurrentWeek() );
			var when = string.Format( "Week{0}-{1:00}", Season,  week );
			var str = new SimpleTableReport(string.Format("GridStats Roster : {0}", when))
			          	{
								ColumnHeadings = true, 
								DoRowNumbers = true
							};
			str.AddColumn( new ReportColumn( "Team",     "FTEAM",     "{0,-20}"     ) ); 
			str.AddColumn( new ReportColumn( "Value",    "VALUE",     "{0}", true   ) ); 
			str.AddColumn( new ReportColumn( "Size",     "SIZE",      "{0}", true   ) ); 
			str.AddColumn( new ReportColumn( "Avg",      "AVG",       "{0:#.#}"     ) ); 
			str.AddColumn( new ReportColumn( "Starters", "STARTERS",  "{0}", true   ) ); 
			str.AddColumn( new ReportColumn( "Backups",  "BACKUPS",   "{0}", true   ) );
			str.AddColumn( new ReportColumn( "Others",   "OTHERS",    "{0}", true   ) ); 
			str.AddColumn( new ReportColumn( "Injured",  "INJURIES",  "{0}" , true  ) );
			str.AddColumn( new ReportColumn( "TeamBias", "BIAS",      "{0,-10}"     ) ); 			
			
			str.LoadBody( BuildTable() );
			FileOut = string.Format( "{0}{1}\\RosterSummary\\{2}\\RosterSummary-{3:00}.htm", 
				Utility.OutputDirectory(), Season, CompCode, week );
			str.RenderAsHtml( FileOut, true );			
		}
		
		private DataTable BuildTable()
		{
			if ( _teamList == null ) LoadTeams();
			
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "FTEAM",      typeof( String) );
			cols.Add( "VALUE",      typeof( Int32 ) );
			cols.Add( "SIZE",       typeof( Int32 ) );			
			cols.Add( "AVG",        typeof( Decimal) );			
			cols.Add( "STARTERS",   typeof( Int32 ) );
			cols.Add( "BACKUPS",    typeof( Int32 ) );
			cols.Add( "OTHERS",     typeof( Int32 ) );
			cols.Add( "INJURIES",   typeof( Int32 ) );
			cols.Add( "BIAS",       typeof( String) );
			
			if ( _teamList != null )
			{
				foreach ( GsTeam t in _teamList )
				{
					var val = string.Format( "{0,3}", t.Value() );
					var nPlayers = t.Starters + t.Backups + t.Others + t.Injuries +
										t.PlayoffStarters + t.PlayoffBackups + t.PlayoffInjuries + t.PlayoffOthers;
					var avgVal = t.Value() / (decimal) nPlayers;
					var dr = dt.NewRow();
					dr[ "FTEAM" ] = t.TeamUrl(); 
					dr[ "SIZE" ] = nPlayers;
					dr[ "AVG" ] = avgVal;
					dr[ "STARTERS" ] = t.Starters + t.PlayoffStarters;
					dr[ "BACKUPS" ] = t.Backups + t.PlayoffBackups;
					dr[ "OTHERS" ] = t.Others + t.PlayoffOthers;
					dr[ "INJURIES" ] = t.Injuries + t.PlayoffInjuries;
					dr[ "VALUE" ] = val;
					dr[ "BIAS" ] = t.BiasOut();
					dt.Rows.Add( dr );
					t.DumpTeam();
				}
				dt.DefaultView.Sort = "AVG DESC";
			}
			return dt;
		}
		
		#endregion
		
		#region  Game Ratings
		
		public void GameRatings( NFLWeek week, string fTeamOwner )
		{
			var when = string.Format( "Week{0}-{1:0#}", week.Season, Int32.Parse( week.Week ) );
#if DEBUG
			Utility.Announce( string.Format( "   GameRatings for {0} owner {1}", when, fTeamOwner ) );
#endif
			var str = new SimpleTableReport( string.Format( "Game Ratings {1}: {0:0#}", when, Name ) )
			          	{ColumnHeadings = true, DoRowNumbers = true};
			str.AddColumn( new ReportColumn( "Rating",   "RATING",    "{0}", typeof( Int32 ), true   ) ); 
			str.AddColumn( new ReportColumn( "Day",      "DAY",       "{0}", typeof( String)  ) ); 
			str.AddColumn( new ReportColumn( "Time",     "HOUR",      "{0}", typeof( String)    ) ); 
			str.AddColumn( new ReportColumn( "AT",       "AT",        "{0}", typeof( String)    ) ); 
			str.AddColumn( new ReportColumn( "HT",       "HT",        "{0}", typeof( String)    ) ); 
			str.AddColumn( new ReportColumn( "TV",       "TV",        "{0}", typeof( String)    ) ); 
			str.AddColumn( new ReportColumn( "Fav",      "FAV",       "{0}", typeof( String)    ) ); 
			str.AddColumn( new ReportColumn( "Spr",      "SPREAD",    "{0}", typeof( String)    ) ); 
			str.AddColumn( new ReportColumn( "myTip",    "TIP",    "{0}", typeof( String)    ) ); 
			
			BuildRatingsTable( str, fTeamOwner, week );

         str.RenderAsHtml(string.Format("{0}Ratings{2}{1}.htm", Utility.OutputDirectory(), when, CompCode), true);				
		}
		
		private void BuildRatingsTable( SimpleTableReport str, string fTeamOwner, NFLWeek week )
		{
			//RosterLib.Utility.Announce( string.Format("  BuildRatingsTable"));

			if (_teamList == null)
			{
				Utility.Announce( string.Format("  BuildRatingsTable: Loading Teams"));
				LoadTeams();
			}

			if (_teamList != null)
			{
				foreach ( GsTeam t in _teamList )
					BuildReportDataset( str, fTeamOwner, week, t);
			}
			str.SetSortOrder( "DAY, HOUR, RATING DESC" );
			return;
		}

		private static void BuildReportDataset(SimpleTableReport str, string fTeamOwner, NFLWeek week, GsTeam t)
		{
			if (week != null)
			{
				if ( ( t != null ) && ( t.OwnerId == fTeamOwner ) )
				{
					foreach ( NFLGame g in week.GameList() )
					{
						if ( g != null )
						{
							var theGame = g.Hour == null ? new NFLGame( g.GameKey() ) : g;
							//RosterLib.Utility.Announce(string.Format("Doing game {0}", theGame.GameCode ) );
							var dr = str.Body.NewRow();
							//RosterLib.Utility.Announce(string.Format("Rating game {0}", theGame.GameCode));
							dr[ "RATING" ] = t.RateGame( theGame );
							//RosterLib.Utility.Announce(string.Format("Setting Day {0}", theGame.GameCode));
							dr[ "DAY" ] = string.Format( "{0} {1}",
																  theGame.AussieDateTime().DayOfWeek.ToString().Substring( 0, 3 ),
																  theGame.AussieDateTime().Day );
							//RosterLib.Utility.Announce(string.Format("Setting Hour {0}", theGame.GameCode));
							dr[ "HOUR" ] = theGame.AussieHour( false );
							//RosterLib.Utility.Announce(string.Format("Setting Names {0}",theGame.GameCode));
							dr[ "AT" ] = theGame.AwayNflTeam == null ? theGame.AwayTeam : theGame.AwayNflTeam.Name;
							dr[ "HT" ] = theGame.AwayNflTeam == null ? theGame.HomeTeam : theGame.HomeNflTeam.Name;
							//RosterLib.Utility.Announce(string.Format("Setting TV {0}", theGame.GameCode));
							dr[ "TV" ] = theGame.IsOnTv ? "*" : "";
							//RosterLib.Utility.Announce(string.Format("Setting Spread Fav {0}", theGame.GameCode));

							dr[ "FAV" ] = theGame.SpreadFavourite();
							//RosterLib.Utility.Announce(string.Format("Setting Spread {0}", theGame.GameCode));
							dr[ "SPREAD" ] = theGame.Spread.ToString();
							//RosterLib.Utility.Announce(string.Format("Setting myTip {0}", theGame.GameCode));
							if ( theGame.MyTip != null )
							{
								if ( theGame.MyTip.Trim().Length > 0 )
									dr[ "TIP" ] = Equals( theGame.MyTip.Substring( 0, 1 ), "H" ) ? theGame.HomeTeam : theGame.AwayTeam;
							}

							str.Body.Rows.Add( dr );
						}
					}
				}
			}
			else
				Utility.Announce(string.Format("BuildReportDataSet: null week"));
		}

		#endregion
		
		#endregion
		
		#region  Data
		
		public void LoadTeams()
		{
			var teamCount = 0;
			_teamList = new ArrayList();		
			//  Part 1 - Get the Teams for the season
			var ds = _tflWs.GetFTeamsDs( Season, CompCode );
			var dt = ds.Tables["comp"];

			//  Part 2 - Iterate through the teams
			foreach ( var t in from DataRow dr in dt.Rows 
									 select new GsTeam( dr, CompCode, this ) )
			{
				_teamList.Add( t );
				teamCount++;
			}
			if ( teamCount == 0 )
				Utility.Announce( string.Format( 
					"GridStatsLeague:LoadTeams did not fond any teams for {0}:{1}",
					CompCode, Season ) );

		}
		
		#endregion		

		#region  Accessors

		public string Name { get; set; }

		public string Season { get; set; }

		public int SeasonNo
		{
			get { return Int32.Parse( Season ); }
		}

		public int WeekNo { get; set; }

		public string CompCode
		{
			get { return _compCode; }
			set { _compCode = value; }
		}

		#endregion
		
	}
}
