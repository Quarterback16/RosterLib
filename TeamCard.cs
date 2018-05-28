using System;
using System.Data;

namespace RosterLib
{
	public class TeamCard
	{
		private readonly bool _doDefence = true;
		private readonly bool _doOffence = true;

		public NflTeam Team { get; set; }

		public TeamCard(NflTeam teamIn)
		{
			Team = teamIn;
		}

		public TeamCard(NflTeam teamIn, bool defence)
		{
			Team = teamIn;
			if (defence)
			{
				_doOffence = false;
				Team.SetDefence();
			}
			else
				_doDefence = false;
		}

		public string Render()
		{
			var fileOut = $"{Utility.OutputDirectory()}{Team.Season}/TeamCards/TeamCard_{Team.TeamCode}.htm";
			var h = new HtmlFile(
                fileOut, 
                "Team Card " 
                + DateTime.Now.ToString("dd MMM yy HH:mm"));
			h.AddToBody(CardOut());
			h.Render();
            return fileOut;
		}

		public string CardOut()
		{
			var card = String.Empty;
			if (_doDefence) card += CardDefence() + "<br>";
			if (_doOffence) card += CardOffence() + "<br>";
			return card;
		}

		#region OFFENCE

		public string CardOffence()
		{
			//  Force the stats to be tallied
			var theSeason = Utility.CurrentSeason();
			if (Utility.CurrentWeek() == "0") theSeason = Utility.LastSeason();
			Team.PpUnit(theSeason);
			Team.PoUnit(theSeason);
			Team.RoUnit(theSeason);

			var innerTable = HtmlLib.TableOpen("class='title' cellpadding='1' cellspacing='1' border='0' width='950'") +
			                    "\n\t"

			                    + Row1() 
									  + Row2() 
									  + Row3()

			                    + HtmlLib.TableClose() + "\n";
			return HtmlLib.TableOpen("cellpadding='1' cellspacing='1' border='1' bordercolor='BLUE'") + "\n\t" +
			       HtmlLib.TableRowOpen() + HtmlLib.TableDataAttr(Team.TeamCode, "ALIGN='LEFT'") +
			       HtmlLib.TableDataAttr("OFFENCE", "ALIGN='RIGHT'") + HtmlLib.TableRowClose() +
			       HtmlLib.TableRowOpen() + HtmlLib.TableDataAttr(innerTable, "COLSPAN='2'") + HtmlLib.TableRowClose() +
			       HtmlLib.TableClose();
		}

		private string Row1()
		{
			return HtmlLib.TableRowOpen() +
			       SpitPlayer("WR", 1, false) + BlankCol() + BlankCol()
			       + SpitPlayer("LT", 1, false) + SpitPlayer("LG", 1, false) + SpitPlayer("C", 1, false) +
			       SpitPlayer("RG", 1, false) + SpitPlayer("RT", 1, false)
			       + SpitPlayer("TE", 1, false) + BlankCol() + SpitPlayer("WR", 2, false)
			       + HtmlLib.TableRowClose() + "\n";
		}

		private string Row2()
		{
			return HtmlLib.TableRowOpen() +
			       BlankCol() + BlankCol() + BlankCol()
			       + SpitUnit("PP", Team.PpRating(), string.Format("Sacks Allowed {0}", Team.TotSacksAllowed))
			       + BlankCol() + SpitPlayer("QB", 1, false)
			       + BlankCol() + BlankCol()
			       + SpitUnit("PO", Team.PoRating(), string.Format("TD - INT ratio {0}-{1}", Team.TotTDp, Team.TotInterceptions))
			       + BlankCol() + BlankCol()
			       + HtmlLib.TableRowClose() + "\n";
		}

		private string Row3()
		{
			return HtmlLib.TableRowOpen() +
			       SpitPlayer("PK", 1, false) + BlankCol() + BlankCol() + BlankCol() + SpitPlayer("FB", 1, false) + BlankCol()
			       + SpitPlayer("RB", 1, false) + BlankCol()
			       + SpitUnit("RO", Team.RoRating(), string.Format("Team rushing {0} ({1})", Team.TotYdr, Team.TotTDr))
			       + BlankCol() + BlankCol()
			       + HtmlLib.TableRowClose() + "\n";
		}

		#endregion

		#region  DEFENCE

		public string CardDefence()
		{
			var theSeason = Utility.CurrentSeason();
			if (Utility.CurrentWeek() == "0") theSeason = Utility.LastSeason();
			Team.PrUnit(theSeason);
			Team.RdUnit(theSeason);
			Team.PdUnit(theSeason);

			var innerTable =
				HtmlLib.TableOpen("class='title' cellpadding='1' cellspacing='1' border='0' width='950'") + "\n\t"
				+ DRow3() + DRow2() + DRow1()
				+ HtmlLib.TableClose() + "\n";
			return HtmlLib.TableOpen("cellpadding='1' cellspacing='1' border='1' bordercolor='BLUE'") + "\n\t" +
			       HtmlLib.TableRowOpen() + HtmlLib.TableDataAttr(Team.TeamCode, "ALIGN='LEFT'") +
			       HtmlLib.TableDataAttr("DEFENCE", "ALIGN='RIGHT'") + HtmlLib.TableRowClose() +
			       HtmlLib.TableRowOpen() + HtmlLib.TableDataAttr(innerTable, "COLSPAN='2'") + HtmlLib.TableRowClose() +
			       HtmlLib.TableClose();
		}

		private string DRow1()
		{
			string htm;

			if (Team.Uses34Defence)
				htm = HtmlLib.TableRowOpen()
				      + SpitUnit("PR", Team.PrRating(), string.Format("Sacks {0}", Team.TotSacks))
				      + BlankCol() + BlankCol()
				      + SpitPlayer("RDE", 1, true) + BlankCol() + BlankCol() + SpitPlayer("NT", 1, true) + BlankCol()
				      + BlankCol() + SpitPlayer("LDE", 1, true) + BlankCol() + BlankCol() + BlankCol()
				      + HtmlLib.TableRowClose() + "\n";
			else
			{
				htm = HtmlLib.TableRowOpen()
				      + SpitUnit("PR", Team.PrRating(), string.Format("Sacks {0}", Team.TotSacks))
				      + BlankCol()
				      + SpitPlayer("RDE", 1, true) + BlankCol() + SpitPlayer("LDT", 1, true) + BlankCol()
				      + SpitPlayer("RDT", 1, true) + BlankCol() + SpitPlayer("LDE", 1, true) + BlankCol() + BlankCol()
				      + HtmlLib.TableRowClose() + "\n";
			}
			return htm;
		}

		private string DRow2()
		{
			string htm;
			if (Team.Uses34Defence)
			{
				htm = HtmlLib.TableRowOpen()
				      +
				      SpitUnit("RD", Team.RdRating(),
				               string.Format("Rushing allowed {0} ({1})", Team.TotYdrAllowed, Team.TotTDrAllowed))
				      + BlankCol()
				      + SpitPlayer("ROLB", 1, true) + BlankCol() + SpitPlayer("RILB", 1, true) + BlankCol() + BlankCol()
				      + SpitPlayer("LILB", 1, true) + BlankCol() + BlankCol() + SpitPlayer("LOLB", 1, true)
				      + HtmlLib.TableRowClose() + "\n";
			}
			else
			{
				htm = HtmlLib.TableRowOpen()
				      +
				      SpitUnit("RD", Team.RdRating(),
				               string.Format("Rushing allowed {0} ({1})", Team.TotYdrAllowed, Team.TotTDrAllowed))
				      + BlankCol() + BlankCol()
				      + SpitPlayer("RLB", 1, true) + BlankCol() + SpitPlayer("MLB", 1, true) + BlankCol()
				      + SpitPlayer("LLB", 1, true) + BlankCol() + BlankCol() + BlankCol()
				      + HtmlLib.TableRowClose() + "\n";
			}
			return htm;
		}

		private string DRow3()
		{
			string htm = HtmlLib.TableRowOpen()
			             + SpitPlayer("RCB", 1, true) + BlankCol() + SpitPlayer("FS", 1, true)
			             +
			             SpitUnit("PD", Team.PdRating(),
			                      string.Format("Ints Tdp ratio {0}-{1}", Team.TotIntercepts, Team.TotTDpAllowed))
			             + BlankCol()
			             + BlankCol() + BlankCol() + SpitPlayer("SS", 1, true) + BlankCol() + BlankCol()
			             + SpitPlayer("LCB", 1, true)
			             + HtmlLib.TableRowClose() + "\n";

			return htm;
		}

		#endregion

		private static string BlankCol()
		{
			return HtmlLib.TableData("&nbsp;") + "\n\t";
		}

		private static string SpitUnit( string code, string rating, string stat1)
		{
			var u = new UnitCell(code, rating, stat1);
			return u.Render();
		}

		private string SpitPlayer( string pos, int slot, bool bDef)
		{
			//  Get a player details
			var player = Team.GetPlayerAt(pos, slot, bDef, true); //  LCB, 1, true
			var p = new PlayerCell(player, pos, CellStyle.Player, Team.TeamCode);
			if ((pos == "TE") || (pos == "LCB") || (pos == "RCB"))
				p.Offset = true;
			return p.Render();
		}
	}

	#region UnitCell class

	public enum CellStyle
	{
		Player = 0,
		DepthChart = 1
	}

	public class UnitCell
	{
		private readonly string _mUnitCode;
		private readonly string _mUnitRating;
		private readonly string _mStats;

		public UnitCell(string unitCode, string unitRating, string stats)
		{
			_mUnitCode = unitCode;
			_mUnitRating = unitRating;
			_mStats = stats;
		}

		public string Render()
		{
			var t = UnitTable();
			var strRet = HtmlLib.TableData(t.Render());
			return strRet;
		}

		private HtmlLib.HtmTable UnitTable()
		{
			var t = new HtmlLib.HtmTable("BORDER='1' BORDERCOLOR='BLACK' CELPADDING='0' CELLSPACING='0'");
			AddRow(ref t);
			return t;
		}

		private void AddRow(ref HtmlLib.HtmTable t)
		{
			var c = new HtmlLib.HtmCol();
			var r = new HtmlLib.HtmRow();
			c.Attr = "BGCOLOR='LIGHTBLUE'";
			c.Attr += " ALIGN='LEFT'";
			c.Data = _mUnitCode;
			r.AddCol(c);
			var c2 = new HtmlLib.HtmCol {Attr = "BGCOLOR='LIGHTBLUE'"};
			c2.Attr += " ALIGN='RIGHT'";
			c2.Data = _mUnitRating;
			r.AddCol(c2);
			t.AddRow(r);
			var c3 = new HtmlLib.HtmCol();
			var r2 = new HtmlLib.HtmRow();
			c3.Attr = "BGCOLOR='LIGHTBLUE'";
			c3.Attr += " ALIGN='CENTER'";
			c3.Attr += " COLSPAN='2'";
			c3.Data = _mStats;
			r2.AddCol(c3);
			t.AddRow(r2);
		}
	}

	#endregion

	#region  PlayerCell Class

	public class PlayerCell
	{
		private string _mName;
		private readonly string _mTeamCode;
		private readonly string _mSlot;
		private readonly CellStyle _mStyle;
		private string _mBgColour;
		private readonly NFLPlayer _mPlayer;

		public PlayerCell(NFLPlayer playerIn, string slotIn, CellStyle style, string teamCode)
		{
			_mSlot = slotIn;
			_mStyle = style;
			_mPlayer = playerIn;
			_mTeamCode = teamCode;
			_mName = _mPlayer == null ? String.Empty : _mPlayer.PlayerName.Trim();
			_mBgColour = "MOCCASIN";
		}

		public bool Offset { get; set; }

		public string Render()
		{
			var no = String.Empty;
			var drafted = "";

			if (_mName.Length > 0)
			{
				_mBgColour = _mPlayer.SetColour("RED");
				no = _mPlayer.JerseyNo;
				if (_mPlayer.IsNewbie())
					_mName = _mPlayer.Url(HtmlLib.Italics(HtmlLib.Bold(_mName)));
//            drafted = string.Format( " {0} {1}", mPlayer.Drafted, (int) mPlayer.ExperiencePoints );
				drafted = string.Format(" {0} {1}", _mPlayer.Drafted, "");
			}

			var t = PlayerTable(drafted, no);

			var strRet = HtmlLib.TableData((Offset ? "<br>" : "") + t.Render());
			return strRet;
		}

		private HtmlLib.HtmTable PlayerTable(string drafted, string no)
		{
			var t = new HtmlLib.HtmTable("BORDER='1' BORDERCOLOR='BLACK' CELPADDING='0' CELLSPACING='0'");

			if (_mStyle == CellStyle.Player)
			{
				AddRow(no + " " + _mName, ref t);
				AddRow(_mSlot + drafted, ref t);
				AddRow(AgeOut(_mName), ref t);
				if (_mName.Length > 0)
					if (_mPlayer.IsOffence()) AddRow(ScoresOut(_mName), ref t);
			}
			else
			{
				_mBgColour = "MOCCASIN";
				AddRow(_mSlot, ref t);
				if (_mPlayer != null) _mBgColour = _mPlayer.SetColour("PINK");
				AddRow(_mName, ref t);
				GetRole("B", _mSlot, ref t);
				GetRole(" ", _mSlot, ref t);
			}
			return t;
		}

		private void GetRole(string role, string slot, ref HtmlLib.HtmTable t)
		{
			var ds = Utility.TflWs.GetPlayer(_mTeamCode, role, slot);
			foreach (DataRow dr in ds.Tables["player"].Rows)
			{
				var p = new NFLPlayer(dr);
				_mBgColour = SetColour("PINK", p);
				AddRow(p.PlayerName, ref t);
			}
			return;
		}

		private static string SetColour(string takenColour, NFLPlayer p)
		{
			var theColour = "WHITE";
			if (p.IsFantasyPlayer())
			{
				if (p.Owner == "CO")
					theColour = "YELLOW";
				else
				{
					if (p.Owner == "**")
					{
						theColour = "LIME";
						if (p.PlayerRole.Trim().Length == 0) theColour = "CYAN";
					}
					else
						theColour = takenColour;
				}
			}
			return theColour;
		}

		private string AgeOut(string name)
		{
			var strOut = String.Empty;
			if (name.Length > 0)
				strOut = String.Format("{0}  {1}  {2}",
				                       _mPlayer.RookieYear, _mPlayer.PlayerAge(), _mPlayer.Seasons());
			return strOut;
		}

		private string ScoresOut(string name)
		{
			string strOut = String.Empty;
			if (name.Length > 0)
			{
				if (_mPlayer.IsFantasyPlayer())
					strOut = String.Format("{0}-{1}  {2:0.0}",
					                       _mPlayer.CurrScores, _mPlayer.Scores, _mPlayer.ScoresPerYear());
			}
			return strOut;
		}

		private void AddRow(string data, ref HtmlLib.HtmTable t)
		{
			var c = new HtmlLib.HtmCol();
			var r = new HtmlLib.HtmRow();
			c.Attr = BgColour();
			c.Attr += " ALIGN='CENTER'";
			c.Data = (data.Length > 0) ? data : HtmlLib.Spaces(19);
			r.AddCol(c);
			t.AddRow(r);
		}

		private string BgColour()
		{
			return "BGCOLOR='" + _mBgColour + "' ";
		}
	}

#endregion

}