using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace RosterLib
{
	/// <summary>
	///  Abstraction of a GridStats Team
	/// </summary>
	public class GsTeam
	{
		private readonly Hashtable _teamHt;

		private readonly ArrayList _playerList;

		public string LeagueId;

		public ILeague League { get; set; }
		
		public GsTeam( DataRow teamRow, string leagueId, ILeague league )
		{
			LeagueId = leagueId;
			League = league;
			_teamHt = new Hashtable();
			Name = teamRow["FRANCHISE"].ToString().Trim();
			OwnerId = teamRow["OWNERID"].ToString().Trim();
			
			_playerList = new ArrayList();
			for (var i = 65; i < 91; i++ )  //  from A-Z
			{
				var playerCode = string.Format( "PLAYER{0}", (char) i );
				AddPlayer( teamRow[ playerCode ].ToString().Trim() );
			}

			//PrintIndexAndKeysAndValues( teamHT );
			FigureOutGreatestBias();
			
			IsHuman = FigureOutHumanity();

#if DEBUG
			Utility.Announce( $"{TeamOut(),-20} has {PlayoffStarters} PlayoffStarters {PlayoffBackups} PlayoffBackups {PlayoffOthers} PlayoffOthers" );
#endif
		}

		/// <summary>
		/// Adds the player to the teams roster and increments counts.
		/// Only if it is a valid code (not blank)
		/// Also tallies the points by NFL team
		/// </summary>
		/// <param name="playerId">The player id.</param>
		private void AddPlayer( string playerId )
		{
			if ( playerId.Trim().Length > 0 )
			{
				var p = new NFLPlayer( playerId );
				_playerList.Add( p );
				if ( p.IsPlayoffBound() )
				{
					if ( p.PlayerRole == "S" )
						PlayoffStarters++;
					else
					{
						if ( p.PlayerRole == "B" )
							PlayoffBackups++;
						else
						{
							if ( p.PlayerRole == "I" )
								PlayoffInjuries++;
							else
								PlayoffOthers++;
						}
					} 
				}
				else
				{ 
					if ( p.PlayerRole == "S" )
						Starters++;
					else
					{
						if ( p.PlayerRole == "B" )
							Backups++;
						else
						{
							if ( p.PlayerRole == "I" )
								Injuries++;
							else
								Others++;
						}
					}
				}

				//  keep track of team bias in a hashtable
				AddTeam( p.TeamCode, PointsFor( p.PlayerRole, p.IsPlayoffBound() ) );
			}
		}
		
		private void FigureOutGreatestBias()
		{
			var myEnumerator = _teamHt.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				if ( (int) myEnumerator.Value > BiasScore )
				{
					TeamBias = (string) myEnumerator.Key;
					BiasScore = (int) myEnumerator.Value;
				}
			}			
		}
		
		private void AddTeam( string teamCode, int points )
		{
			if ( string.IsNullOrEmpty( teamCode ) || teamCode == "??" )
				return;

			if ( _teamHt.ContainsKey( teamCode ) )
				_teamHt[ teamCode ] = (int) _teamHt[ teamCode ] + points;
			else
				_teamHt.Add( teamCode, points );
		}
		
		public int Value()
		{
			return ( Starters * PointsFor( "S", false ) ) + 
				    ( Backups  * PointsFor( "B", false ) ) + 
					 ( Others   * PointsFor( "O", false ) ) +
				    ( PlayoffStarters  * PointsFor( "S", true ) ) + 
				    ( PlayoffBackups   * PointsFor( "B", true ) ) + 
					 ( PlayoffOthers    * PointsFor( "O", true ) );
		}
		
		public string BiasOut()
		{
			return string.Format( "{0}-{1:#0} ({2:###.#}%)", TeamBias, BiasScore, BiasPercent() );
		}
		
		private decimal BiasPercent()
		{
			return ( BiasScore / (decimal) Value() ) * 100.0M;
		}
		
		private static int PointsFor( string role, bool playoffBound )
		{
			int points;
			switch ( role )
			{
				case "S":
					points = 5;
					break;
				case "B":
					points = 2;
					break;
				case "I":
					points = 0;
					break;					
				default:
					points = 1;
					break;
			}
			if ( playoffBound ) points *= 2;  //  Playoff players count double
			return points;
		}
		
		public int RateGame( NFLGame game )
		{
			int rating = 0;
			if (_playerList != null)
			{
				rating = ( from NFLPlayer p in _playerList
							  where ( p.CurrTeam.TeamCode == game.AwayTeam ) || ( p.CurrTeam.TeamCode == game.HomeTeam )
							  select PointsFor( p.PlayerRole, p.IsPlayoffBound() ) ).Sum();
			}
			return rating;
		}
		
		#region  Accessors

		public string Name { get; set; }

		public int Starters { get; set; }

		public int Backups { get; set; }

		public int Others { get; set; }

		public string TeamBias { get; set; }

		public int BiasScore { get; set; }

		public int Injuries { get; set; }

		public bool IsHuman { get; set; }

		public string OwnerId { get; set; }

		public int PlayoffStarters { get; set; }

		public int PlayoffBackups { get; set; }

		public int PlayoffOthers { get; set; }

		public int PlayoffInjuries { get; set; }

		#endregion
		
		/// <summary>
		///   Zoom in at the player level
		/// </summary>
		public void DumpTeam()
		{
			var teamdump = new SimpleTableReport(string.Format("Team: {0}", Name)) {ColumnHeadings = true, DoRowNumbers = true};
			teamdump.AddColumn( new ReportColumn( "Player", "PLAYER", "{0,20}", BgPicker ) );
			teamdump.AddColumn( new ReportColumn( "Pos", "POS", "{0}" ) );
			teamdump.AddColumn( new ReportColumn( "Role", "ROLE", "{0}" ) );
			teamdump.AddColumn( new ReportColumn( "Team", "TEAM", "{0}" ) );
			teamdump.AddColumn( new ReportColumn( "Seasons", "SEASONS", "{0}" ) );
			teamdump.AddColumn( new ReportColumn( "ScoreAvg", "SCORES", "{0:0.0}", true ) );
			teamdump.LoadBody( BuildTable() );
			teamdump.RenderAsHtml( DumpFileName(), true );			
		}

		private static string BgPicker( string theValue )
		{
			string sColour;

			switch (theValue)
			{
				case "S":
					sColour = "YELLOW";
					break;
				case "B":
					sColour = "GREEN";
					break;
				case "I":
					sColour = "PINK";
					break;

				default:
					sColour = "WHITE";
					break;
			}
			return sColour;
		}

		private string DumpFileName()
		{
          return string.Format("{0}{1}{2}", 
				 Utility.OutputDirectory(),
				 string.Format( "{0}\\RosterSummary\\{1}\\", League.Season, LeagueId ),
				 ShortFileName( League.SeasonNo, League.WeekNo ));         
		}

		public string ShortFileName( int season, int weekNo )
		{
			return string.Format( "teamdumps\\teamdump-{0}-{1:00}.htm",	OwnerId, weekNo );
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add( "PLAYER", typeof ( String ) );
			cols.Add( "CATEGORY", typeof ( String ) );
			cols.Add( "POS", typeof ( String ) );
			cols.Add( "ROLE", typeof ( String ) );
			cols.Add( "TEAM", typeof ( String ) );
			cols.Add( "SEASONS", typeof( Int32 ) );
			cols.Add( "SCORES", typeof ( Decimal ) );

			foreach ( NFLPlayer p in _playerList )
			{
				var dr = dt.NewRow();
				dr[ "PLAYER" ] = p.PlayerOut();
				dr[ "CATEGORY" ] = p.PlayerCat;
				dr[ "POS" ] = p.PlayerPos;
				dr[ "ROLE" ] = p.PlayerRole;
				dr[ "TEAM" ] = p.TeamCode;
				dr[ "SEASONS" ] = p.NoOfSeasons();
				dr[ "SCORES" ] = p.ScoresPerYear();
				dt.Rows.Add( dr );
			}
			dt.DefaultView.Sort = "CATEGORY,SCORES DESC";
			return dt;
		}

		public string TeamUrl()
		{
			return string.Format( "<a href='{0}'>{1}</a>", ShortFileName( League.SeasonNo, League.WeekNo ), TeamOut() );
		}

		private string TeamOut()
		{
			return string.Format( "{0}{1}", Name, HumanFlag( IsHuman ) );
		}

		private static string HumanFlag( bool isHuman )
		{
			return isHuman ? "*" : "";
		}

		private bool FigureOutHumanity()
		{
			if ( OwnerId == "O001" ) return true;
			if ( OwnerId == "HHGS4" ) return true;
			if ( OwnerId == "KNGS4") return true;
			if ( OwnerId == "CKGS4") return true;
			if ( OwnerId == "GS404") return true;
			if ( OwnerId == "B004") return true;
			if ( OwnerId == "B015") return true;
			if ( OwnerId == "B007") return true;
			return OwnerId == @"B008";
		}
	}
}
