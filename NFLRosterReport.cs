using System;
using System.Collections;
using System.Data;
using System.Xml;
using NLog;
using RosterLib.Interfaces;
using System.Text;

namespace RosterLib
{
	/// <summary>
	/// Summary description for NFLRosterReport
	/// </summary>
	public class NFLRosterReport
	{
		public Logger Logger { get; private set; }
		public string FileOut = "";
		public string Season { get; set; }
		public NflConference Nfc;
		public NflConference Afc;
		private HtmlFile _html;
		public ArrayList ProjectionList;
		private readonly ArrayList _confList;

		#region Constructors

		/// <summary>
		///   Creates the 2 NFL conferences.
		/// </summary>
		public NFLRosterReport( string season )
		{
			Logger = LogManager.GetCurrentClassLogger();
			TimeTaken = "";
			Season = season;
			TraceIt( "NewRosterReport Constructor" );
			_confList = new ArrayList();
			Nfc = new NflConference( "NFC", season );
			_confList.Add( Nfc );
			LoadNfc();
			Afc = new NflConference( "AFC", season );
			_confList.Add( Afc );
			LoadAfc();
			ProjectionList = new ArrayList();
			TraceIt( "NewRosterReport Constructor - Done" );
		}

		#endregion Constructors

		public void LoadNfc()
		{

			TraceIt( "NewRosterReport:LoadNFC Loading NFC");

#if !QUICKRUN
			Nfc.AddDiv( "East", "A" );
			Nfc.AddDiv( "North", "B" );
			Nfc.AddDiv( "South", "C" );
#endif
			Nfc.AddDiv( "West", "D" );

			TraceIt( "NewRosterReport:LoadNFC Loading NFC - finished");
		}

		public void LoadAfc()
		{
			TraceIt( "NewRosterReport:LoadAFC Loading AFC");

#if !QUICKRUN
			Afc.AddDiv( "East", "E" );
			Afc.AddDiv( "North", "F" );
			Afc.AddDiv( "South", "G" );
#endif
			Afc.AddDiv( "West", "H" );

			TraceIt( "NewRosterReport:LoadAFC Loading AFC - finished");
		}

		/// <summary>
		///  Quickloads the conferences, using brief constructos.
		/// </summary>
		public void QuickLoadConferences()
		{
			Announce("NewRosterReport:QuickLoadConferences Loading Divisions");

			Nfc.QuickAddDiv( "East", "A" );
			Nfc.QuickAddDiv( "North", "B" );
			Nfc.QuickAddDiv( "South", "C" );
			Nfc.QuickAddDiv( "West", "D" );

			Afc.QuickAddDiv( "East", "E" );
			Afc.QuickAddDiv( "North", "F" );
			Afc.QuickAddDiv( "South", "G" );
			Afc.QuickAddDiv( "West", "H" );
		}

		public void CurrentRoster()
		{
			FileOut = string.Format( "{0}{1}\\RosterReport\\RosterReport.htm", Utility.OutputDirectory(), Season );
			_html = new HtmlFile( FileOut,
								 "NFL Rosters as of " + DateTime.Now.ToString( "ddd dd MMM yy HH:mm" ),
								 "dir='ltr' xmlns:v='urn:schemas-microsoft-com:vml' gpmc_reportInitialized='false'",
								 "report.css",
								 "report.vb",
								 "report.js" );
		}

		public void KickerProjection()
		{
			Announce( "Kicker Projections" );
			FileOut = string.Format( "{0}Kickers\\Kickers_{1}.htm", Utility.OutputDirectory(), Utility.CurrentSeason() );

			_html = new HtmlFile( FileOut,
								 " Kicker Projections as of " + DateTime.Now.ToString( "ddd dd MMM yy" ) );

			_html.AddToBody( Header( "Kicker Projections" ) );
			_html.AddToBody( KickersOut() );
			_html.Render();
		}

		public void Announce( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Info( "   " + message );
		}

		public void TraceIt( string message )
		{
			if ( Logger == null )
				Logger = LogManager.GetCurrentClassLogger();

			Logger.Trace( "   " + message );
		}

		public void SeasonProjection( string metricName, string season, string week, DateTime projectionDate )
		{
			Announce(string.Format("SeasonProjection metric={0} ...", metricName));

			FileOut = ProjectionFileName( metricName, season );

			_html = new HtmlFile( FileOut,
								 " Win Projections as of " + projectionDate.ToString( "ddd dd MMM yy" ) );

			_html.AddToBody( Header( "Season Projections " + metricName + " - " + season ) );
			_html.AddToBody( SeasonOut( metricName, projectionDate ) );
			_html.Render();
			ProjectionList.Add( metricName );
			Utility.CopySubFile( ProjectionFileName1( metricName, season ),
			ProjectionFileName2( metricName, season, week ) );
		}

		public string ProjectionFileName( string metricName, string season )
		{
			return string.Format( "{0}{2}\\Projections\\Proj-{1}-{2}.htm",
								 Utility.OutputDirectory(), metricName,
								 season );
		}

		public string ProjectionFileName1( string metricName, string season )
		{
			return string.Format( "{1}\\Projections\\Proj-{0}-{1}.htm",
								 metricName, season );
		}

		public string ProjectionFileName2( string metricName, string season, string week )
		{
			return string.Format( "{1}\\Projections\\Proj-{0}-{1}-{2:0#}.htm",
								 metricName,
								 season, Int32.Parse( week ) );
		}

		public void DumpProjections()
		{
			Announce( "Dumping Projections" );

			if ( ProjectionList == null ) return;

			//  for each projection in the report
			foreach ( var pl in ProjectionList )
			{
				var metricName = ( String ) pl;
				//    load the totals into a simple report
				var dt = new DataTable();
				var cols = dt.Columns;
				cols.Add( "TEAM", typeof( String ) );
				cols.Add( "STARTER", typeof( String ) );
				cols.Add( "TOTAL", typeof( Decimal ) );
				cols.Add( "GS", typeof( String ) );
				cols.Add( "G2", typeof( String ) );

				foreach ( NFLDivision d in Afc.DivList )
				{
					foreach ( NflTeam t in d.TeamList )
					{
						foreach ( NFLOutputMetric m in t.ProjectionList )
						{
							if ( m.Name == metricName )
							{
								AddRow( dt, m.Total, t, metricName );
								break;
							}
						}
					}
				}
				foreach ( NFLDivision d in Nfc.DivList )
				{
					foreach ( NflTeam t in d.TeamList )
					{
						foreach ( NFLOutputMetric m in t.ProjectionList )
						{
							if ( m.Name == metricName )
							{
								AddRow( dt, m.Total, t, metricName );
								break;
							}
						}
					}
				}
				var st = new SimpleTableReport { ReportHeader = "Projected " + metricName, ColumnHeadings = true };
				st.AddColumn( new ReportColumn( "Team", "TEAM", "{0,-20}" ) );
				st.AddColumn( new ReportColumn( "Starter", "STARTER", "{0,-20}" ) );
				st.AddColumn( new ReportColumn( "Total", "TOTAL", "{0:##0}", true ) );
				st.AddColumn( new ReportColumn( "GS11", "GS", "{0,-10}" ) );
				st.AddColumn( new ReportColumn( "GS3", "G2", "{0,-10}" ) );
				dt.DefaultView.Sort = "TOTAL DESC";
				st.LoadBody( dt );
				st.ShowElapsedTime = false;
				st.RenderAsHtml(
				   string.Format( "{0}{2}\\Projections\\Projected-{1}.htm",
					  Utility.OutputDirectory(), metricName, Utility.CurrentSeason() ), true );
			}
		}

		private static void AddRow( DataTable dt, int stat, NflTeam t, string metricName )
		{
			var dr = dt.NewRow();
			var p = t.GetCurrentStarter( PosInFocus( metricName ) );
			var starterName = String.Empty;
			var gs11Owner = String.Empty;
			var gs2Owner = String.Empty;

			if ( p != null )
			{
				starterName = p.PlayerName;
				gs11Owner = Utility.TflWs.GetStatus( p.PlayerCode, "GS", Utility.CurrentSeason() );
				gs2Owner = Utility.TflWs.GetStatus( p.PlayerCode, "G2", Utility.CurrentSeason() );
			}

			dr[ "TEAM" ] = t.Name;
			dr[ "TOTAL" ] = Normalised( stat, metricName );
			dr[ "STARTER" ] = ( metricName.Equals( "spread" ) ? "" : starterName );
			dr[ "GS" ] = gs11Owner;
			dr[ "G2" ] = gs2Owner;

			dt.Rows.Add( dr );
		}

		private static string PosInFocus( IEquatable<string> metricName )
		{
			var posOut = String.Empty;
			if ( metricName.Equals( "Tdp" ) )
				posOut = "QB";
			if ( metricName.Equals( "Tdr" ) )
				posOut = "RB";
			return posOut;
		}

		private static int Normalised( int metric, string metricName )
		{
			var metricOut = metric;
			if ( metricName.Equals( "Tdp" ) )
			{
				decimal metricNormalised = ( metric + 14 ) / 2;
				metricNormalised = Math.Round( metricNormalised );
				metricOut = Convert.ToInt32( metricNormalised );
			}
			if ( metricName.Equals( "Tdr" ) )
			{
				decimal metricNormalised = ( metric + 10 ) / 2;
				metricNormalised = Math.Round( metricNormalised );
				metricOut = Convert.ToInt32( metricNormalised );
			}
			return metricOut;
		}

		public void Render()
		{
			_html.AddToBody( Header( "NFL Rosters" ) );
			_html.AddToBody( TeamListOut() );
			_html.Render();
		}

		public void TeamCards()
		{
			//  for each conference, spit out the team cards
			Announce( "Team Cards..." );
			if ( Nfc != null ) Nfc.TeamCards();
			if ( Afc != null ) Afc.TeamCards();
		}

		public string TimeTaken { get; set; }

		private string TeamListOut()
		{
			var teamsOut = "\n";
			if ( Nfc != null ) teamsOut += Nfc.ConfHtml();
			if ( Afc != null ) teamsOut += Afc.ConfHtml();
			return teamsOut;
		}

		private string KickersOut()
		{
			var totalFieldGoals = 0;
			var s = HtmlLib.TableOpen( "border=1 cellpadding='0' cellspacing='0'" );
			if ( Nfc != null )
			{
				s += Nfc.Kickers();
				totalFieldGoals += Nfc.FieldGoals;
			}
			if ( Afc != null )
			{
				s += Afc.Kickers();
				totalFieldGoals += Afc.FieldGoals;
			}
			return s + "<br>" + string.Format( "Total Field Goals : {0}", totalFieldGoals );
		}

		private string SeasonOut( string metric, DateTime projectionDate )
		{
			IPrognosticate predictor;
			if ( metric.Equals( "Spread" ) )
				//predictor = new NibblePredictor(Utility.CurrentSeason());  //  which is more accurate??
				predictor = new UnitPredictor
				{
					TakeActuals = true,
					AuditTrail = false,
					WriteProjection = true,
					StorePrediction = true,
					RatingsService = new UnitRatingsService(new TimeKeeper(null) )
				};
			else
				predictor = new WizPredictor();
			var s = HtmlLib.TableOpen( "border=1 cellpadding='0' cellspacing='0'" );
#if !DEBUG
			//  to save time testing
			if ( Nfc != null ) s += Nfc.SeasonProjection( metric, predictor, projectionDate );
#endif
			if ( Afc != null ) s += Afc.SeasonProjection( metric, predictor, projectionDate );
			return s;
		}

		private string Header( string cHeading )
		{
			var htmlOut = HtmlLib.TableOpen( "class='title' cellpadding='0' cellspacing='0'" ) + "\n\t"
						  + HtmlLib.TableRowOpen() + "\n\t\t"
						  + HtmlLib.TableDataAttr( cHeading, "colspan='2' class='gponame'" ) + "\n\t"
						  + HtmlLib.TableRowClose() + "\n\t"
						  + HtmlLib.TableRowOpen() + "\n\t\t"
						  + HtmlLib.TableDataAttr( "Report Date:" + DateTime.Now.ToString( "dd MMM yy  HH:mm" ) +
												  "   elapsed:" + TimeTaken, "id='dtstamp'" ) + "\n\t\t"
						  + HtmlLib.TableData( HtmlLib.Div( "objshowhide", "tabindex='0'" ) ) + "\n\t"
						  + HtmlLib.TableRowClose() + "\n"
						  + HtmlLib.TableClose() + "\n";
			return htmlOut;
		}

#region Roster Experience

		/// <summary>
		///   Looks at the EP xml to dump out a report
		/// </summary>
		public void RosterExperience()
		{
			Utility.Announce( "NFLRosterReport.RosterExperience - creating simple report" );

			CheckPreRequisites();

			var str =
				new SimpleTableReport( string.Format( "Roster Experience {0}", Utility.CurrentSeason() ) );
			str.AddStyle(
				"#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 641px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle( "#main { margin-left:1em; }" );
			str.AddStyle( "#dtStamp { font-size:0.8em; }" );
			str.AddStyle( ".end { clear: both; }" );
			str.AddStyle( ".gponame { color:white; background:black }" );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Team", "TEAM", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Player", "PLAYER", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Unit", "UNIT", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "EP", "EP", "{0}", typeof( Int32 ), true ) );
			BuildTable( str );
			str.SetSortOrder( "EP DESC" );
			str.RenderAsHtml(
				string.Format( "{0}Experience-{1}-{2}.htm", Utility.OutputDirectory(), Utility.CurrentSeason(),
							  Utility.CurrentWeek() ), true );
		}

		private void BuildTable( SimpleTableReport str )
		{
			foreach ( NflConference c in _confList )
				foreach ( NFLDivision d in c.DivList )
					foreach ( NflTeam t in d.TeamList )
						foreach ( NFLPlayer p in t.PlayerList )

						{
							p.ExperiencePoints = Masters.Epm.GetEp( p.PlayerCode );
							var dr = str.Body.NewRow();
							dr[ "TEAM" ] = t.Name;
							dr[ "PLAYER" ] = p.PlayerOut();
							dr[ "UNIT" ] = p.Unit();
							dr[ "EP" ] = p.ExperiencePoints;
							str.Body.Rows.Add( dr );
						}
		}

#endregion Roster Experience

#region EP.XML

		private void CheckPreRequisites()
		{
			Utility.Announce( "RosterReport.CheckingPreRequisites" );

			if ( _confList == null )
				Utility.Announce( "confList is null - no xmlOutput" );

			if ( _confList != null )
				foreach ( NflConference c in _confList )
				{
					if ( c.DivList == null )
						Utility.Announce( "c.divList is null - no xmlOutput" );

					if ( c.DivList != null )
						foreach ( NFLDivision d in c.DivList )
						{
							if ( d.TeamList == null )
								Utility.Announce( "d.teamList is null - no xmlOutput" );

							if ( d.TeamList != null )
								foreach ( NflTeam t in d.TeamList )
									if ( t.PlayerList == null )
										Utility.Announce( "t.playerList is null - no xmlOutput" );
						}
				}
			Utility.Announce( "RosterReport.CheckingPreRequisites - done" );
		}

		public void XmlOutput()
		{
			Utility.Announce( "NFLRosterReport.XmlOutput: Producing XML output" );

			CheckPreRequisites();

			var writer = new
				XmlTextWriter( string.Format( "{0}\\xml\\EP.xml", Utility.OutputDirectory() ), null );

			writer.WriteStartDocument();
			writer.WriteComment( "Comments: Player Experience Points" );
			//writer.WriteProcessingInstruction( "Instruction","Player Record");
			writer.WriteStartElement( "playerList" );
			//  All the players
			foreach ( NflConference c in _confList )
				foreach ( NFLDivision d in c.DivList )
					foreach ( NflTeam t in d.TeamList )
						foreach ( NFLPlayer p in t.PlayerList )
							SpitPlayer( p, writer );

			writer.WriteEndDocument();
			writer.Close();
			Utility.Announce( "EP.xml created" );

			Masters.Gm.Dump2Xml();
		}

		private static void SpitPlayer( NFLPlayer p, XmlTextWriter writer )
		{
			var ep = Masters.Epm.GetEp( p.PlayerCode );
			if ( ep == -1.0M )
			{
				if ( p.PerformanceList == null ) p.LoadPerformances( Config.AllGames, false, Utility.CurrentSeason() );
				if ( p.PerformanceList != null )
					if ( p.PerformanceList.Count == 0 ) p.LoadPerformances( Config.AllGames, false, Utility.CurrentSeason() );
				p.CalculateEp( Utility.CurrentSeason() );
			}
			else
			{
				p.ExperiencePoints = ep;
				p.PerformanceList = Masters.Epm.GetGameList( p.PlayerCode );
			}

			writer.WriteStartElement( "player" );

			WriteElement( writer, "id", p.PlayerCode );
			WriteElement( writer, "name", p.PlayerName );
			WriteElement( writer, "ep", p.ExperiencePoints.ToString() );
			WriteEpList( writer, p );
			writer.WriteEndElement();
		}

		private static void WriteEpList( XmlTextWriter writer, NFLPlayer p )
		{
			writer.WriteStartElement( "ep-list" );
			foreach ( NflPerformance g in p.PerformanceList )
			{
				if ( g.Game != null )
				{
					if ( g.ExperiencePoints > 0.0M )
					{
						writer.WriteStartElement( "week" );
						WriteElement( writer, "weekcode", g.Game.WeekCodeOut() );
						WriteElement( writer, "ep", g.ExperiencePoints.ToString() );
						writer.WriteEndElement();
					}
				}
			}
			writer.WriteEndElement();
		}

		private static void WriteElement( XmlTextWriter writer, string name, string text )
		{
			writer.WriteStartElement( name );
			writer.WriteString( text );
			writer.WriteEndElement();
		}

#endregion EP.XML

#region Player Reports

		public void PlayerReports( int reportsToDo, IKeepTheTime timekeeper )
		{
			var reportsDone = 0;
			var totalReports = 0;
			var totalPlayers = 0;
			//  All the players
			foreach ( NflConference c in _confList )
			{
				foreach ( NFLDivision d in c.DivList )
				{
					foreach ( NflTeam t in d.TeamList )
					{
						t.LoadPlayerUnits();
						Announce( $"   Team {t} has {t.PlayerList.Count} current players" );
						totalPlayers += t.PlayerList.Count;
						var reportsAvailable = 0;
						foreach ( NFLPlayer p in t.PlayerList )
						{
							if (p.RookieYear.Equals( timekeeper.Season ) )
							{
								Logger.Trace( $"      Skipping rookie {p}" );
								continue;
							}
							if ( p.IsPlayerReport() )
							{
								reportsAvailable++;
							}
							else
							{
								if ( reportsToDo > 0 && reportsDone >= reportsToDo )
								{
									Announce( $"Quota of {reportsToDo} met" );

									return;
								}
								p.PlayerReport();
								reportsDone++;
							}
						}
						Announce( $"   Team {t} has {reportsAvailable} player reports" );
						totalReports += reportsAvailable; 
					}
				}
			}
			Announce( $"   League has {totalPlayers} current players" );
			Announce( $"   League has {totalReports} current player reports" );
		}

		public void DeletePlayerReports( StringBuilder body )
		{
			var reportsDeleted = 0;
			//  All the players
			foreach ( NflConference c in _confList )
			{
				foreach ( NFLDivision d in c.DivList )
				{
					foreach ( NflTeam t in d.TeamList )
					{
						t.LoadPlayerUnits();
						Announce( $"   Team {t} has {t.PlayerList.Count} current players" );
						foreach ( NFLPlayer p in t.PlayerList )
						{
							if ( p.DeletePlayerReport() )
							{
								Announce( $"   {p.PlayerReportFileName()} deleted" );
								reportsDeleted++;
							}
						}
					}
				}
			}
			var msg = $" {reportsDeleted} reports Deleted ";
			body.AppendLine( msg );

			Announce( msg );
		}

		#endregion Player Reports

		public void RetirePlayers( StringBuilder body )
		{
			var playersRetired = 0;
			var theSeason = Int32.Parse( Season );
			//  All the players
			foreach ( NflConference c in _confList )
			{
				foreach ( NFLDivision d in c.DivList )
				{
					foreach ( NflTeam t in d.TeamList )
					{
						t.LoadPlayerUnits();
						Announce( $"   Team {t} has {t.PlayerList.Count} current players" );
						foreach ( NFLPlayer p in t.PlayerList )
						{
							if ( p.IsProbablyRetired( theSeason ) )
							{
								if ( p.Retire() )
								{
									playersRetired++;
									body.AppendLine( $"{p.PlayerName} retired" );
								}
							}
						}
					}
				}
			}
			var msg = $" {playersRetired} players Retired";
			body.AppendLine( msg );

			Announce( msg );
		}

		#region CSV output

		/// <summary>
		///   Make a big report then write it as CSV
		/// </summary>
		public void DumpPlayersToCsv()
		{
			//  Define the simple report
			var when = string.Format( "Week{0}-{1:0#}", Utility.CurrentSeason(), Int32.Parse( Utility.CurrentWeek() ) + 1 );
			var str = new SimpleTableReport( string.Format( "Player Stats : {0}", when ) );
			str.AddColumn( new ReportColumn( "Jersey", "JERSEY", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Name", "NAME", "{0,-15}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Team", "CURRTEAM", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Unit", "UNIT", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Rookie", "ROOKIEYR", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Drafted", "DRAFTED", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Category", "CATEGORY", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Pos", "POSDESC", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Role", "ROLE", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Owner", "OWNER", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Age", "AGE", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Stars", "STARS", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Value", "VALUE", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Scores", "SCORES", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Rushes", "RUSHES", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "YDr", "YDR", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "AvgYDr", "AVGYDR", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Tdr", "TDR", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Catches", "CATCHES", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "YDc", "YDC", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "AvgYDc", "AVGYDC", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "TDc", "TDC", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "COM", "COM", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "ATTEMPTS", "ATTEMPTS", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "YDp", "YDp", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "AVGYDp", "AVGYDp", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "Tdp", "Tdp", "{0}", typeof( String ) ) );
			str.AddColumn( new ReportColumn( "INT", "INT", "{0}", typeof( String ) ) );

			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "JERSEY", typeof( String ) );
			cols.Add( "NAME", typeof( String ) );
			cols.Add( "TEAM", typeof( String ) );
			cols.Add( "UNIT", typeof( String ) );
			cols.Add( "ROOKIEYR", typeof( String ) );
			cols.Add( "DRAFTED", typeof( String ) );
			cols.Add( "CATEGORY", typeof( String ) );
			cols.Add( "POSDESC", typeof( String ) );
			cols.Add( "ROLE", typeof( String ) );
			cols.Add( "OWNER", typeof( String ) );
			cols.Add( "AGE", typeof( String ) );
			cols.Add( "STARS", typeof( String ) );
			cols.Add( "VALUE", typeof( String ) );
			cols.Add( "SCORES", typeof( String ) );
			cols.Add( "RUSHES", typeof( String ) );
			cols.Add( "YDR", typeof( String ) );
			cols.Add( "AVGYDR", typeof( String ) );
			cols.Add( "TDR", typeof( String ) );
			cols.Add( "CATCHES", typeof( String ) );
			cols.Add( "YDC", typeof( String ) );
			cols.Add( "AVGYDC", typeof( String ) );
			cols.Add( "TDC", typeof( String ) );
			cols.Add( "COM", typeof( String ) );
			cols.Add( "ATTEMPTS", typeof( String ) );
			cols.Add( "YDp", typeof( String ) );
			cols.Add( "AVGYDp", typeof( String ) );
			cols.Add( "Tdp", typeof( String ) );
			cols.Add( "INT", typeof( String ) );

			//  Load the body
			foreach ( NflConference c in _confList )
				foreach ( NFLDivision d in c.DivList )
					foreach ( NflTeam t in d.TeamList )
					{
						if ( t.PlayerList == null ) t.LoadPlayerUnits();
						foreach ( NFLPlayer p in t.PlayerList )
						{
							p.LoadPerformances( false, false, Utility.SeasonInFocus() );

							if ( !string.IsNullOrEmpty( p.PlayerName ) )
							{
								var dr = dt.NewRow();
								dr[ "JERSEY" ] = p.JerseyNo;
								dr[ "NAME" ] = p.PlayerName;
								dr[ "TEAM" ] = p.CurrTeam.TeamCode;
								dr[ "UNIT" ] = p.Unit();
								dr[ "ROOKIEYR" ] = p.RookieYear;
								dr[ "DRAFTED" ] = p.Drafted.Trim();
								dr[ "CATEGORY" ] = p.PlayerCat;
								dr[ "POSDESC" ] = p.PlayerPos.Replace( ",", "-" );
								dr[ "ROLE" ] = p.PlayerRole;
								dr[ "OWNER" ] = p.Owner;
								dr[ "AGE" ] = p.PlayerAge();
								dr[ "STARS" ] = p.StarRating;
								dr[ "VALUE" ] = p.Value().ToString();
								dr[ "SCORES" ] = p.Scores.ToString();
								if ( p.TotStats == null )
								{
									dr[ "RUSHES" ] = "0";
									dr[ "YDR" ] = "0";
									dr[ "AVGYDR" ] = "0.0";
									dr[ "TDR" ] = "0";
									dr[ "CATCHES" ] = "0";
									dr[ "YDC" ] = "0.0";
									dr[ "AVGYDC" ] = "0";
									dr[ "TDC" ] = "0";
									dr[ "COM" ] = "0";
									dr[ "ATTEMPTS" ] = "0";
									dr[ "YDp" ] = "0";
									dr[ "AVGYDp" ] = "0.0";
									dr[ "Tdp" ] = "0";
									dr[ "INT" ] = "0";
								}
								else
								{
									dr[ "RUSHES" ] = p.TotStats.Rushes.ToString();
									dr[ "YDR" ] = p.TotStats.YDr.ToString();
									dr[ "AVGYDR" ] =
										string.Format( "{0:0.0}", Utility.Average( p.TotStats.YDr, p.TotStats.Rushes ) );
									dr[ "TDR" ] = p.TotStats.Tdr.ToString();
									dr[ "CATCHES" ] = p.TotStats.Catches.ToString();
									dr[ "YDC" ] = p.TotStats.YDc.ToString();
									dr[ "AVGYDC" ] =
										string.Format( "{0:0.0}", Utility.Average( p.TotStats.YDc, p.TotStats.Catches ) );
									dr[ "TDC" ] = p.TotStats.Tdc.ToString();
									dr[ "COM" ] = p.TotStats.Completions.ToString();
									dr[ "ATTEMPTS" ] = p.TotStats.PassAtts.ToString();
									dr[ "YDp" ] = p.TotStats.YDp.ToString();
									dr[ "AVGYDp" ] =
										string.Format( "{0:0.0}", Utility.Average( p.TotStats.YDp, p.TotStats.Completions ) );
									dr[ "Tdp" ] = p.TotStats.Tdp.ToString();
									dr[ "INT" ] = p.TotStats.PassInt.ToString();
								}
								dt.Rows.Add( dr );
							}
						}
					}
			str.LoadBody( dt );
			str.RenderAsCsv( "Players", Logger );
		}

#endregion CSV output
	}
}