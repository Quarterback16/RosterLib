
using System;
using System.Collections;
using System.Data;
using TFLLib;

namespace RosterLib
{
	/// <summary>
	///  An abstraction of a ESPN League.
	/// </summary>
	public class EspnLeague : ILeague
	{
		private readonly DataLibrarian _tflWs;
		private ArrayList _teamList;  //  A collection of Teams

		public int WeekNo { get; set; }

		public int SeasonNo
		{
			get { return Int32.Parse( Season ); }
		}

		public string FileOut { get; set; }

		public EspnLeague( string nameIn, string compCode, string season, int weekNo )
		{
			RosterLib.Utility.Announce( string.Format( "EspnReport {0}...", nameIn ) );

			Name = nameIn;
			CompCode = compCode;
			Season = season;
			WeekNo = weekNo;

			_tflWs = Utility.TflWs;
		}
		
		#region Reports
		
		#region  Roster Report
		/// <summary>
		///   This is a model SimpleTableReport, using expanded ReportColumn constructors
		/// </summary>
		public void RosterReport()
		{
			string when = string.Format( "Week{0}-{1:00}", Season, Int32.Parse( Utility.CurrentWeek() ) );
			SimpleTableReport str = new SimpleTableReport( string.Format( "ESPN Roster : {0}", when ) );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
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
            str.RenderAsHtml(string.Format("{0}rosters\\Roster{2}{1}.htm", Utility.OutputDirectory(), when, CompCode), true);			
		}
		

		private DataTable BuildTable()
		{
			if ( _teamList == null ) LoadTeams();
			
			DataTable dt = new DataTable();
			DataColumnCollection cols = dt.Columns;
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
					string val = string.Format( "{0,3}", t.Value() );
					int nPlayers = t.Starters + t.Backups + t.Others + t.Injuries +
										t.PlayoffStarters + t.PlayoffBackups + t.PlayoffOthers;
					decimal avgVal = t.Value() / nPlayers;
					DataRow dr = dt.NewRow();
					dr[ "FTEAM" ] = string.Format( "{0}{1}", t.Name, HumanFlag( t.IsHuman ) );
					dr[ "SIZE" ] = nPlayers;
					dr[ "AVG" ] = avgVal;
					dr[ "STARTERS" ] = t.Starters + t.PlayoffStarters;
					dr[ "BACKUPS" ] = t.Backups + t.PlayoffBackups;
					dr[ "OTHERS" ] = t.Others + t.PlayoffOthers;
					dr[ "INJURIES" ] = t.Injuries + t.PlayoffInjuries;
					dr[ "VALUE" ] = val;
					dr[ "BIAS" ] = t.BiasOut();
					dt.Rows.Add( dr );
				}
				dt.DefaultView.Sort = "VALUE DESC";
			}
			return dt;
		}
		
		private static string HumanFlag( bool isHuman )
		{
			return isHuman ? "*" : "";
		}
		
		#endregion
		
		#region  Game Ratings
		
		public void GameRatings( NFLWeek week, string fTeamOwner )
		{
			string when = string.Format( "Week{0}-{1:0#}", week.Season, Int32.Parse( week.Week ) );
			//RosterLib.Utility.Announce( string.Format( "GameRatings for {0} owner {1}", when, fTeamOwner ) );
			var str = new SimpleTableReport( string.Format( "Game Ratings {1}: {0}", when, Name ) );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
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

         str.RenderAsHtml(string.Format("{0}gameratings\\Ratings{2}{1}.htm", Utility.OutputDirectory(), when, CompCode), true);				
		}
		
		private void BuildRatingsTable( SimpleTableReport str, string fTeamOwner, NFLWeek week )
		{
			//RosterLib.Utility.Announce(string.Format("BuildRatingsTable"));

			if (_teamList == null)
			{
				//RosterLib.Utility.Announce(string.Format("BuildRatingsTable: Loading Teams"));
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
							//RosterLib.Utility.Announce(string.Format("Doing game {0}", g.GameCode ) );
							DataRow dr = str.Body.NewRow();
							//RosterLib.Utility.Announce(string.Format("Rating game {0}", g.GameCode));
							dr[ "RATING" ] = t.RateGame( g );
							//RosterLib.Utility.Announce(string.Format("Setting Day {0}", g.GameCode));
							dr[ "DAY" ] = string.Format( "{0} {1}",
																  g.AussieDateTime().DayOfWeek.ToString().Substring( 0, 3 ),
																  g.AussieDateTime().Day );
							//RosterLib.Utility.Announce(string.Format("Setting Hour {0}", g.GameCode));
							dr[ "HOUR" ] = g.AussieHour( false );
							//RosterLib.Utility.Announce(string.Format("Setting Names {0}", g.GameCode));
							dr[ "AT" ] = g.AwayNflTeam == null ? g.AwayTeam : g.AwayNflTeam.Name;
							dr[ "HT" ] = g.AwayNflTeam == null ? g.HomeTeam : g.HomeNflTeam.Name;
							//RosterLib.Utility.Announce(string.Format("Setting TV {0}", g.GameCode));
							dr[ "TV" ] = g.IsOnTv ? "*" : "";
							//RosterLib.Utility.Announce(string.Format("Setting Spread Fav {0}", g.GameCode));

							dr[ "FAV" ] = g.SpreadFavourite();
							//RosterLib.Utility.Announce(string.Format("Setting Spread {0}", g.GameCode));
							dr[ "SPREAD" ] = g.Spread.ToString();
							//RosterLib.Utility.Announce(string.Format("Setting myTip {0}", g.GameCode));
							if ( g.MyTip != null )
							{
								if ( g.MyTip.Trim().Length > 0 )
									dr[ "TIP" ] = Equals( g.MyTip.Substring( 0, 1 ), "H" ) ? g.HomeTeam : g.AwayTeam;
							}

							str.Body.Rows.Add( dr );
						}
					}
				}
			}
			else
				RosterLib.Utility.Announce(string.Format("BuildReportDataSet: null week"));
		}

		#endregion
		
		#endregion
		
		#region  Data
		
		public void LoadTeams()
		{
			_teamList = new ArrayList();		
			//  Part 1 - Get the Teams for the season
			DataSet ds = _tflWs.GetFTeamsDs( Season, CompCode );
			DataTable dt = ds.Tables["comp"];

			//  Part 2 - Iterate through the teams
			foreach (DataRow dr in dt.Rows)
			{
				GsTeam t = new GsTeam( dr, CompCode, this );
				
				_teamList.Add( t );
			}
		}
		
		#endregion
		
		#region  Accessors
		
		public string Name{ get; set; }

		public string Season{ get; set; }

		public string CompCode{ get; set; }

		#endregion

		public GsTeam GetTeam( string ownerCode )
		{
			GsTeam selectedTeam = null;
			if ( _teamList != null )
			{
				foreach ( GsTeam t in _teamList )
				{
					if ( t.OwnerId.Equals( ownerCode ) )
					{
						selectedTeam = t;
						break;
					}
				}
			}
			return selectedTeam;
		}
	}
}

