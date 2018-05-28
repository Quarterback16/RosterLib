using RosterLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	/// <summary>
	///   A rush unit can have an Aceback and an R2, or a committee of 2
	///   this adjusts the workload in the projections
	/// </summary>
	public class RushUnit : NflUnit
	{
		public List<NFLPlayer> Runners { get; set; }

		public NFLPlayer GoalLineBack { get; set; }
		public NFLPlayer ThirdDownBack { get; set; }
		public NFLPlayer AceBack { get; set; }
		public NFLPlayer R1 { get; set; }
		public NFLPlayer R2 { get; set; }

		public int nR1 { get; set; }
		public int nR2 { get; set; }
		public string Committee { get; set; }
		public List<NFLPlayer> Starters { get; set; }
		public string TeamCode { get; set; }

		public bool IsAceBack { get; set; }

		public ILoadRunners Loader { get; set; }

		#region Constructors

		public RushUnit(ILoadRunners loader = null)
		{
			if ( loader == null )
				Loader = new LoadRunners();
			else
				Loader = loader;

			Runners = new List<NFLPlayer>();
			Starters = new List<NFLPlayer>();
			nR2 = 0;
		}

		#endregion

		public void Add( NFLPlayer player )
		{
			if ( Runners.Contains( player ) )
				Announce( string.Format( "Duplicate player {0}", player.PlayerName ) );
			else
				Runners.Add( player );
		}

		public List<string> Load( string teamCode )
		{
			Reset();
			Announce( $"Loading Runners for {teamCode}" );
			TeamCode = teamCode;
			var runners = Loader.Load( teamCode );
			foreach ( var runner in runners )
				Add( runner );
			SetSpecialRoles();
			return DumpUnit();
		}

		protected void SetSpecialRoles()
		{
			SetThirdDownBack();
			SetGoalLineBack();
			SetAceBack();
			if ( AceBack != null )
				SetBackup();
		}

		private void SetBackup()
		{
			//  can only have one backup, any others need to be given roles of R for reserve
			foreach ( var p in Runners )
			{
				if ( p.IsBackup() && !p.IsFullback() )
				{
					R2 = p;
					nR2++;

					Announce( string.Format( "Setting Backup to {0}", p.PlayerName ) );
				}
			}
			Announce( string.Format( "{0} backups", nR2 ) );
		}

		private void SetGoalLineBack()
		{
			foreach ( var p in Runners )
			{
				if ( !p.IsShortYardageBack() ) continue;
				GoalLineBack = p;
				Announce( $"{p.PlayerName} is the Goalline back" );
				break;
			}
		}

		private void SetThirdDownBack()
		{
			foreach ( var p in Runners )
			{
				if ( !p.IsThirdDownBack() ) continue;
				ThirdDownBack = p;
				Announce( $"{p.PlayerName} is the 3D back" );
				break;
			}
		}

		private void SetAceBack()
		{
			var nStarters = 0;
			Committee = string.Empty;

			foreach ( var p in Runners )
			{
				Announce( $"Plyr {p.PlayerName} role:{p.PlayerRole} pos:{p.PlayerPos}" );

				if ( !p.IsStarter() || p.IsFullback() ) continue;

				nStarters++;
				nR1++;
				AceBack = p;
				if ( R1 == null )
					R1 = p;

				Committee += p.PlayerNameShort + " + ";
				Starters.Add( p );

				Announce( $"Setting Ace to {p.PlayerName}" );
			}

			Announce( string.Format( "{0} starters", nStarters ) );
			if ( nStarters == 1 )
			{
				IsAceBack = true;
				return;
			}
			AceBack = null;
			IsAceBack = false;
		}

		private void Reset()
		{
			Runners.Clear();
			GoalLineBack = null;
			AceBack = null;
			ThirdDownBack = null;

			R1 = null;
			R2 = null;

			nR1 = 0;
			nR2 = 0;
			Committee = string.Empty;
			Starters.Clear();
			IsAceBack = false;
		}

		public bool HasIntegrityError()
		{
			if ( ( AceBack == null ) && ( R1 == null ) && ( R2 == null ) )
				return true;

			if ( nR2 > 1 )   //  zero is okay (committee), only want 1 backup
			{
				var msg = $"  >>> {nR2} is Too many R2 for {TeamCode}";

				Announce( msg );

				AddError( msg );
				return true;
			}
			return false;
		}

		public List<string> DumpUnit()
		{
			var output = new List<string>();
			var unit = string.Empty;
			var starters = ( AceBack == null || string.IsNullOrEmpty( AceBack.ToString() ) ) ? Committee : AceBack.ToString();
			unit += DumpPlayer( "R1", starters, nR1 ) + Environment.NewLine;
			unit += DumpPlayer( "R2", R2 == null ? string.Empty : R2.PlayerNameShort, nR2 ) + Environment.NewLine;

			var ace = $"Ace back : {AceBack}";
			Announce( ace );
			unit += ace + Environment.NewLine;
			var r2 = string.Format( "R2       : {0}", R2 );
			Announce( r2 );
			unit += r2 + Environment.NewLine;
			var goalline = $"Goaline  : {GoalLineBack}";
			Announce( goalline );
			unit += goalline + Environment.NewLine;

			var thirdDown = $"3D:  : {ThirdDownBack}";
			Announce( thirdDown );
			unit += thirdDown + Environment.NewLine;

			foreach ( var runner in Runners )
			{
				var runr = string.Format( "{3,2} {0,-25} : {1} : {2}",
				   runner.PlayerName.PadRight( 25 ), runner.PlayerRole, runner.PlayerPos,
				   runner.JerseyNo );
				Announce( runr );
				unit += runr + Environment.NewLine;
			}
			output.Add( unit + Environment.NewLine );
			output.Add( $"Run approach is {DetermineApproach()}" + Environment.NewLine );
			return output;
		}

		private string DumpPlayer( string pos, string player, int count )
		{
			var plyrName = player ?? "none";
			var p = string.Format( "{2} ({1}): {0}", plyrName, count, pos );
			Announce( p );
			return p;
		}

		public List<string> LoadCarries( string season, string week )
		{
			var output = new List<string>();
			var totRushes = 0;
			var totTouches = 0;
			foreach ( var runner in Runners )
			{
				//  remove SH designation
				runner.PlayerPos = runner.PlayerPos.Replace( ",SH", "" );

				var carries = Utility.TflWs.PlayerStats(
				   Constants.K_STATCODE_RUSHING_CARRIES, season, week, runner.PlayerCode );

				runner.TotStats = new PlayerStats();
				if ( !int.TryParse( carries, out int rushes ) )
					rushes = 0;

				var receptions = Utility.TflWs.PlayerStats(
				   Constants.K_STATCODE_PASSES_CAUGHT, season, week, runner.PlayerCode );
				if ( !int.TryParse( receptions, out int catches ) )
					catches = 0;

				runner.TotStats.Rushes = rushes;
				runner.TotStats.Touches = rushes + catches;

				totRushes += rushes;
				totTouches += rushes + catches;
			}
			if ( totRushes > 0 ) //  not bye wk
			{
				var compareByCarries = new Comparison<NFLPlayer>( CompareTeamsByCarries );
				Runners.Sort( compareByCarries );
				//  look for SH back
				var sh = GetShortYardageBack( season, week, TeamCode );
				foreach ( var runner in Runners )
				{
					if ( runner.PlayerCode.Equals( sh ) )
						runner.PlayerPos += ",SH";
				}
				output = DumpUnitByTouches( totTouches );

				SetSpecialRoles();

				foreach ( var runner in Runners )
					Utility.TflWs.StorePlayerRoleAndPos( runner.PlayerRole, runner.PlayerPos, runner.PlayerCode );
			}
			else
				Announce( string.Format( "{0}:{1} is a bye week for {2}", season, week, TeamCode ) );
			return output;
		}

		public string GetShortYardageBack( string season, string week, string teamCode )
		{
			var sh = "???";
			var ds = Utility.TflWs.PenaltyScores( season, week, teamCode );
			if ( ds != null )
			{
				var dt = ds.Tables[ 0 ];
				foreach ( DataRow dr in dt.Rows )
				{
					sh = dr[ "PLAYERID1" ].ToString();
				}
			}
			return sh;
		}

		private static int CompareTeamsByCarries( NFLPlayer x, NFLPlayer y )
		{
			if ( x == null )
			{
				if ( y == null )
					return 0;
				return -1;
			}
			return y == null ? 1 : y.TotStats.Rushes.CompareTo( x.TotStats.Rushes );
		}

		public List<string> DumpUnitByCarries( int totRushes )
		{
			var output = new List<string>();
			foreach ( var runner in Runners )
			{
				var load = Utility.Percent( runner.TotStats.Rushes, totRushes );

				//  Returned fron the dead
				if ( load > 0 && ( runner.PlayerRole == Constants.K_ROLE_INJURED || runner.PlayerRole == Constants.K_ROLE_SUSPENDED ) )
					runner.PlayerRole = Constants.K_ROLE_DEEP_RESERVE;

				if ( runner.PlayerRole != Constants.K_ROLE_INJURED && runner.PlayerRole != Constants.K_ROLE_SUSPENDED )
				{
					runner.PlayerRole = Constants.K_ROLE_DEEP_RESERVE;
					if ( load > 0.0M )
						runner.PlayerRole = Constants.K_ROLE_RESERVE;
					if ( load > 10.0M )
						runner.PlayerRole = Constants.K_ROLE_BACKUP;
					if ( load > 30.0M )
						runner.PlayerRole = Constants.K_ROLE_STARTER;
				}

				var msg = string.Format( "{0,-25} : {1} : {2,3} : {3,5:##0.0}% : {4} : {5}",
				   runner.PlayerName.PadRight( 25 ), runner.PlayerRole, runner.TotStats.Rushes,
				   load, runner.PlayerRole, runner.PlayerPos
				   );
				Announce( msg );
				output.Add( msg );
			}
			return output;
		}

		public List<string> DumpUnitByTouches( int totTouches )
		{
			var output = new List<string>();
			foreach ( var runner in Runners )
			{
				var load = Utility.Percent( runner.TotStats.Touches, totTouches );

				//  Returned fron the dead
				if ( load > 0 &&
				   ( runner.PlayerRole == Constants.K_ROLE_INJURED || runner.PlayerRole == Constants.K_ROLE_SUSPENDED ) )
					runner.PlayerRole = Constants.K_ROLE_DEEP_RESERVE;

				if ( runner.PlayerRole != Constants.K_ROLE_INJURED
				   && runner.PlayerRole != Constants.K_ROLE_SUSPENDED )
				{
					runner.PlayerRole = Constants.K_ROLE_DEEP_RESERVE;
					if ( load > 0.0M )
						runner.PlayerRole = Constants.K_ROLE_RESERVE;
					if ( load > 10.0M )
						runner.PlayerRole = Constants.K_ROLE_BACKUP;
					if ( load > 30.0M )
						runner.PlayerRole = Constants.K_ROLE_STARTER;
				}

				var msg = string.Format( "{0,-25} : {6} : {1} : {2,3} : {3,5:##0.0}% : {4} : {5}",
				   runner.ProjectionLink( 25 ), runner.PlayerRole, runner.TotStats.Touches,
				   load, runner.PlayerRole, runner.PlayerPos, runner.Owner
				   );
				runner.TotStats.TouchLoad = load;
				Announce( msg );
				output.Add( msg );
			}
			return output;
		}

		public bool TandemBack( NFLPlayer p )
		{
			return Starters.Count == 2 && Starters.Contains( p );
		}

		public bool IsLoaded()
		{
			return Runners != null && Runners.Count > 0;
		}

		public RunApproach DetermineApproach()
		{
			if ( nR1 > 1 )
				return RunApproach.Committee;

			if ( nR1 == 1 && nR2 == 1 )
				return RunApproach.Standard;

			if ( nR1 == 1 && nR2 == 0 )
				return RunApproach.Ace;

			//if ( nR1 == 1 && nR2 > 1 )
			//	return RunApproach.RoleBased;

			Logger.Trace( $"Unknown approach for {TeamCode}" );
			return RunApproach.Standard;
		}
	}

	public enum RunApproach
	{
		Unknown,
		Ace,
		Standard,
		Committee
	}
}