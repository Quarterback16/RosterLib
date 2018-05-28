using NLog;
using RosterLib.Interfaces;
using System;
using System.Collections.Generic;

namespace RosterLib
{
	public class PullMetricsFromPrediction
	{
		public Logger Logger { get; set; }

		private IDictionary<RunApproach, IAllocateTDrStrategy> tdrAllocationStrategies;
		private IDictionary<RunApproach, IAllocateYDrStrategy> ydrAllocationStrategies;

		public PullMetricsFromPrediction( PlayerGameProjectionMessage input )
		{
			Logger = LogManager.GetCurrentClassLogger();
			tdrAllocationStrategies = new Dictionary<RunApproach, IAllocateTDrStrategy>
			{
				{ RunApproach.Ace, new AceAllocateTDrStrategy() },
				{ RunApproach.Standard, new StandardAllocateTDrStrategy() },
				{ RunApproach.Committee, new CommitteeAllocateTDrStrategy() }
			};
			ydrAllocationStrategies = new Dictionary<RunApproach, IAllocateYDrStrategy>
			{
				{ RunApproach.Ace, new AceAllocateYDrStrategy() },
				{ RunApproach.Standard, new StandardAllocateYDrStrategy() },
				{ RunApproach.Committee, new CommitteeAllocateYDrStrategy() }
			};
			Process( input );
		}

		private void Process( PlayerGameProjectionMessage input )
		{
			if ( input.Game == null ) return;

			Logger.Trace( $"Processing {input.Game.GameCodeOut()}:{input.Game.GameName()}" );
			DoRushingUnit( input, input.Game.HomeNflTeam.TeamCode, isHome: true );
			DoRushingUnit( input, input.Game.AwayNflTeam.TeamCode, isHome: false );

			DoPassingUnit( input, input.Game.HomeNflTeam.TeamCode, isHome: true );
			DoPassingUnit( input, input.Game.AwayNflTeam.TeamCode, isHome: false );

			DoKickingUnit( input, input.Game.HomeNflTeam.TeamCode, isHome: true );
			DoKickingUnit( input, input.Game.AwayNflTeam.TeamCode, isHome: false );
			if ( input.Game.PlayerGameMetrics.Count < 12 )
				Utility.Announce( string.Format( "Missing metrics from {0}", input.Game ) );
		}

		#region Kicking

		private void DoKickingUnit( PlayerGameProjectionMessage input, string teamCode, bool isHome )
		{
			var kicker = GetKicker( teamCode );
			if ( kicker != null )
			{
				var projFG = ( isHome ) ? input.Prediction.HomeFg : input.Prediction.AwayFg;
				var projPat = ( isHome ) ? input.Prediction.TotalHomeTDs() : input.Prediction.TotalAwayTDs();
				AddKickingPlayerGameMetric( input, kicker.PlayerCode, projFG, projPat );
			}
			else
				Utility.Announce( string.Format( "No kicker found for {0}", teamCode ) );
		}

		private static NFLPlayer GetKicker( string teamCode )
		{
			var team = new NflTeam( teamCode );
			team.SetKicker();
			return team.Kicker;
		}

		private static void AddKickingPlayerGameMetric( PlayerGameProjectionMessage input,
						string playerId, int projFg, int projPat )
		{
			if ( input == null || playerId == null ) return;
			var pgm = new PlayerGameMetrics
			{
				PlayerId = playerId,
				GameKey = input.Game.GameKey(),
				ProjFG = projFg,
				ProjPat = projPat
			};
			input.Game.PlayerGameMetrics.Add( pgm );
		}

		#endregion Kicking

		#region Passing

		private void DoPassingUnit( PlayerGameProjectionMessage input, string teamCode, bool isHome )
		{
			PassUnit unit;
			if ( isHome )
				unit = input.Game.HomeNflTeam.PassUnit;
			else
				unit = input.Game.AwayNflTeam.PassUnit;

			if ( unit == null ) unit = new PassUnit();
			if ( !unit.IsLoaded() ) unit.Load( teamCode );

			// give it to the QB
			if ( unit.Q1 != null )
			{
				var projYDp = ( isHome ) ? input.Prediction.HomeYDp : input.Prediction.AwayYDp;
				var projTDp = ( isHome ) ? input.Prediction.HomeTDp : input.Prediction.AwayTDp;
				AddPassinglayerGameMetric( input, unit.Q1.PlayerCode, projYDp, projTDp );
			}
			// Receivers  W1 40%, W2 25%, W3 10%, TE 20% (todo 3D 5%)
			int projYDc, projTDc;
			if ( unit.W1 != null )
			{
				projYDc = ( int ) ( .35 * ( ( isHome ) ? input.Prediction.HomeYDp : input.Prediction.AwayYDp ) );
				projTDc = W1TdsFrom( ( isHome ) ? input.Prediction.HomeTDp : input.Prediction.AwayTDp );
				projYDc = AllowForInjuryRisk( unit.W1, projYDc );
				AddCatchingPlayerGameMetric( input, unit.W1.PlayerCode, projYDc, projTDc );
			}
			if ( unit.W2 != null )
			{
				projYDc = ( int ) ( .25 * ( ( isHome ) ? input.Prediction.HomeYDp : input.Prediction.AwayYDp ) );
				projTDc = W2TdsFrom( ( isHome ) ? input.Prediction.HomeTDp : input.Prediction.AwayTDp );
				projYDc = AllowForInjuryRisk( unit.W2, projYDc );
				AddCatchingPlayerGameMetric( input, unit.W2.PlayerCode, projYDc, projTDc );
			}
			if ( unit.W3 != null )
			{
				projYDc = ( int ) ( .15 * ( ( isHome ) ? input.Prediction.HomeYDp : input.Prediction.AwayYDp ) );
				projTDc = 0;
				projYDc = AllowForInjuryRisk( unit.W3, projYDc );
				AddCatchingPlayerGameMetric( input, unit.W3.PlayerCode, projYDc, projTDc );
			}
			if ( unit.TE != null )
			{
				projYDc = ( int ) ( .20 * ( ( isHome ) ? input.Prediction.HomeYDp : input.Prediction.AwayYDp ) );
				projTDc = TETdsFrom( ( isHome ) ? input.Prediction.HomeTDp : input.Prediction.AwayTDp );
				projYDc = AllowForInjuryRisk( unit.TE, projYDc );
				AddCatchingPlayerGameMetric( input, unit.TE.PlayerCode, projYDc, projTDc );
			}
		}

		private void AddCatchingPlayerGameMetric( PlayerGameProjectionMessage input,
		   string playerId, int projYDc, int projTDc )
		{
			if ( input == null || playerId == null ) return;
			if ( string.IsNullOrEmpty( playerId ) || input.Game == null ) return;
			var pgm = new PlayerGameMetrics
			{
				PlayerId = playerId,
				GameKey = input.Game.GameKey(),
				ProjYDc = projYDc,
				ProjTDc = projTDc
			};
#if DEBUG
			Utility.Announce( pgm.ToString() );
#endif
			input.Game.PlayerGameMetrics.Add( pgm );
		}

		private static int W1TdsFrom( int totalTds )
		{
			var tds = 0;
			switch ( totalTds )
			{
				case 1:
					tds = 1;
					break;

				case 2:
					tds = 1;
					break;

				case 3:
					tds = 1;
					break;

				case 4:
					tds = 2;
					break;

				case 5:
					tds = 2;
					break;

				case 6:
					tds = 3;
					break;
			}
			return tds;
		}

		private static int W2TdsFrom( int totalTds )
		{
			var tds = 0;
			switch ( totalTds )
			{
				case 1:
					tds = 0;
					break;

				case 2:
					tds = 1;
					break;

				case 3:
					tds = 1;
					break;

				case 4:
					tds = 1;
					break;

				case 5:
					tds = 2;
					break;

				case 6:
					tds = 2;
					break;
			}
			return tds;
		}

		private static int TETdsFrom( int totalTds )
		{
			var tds = 0;
			switch ( totalTds )
			{
				case 1:
					tds = 0;
					break;

				case 2:
					tds = 0;
					break;

				case 3:
					tds = 1;
					break;

				case 4:
					tds = 1;
					break;

				case 5:
					tds = 1;
					break;

				case 6:
					tds = 1;
					break;
			}
			return tds;
		}

		private static void AddPassinglayerGameMetric( PlayerGameProjectionMessage input, string playerId, int projYDp, int projTDp )
		{
			if ( input == null || playerId == null ) return;
			var pgm = new PlayerGameMetrics
			{
				PlayerId = playerId,
				GameKey = input.Game.GameKey(),
				ProjYDp = projYDp,
				ProjTDp = projTDp
			};
#if DEBUG
			Utility.Announce( pgm.ToString() );
#endif
			input.Game.PlayerGameMetrics.Add( pgm );
		}

		#endregion Passing

		#region Rushing

		private void DoRushingUnit(
			PlayerGameProjectionMessage input, string teamCode, bool isHome )
		{
			RushUnit ru;
			if ( isHome )
				ru = input.Game.HomeNflTeam.RunUnit;
			else
				ru = input.Game.AwayNflTeam.RunUnit;

			if ( ru == null )
				ru = new RushUnit();

			if( !ru.IsLoaded() )
				ru.Load( teamCode );

			var pgms = new PlayerGameMetricsCollection( input.Game );
			var projTDr = ( isHome ) ? input.Prediction.HomeTDr : input.Prediction.AwayTDr;
			projTDr = AllowForVultures( ru, projTDr, pgms );
			AllocateTDr( ru, projTDr, pgms);
			input.Game.PlayerGameMetrics = pgms.Pgms;

			var projYDr = ( isHome ) ? input.Prediction.HomeYDr : input.Prediction.AwayYDr;
			AllocateYDr( ru, projYDr, pgms );

			if ( ru.ThirdDownBack != null )
			{
				var projYDc = ( int ) ( .05 * ( ( isHome ) ? input.Prediction.HomeYDp : input.Prediction.AwayYDp ) );
				var pgm = pgms.GetPgmFor( ru.ThirdDownBack.PlayerCode );
				pgm.ProjYDc += projYDc;
				pgms.Update( pgm );
			}
		}

		#region Goalline Vultures

		private int AllowForVultures( RushUnit ru, int projTDr, PlayerGameMetricsCollection pgms )
		{
			var nTDr = projTDr;
			if ( ru.GoalLineBack != null )
			{
				var pgm = pgms.GetPgmFor( ru.GoalLineBack.PlayerCode );
				var vulturedTDs = VulturedTdsFrom( nTDr );
				pgm.ProjTDr += vulturedTDs;
				pgms.Update( pgm );
				nTDr -= vulturedTDs;
			}
			return nTDr;
		}

		private int VulturedTdsFrom( int nTDr )
		{
			if ( nTDr > 4 ) return 2;
			if ( nTDr > 1 ) return 1;
			return 0;
		}

		#endregion

		private void AllocateTDr( 
			RushUnit ru, int projTDr, PlayerGameMetricsCollection pgms )
		{
			var approach = ru.DetermineApproach();
			tdrAllocationStrategies[ approach ].Allocate( ru, projTDr, pgms );
		}

		private void AllocateYDr( 
			RushUnit ru, int projYDr, PlayerGameMetricsCollection pgms )
		{
			var approach = ru.DetermineApproach();
			ydrAllocationStrategies[ approach ].Allocate( ru, projYDr, pgms );
		}


		#endregion Rushing

		private static void AddPlayerGameMetric( PlayerGameProjectionMessage input, string playerId, int projYDr, decimal projTDr )
		{
			var pgm = new PlayerGameMetrics
			{
				PlayerId = playerId,
				GameKey = input.Game.GameKey(),
				ProjYDr = projYDr,
				ProjTDr = projTDr
			};
#if DEBUG
			Utility.Announce( pgm.ToString() );
#endif
			if ( input.Game.PlayerGameMetrics == null ) input.Game.PlayerGameMetrics = new List<PlayerGameMetrics>();
			input.Game.PlayerGameMetrics.Add( pgm );
		}

		public static int AllowForInjuryRisk( NFLPlayer p, int proj )
		{
			if ( p == null )
				Utility.Announce( "AllowForInjuryRisk:Null Player" );
			else
			{
				Int32.TryParse( p.Injuries(), out int nInjury );
				if ( nInjury > 0 )
				{
					var injChance = ( ( nInjury * 10.0M ) / 100.0M );
					var effectiveness = 1 - injChance;
					proj = ( int ) ( proj * effectiveness );
				}
			}
			return proj;
		}
	}
}