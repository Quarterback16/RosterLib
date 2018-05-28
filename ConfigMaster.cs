using System;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace RosterLib
{
	/// <summary>
	///    A singleton that knows all the config settings.
	/// </summary>
	public class ConfigMaster
	{
		private static ConfigMaster _mConfigMaster;

		private ConfigMaster()
		{
		} //  private constructor

		[MethodImpl( MethodImplOptions.Synchronized )]
		public static ConfigMaster GetInstance()
		{
			if (_mConfigMaster == null)
			{
				//  One time initialisation
				_mConfigMaster = new ConfigMaster();
				return _mConfigMaster;
			}
			return _mConfigMaster;
		}

		#region  Accessors

		public bool GenerateStatsXml
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "GenerateStatsXml" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool GenerateYahooXml
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "GenerateYahooXml" ], "TRUE", true ) == 0;
				}
			}
		}

		public string NflConnectionString
		{
			get { return ConfigurationManager.AppSettings[ "NflConnectionString" ]; }
		}

		public bool OldMatrix
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "OldMatrix" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Espn
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "ESPN" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool NewFormat
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "NewFormat" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Returners
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Returners" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool DepthCharts
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "DepthCharts" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Starters
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Starters" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Roster
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Roster" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool ShowPerformance
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "ShowPerformance" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool HotLists
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "HotList" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool HillenTips
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "HillenTips" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool StatsGrids
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "StatsGrids" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool SuggestedLineups
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "SuggestLineup" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool ProjectedLineups
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "ProjectLineup" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool SoS
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "SoS" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool FrequencyTables
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "FrequencyTables" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool TeamMetrics
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "TeamMetrics" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool GridStatsReport
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "GridStats" ], "TRUE", true ) == 0;
				}
			}
		}


		public bool Projections
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Projections" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool TeamCards
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "TeamCards" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool PlayerReports
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "PlayerReports" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool DefensiveScoring
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "DefensiveScoring" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool PlayerCsv
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "PlayerCsv" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Kickers
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Kickers" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Rankings
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Rankings" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool OffensiveLine
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "OffensiveLine" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Scorers
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Scorers" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Experience
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Experience" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool VictoryPoints
		{
			get { return String.Compare( ConfigurationManager.AppSettings[ "VictoryPoints" ], "TRUE", true ) == 0; }
		}

		public bool FaMarket
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "FAMarket" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool UnitReports
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "UnitReports" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool UnitsByWeek
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "UnitsByWeek" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool StarRatings
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "StarRatings" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool GsPerformance
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "GsPerformance" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool YahooProjections
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "YahooProjections" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool FpProjections
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "FP-Projections" ], "TRUE", true ) == 0;
				}
			}
		}


		public bool EspnPerformance
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "EspnPerformance" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool BalanceReport
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "BalanceReport" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool FreeAgents
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "FreeAgents" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool NflUk
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "NflUk" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool LineupCards
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "LineupCards" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool Gs4WrRanks
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "RateWR" ], "TRUE", true ) == 0;
				}
			}
		}


		public bool MatchUps
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "MatchUps" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool BackTest
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "BackTest" ], "TRUE", true ) == 0;
				}
			}
		}


		public bool Plays
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "Plays" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool GameLog
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "GameLog" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool AllPlayerGames
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "AllPlayerGames" ], "TRUE", true ) == 0;
				}
			}
		}


		public bool HideBackups
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "HideBackups" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool HideReserves
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "HideReserves" ], "TRUE", true ) == 0;
				}
			}
		}

		public bool HideInjuries
		{
			get
			{
				{
					return String.Compare( ConfigurationManager.AppSettings[ "HideInjuries" ], "TRUE", true ) == 0;
				}
			}
		}

		#endregion

		public string PrimaryDrive
		{
			get
			{
				{
					return ConfigurationManager.AppSettings[ "PrimaryDrive" ];
				}
			}
		}

		public string OutputDirectory
		{
			get
			{
				{
					return ConfigurationManager.AppSettings[ "OutputDirectory" ];
				}
			}
		}

		public string XmlDirectory
		{
			get
			{
				{
					return ConfigurationManager.AppSettings[ "XmlDirectory" ];
				}
			}
		}

		public string TflFolder
		{
			get
			{
				{
					return ConfigurationManager.AppSettings[ "tfl-directory" ];
				}
			}
		}
	}
}