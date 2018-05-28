using System;

namespace RosterLib
{
	/// <summary>
	/// Applys the GS4 scoring rules to a player.
	/// </summary>
	public class NFLUKScorer : IRatePlayers
	{
		public NFLUKScorer()
		{
			Name = "NflUk.com Scorer";
			Week = new NFLWeek( Utility.CurrentWeek(), Utility.CurrentSeason() );
		}

		#region IRatePlayers Members

		public bool ScoresOnly { get; set; }

		public Decimal RatePlayer(NFLPlayer plyr, NFLWeek weekIn, bool takeCache = true )
		{
			//  Rate players on their projected F points for the next start
			plyr.ProjectNextWeek();
			//  Kicker
			int fgPoints = plyr.ProjectedFg * 3;         
			//  QB
			decimal tdpPoints = plyr.ProjectedTDp * 3;
			//  RB
			decimal tdrPoints = plyr.ProjectedTDr * 6;
			int YDrPoints = plyr.ProjectedYDr/10;
			//  PR
			decimal tdcPoints = plyr.ProjectedTDc * 6;
			int YDcPoints = plyr.ProjectedYDc / 10;

			plyr.Points = YDcPoints + fgPoints + tdpPoints + tdrPoints + YDrPoints + tdcPoints;

			return plyr.Points;
		}

		public int RateTeam( NflTeam team )
		{
			//  Defensive team ratings
			team.ProjectNextWeek();
			//  Sacks are worth 3 points each
			int sackPoints = team.ProjectedSacks*3;
			//  Interceptions are worth 6 points each
			int intPoints = team.ProjectedSteals * 6;
			team.Points = sackPoints + intPoints;

			return team.Points;         
		}

		public string Name
		{
			get
			{
				// TODO:  Add GS4Scorer.Name getter implementation
				return Name;
			}
			set
			{
				// TODO:  Add GS4Scorer.Name setter implementation
			}
		}

		public XmlCache Master
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public NFLWeek Week { get; set; }
		public bool AnnounceIt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		#endregion
	}
}

