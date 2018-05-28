using NLog;
using RosterLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class PassUnit : NflUnit
	{
		public string TeamCode { get; set; }

		public List<NFLPlayer> Quarterbacks { get; set; }
		public List<NFLPlayer> Receivers { get; set; }

		public NFLPlayer Q1 { get; set; }
		public NFLPlayer Q2 { get; set; }

		public int nQ1 { get; set; }
		public int nQ2 { get; set; }

		public NFLPlayer W1 { get; set; }
		public NFLPlayer W2 { get; set; }
		public NFLPlayer W3 { get; set; }

		public int nW1 { get; set; }
		public int nW2 { get; set; }
		public int nW3 { get; set; }

		public NFLPlayer TE { get; set; }
		public int nTE { get; set; }

		public bool IsAceReceiver { get; set; }
		public NFLPlayer AceReceiver { get; set; }

		public bool IsAceTightEnd { get; set; }
		public NFLPlayer AceTightEnd { get; set; }

		public ILoadPassUnit Loader { get; set; }

		#region Constructors

		public PassUnit( ILoadPassUnit loader = null )
		{
			if ( loader == null )
				Loader = new LoadPassUnit();
			else
				Loader = loader;

			Quarterbacks = new List<NFLPlayer>();
			Receivers = new List<NFLPlayer>();
			nQ1 = 0;
			nQ2 = 0;
			nW1 = 0;
			nW2 = 0;
			nW3 = 0;
			nTE = 0;
			if ( Logger == null ) Logger = LogManager.GetCurrentClassLogger();
		}

		#endregion

		public List<string> DumpUnit()
		{
			var output = new List<string>();
			var unit = string.Empty;
			unit += DumpPlayer( "Q1", Q1, nQ1 ) + Environment.NewLine;
			unit += DumpPlayer( "Q2", Q2, nQ2 ) + Environment.NewLine;
			unit += DumpPlayer( "W1", W1, nW1 ) + Environment.NewLine;
			unit += DumpPlayer( "W2", W2, nW2 ) + Environment.NewLine;
			unit += DumpPlayer( "W3", W3, nW3 ) + Environment.NewLine;
			unit += DumpPlayer( "TE", TE, nTE ) + Environment.NewLine;
			foreach ( var receiver in Receivers )
			{
				var rec = string.Format( "{3,2} {0,-25} : {1} : {2}",
				   receiver.ProjectionLink( 25 ), receiver.PlayerRole, receiver.PlayerPos, receiver.JerseyNo );
				unit += rec + Environment.NewLine;
				Utility.Announce( rec );
			}
			output.Add( unit + Environment.NewLine );
			return output;
		}

		private string DumpPlayer( string pos, NFLPlayer player, int count )
		{
			var plyrName = ( player == null ) ? "none" : player.PlayerName;
			var p = string.Format( "{2} ({1}): {0}", plyrName, count, pos );
			Announce( p );
			return p;
		}

		public List<string> Load( string teamCode )
		{
			TeamCode = teamCode;
			Quarterbacks = Loader.Load( teamCode, Constants.K_QUARTERBACK_CAT );
			Receivers = Loader.Load( teamCode, Constants.K_RECEIVER_CAT );
			SetQbRoles();
			SetReceiverRoles();
			if ( Q1 == null )
				Announce( $"   >>> Warning no Q1 for {TeamCode}" );
			return DumpUnit();
		}

		private void SetQbRoles()
		{
			//  can only have one backup, any others need to be given roles of R for reserve
			foreach ( var p in Quarterbacks )
			{
#if DEBUG
				Announce( $"   Examining QB {p.PlayerName}" );
#endif
				if ( p.IsQuarterback() )
				{
					if ( p.IsStarter() )
					{
						Q1 = p;
						nQ1++;
#if DEBUG
						Announce( $"      Setting Starter to {p.PlayerName}" );
#endif
					}
					if ( p.IsBackup() )
					{
						Q2 = p;
						nQ2++;
#if DEBUG
						Announce( $"       Setting Backup to {p.PlayerName}" );
#endif
					}
				}
			}
		}

		public void SetReceiverRoles()
		{
			//  can only have one backup, any others need to be given roles of R for reserve
			foreach ( var p in Receivers )
			{
#if DEBUG
				Announce( string.Format( "   Examining Rec {0}", p.PlayerName ) );
#endif
				if ( p.HasPos( "W1" ) )
				{
					if ( p.IsStarter() )
					{
						W1 = p;
						nW1++;
#if DEBUG
						Announce( string.Format( "      Setting W1 to {0}", p.PlayerName ) );
#endif
					}
				}

				if ( p.HasPos( "W2" ) )
				{
					if ( p.IsStarter() )
					{
						W2 = p;
						nW2++;
#if DEBUG
						Announce( string.Format( "      Setting W2 to {0}", p.PlayerName ) );
#endif
					}
				}

				if ( p.HasPos( "W3" ) )
				{
					if ( p.IsStarter() )
					{
						W3 = p;
						nW3++;
#if DEBUG
						Announce( string.Format( "      Setting W3 to {0}", p.PlayerName ) );
#endif
					}
				}

				if ( p.HasPos( "TE" ) )
				{
					if ( p.IsStarter() )
					{
						TE = p;
						nTE++;
#if DEBUG
						Announce( string.Format( "      Setting TE to {0}", p.PlayerName ) );
#endif
					}
				}
			}
		}

		public bool IsLoaded()
		{
			return Quarterbacks != null && Quarterbacks.Count > 0;
		}

		public bool HasIntegrityError()
		{
			var errors = 0;
			if ( Q1 == null )
			{
				errors = AddError( errors, "Q1" );
			}
			else if ( W1 == null )
			{
				errors = AddError( errors, "W1" );
			}
			else if ( W2 == null )
			{
				errors = AddError( errors, "W2" );
			}
			else if ( TE == null )
			{
				errors = AddError( errors, "TE" );
			}

			errors += CheckCount( nQ1, "Q1", 1 );  // must v a QB
			errors += CheckCount( nW1, "W1", 1 );  // must v a W1
			errors += CheckCount( nW2, "W2", 1 );  //  must v a W2
			errors += CheckCount( nTE, "TE", 2 );  //  some teams start 2 TEs

#if DEBUG
			Announce( string.Format( "{0} errors", errors ) );
#endif
			return ( errors > 0 );
		}

		private int AddError( int errors, string pos )
		{
			var msg = string.Format( "No {1} for {0}", TeamCode, pos );
			ErrorMessages.Add( msg );
			Utility.Announce( msg );
			errors++;
			return errors;
		}

		private int CheckCount( int theCount, string thePos, int max )
		{
			if ( theCount <= max ) return 0;
			Announce( string.Format( "{1} is Too many {2} for {0}", TeamCode, theCount, thePos ) );
			return 1;
		}

		public List<string> AnalyseWideouts( string season, string week )
		{
			var output = new List<string>();

			var totTouches = 0;
			foreach ( var p in Receivers )
			{
				if ( !p.IsWideout() ) continue;

				//  remove designation2
				p.PlayerPos = p.PlayerPos.Replace( ",W1", "" );
				p.PlayerPos = p.PlayerPos.Replace( ",W2", "" );
				p.PlayerPos = p.PlayerPos.Replace( ",W3", "" );

				//  how many touches
				var receptions = Utility.TflWs.PlayerStats(
				   Constants.K_STATCODE_PASSES_CAUGHT, season, week, p.PlayerCode );
				var carries = Utility.TflWs.PlayerStats(
				   Constants.K_STATCODE_RUSHING_CARRIES, season, week, p.PlayerCode );

				p.TotStats = new PlayerStats();
				int catches;
				if ( !int.TryParse( receptions, out catches ) )
					catches = 0;
				int runs;
				if ( !int.TryParse( carries, out runs ) )
					runs = 0;
				p.TotStats.Touches = catches + runs;
				totTouches += catches + runs;
			}
			if ( totTouches > 0 ) //  not bye wk
			{
				var compareByTouches = new Comparison<NFLPlayer>( ComparePlayersByTouches );

				Receivers.Sort( compareByTouches );
				return DumpUnitByTouches( totTouches );
			}
			Announce( string.Format( "{0}:{1} is a bye week for {2}", season, week, TeamCode ) );

			return output;
		}

		public List<string> AnalyseQuarterbacks( string season, string week )
		{
			var output = new List<string>();
			var totYdp = 0;
			foreach ( var p in Quarterbacks )
			{
				if ( !p.IsQuarterback() ) continue;
				//  how many yards
				var passYardage = Utility.TflWs.PlayerStats(
				   Constants.K_STATCODE_PASSING_YARDS, season, week, p.PlayerCode );
				p.TotStats = new PlayerStats();
				int passYds;
				if ( !int.TryParse( passYardage, out passYds ) )
					passYds = 0;

				p.TotStats.YDp = passYds;
				totYdp += passYds;
			}
			if ( totYdp > 0 ) //  not bye wk
			{
				var compareByYdp = new Comparison<NFLPlayer>( ComparePlayersByYdp );

				Quarterbacks.Sort( compareByYdp );
				return DumpUnitByPassingYardage( totYdp );
			}
			Announce( string.Format( "{0}:{1} is a bye week for {2}", season, week, TeamCode ) );
			return output;
		}

		public List<string> AnalyseTightends( string season, string week )
		{
			var output = new List<string>();
			var totTouches = 0;
			foreach ( var p in Receivers )
			{
				if ( !p.IsTightEnd() ) continue;
				//  how many touches
				var receptions = Utility.TflWs.PlayerStats(
				   Constants.K_STATCODE_PASSES_CAUGHT, season, week, p.PlayerCode );
				var carries = Utility.TflWs.PlayerStats(
				   Constants.K_STATCODE_RUSHING_CARRIES, season, week, p.PlayerCode );

				p.TotStats = new PlayerStats();
				int catches;
				if ( !int.TryParse( receptions, out catches ) )
					catches = 0;
				int runs;
				if ( !int.TryParse( carries, out runs ) )
					runs = 0;
				p.TotStats.Touches = catches + runs;
				totTouches += catches + runs;
			}
			if ( totTouches > 0 ) //  not bye wk
			{
				var compareByYdc = new Comparison<NFLPlayer>( ComparePlayersByTouches );

				Receivers.Sort( compareByYdc );
				return DumpTightendsByTouches( totTouches );
			}
			var msg = string.Format( "TE:{0}:{1} is a bye week for {2}", season, week, TeamCode );
			Announce( msg );
			return output;
		}

		public List<string> DumpTightendsByTouches( int tot )
		{
			var output = new List<string>();

			foreach ( var p in Receivers )
			{
				if ( !p.IsTightEnd() ) continue;
				var load = Utility.Percent( p.TotStats.Touches, tot );
				p.TotStats.TouchLoad = load;

				//  Returned fron the dead
				if ( load > 0 && ( p.PlayerRole == Constants.K_ROLE_INJURED || p.PlayerRole == Constants.K_ROLE_SUSPENDED ) )
					p.PlayerRole = Constants.K_ROLE_DEEP_RESERVE;

				if ( p.PlayerRole != Constants.K_ROLE_INJURED && p.PlayerRole != Constants.K_ROLE_SUSPENDED )
				{
					p.PlayerRole = Constants.K_ROLE_RESERVE;
					if ( load > 0 )
						p.PlayerRole = Constants.K_ROLE_BACKUP;

					if ( load > 40 )
					{
						p.PlayerRole = Constants.K_ROLE_STARTER;
						if ( load > 75 )
						{
							IsAceTightEnd = true;
							AceTightEnd = p;
						}
					}
				}

				var msg = string.Format( "{0,-25} ({6}) : {7} : {1} : {2,3} : {3,5:##0.0}% : {4} : {5}",
										 p.ProjectionLink( 25 ), p.PlayerRole, p.TotStats.Touches,
										 load, p.PlayerRole, p.PlayerPos, p.PlayerAge(), p.Owner
				   );
				Announce( msg );
				output.Add( msg );
				Utility.TflWs.StorePlayerRoleAndPos( p.PlayerRole, p.PlayerPos, p.PlayerCode );
			}
			return output;
		}

		public List<string> DumpTightendsByYardage( int tot )
		{
			var output = new List<string>();

			foreach ( var p in Receivers )
			{
				if ( !p.IsTightEnd() ) continue;
				var load = Utility.Percent( p.TotStats.YDc, tot );

				//  Returned fron the dead
				if ( load > 0 && ( p.PlayerRole == Constants.K_ROLE_INJURED || p.PlayerRole == Constants.K_ROLE_SUSPENDED ) )
					p.PlayerRole = Constants.K_ROLE_DEEP_RESERVE;

				if ( p.PlayerRole != Constants.K_ROLE_INJURED && p.PlayerRole != Constants.K_ROLE_SUSPENDED )
				{
					p.PlayerRole = Constants.K_ROLE_RESERVE;
					if ( load > 0 )
						p.PlayerRole = Constants.K_ROLE_BACKUP;

					if ( load > 40 )
						p.PlayerRole = Constants.K_ROLE_STARTER;
				}

				var msg = string.Format( "{0,-25} ({6}) : {1} : {2,3} : {3,5:##0.0}% : {4} : {5}",
										 p.ProjectionLink( 25 ), p.PlayerRole, p.TotStats.YDc,
										 load, p.PlayerRole, p.PlayerPos, p.PlayerAge()
				   );
				Announce( msg );
				output.Add( msg );
				Logger.Info( "Storing role and pos " + msg );
				Utility.TflWs.StorePlayerRoleAndPos( p.PlayerRole, p.PlayerPos, p.PlayerCode );
			}
			return output;
		}

		public List<string> DumpUnitByYardage( int tot )
		{
			var output = new List<string>();

			var wideCnt = 0;
			foreach ( var p in Receivers )
			{
				if ( !p.IsWideout() ) continue;

				var load = Utility.Percent( p.TotStats.YDc, tot );

				//  Returned fron the dead
				if ( load > 0 && ( p.PlayerRole == Constants.K_ROLE_INJURED || p.PlayerRole == Constants.K_ROLE_SUSPENDED ) )
					p.PlayerRole = Constants.K_ROLE_DEEP_RESERVE;

				if ( p.PlayerRole != Constants.K_ROLE_INJURED && p.PlayerRole != Constants.K_ROLE_SUSPENDED )
				{
					p.PlayerRole = Constants.K_ROLE_RESERVE;
					if ( load > 0 )
					{
						p.PlayerRole = Constants.K_ROLE_BACKUP;
						if ( load > 10 )
						{
							wideCnt++;
							if ( wideCnt < 4 )
							{
								p.PlayerPos += string.Format( ",W{0}", wideCnt );
								p.PlayerRole = Constants.K_ROLE_STARTER;
							}
						}
					}
				}

				var msg = string.Format( "{0,-25} ({6}) : {1} : {2,3} : {3,5:##0.0}% : {4} : {5}",
										 p.ProjectionLink( 25 ), p.PlayerRole, p.TotStats.YDc,
										 load, p.PlayerRole, p.PlayerPos, p.PlayerAge()
				   );
				Announce( msg );
				output.Add( msg );

				Utility.TflWs.StorePlayerRoleAndPos( p.PlayerRole, p.PlayerPos, p.PlayerCode );
			}
			return output;
		}

		public List<string> DumpUnitByTouches( int tot )
		{
			var output = new List<string>();

			var wideCnt = 0;
			foreach ( var p in Receivers )
			{
				if ( !p.IsWideout() ) continue;

				var load = Utility.Percent( p.TotStats.Touches, tot );
				p.TotStats.TouchLoad = load;

				//  Returned fron the dead
				if ( load > 0 && ( p.PlayerRole == Constants.K_ROLE_INJURED || p.PlayerRole == Constants.K_ROLE_SUSPENDED ) )
					p.PlayerRole = Constants.K_ROLE_DEEP_RESERVE;

				if ( p.PlayerRole != Constants.K_ROLE_INJURED && p.PlayerRole != Constants.K_ROLE_SUSPENDED )
				{
					p.PlayerRole = Constants.K_ROLE_RESERVE;
					if ( load > 0 )
					{
						p.PlayerRole = Constants.K_ROLE_BACKUP;
						if ( load > 10 )
						{
							wideCnt++;
							if ( wideCnt < 4 )
							{
								p.PlayerPos += string.Format( ",W{0}", wideCnt );
								p.PlayerRole = Constants.K_ROLE_STARTER;
							}
							if ( load > 45 )
							{
								IsAceReceiver = true;
								AceReceiver = p;
							}
						}
					}
				}

				var msg = string.Format( "{0,-25} ({6}) : {7} : {1} : {2,3} : {3,5:##0.0}% : {4} : {5}",
										 p.ProjectionLink( 25 ), p.PlayerRole, p.TotStats.Touches,
										 load, p.PlayerRole, p.PlayerPos, p.PlayerAge(), p.Owner
				   );
				Announce( msg );
				output.Add( msg );

				Utility.TflWs.StorePlayerRoleAndPos( p.PlayerRole, p.PlayerPos, p.PlayerCode );
			}
			return output;
		}

		private static int ComparePlayersByTouches( NFLPlayer x, NFLPlayer y )
		{
			if ( x == null )
			{
				if ( y == null )
					return 0;
				return -1;
			}
			return y == null ? 1 : y.TotStats.Touches.CompareTo( x.TotStats.Touches );
		}

		private static int ComparePlayersByYdp( NFLPlayer x, NFLPlayer y )
		{
			if ( x == null )
			{
				if ( y == null )
					return 0;
				return -1;
			}
			return y == null ? 1 : y.TotStats.YDp.CompareTo( x.TotStats.YDp );
		}

		public List<string> DumpUnitByPassingYardage( int tot )
		{
			var output = new List<string>();

			foreach ( var p in Quarterbacks )
			{
				if ( !p.IsQuarterback() ) continue;
				var load = Utility.Percent( p.TotStats.YDp, tot );

				//  Returned fron the dead
				if ( load > 0 && ( p.PlayerRole == Constants.K_ROLE_INJURED || p.PlayerRole == Constants.K_ROLE_SUSPENDED ) )
					p.PlayerRole = Constants.K_ROLE_RESERVE;

				if ( p.PlayerRole != Constants.K_ROLE_INJURED && p.PlayerRole != Constants.K_ROLE_SUSPENDED )
				{
					p.PlayerRole = Constants.K_ROLE_BACKUP;
					if ( load > 0 )
					{
						p.PlayerRole = Constants.K_ROLE_BACKUP;
						if ( load > 20 )
							p.PlayerRole = Constants.K_ROLE_STARTER;
					}
				}

				var msg = AnnouncePlayer( p, load );
				output.Add( msg );

				Utility.TflWs.StorePlayerRoleAndPos( p.PlayerRole, p.PlayerPos, p.PlayerCode );
			}
			return output;
		}

		private string AnnouncePlayer( NFLPlayer p, decimal load )
		{
			var msg = string.Format( "{0,-25} ({6}) : {7} : {1} : {2,3} : {3,5:##0.0}% : {4} : {5}",
				   p.ProjectionLink( 25 ), p.PlayerRole, p.TotStats.YDc,
			   load, p.PlayerRole, p.PlayerPos, p.PlayerAge(), p.Owner
			   );
			Announce( msg );
			return msg;
		}
	}
}