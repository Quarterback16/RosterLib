using System;
using System.Data;

namespace RosterLib
{
	/// <summary>
	/// Applys the ESPN scoring rules to a player.
	/// </summary>
	public class StatProjector : IRatePlayers
	{
		public StatProjector(NFLWeek week)
		{
			Name = "StatProjector";
			Week = week;
		}

		#region IRatePlayers Members

		public bool ScoresOnly { get; set; }

		public Decimal RatePlayer(NFLPlayer plyr, NFLWeek week, bool takeCache = true )
		{
			// Points for Scores and points for stats
			if (week.WeekNo.Equals(0)) return 0;
			// Only starter or backup will score
			if ( ! ( plyr.PlayerRole.Equals( Constants.K_ROLE_STARTER ) || plyr.PlayerRole.Equals( Constants.K_ROLE_STARTER ) ) )
				return 0;

			Week = week; //  set the global week, other wise u will get the same week all the time
			plyr.Points = 0; //  start from scratch
			var currentWeek = Int32.Parse(Utility.CurrentWeek());

			#region  Passing

			//  determine receiving stats
			decimal YD1p = 0;
			var nGames = 0;
			//  yards receiving will be the average of the last x games

			//  Last week
			var lastWeek = week.PreviousWeek(week, false, true);
			var ds = plyr.LastStats(
				Constants.K_STATCODE_PASSING_YARDS, lastWeek.WeekNo, lastWeek.WeekNo, lastWeek.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD1p += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards receiving in week {2}",
			                                         plyr.PlayerName, YD1p, lastWeek.WeekKey() ) );

			//  two weeks ago
			var twoWeeksAgo = lastWeek.PreviousWeek(lastWeek, false, true);
			decimal YD2p = 0;
			ds = plyr.LastStats( Constants.K_STATCODE_PASSING_YARDS, 
				twoWeeksAgo.WeekNo, twoWeeksAgo.WeekNo, twoWeeksAgo.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD2p += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards passing in week {2}",
																  plyr.PlayerName, YD2p, twoWeeksAgo.WeekKey() ) );

			// Three weeks ago
			var threeWeeksAgo = lastWeek.PreviousWeek( twoWeeksAgo, false, true );
			decimal YD3p = 0;
			ds = plyr.LastStats( Constants.K_STATCODE_PASSING_YARDS,
				threeWeeksAgo.WeekNo, threeWeeksAgo.WeekNo, threeWeeksAgo.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD3p += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards passing in week {2}",
																  plyr.PlayerName, YD3p, threeWeeksAgo.WeekKey() ) );

			if (nGames > 0)
				plyr.TotStats.YDp = (int) (YD1p + YD2p + YD3p)/nGames;

			Utility.Announce(string.Format("  {0,-15} has averaged {1,4:##0} Yards passing in last 3 weeks",
			                                         plyr.PlayerName, plyr.TotStats.YDp));

			if (plyr.TotStats.YDp > 0)
			{
				// adjust for role
				if (plyr.PlayerRole.Equals( Constants.K_ROLE_BACKUP))
				{
					plyr.TotStats.YDp = (plyr.TotStats.YDp/2);
					Utility.Announce(string.Format("  {0,-15} average halved to {1,4:##0} due to being a backup",
					                                         plyr.PlayerName, plyr.TotStats.YDp));
				}
			}

			/////////////////////////
			//  home bonus
			if (plyr.TotStats.YDp != 0)
			{
				if (plyr.IsAtHome(Week))
					plyr.TotStats.YDp = (int) (plyr.TotStats.YDp*1.1);
				else
					plyr.TotStats.YDp = (int) (plyr.TotStats.YDp*.9);
				Utility.Announce(string.Format("  {0,-15} average adjusted to {1,4:##0} home v away",
				                                         plyr.PlayerName, plyr.TotStats.YDp));
			}

			///////////////////
			//  opponent factor
			if (plyr.TotStats.YDp != 0)
			{
				var opponent = plyr.CurrTeam.OpponentFor(Week.Season, Week.WeekNo);
				if (opponent != null)
				{
					var oppRating = plyr.OpponentRating(opponent.Ratings);
					var ratingModifier = QbRatingModifier(plyr.OpponentRating(opponent.Ratings));
					plyr.TotStats.YDp = (int) (plyr.TotStats.YDp*ratingModifier);
					Utility.Announce(
						string.Format("  {0} output modified by {1} to {4} due to opponent ({3}) rating modifier of {2}",
						              plyr.PlayerName, ratingModifier, oppRating, opponent.TeamCode, plyr.TotStats.YDp));
					plyr.OppRate = oppRating;
				}
			}

			///////////////
			//  line factor
			if (plyr.TotStats.YDp != 0)
			{
				var game = plyr.CurrTeam.GameFor(Week.Season, Week.WeekNo);
				if (game != null)
				{
					if (game.SpreadFavourite().Equals(plyr.TeamCode))
					{
						if (Math.Abs(game.Spread) > 6.5M)
							plyr.TotStats.YDp = (int) (plyr.TotStats.YDp*1.1);
						else
							plyr.TotStats.YDp = (int) (plyr.TotStats.YDp*1.05);
						plyr.PlayerSpread = string.Format("+{0:#.#}", Math.Abs(game.Spread));
					}
					else
					{
						if (Math.Abs(game.Spread) > 6.5M)
							plyr.TotStats.YDp = (int) (plyr.TotStats.YDp*0.95);
						else
							plyr.TotStats.YDp = (int) (plyr.TotStats.YDp*0.9);
						plyr.PlayerSpread = string.Format("-{0:#.#}", Math.Abs(game.Spread));
					}
					Utility.Announce(string.Format("  {0} output modified to {1} due to the spread ({2})",
					                                         plyr.PlayerName, plyr.TotStats.YDp, plyr.PlayerSpread));

					plyr.Opponent = game.OpponentOut(plyr.CurrTeam.TeamCode);
				}
			}

			Utility.Announce(string.Format("    {0} is projected for {1} Yards receiving in week {2}",
			                                         plyr.PlayerName, plyr.TotStats.YDp, Week.WeekNo));

			//  1 pt / 25 yds
			var ptsForYDs = (int) (plyr.TotStats.YDp/25.0M);
			plyr.Points += ptsForYDs;

			if (plyr.TotStats.YDp > 0)
			{
				//  one TD for each 100 yds passing
				plyr.TotStats.Tdp = (plyr.TotStats.YDp/100);
				plyr.Points += plyr.TotStats.Tdp*4;
			}

			Utility.Announce( string.Format( "      {0} has {1} points for YDc", plyr.PlayerName, ptsForYDs ) );

			#endregion

			#region  Catching

			//  determine receiving stats
			decimal YD1c = 0;
			nGames = 0;
			//  yards receiving will be the average of the last x games
			ds = plyr.LastStats( Constants.K_STATCODE_RECEPTION_YARDS, 
				lastWeek.WeekNo, lastWeek.WeekNo, lastWeek.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD1c += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards receiving in week {2}",
			                                         plyr.PlayerName, YD1c, lastWeek.WeekKey()));

			decimal YD2c = 0;
			ds = plyr.LastStats( Constants.K_STATCODE_RECEPTION_YARDS,
				twoWeeksAgo.WeekNo, twoWeeksAgo.WeekNo, twoWeeksAgo.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD2c += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards receiving in week {2}",
																  plyr.PlayerName, YD2c, twoWeeksAgo.WeekKey() ) );

			decimal YD3c = 0;
			ds = plyr.LastStats( Constants.K_STATCODE_RECEPTION_YARDS,
				threeWeeksAgo.WeekNo, threeWeeksAgo.WeekNo, threeWeeksAgo.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD3c += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards receiving in week {2}",
																  plyr.PlayerName, YD3c, threeWeeksAgo.WeekKey() ) );

			if (nGames > 0)
				plyr.TotStats.YDc = (int) (YD1c + YD2c + YD3c)/nGames;

			Utility.Announce(string.Format("  {0,-15} has averaged {1,4:##0} Yards receiving in last 3 weeks",
			                                         plyr.PlayerName, plyr.TotStats.YDc ) );

			if (plyr.TotStats.YDc > 0)
			{
				// adjust for role
				if (plyr.PlayerRole.Equals( Constants.K_ROLE_BACKUP ) )
				{
					plyr.TotStats.YDc = (plyr.TotStats.YDc/2);
					Utility.Announce(string.Format("  {0,-15} average halved to {1,4:##0} due to being a backup",
					                                         plyr.PlayerName, plyr.TotStats.YDc));
				}
			}

			/////////////////////////
			//  returner bonus 25 yds
			if (plyr.IsReturner())
			{
				plyr.TotStats.YDc += 25;
				Utility.Announce(string.Format("  {0,-15} returner bonus to {1,4:##0}",
				                                         plyr.PlayerName, plyr.TotStats.YDc));
			}

			/////////////////////////
			//  home bonus
			if (plyr.TotStats.YDc != 0)
			{
				if (plyr.IsAtHome(Week))
					plyr.TotStats.YDc = (int) (plyr.TotStats.YDc*1.2);
				else
					plyr.TotStats.YDc = (int) (plyr.TotStats.YDc*.8);
				Utility.Announce(string.Format("  {0,-15} average adjusted to {1,4:##0} home v away",
				                                         plyr.PlayerName, plyr.TotStats.YDc));
			}

			///////////////////
			//  opponent factor
			if (plyr.TotStats.YDc != 0)
			{
				var opponent = plyr.CurrTeam.OpponentFor(Week.Season, Week.WeekNo);
				if (opponent != null)
				{
					var oppRating = plyr.OpponentRating(opponent.Ratings);
					var ratingModifier = RatingModifier(plyr.OpponentRating(opponent.Ratings));
					plyr.TotStats.YDc = (int) (plyr.TotStats.YDc*ratingModifier);
					Utility.Announce(string.Format("  {0} output modified by {1} due to opponent ({3}) rating of {2}",
					                                         plyr.PlayerName, ratingModifier, oppRating, opponent.TeamCode));
					plyr.OppRate = oppRating;
				}
			}

			///////////////
			//  line factor
			if (plyr.TotStats.YDc != 0)
			{
				var game = plyr.CurrTeam.GameFor(Week.Season, Week.WeekNo);
				if (game != null)
				{
					if (game.SpreadFavourite().Equals(plyr.TeamCode))
					{
						if (Math.Abs(game.Spread) > 6.5M)
							plyr.TotStats.YDc = (int) (plyr.TotStats.YDc*1.2);
						else
							plyr.TotStats.YDc = (int) (plyr.TotStats.YDc*1.1);
						plyr.PlayerSpread = string.Format("+{0:#.#}", Math.Abs(game.Spread));
					}
					else
					{
						if (Math.Abs(game.Spread) > 6.5M)
							plyr.TotStats.YDc = (int) (plyr.TotStats.YDc*0.8);
						else
							plyr.TotStats.YDc = (int) (plyr.TotStats.YDc*0.9);
						plyr.PlayerSpread = string.Format("-{0:#.#}", Math.Abs(game.Spread));
					}
					Utility.Announce(string.Format("  {0} output modified to {1} due to the spread ({2})",
					                                         plyr.PlayerName, plyr.TotStats.YDc, plyr.PlayerSpread));

					plyr.Opponent = game.OpponentOut(plyr.CurrTeam.TeamCode);
				}
			}

			Utility.Announce(string.Format("    {0} is projected for {1} Yards receiving in week {2}",
			                                         plyr.PlayerName, plyr.TotStats.YDc, Week.WeekNo));

			//  1 pt / 10 yds
			ptsForYDs = (int) (plyr.TotStats.YDc/10.0M);
			plyr.Points += ptsForYDs;

			Utility.Announce( string.Format( "      {0} has {1} points for YDc", plyr.PlayerName, ptsForYDs ) );

			if (plyr.TotStats.YDc > 0)
			{
				//  one TD for each 75 yds caught
				plyr.TotStats.Tdc = (plyr.TotStats.YDc/75);
				plyr.Points += plyr.TotStats.Tdc*6;
			}

			#endregion

			#region  Running

			decimal YD1r = 0;
			nGames = 0;
			//  yards running will be the average of the last x games
			ds = plyr.LastStats( Constants.K_STATCODE_RUSHING_YARDS, lastWeek.WeekNo, lastWeek.WeekNo, lastWeek.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD1r += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards receiving in week {2}",
			                                         plyr.PlayerName, YD1r, lastWeek.WeekKey() ) );

			decimal YD2r = 0;
			ds = plyr.LastStats( Constants.K_STATCODE_RUSHING_YARDS, twoWeeksAgo.WeekNo, twoWeeksAgo.WeekNo, twoWeeksAgo.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD2r += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards receiving in week {2}",
																  plyr.PlayerName, YD2r, twoWeeksAgo.WeekKey() ) );

			decimal YD3r = 0;
			ds = plyr.LastStats( Constants.K_STATCODE_RUSHING_YARDS,
				threeWeeksAgo.WeekNo, threeWeeksAgo.WeekNo, threeWeeksAgo.Season );
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				YD3r += Decimal.Parse(dr["QTY"].ToString());
				nGames++;
			}
			Utility.Announce(string.Format("  {0,-15} has {1,4:##0} Yards receiving in week {2}",
																  plyr.PlayerName, YD3r, threeWeeksAgo.WeekKey()) );

			if (nGames > 0)
				plyr.TotStats.YDr = (int) (YD1r + YD2r + YD3r)/nGames;

			Utility.Announce(string.Format("  {0,-15} has averaged {1,4:##0} Yards rushing in last 3 weeks",
			                                         plyr.PlayerName, plyr.TotStats.YDr));

			if (plyr.TotStats.YDr > 0)
			{
				// adjust for role
				if (plyr.PlayerRole.Equals(Constants.K_ROLE_BACKUP))
				{
					plyr.TotStats.YDr = (plyr.TotStats.YDr/2);
					Utility.Announce(string.Format("  {0,-15} average halved to {1,4:##0} due to being a backup",
					                                         plyr.PlayerName, plyr.TotStats.YDr ) );
				}
			}

			/////////////////////////
			//  returner bonus 25 yds
			if (plyr.IsReturner())
			{
				plyr.TotStats.YDr += 25;
				Utility.Announce(string.Format("  {0,-15} returner bonus to {1,4:##0}",
				                                         plyr.PlayerName, plyr.TotStats.YDr));
			}

			/////////////////////////
			//  home bonus
			if (plyr.TotStats.YDr != 0)
			{
				if (plyr.IsAtHome(Week))
					plyr.TotStats.YDr = (int) (plyr.TotStats.YDr*1.2);
				else
					plyr.TotStats.YDr = (int) (plyr.TotStats.YDr*.8);
				Utility.Announce(string.Format("  {0,-15} average adjusted to {1,4:##0} home v away",
				                                         plyr.PlayerName, plyr.TotStats.YDr));
			}

			///////////////////
			//  opponent factor
			if (plyr.TotStats.YDr != 0)
			{
				var opponent = plyr.CurrTeam.OpponentFor(Week.Season, Week.WeekNo);
				if (opponent != null)
				{
					var oppRating = plyr.OpponentRating(opponent.Ratings);
					var ratingModifier = RatingModifier(plyr.OpponentRating(opponent.Ratings));
					plyr.TotStats.YDr = (int) (plyr.TotStats.YDr*ratingModifier);
					Utility.Announce(string.Format("  {0} output modified by {1} due to opponent ({3}) rating of {2}",
					               plyr.PlayerName, ratingModifier, oppRating, opponent.TeamCode));
					plyr.OppRate = oppRating;
				}
			}

			///////////////
			//  line factor
			if (plyr.TotStats.YDr != 0)
			{
				var game = plyr.CurrTeam.GameFor(Week.Season, Week.WeekNo);
				if (game != null)
				{
					if (game.SpreadFavourite().Equals(plyr.TeamCode))
					{
						if (Math.Abs(game.Spread) > 6.5M)
							plyr.TotStats.YDr = (int) (plyr.TotStats.YDr*1.2);
						else
							plyr.TotStats.YDr = (int) (plyr.TotStats.YDr*1.1);
						plyr.PlayerSpread = string.Format("+{0:#.#}", Math.Abs(game.Spread));
					}
					else
					{
						if (Math.Abs(game.Spread) > 6.5M)
							plyr.TotStats.YDr = (int) (plyr.TotStats.YDr*0.8);
						else
							plyr.TotStats.YDr = (int) (plyr.TotStats.YDr*0.9);
						plyr.PlayerSpread = string.Format("-{0:#.#}", Math.Abs(game.Spread));
					}
					Utility.Announce(string.Format("  {0} output modified to {1} due to the spread ({2})",
					                                         plyr.PlayerName, plyr.TotStats.YDr, plyr.PlayerSpread));

					plyr.Opponent = game.OpponentOut(plyr.CurrTeam.TeamCode);
				}
			}

			Utility.Announce(string.Format("    {0} is projected for {1} Yards rushing in week {2}",
			                                         plyr.PlayerName, plyr.TotStats.YDr, Week.WeekNo));

			//  1 pt / 10 yds
			ptsForYDs = (int) (plyr.TotStats.YDr/10.0M);
			plyr.Points += ptsForYDs;

			Utility.Announce( string.Format( "      {0} has {1} points for YDr", plyr.PlayerName, ptsForYDs ) );

			if (plyr.TotStats.YDr > 0)
			{
				//  one TD for each 75 yds caught
				plyr.TotStats.Tdr = (plyr.TotStats.YDr/75);
				plyr.Points += plyr.TotStats.Tdr*6;
			}

			#endregion

			if (plyr.PlayerCat.Equals(Constants.K_KICKER_CAT))
			{
				#region  Kicking

				//  determine FG
				decimal fg = 0;
				//  FGs will be the average of the last x games
				ds = plyr.LastScores( Constants.K_SCORE_FIELD_GOAL, 
					threeWeeksAgo.WeekNo, lastWeek.WeekNo, lastWeek.Season, "1" );
				foreach (DataRow dr in ds.Tables[0].Rows)
					fg++;

				Utility.Announce(string.Format("  {0,-15} has {1,4:##0} FG in last 3 weeks",
				                                         plyr.PlayerName, fg ) );

				if (fg > 0)
					plyr.TotStats.Fg = (int) (fg/3);

				if (plyr.TotStats.Fg > 0)
				{
					// adjust for role
					if (plyr.PlayerRole.Equals(Constants.K_ROLE_BACKUP))
					{
						plyr.TotStats.YDr = (plyr.TotStats.Fg/2);
						Utility.Announce(string.Format("  {0,-15} average halved to {1,4:##0} due to being a backup",
						                                         plyr.PlayerName, plyr.TotStats.Fg));
					}
				}

				/////////////////////////
				//  home bonus
				if (plyr.TotStats.Fg != 0)
				{
					if (plyr.IsAtHome(Week))
						plyr.TotStats.Fg++;
					else
						plyr.TotStats.Fg--;
					Utility.Announce(string.Format("  {0,-15} fg adjusted to {1,4:##0} home v away",
					                                         plyr.PlayerName, plyr.TotStats.Fg));
				}

				///////////////
				//  line factor
				if (plyr.TotStats.Fg != 0)
				{
					var game = plyr.CurrTeam.GameFor(Week.Season, Week.WeekNo);
					if (game != null)
					{
						if (game.SpreadFavourite().Equals(plyr.TeamCode))
						{
							if (Math.Abs(game.Spread) > 6.5M)
								plyr.TotStats.Fg += 2;
							else
								plyr.TotStats.Fg ++;
							plyr.PlayerSpread = string.Format("+{0:#.#}", Math.Abs(game.Spread));
						}
						else
						{
							if (Math.Abs(game.Spread) > 6.5M)
								plyr.TotStats.Fg -= 2;
							else
								plyr.TotStats.Fg--;
							plyr.PlayerSpread = string.Format("-{0:#.#}", Math.Abs(game.Spread));
						}
						Utility.Announce(string.Format("  {0} output modified to {1} due to the spread ({2})",
						                                         plyr.PlayerName, plyr.TotStats.Fg, plyr.PlayerSpread));

						plyr.Opponent = game.OpponentOut(plyr.CurrTeam.TeamCode);
					}
				}

				if (plyr.TotStats.Fg < 0) plyr.TotStats.Fg = 0;

				Utility.Announce(string.Format("    {0} is projected for {1} FGs in week {2}",
				                                         plyr.PlayerName, plyr.TotStats.Fg, Week.WeekNo));

				//  1 pt / 10 yds
				decimal ptsForFgs = (int) (plyr.TotStats.Fg*3.0M);
				plyr.Points += ptsForFgs;

				//  determine PAT
				decimal pat = 0;
				//  yards running will be the average of the last x games
				ds = plyr.LastScores(RosterLib.Constants.K_SCORE_PAT, currentWeek - 3, currentWeek - 1, Week.Season, "1");
				foreach (DataRow dr in ds.Tables[0].Rows)
					pat++;

				Utility.Announce(string.Format("  {0,-15} has {1,4:##0} PATs in last 3 weeks",
				                                         plyr.PlayerName, pat));

				plyr.TotStats.Pat = (int) pat;

				if (plyr.TotStats.Pat > 0)
					// adjust for role
					if (plyr.PlayerRole.Equals(Constants.K_ROLE_BACKUP))
						plyr.TotStats.YDr = (plyr.TotStats.YDr/2);


				/////////////////////////
				//  home bonus
				if (plyr.TotStats.Pat != 0)
				{
					if (plyr.IsAtHome(Week))
						plyr.TotStats.Pat++;
					else
						plyr.TotStats.Pat--;
					Utility.Announce(string.Format("  {0,-15} pat adjusted to {1,4:##0} home v away",
					                                         plyr.PlayerName, plyr.TotStats.Pat ) );
				}

				///////////////
				//  line factor
				if (plyr.TotStats.Pat != 0)
				{
					var game = plyr.CurrTeam.GameFor(Week.Season, Week.WeekNo);
					if (game != null)
					{
						if (game.SpreadFavourite().Equals(plyr.TeamCode))
						{
							if (Math.Abs(game.Spread) > 6.5M)
								plyr.TotStats.Pat += 2;
							else
								plyr.TotStats.Pat++;
							plyr.PlayerSpread = string.Format("+{0:#.#}", Math.Abs(game.Spread));
						}
						else
						{
							if (Math.Abs(game.Spread) > 6.5M)
								plyr.TotStats.Pat -= 2;
							else
								plyr.TotStats.Pat--;
							plyr.PlayerSpread = string.Format("-{0:#.#}", Math.Abs(game.Spread));
						}
						Utility.Announce(string.Format("  {0} output modified to {1} due to the spread ({2})",
						                                plyr.PlayerName, plyr.TotStats.Pat, plyr.PlayerSpread));

						plyr.Opponent = game.OpponentOut(plyr.CurrTeam.TeamCode);
					}
				}

				if (plyr.TotStats.Pat < 0) plyr.TotStats.Pat = 0;

				Utility.Announce(string.Format("    {0} is projected for {1} PATs in week {2}",
				                                         plyr.PlayerName, plyr.TotStats.Pat, Week.WeekNo));

				//  1 pt / 10 yds
				decimal ptsForPats = (int) (plyr.TotStats.Pat*1.0M);
				plyr.Points += ptsForPats;

				#endregion
			}

			Utility.Announce( string.Format( "{0} has {1} in week {2}:{3}", 
				plyr.PlayerName, plyr.Points, week.Season, week.Week ) );

			return plyr.Points;
		}

		public string Name { get; set; }

		public XmlCache Master
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public NFLWeek Week { get; set; }
		public bool AnnounceIt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		#endregion

		private static decimal RatingModifier(string rating)
		{
			var modifier = 1.0M;
			switch (rating)
			{
				case "A":
					modifier = 0.5M;
					break;
				case "B":
					modifier = 0.75M;
					break;
				case "D":
					modifier = 1.25M;
					break;
				case "E":
					modifier = 1.5M;
					break;
			}
			return modifier;
		}

		private static decimal QbRatingModifier(string rating)
		{
			var modifier = 1.0M;
			switch (rating)
			{
				case "A":
					modifier = 0.8M;
					break;
				case "B":
					modifier = 0.9M;
					break;
				case "D":
					modifier = 1.1M;
					break;
				case "E":
					modifier = 1.2M;
					break;
			}
			return modifier;
		}

	}
}