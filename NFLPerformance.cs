using System;
using System.Data;
using TFLLib;

namespace RosterLib
{
	/// <summary>
	/// Record of what someone did in an NFl Game.
	/// </summary>
	public class NflPerformance
	{
		private readonly DataLibrarian _tflWs;

		private readonly NFLPlayer _thePlayer;

		public PlayerStats PerfStats;

		public NflPerformance( int seasonIn, int weekIn, string teamIn, NFLPlayer myPlayer )
		{
			// constructor
			TeamCode = teamIn;
			Season = seasonIn;
			Week = weekIn;
			_thePlayer = myPlayer;
			_tflWs = Utility.TflWs;

			PerfStats = new PlayerStats();

			if ( teamIn == "??" ) return;

			// What was the result
			var ds = _tflWs.ResultFor( TeamCode, seasonIn, weekIn );
			if ( ds.Tables[ 0 ].Rows.Count > 0 )
			{
				var dr = ds.Tables[ 0 ].Rows[ 0 ];
				var gameCode = string.Format( "{0}:{1}-{2}", seasonIn, dr[ "WEEK" ], dr[ "GAMENO" ] );

				Game = Masters.Gm.GetGame( gameCode ) ?? new NFLGame( dr );
				if ( Game.Played() )
				{
					LoadScores();
					LoadStats();
				}
			}
			else
				TeamCode = "bye";
		}

		public NflPerformance( string seasonIn, string weekIn, decimal epIn )
		{
			// constructor
			Season = Int32.Parse( seasonIn );
			Week = Int32.Parse( weekIn );
			ExperiencePoints = epIn;
			Game = new NFLGame( seasonIn, weekIn );
		}

		public string PerfHeaders()
		{
			return HtmlLib.TableRowOpen() + "\n" + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + HtmlLib.HTMLPadL( "Week", 2 ) + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + HtmlLib.HTMLPadL( "Game", 2 ) + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + HtmlLib.HTMLPadL( "Date", 2 ) + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + HtmlLib.HTMLPadL( "Result", 2 ) + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + HtmlLib.HTMLPadL( Col1Header(), 2 ) + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + HtmlLib.HTMLPadL( Col2Header(), 2 ) + HtmlLib.TableRowHeaderClose() + "\n"
				   + HtmlLib.TableRowHeaderOpen( "scope='col'" ) + "\n"
				   + HtmlLib.TableRowClose() + "\n";
		}

		private string Col1Header()
		{
			string s;
			switch ( _thePlayer.PlayerCat )
			{
				case "1":
					s = "Passing";
					break;

				case "2":
					s = "Rushing";
					break;

				case "3":
					s = "Receiving";
					break;

				case "4":
					s = "Conversions";
					break;

				default:
					s = "Sacks-Steals";
					break;
			}
			return s;
		}

		private string Col2Header()
		{
			string s;
			switch ( _thePlayer.PlayerCat )
			{
				case "1":
					s = "Rushing";
					break;

				case "2":
					s = "Receiving";
					break;

				case "3":
					s = "Rushing";
					break;

				case "4":
					s = "Total Pts";
					break;

				default:
					s = "";
					break;
			}
			return s;
		}

		public string PerfRow()
		{
			if ( ( TeamCode == null ) || ( TeamCode == "??" ) || ( TeamCode == "bye" ) )
				return String.Empty;

			return HtmlLib.TableRowOpen( RowClass() ) + "\n"
				   + HtmlLib.TableData( String.Format( "{0}:{1:0#}", Season, Week ) ) + "\n"
				   + HtmlLib.TableData( GameOut() ) + "\n"
				   + HtmlLib.TableData( Game.GameDate.ToString( "dd/MM/yyyy" ) ) + "\n"
				   + HtmlLib.TableData( ResultOut() ) + "\n"
				   + HtmlLib.TableData( PerfStats.Stat1( _thePlayer.PlayerCat, false ) ) + "\n"
				   + HtmlLib.TableData( PerfStats.Stat2( _thePlayer.PlayerCat ) ) + "\n"
				   + HtmlLib.TableRowClose() + "\n";
		}

		private string RowClass()
		{
			if ( ( Week == 2 ) || ( Week == 4 ) || ( Week == 6 ) || ( Week == 8 ) || ( Week == 10 ) || ( Week == 12 ) ||
				 ( Week == 14 ) || ( Week == 16 ) )
				return "class='monoeven' ";

			return "class='mono' ";
		}

		private string GameOut()
		{
			string s;
			if ( Game.IsHome( TeamCode ) )
				s = TeamCode + " v " + Game.AwayTeam;
			else
				s = TeamCode + " @ " + Game.HomeTeam;
			return s;
		}

		private string ResultOut()
		{
			return Game.ResultOut( TeamCode, false ) + " " + Game.ScoreOut( TeamCode );
		}

		#region Load methods

		private void LoadScores()
		{
			DataSet ds = _tflWs.PlayerScoresDs( Season.ToString(), String.Format( "{0:0#}", Week ), _thePlayer.PlayerCode );
			DataTable dt = ds.Tables[ "score" ];
			foreach ( DataRow dr in dt.Rows )
			{
				switch ( dr[ "SCORE" ].ToString() )
				{
					case "P":
						if ( dr[ "PLAYERID1" ].ToString() == _thePlayer.PlayerCode )
							PerfStats.Tdc++;
						else
							PerfStats.Tdp++;
						break;

					case "R":
						PerfStats.Tdr++;
						break;

					case "3":
						PerfStats.Fg++;
						break;

					case "1":
						PerfStats.Pat++;
						break;

					case "I":
						PerfStats.IntRet++;
						break;

					case "F":
						PerfStats.FumRet++;
						break;

					case "K":
						PerfStats.KickRet++;
						break;

					case "2":
						if ( dr[ "PLAYERID1" ].ToString() == _thePlayer.PlayerCode )
							PerfStats.PatPass++;
						else
							PerfStats.PatCatch++;
						break;

					case "N":
						PerfStats.PatRun++;
						break;

					case "S":
						PerfStats.Safety++;
						break;
				}
			}
		}

		private void LoadStats()
		{
			var ds = _tflWs.PlayerStatsDs( Season.ToString(), String.Format( "{0:0#}", Week ), _thePlayer.PlayerCode );
			DataTable dt = ds.Tables[ "stat" ];
			foreach ( DataRow dr in dt.Rows )
			{
				switch ( dr[ "STAT" ].ToString() )
				{
					case "Y":
						PerfStats.YDr += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "R":
						PerfStats.Rushes += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "C":
						PerfStats.Completions += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "A":
						PerfStats.PassAtts += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "S":
						PerfStats.YDp += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "Z":
						PerfStats.PassInt += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "P":
						PerfStats.Catches += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "G":
						PerfStats.YDc += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "Q":
						PerfStats.Sacks += Decimal.Parse( dr[ "QTY" ].ToString() );
						break;

					case "M":
						PerfStats.Ints += ( int ) Decimal.Parse( dr[ "QTY" ].ToString() );
						break;
				}
			}
		}

		#endregion Load methods

		#region Accessors

		public string TeamCode { get; set; }

		public int Season { get; set; }

		public int Week { get; set; }

		public NFLGame Game { get; set; }

		public decimal ExperiencePoints { get; set; }

		#endregion Accessors
	}
}