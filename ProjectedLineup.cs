using System;
using System.Collections.Generic;
using System.Data;

namespace RosterLib
{
	public class ProjectedLineup
	{
		public string LeagueId { get; set; }
		public FantasyLeague League { get; set; }

		/// <summary>
		///   The team we are suggesting a lineup for
		/// </summary>
		public string TeamCode { get; set; }

		public string Season { get; set; }
		public int Week { get; set; }

		private List<NFLPlayer> _playerList;
		private readonly List<NFLPlayer> _usedPlayers;

		public ProjectedLineup(string leagueId, string teamCode, string season, int week)
		{
			LeagueId = leagueId;
			League = new FantasyLeague(leagueId);
			TeamCode = teamCode;
			Season = season;
			Week = week;
			_usedPlayers = new List<NFLPlayer>();
		}

		public void Render()
		{
			var str = new SimpleTableReport(string.Format("Projected Lineup {0}:{1:#0} {2}", Season, Week, LeagueId));
			str.AddStyle(
				"#container { text-align: left; background-color: #ccc; margin: 0 auto; border: 1px solid #545454; padding:10px; font: 13px/19px Trebuchet MS, Georgia, Times New Roman, serif; }");
			str.AddStyle("#main { margin-left:1em; }");
			str.AddStyle("#dtStamp { font-size:0.8em; }");
			str.AddStyle(".end { clear: both; }");
			str.AddStyle(".gponame { color:white; background:black }");
			str.ColumnHeadings = true;
			str.DoRowNumbers = false;
			str.ShowElapsedTime = false;
			str.IsFooter = false;
			str.AddColumn(new ReportColumn("Week", "WEEK", "{0}"));
			if (LeagueId.Equals(Constants.K_LEAGUE_Gridstats_NFL1))
			{
				str.AddColumn(new ReportColumn("Q1", "Q1", "{0}"));
				str.AddColumn(new ReportColumn("R1", "R1", "{0}"));
				str.AddColumn(new ReportColumn("R2", "R2", "{0}"));
				str.AddColumn(new ReportColumn("T1", "T1", "{0}"));
				str.AddColumn(new ReportColumn("W1", "W1", "{0}"));
				str.AddColumn(new ReportColumn("W2", "W2", "{0}"));
				str.AddColumn(new ReportColumn("K1", "K1", "{0}"));
				str.AddColumn(new ReportColumn("Q2", "Q2", "{0}"));
				str.AddColumn(new ReportColumn("R4", "R4", "{0}"));
				str.AddColumn(new ReportColumn("R3", "R3", "{0}"));
				str.AddColumn(new ReportColumn("T2", "T2", "{0}"));
				str.AddColumn(new ReportColumn("W4", "W4", "{0}"));
				str.AddColumn(new ReportColumn("W3", "W3", "{0}"));
				str.AddColumn(new ReportColumn("K2", "K2", "{0}"));
			}
			else
			{
				str.AddColumn(new ReportColumn("QB", "QB", "{0}"));
				str.AddColumn(new ReportColumn("RB1", "RB1", "{0}"));
				str.AddColumn(new ReportColumn("RB2", "RB2", "{0}"));
				str.AddColumn(new ReportColumn("WR1", "WR1", "{0}"));
				str.AddColumn(new ReportColumn("WR2", "WR2", "{0}"));
				str.AddColumn(new ReportColumn("WR3", "WR3", "{0}"));
				str.AddColumn(new ReportColumn("TE", "TE", "{0}"));
				str.AddColumn(new ReportColumn("PK", "PK", "{0}"));
			}

			str.LoadBody(BuildTable());
			str.RenderAsHtml(FileName(), true);
		}

		private string FileName()
		{
			return string.Format("{0}Lineups//{1}//{3}//Projected-{2:00}-{4}.htm", 
					Utility.OutputDirectory(), Season, Week, LeagueId, TeamCode);
		}

		private DataTable BuildTable()
		{
			var dt = new DataTable();
			var cols = dt.Columns;
			cols.Add("WEEK", typeof (String));
			if (LeagueId.Equals(RosterLib.Constants.K_LEAGUE_Gridstats_NFL1))
			{
				cols.Add("Q1", typeof (String));
				cols.Add("R1", typeof (String));
				cols.Add("R2", typeof (String));
				cols.Add("T1", typeof (String));
				cols.Add("W1", typeof (String));
				cols.Add("W2", typeof (String));
				cols.Add("K1", typeof (String));

				cols.Add("Q2", typeof (String));
				cols.Add("R4", typeof (String));
				cols.Add("R3", typeof (String));
				cols.Add("T2", typeof (String));
				cols.Add("W4", typeof (String));
				cols.Add("W3", typeof (String));
				cols.Add("K2", typeof (String));
			}
			else
			{
				cols.Add("QB", typeof (String));
				cols.Add("RB1", typeof (String));
				cols.Add("RB2", typeof (String));
				cols.Add("WR1", typeof (String));
				cols.Add("WR2", typeof (String));
				cols.Add("WR3", typeof (String));
				cols.Add("TE", typeof (String));
				cols.Add("PK", typeof (String));
			}

			//  for each week of the season
			for (int w = Week; w <= RosterLib.Constants.K_WEEKS_IN_REGULAR_SEASON; w++)
			{
				var dr = dt.NewRow();
				dr["WEEK"] = w;

				// starters
				foreach (LineupSlot s in League.LineupSlots)
				{
					PickPlayer(s, w);

					dr[s.SlotCode] = string.Format("{0} ({1})",
					                               s.PlayerSelected.PlayerNameShort, s.PlayerSelected.Points);
				}
				dt.Rows.Add(dr);
				//break;  // debug only
			}

			return dt;
		}

		private void PickPlayer(LineupSlot slot, int week)
		{
			//  select eligible players from the fantasy roster
			DataRow dr = Utility.TflWs.GetFTeamDr(Season, LeagueId, TeamCode);
			_playerList = new List<NFLPlayer>();
			for (int i = 65; i < 91; i++) //  from A-Z
			{
				string playerCode = string.Format("PLAYER{0}", (char) i);
				AddPlayer(dr[playerCode].ToString().Trim(), slot.SlotType);
			}
			// pick the best player from the list
			slot.PlayerSelected = ChooseBestPlayer(slot, week);
		}

		private NFLPlayer ChooseBestPlayer(LineupSlot slot, int week)
		{
			NFLPlayer selectedPlayer = GetPlayer(slot.Rank, week);
			_usedPlayers.Add(selectedPlayer);
			return selectedPlayer;
		}

		private NFLPlayer GetPlayer(int rank, int week)
		{
			NFLPlayer selectedPlayer = new NFLPlayer("-", "**", "?", "*", "-", "*", null);
			NFLGame game;
			List<NFLPlayer> availablePlayers = new List<NFLPlayer>();
			foreach (NFLPlayer plyr in _playerList)
			{
				game = plyr.CurrTeam.GameFor(Season, week);
				if (game == null)
				{
					RosterLib.Utility.Announce(string.Format("Bye game - {0}:{1}:{2}", Season, week, plyr.PlayerName));
					plyr.Points = -999;
				}
				else
				{
					plyr.Points = RankPoints(plyr, game);
					availablePlayers.Add(plyr);
				}
			}
			//  pick out the player depending on rank
			availablePlayers.Sort(delegate(NFLPlayer p1, NFLPlayer p2) { return p1.Points.CompareTo(p2.Points)*-1; // descending
			});
			if (rank - 1 < availablePlayers.Count)
				selectedPlayer = availablePlayers[rank - 1];

			return selectedPlayer;
		}

		private decimal PlayerSpread(decimal spread, bool isHome)
		{
			if (isHome) return spread;
			return 0 - spread;
		}

		private int RankPoints(NFLPlayer player, NFLGame game)
		{
			decimal points = 0;
			points = PlayerSpread(game.GetSpread(), game.IsHome(player.CurrTeam.TeamCode));
			if (player.PlayerRole.Equals(RosterLib.Constants.K_ROLE_STARTER))
				points += 14; //  get 2 TD if you are a starter
			return (int) points;
		}

		private void AddPlayer(string playerId, string[] slotTypes)
		{
			if (playerId.Trim().Length > 0)
			{
				// determine eligible players
				NFLPlayer p = new NFLPlayer(playerId);
				if (IsEligible(p, slotTypes))
					_playerList.Add(p);
			}
		}

		private bool IsUsed(NFLPlayer p)
		{
			bool isUsed = false;
			foreach (NFLPlayer up in _usedPlayers)
			{
				if (p.PlayerCode.Equals(up.PlayerCode))
				{
					isUsed = true;
					break;
				}
			}
			return isUsed;
		}

		private bool IsEligible(NFLPlayer p, string[] slotTypes)
		{
			bool isEligible = false;

			if (p.PlayerRole.Equals(RosterLib.Constants.K_ROLE_BACKUP) ||
			    p.PlayerRole.Equals(RosterLib.Constants.K_ROLE_STARTER))
			{
				foreach (string slottype in slotTypes)
				{
					if (slottype.Equals("T"))
					{
						if (p.PlayerCat.Equals("3") && p.PlayerPos.Contains("TE"))
						{
							isEligible = true;
							break;
						}
					}
					if (slottype.Equals("W"))
					{
						if (p.PlayerCat.Equals("3") && p.PlayerPos.Contains("WR"))
						{
							isEligible = true;
							break;
						}
					}
					if (p.PlayerCat.Equals(slottype))
					{
						isEligible = true;
						break;
					}
				}
			}
			return isEligible;
		}
	}
}