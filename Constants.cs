using System;

namespace RosterLib
{
	public static class Constants
	{
		public const string K_WORK_MACHINE = "WDE308498";
		//public const string K_WORK_DRIVE = "E";  //deprecated - use PrimaryDrive()
		//public const string K_OUTPUT_DIRECTORY = K_WORK_DRIVE + "//Public//GridStat//";  // use OutputDirectory()

		#region Magic Numbers

		public const Int32 K_WEEKS_IN_A_SEASON = 21;
		public const Int32 K_WEEKS_IN_REGULAR_SEASON = 17;
		public const Int32 K_GAMES_IN_REGULAR_SEASON = 16;

		#endregion Magic Numbers

		#region Player Categories

		public const string K_QUARTERBACK_CAT = "1";
		public const string K_RUNNINGBACK_CAT = "2";
		public const string K_RECEIVER_CAT = "3";
		public const string K_KICKER_CAT = "4";
		public const string K_LINEMAN_CAT = "5";
		public const string K_DEFENSIVEBACK_CAT = "6";
		public const string K_OFFENSIVELINE_CAT = "7";
		public const string K_DEFENSIVETEAM_CAT = "8";

		public const string K_RUSHING_CATEGORIES = "123";
		public const string K_PASSING_CATEGORIES = "12";
		public const string K_RECEIVING_CATEGORIES = "123";
		public const string K_DEFENSIVE_CATEGORIES = "56";

		#endregion Player Categories

		#region Stat Codes

		public const string K_STATCODE_SACK = "Q";
		public const string K_STATCODE_RUSHING_YARDS = "Y";
		public const string K_STATCODE_RUSHING_CARRIES = "R";
		public const string K_STATCODE_RECEPTION_YARDS = "G";
		public const string K_STATCODE_PASSES_CAUGHT = "P";
		public const string K_STATCODE_PASSING_YARDS = "S";
		public const string K_STATCODE_FUMBLES_LOST = "F";
		public const string K_STATCODE_INTERCEPTIONS_MADE = "M";
		public const string K_STATCODE_INTERCEPTIONS_THROWN = "Z";

		#endregion Stat Codes

		#region Score types

		public const string K_SCORE_FUMBLE_RETURN = "F";
		public const string K_SCORE_INTERCEPT_RETURN = "I";
		public const string K_SCORE_KICK_RETURN = "K";
		public const string K_SCORE_PUNT_RETURN = "T";
		public const string K_SCORE_SAFETY = "S";

		public const string K_SCORE_TD_PASS = "P";
		public const string K_SCORE_TD_RUN = "R";
		public const string K_SCORE_TD_CATCH = "C";

		public const string K_SCORE_FIELD_GOAL = "3";
		public const string K_SCORE_PAT = "1";
		public const string K_SCORE_PAT_PASS = "2";
		public const string K_SCORE_PAT_RUN = "N";

		#endregion Score types

		#region Roles

		public const string K_ROLE_STARTER = "S";
		public const string K_ROLE_BACKUP = "B";
		public const string K_ROLE_RESERVE = "R";
		public const string K_ROLE_DEEP_RESERVE = "D";
		public const string K_ROLE_INJURED = "I";
		public const string K_ROLE_SUSPENDED = "X";
		public const string K_ROLE_HOLDOUT = "H";

		#endregion Roles

		#region Stat types

		public const string K_RUSHING_STATS = "RY";
		public const string K_RUSHING_CARRIES = "R";
		public const string K_RUSHING_YARDS = "Y";
		public const string K_PASSING_STATS = "CSAZ";
		public const string K_PASS_COMPLETIONS = "C";
		public const string K_PASSING_ATTEMPTS = "A";
		public const string K_PASSING_YARDAGE = "S";
		public const string K_PASSES_INTERCEPTED = "Z";
		public const string K_RECEPTION_STATS = "PG";
		public const string K_PASSES_CAUGHT = "P";
		public const string K_RECEPTION_YARDAGE = "G";
		public const string K_DEFENSIVE_STATS = "MQ";
		public const string K_QUARTERBACK_SACKS = "Q";
		public const string K_INTERCEPTIONS_MADE = "M";
		public const string K_OFFENSIVE_PLAYS = "RA";

		#endregion Stat types

		#region League Codes

		public const string K_LEAGUE_Gridstats_NFL1 = "G1";
		public const string K_LEAGUE_Yahoo = "YH";
		public const string K_LEAGUE_PerfectChallenge = "PC";
		public const string K_LEAGUE_50_Dollar_Challenge = "TN";
		public const string K_LEAGUE_Rants_n_Raves = "RR";

		#endregion League Codes

		#region Owner Codes

		public const string KOwnerSteveColonna = "O001";

		#endregion Owner Codes

		#region Reasonablness Check Rules

		public const string K_CHECK_TOTAL_SACKS = "Season-Sacks-Allowed";

		#endregion Reasonablness Check Rules

		public static class DefaultFileName
		{
			public static readonly string YahooXml = "YahooOutput.xml";
		}

		public static class Colour
		{
			public static readonly string Default = "MAGENTA";

			public static readonly string Bad = "TOMATO";
			public static readonly string Average = "MEDIUMSPRINGGREEN";
			public static readonly string Good = "SKYBLUE";
			public static readonly string Excellent = "YELLOW";
		}
	}
}