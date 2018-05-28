using System;
using System.Collections;
using System.Data;
using System.Linq;
using TFLLib;

namespace RosterLib
{
	/// <summary>
	///   Holds all the EP Metrics in History.
	/// </summary>
	public class MetricsBase
	{
		private readonly DataLibrarian _tflWs;
		private string _season;

		public IBreakdown Breakdowns { get; set; }

		public bool DoBreakdowns { get; set; }

		public MetricsBase()
		{
			_tflWs = Utility.TflWs;
			Season = Utility.CurrentSeason();
			TeamList = new ArrayList();
			DoBreakdowns = false;
		}

        public MetricsBase(IBreakdown breakdowns, string season )
        {
            _tflWs = Utility.TflWs;
            Season = season;
            TeamList = new ArrayList();
            Breakdowns = breakdowns;
        }

		//[Inject]
		public MetricsBase( IBreakdown breakdowns )
		{
			_tflWs = Utility.TflWs;
			Season = Utility.CurrentSeason();
			TeamList = new ArrayList();
			Breakdowns = breakdowns;
		}	

		#region  Accessors
		
		public string Season
		{
			get { return _season; }
			set { _season = value; }
		}

		public ArrayList TeamList { get; set; }

		#endregion
		
		public void Load(bool skipPostseason )
		{
			var ds = _tflWs.GetTeams( Season, "" );
			var teams = ds.Tables["team"];
			foreach ( DataRow dr in teams.Rows )
			{
				var teamCode = dr[ "TEAMID" ].ToString();
				var team = Utility.GetTeam( teamCode );
				LoadTeam( team, teamCode, skipPostseason );
				TeamList.Add( team );

				if (DoBreakdowns) DumpBreakdowns(teamCode);
			}
		}

		private void DumpBreakdowns(string teamCode)
		{
			Breakdowns.Dump( teamCode+"-Tdr", string.Format("{0}{2}/breakdowns/{1}-TDR.htm",
			                 Utility.OutputDirectory(), teamCode, Season ) );
			Breakdowns.Dump( teamCode + "-Tdp", string.Format( "{0}{2}/breakdowns/{1}-TDP.htm",
								  Utility.OutputDirectory(), teamCode, Season ) );

		}

		public ArrayList Load( string seasonIn, bool skipPostseason )
		{
			Season = seasonIn;
			var ds = _tflWs.GetTeams( _season, "" );
			var teams = ds.Tables["team"];
			foreach ( DataRow dr in teams.Rows )
			{
				var teamCode = dr[ "TEAMID" ].ToString();

				var team = Utility.GetTeam(teamCode);
				team.Season = seasonIn;
				LoadTeam(team, teamCode, skipPostseason );
            team.SetRecord(seasonIn, skipPostseason );
				TeamList.Add(team);
				if ( DoBreakdowns ) DumpBreakdowns( teamCode );
#if DEBUG
				//break;  //limits to one team 
#endif
			}
			return TeamList;
		}

		private void LoadTeam( NflTeam team, string teamCode, bool skipPostseason )
		{
			var ep = new EpMetric();
			var metricsHt = new Hashtable();

		   DataSet dg;
         dg = skipPostseason ? _tflWs.GetAllRegularSeasonGames( teamCode, Season ) 
            : _tflWs.GetAllGames( teamCode, Season );
			var games = dg.Tables[ "sched" ];
			foreach ( DataRow drg in games.Rows )
			{
				var g = new NFLGame( drg );
//last 2 yrs				if (!g.IsRecent()) continue;
			   if (skipPostseason && g.IsPlayoff()) continue;

				if ( ! g.MetricsCalculated )  g.TallyMetrics(String.Empty);
				var hashCode = string.Format( "{0}{1}{2}", teamCode, g.Season, g.Week );
				if ( g.IsHome( teamCode ) )
				{
					ep.HomeTeam = teamCode;
					ep.OffTDp = g.HomeTDp;
					ep.OffTDr = g.HomeTDr;
					ep.OffSakAllowed = g.HomeSaKa;
					ep.DefTDp = g.AwayTDp;
					ep.DefTDr = g.AwayTDr;
					ep.DefSak = g.AwaySaKa;
					ep.HomePasses = g.HomePasses;
					ep.HomeRuns = g.HomeRuns;
				}
				else
				{
					ep.AwayTeam = g.AwayTeam;
					ep.OffTDp = g.AwayTDp;
					ep.OffTDr = g.AwayTDr;
					ep.OffSakAllowed = g.AwaySaKa;
					ep.DefTDp = g.HomeTDp;
					ep.DefTDr = g.HomeTDr;
					ep.DefSak = g.HomeSaKa;	
					ep.AwayPasses = g.AwayPasses;
					ep.AwayRuns = g.AwayRuns;
				}

				if ( DoBreakdowns ) AddBreakdown( teamCode, ep, g );

				if (ep.Total() <= 0) continue;

				ep.WeekSeed = Utility.WeekSeed( g.Season, g.Week );
				if ( ! metricsHt.ContainsKey( hashCode ) )
					metricsHt.Add( hashCode, ep );
			}
			team.SetMetrics( metricsHt );
#if DEBUG
			Utility.PrintIndexAndKeysAndValues( metricsHt );
#endif
		}

		private void AddBreakdown(string teamCode, EpMetric ep, NFLGame g)
		{
			var scorers = string.Empty;

			if ( ep.OffTDr > 0 )
				scorers = g.ScorersOff( Constants.K_SCORE_TD_RUN, teamCode );

			Breakdowns.AddLine( string.Format( "{0}-Tdr", teamCode ), 
				string.Format("{0}  {1:#0} {2}",
			   g.GameName(), ep.OffTDr, scorers ));

			if ( ep.OffTDp > 0 )
				scorers = g.ScorersOff( Constants.K_SCORE_TD_PASS, teamCode );

			Breakdowns.AddLine( string.Format( "{0}-Tdp", teamCode ),
				string.Format( "{0}  {1:#0} {2}",
				g.GameName(), ep.OffTDp, scorers ) );
		}

		#region  Output
		
		public void RenderTeams( bool skipPostseason )
		{
			Load(skipPostseason:false);
			foreach (var team in TeamList.Cast<NflTeam>())
				RenderTeam(team, skipPostseason);
		}

      //public void RenderTeam( string teamCode )
      //{
      //   var team = new NflTeam( teamCode );
      //   RenderTeam( team );		
      //}
		
		public void RenderTeam( NflTeam team, bool skipPostseason )
		{
			if ( team.MetricsHt == null ) LoadTeam( team, team.TeamCode, skipPostseason );
			var metricsHt = team.MetricsHt;
			var str = new SimpleTableReport( string.Format( "Metrics-{0}", team.TeamCode ) );
			str.AddStyle(  "#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; width: 761px; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }" );
			str.AddStyle(  "#main { margin-left:1em; }" );
			str.AddStyle(  "#dtStamp { font-size:0.8em; }" );
			str.AddStyle(  ".end { clear: both; }" );
			str.AddStyle(  ".gponame { color:white; background:black }" );
			str.ColumnHeadings = true;
			str.DoRowNumbers = true;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn( new ReportColumn( "Week",       "WeekSeed",  "{0}", typeof( String ), false       ) ); 
			str.AddColumn( new ReportColumn( "OffTDp",      "OffTDp",   "{0}", typeof( Int32 ), true       ) ); 
			str.AddColumn( new ReportColumn( "Mult",        "AvgOffTDp", "{0:#.#}", typeof( Decimal ), false   ) ); 
			str.AddColumn( new ReportColumn( "OffTDr",      "OFFTDR",   "{0}", typeof( Int32 ), true       ) ); 
			str.AddColumn( new ReportColumn( "Mult",         "AvgOffTDr", "{0:#.#}", typeof( Decimal ), false   ) ); 
			str.AddColumn( new ReportColumn( "OffSAKa",     "OFFSAKa",  "{0}", typeof( Decimal ), true ) ); 
			str.AddColumn( new ReportColumn( "Mult",         "AvgOffSaka", "{0:#.#}", typeof( Decimal ), false   ) ); 			
			str.AddColumn( new ReportColumn( "DefTDpa",     "DefTDpa",  "{0}", typeof( Int32 ), true       ) ); 
			str.AddColumn( new ReportColumn( "Mult",         "AvgDefTDp", "{0:#.#}", typeof( Decimal ), false   ) ); 
			str.AddColumn( new ReportColumn( "DefTDra",     "DEFTDRa",  "{0}", typeof( Int32 ), true       ) ); 
			str.AddColumn( new ReportColumn( "Mult",         "AvgDefTDr", "{0:#.#}", typeof( Decimal ), false   ) ); 
			str.AddColumn( new ReportColumn( "DefSAK",      "DEFSAK",   "{0}", typeof( Decimal ), true ) ); 
			str.AddColumn( new ReportColumn( "Mult",         "AvgDefSak", "{0:#.#}", typeof( Decimal ), false   ) ); 			
			BuildTable( str, metricsHt, team );
			str.SetSortOrder( "WeekSeed" );
         str.RenderAsHtml(string.Format("{0}Metrics-{1}.htm", Utility.OutputDirectory(), team.TeamCode), true);
		}
	
		private static void BuildTable( SimpleTableReport str, Hashtable metricsHt, NflTeam team )
		{
			if ( metricsHt != null )
			{
				var myEnumerator = metricsHt.GetEnumerator();			
				while ( myEnumerator.MoveNext() )
				{
					var ep = (EpMetric) myEnumerator.Value;
					var dr = str.Body.NewRow();
					dr[ "WEEKSEED" ] = Utility.SeedOut(ep.WeekSeed);
					dr[ "OFFTDP"  ] = ep.OffTDp;
					dr[ "AVGOFFTDP" ] = team.PoMultiplierAt( ep.WeekSeed );
					dr[ "OFFTDR"  ] = ep.OffTDr;
					dr[ "AVGOFFTDR" ] = team.RoMultiplierAt( ep.WeekSeed );
					dr[ "OFFSAKA" ] = ep.OffSakAllowed;
					dr[ "AVGOFFSAKA" ] = team.PpMultiplierAt( ep.WeekSeed );
					dr[ "DEFTDPA" ] = ep.DefTDp;
					dr[ "DEFTDRA" ] = ep.DefTDr;
					dr[ "DEFSAK"  ] = ep.DefSak;
					dr[ "AVGDEFTDP" ] = team.PdMultiplierAt( ep.WeekSeed );
					dr[ "AVGDEFTDR" ] = team.RdMultiplierAt( ep.WeekSeed );
					dr[ "AVGDEFSAK" ] = team.PrMultiplierAt( ep.WeekSeed );

					str.Body.Rows.Add( dr );
				}
			}
		}
		
		
		#endregion
		
	}
	
	struct EpMetric
	{
		public int WeekSeed;
		public string HomeTeam;
		public string AwayTeam;
		public int OffTDp;
		public int OffTDr;
		public decimal OffSakAllowed;
		public int DefTDp;
		public int DefTDr;
		public decimal DefSak;
		public int HomePasses;
		public int AwayPasses;
		public int HomeRuns;
		public int AwayRuns;
		public decimal Total()
		{
			var tot = OffSakAllowed + DefSak;
			tot += OffTDp + OffTDr + DefTDp + DefTDr;
			return tot;
		}
	}
}
