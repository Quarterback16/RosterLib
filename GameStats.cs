using System;

namespace RosterLib
{
	/// <summary>
	///   Things that we count after a game has been played.
	/// </summary>
	public class GameStats : IComparable
	{
		private string homeTeam;
      private string awayTeam;
      private int week;

      //  2 dimensional array may be most efficient

      private const int K_AWAY = 0;
      private const int K_HOME = 1;

      private const int K_METRIC_YDP = 0;
      private const int K_METRIC_YDR = 1;
      private const int K_METRIC_SAK_ALLOWED = 2;
      private const int K_METRIC_SACKS = 3;
      //private const int K_METRIC_YDP_ALLOWED = 4;
      //private const int K_METRIC_YDR_ALLOWED = 5;
      private const int K_METRIC_TDp = 6;
      private const int K_METRIC_TDr = 7;
		private const int K_METRIC_FG  = 8;
		

      private decimal [,] metric; 

		public GameStats( string gameCode, string homeTeam, string awayTeam, int week )
		{
         //RosterGrid.WriteLog( string.Format( "GameStats Game {0}:{1} {2} @ {3}", week, gameCode, awayTeam, homeTeam ) );
         this.Code = gameCode;
         this.homeTeam = homeTeam;
         this.awayTeam = awayTeam;
         Week = week;

         metric = new decimal[ 2, 9 ]
         { 
                           { 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M },
                           { 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M, 0.0M }
         };
		}

      public int Week
      {
         get { return week; }
         set { week = value; }
      }

      public string HomeTeam
      {
         get { return homeTeam; }
         set { homeTeam = value; }
      }

      public string AwayTeam
      {
         get { return awayTeam; }
         set { awayTeam = value; }
      }

		public string Code { get; set; }

		public decimal HomeRushingYds
      {
         get { return metric[ K_HOME, K_METRIC_YDR ]; }
         set { metric[ K_HOME, K_METRIC_YDR ] = value; }
      }

      public decimal AwayRushingYds
      {
         get { return metric[ K_AWAY, K_METRIC_YDR ]; }
         set { metric[ K_AWAY, K_METRIC_YDR ] = value; }
      }

		public bool Played()
		{
			if (( HomePassingYds + AwayPassingYds ) == 0.0M )
				return false;
			else
				return true;
		}

      public decimal HomePassingYds
      {
         get { return metric[ K_HOME, K_METRIC_YDP ]; }
         set { metric[ K_HOME, K_METRIC_YDP ] = value; }
      }

      public decimal AwayPassingYds
      {
         get { return metric[ K_AWAY, K_METRIC_YDP ]; }
         set { metric[ K_AWAY, K_METRIC_YDP ] = value; }
      }
 
      public decimal HomeSacksAllowed
      {
         get { return metric[ K_HOME, K_METRIC_SAK_ALLOWED ]; }
         set { metric[ K_HOME, K_METRIC_SAK_ALLOWED ] = value; }
      }

      public decimal AwaySacksAllowed
      {
         get { return metric[ K_AWAY, K_METRIC_SAK_ALLOWED ]; }
         set { metric[ K_AWAY, K_METRIC_SAK_ALLOWED ] = value; }
      }

      public decimal HomeSacks
      {
         get { return metric[ K_HOME, K_METRIC_SACKS ]; }
         set { metric[ K_HOME, K_METRIC_SACKS ] = value; }
      }

      public decimal AwaySacks
      {
         get { return metric[ K_AWAY, K_METRIC_SACKS ]; }
         set { metric[ K_AWAY, K_METRIC_SACKS ] = value; }
      }

      public decimal HomeTDpasses
      {
         get { return metric[ K_HOME, K_METRIC_TDp ]; }
         set { metric[ K_HOME, K_METRIC_TDp ] = value; }
      }

      public decimal AwayTDpasses
      {
         get { return metric[ K_AWAY, K_METRIC_TDp ]; }
         set { metric[ K_AWAY, K_METRIC_TDp ] = value; }
      }

      public decimal HomeTDruns
      {
         get { return metric[ K_HOME, K_METRIC_TDr ]; }
         set { metric[ K_HOME, K_METRIC_TDr ] = value; }
      }

      public decimal AwayTDruns
      {
         get { return metric[ K_AWAY, K_METRIC_TDr ]; }
         set { metric[ K_AWAY, K_METRIC_TDr ] = value; }
      }

		public decimal HomeFGs
		{
			get { return metric[ K_HOME, K_METRIC_FG ]; }
			set { metric[ K_HOME, K_METRIC_FG ] = value; }
		}

		public decimal AwayFGs
		{
			get { return metric[ K_AWAY, K_METRIC_FG ]; }
			set { metric[ K_AWAY, K_METRIC_FG ] = value; }
		}

		public string Season
		{
			get { return Code.Substring( 0, 4 ); }
		}

      public bool isPlayoff()
      {
         return week > 17;
      }

		#region IComparable Members

		public int CompareTo(object obj)
		{
			// 1. Cast the obj into a GameStats
			GameStats gs = (GameStats) obj;
			// 2. Use the inherited behaviour of a string
			return( Code.CompareTo( gs.Code ));
		}

		#endregion        
   }



}
