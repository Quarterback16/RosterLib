using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace RosterLib
{
	/// <summary>
	///   A cache of games for calculating EP.
	///   To try and speed up production of ep.xml
	/// </summary>
	public class GameMaster : XmlCache
	{
		private readonly Hashtable _gameHt;
		private readonly XmlDocument _gameXmlDoc;

		public GameMaster(string name) : base(name)
		{
			_gameHt = new Hashtable();
			try
			{
				//  load HT from the xml
				_gameXmlDoc = new XmlDocument();
				_gameXmlDoc.Load( string.Format( "{0}xml\\NFLGame.xml", Utility.OutputDirectory() ) );
				var gameListNode = _gameXmlDoc.ChildNodes[2];
				foreach (XmlNode gameNode in gameListNode.ChildNodes)
					AddXmlGame(gameNode);
			}
			catch (IOException e)
			{
				Utility.Announce(string.Format("Unable to open NFLGame.xml xmlfile - {0}", e.Message));
			}
		}

		private void AddXmlGame(XmlNode gameNode)
		{
			AddGame(new NFLGame(gameNode));
		}

		public void AddGame(NFLGame g)
		{
			if (! _gameHt.ContainsKey(g.GameKey()))
			{
				_gameHt.Add(g.GameKey(), g);
				IsDirty = true;
			}
		}

		/// <summary>
		///   gameCode is in the format :- Format( "{0}:{1}-{2}", Season, Week, GameCode )
		/// </summary>
		/// <param name="gameCode"></param>
		/// <returns></returns>
		public NFLGame GetGame(string gameCode)
		{
#if DEBUG2
			Utility.Announce( string.Format("Gm:GetGame - cache get on {0}", gameCode ) );
#endif
			var g = (NFLGame) _gameHt[gameCode];
			if (g == null)
			{
				CacheMisses++;
#if DEBUG2
				Utility.Announce( string.Format( "Gm:cache miss on {0}", gameCode ) );
#endif
			}
			else
			{
#if DEBUG2
				Utility.Announce( "Gm:Got game from cache" );
#endif
				CacheHits++;

			}
			return g;
		}

		public decimal HomeAwayRatio()
		{
			var homeWins = 0;
			var awayWins = 0;
			var myEnumerator = _gameHt.GetEnumerator();
			while (myEnumerator.MoveNext())
			{
				var g = (NFLGame) myEnumerator.Value;
				if (g.Played())
					if (g.HomeWin())
						homeWins++;
					else
						awayWins++;
			}

			var ratio = Ratio(homeWins, awayWins);
			Utility.Announce( string.Format("Home-Away Ratio:- Home M_wins {0} Away M_wins {1} Ratio {2:###.#}%",
			                                         homeWins, awayWins, ratio*100.0M));
			return ratio;
		}

		private static decimal Ratio(int h, int a)
		{
			decimal ratio = 0;
			if ((h + a) > 0)
				ratio = Convert.ToDecimal(h)/(Convert.ToDecimal(h) + Convert.ToDecimal(a));
			return ratio;
		}

		public void Dump2Xml()
		{
			if (_gameHt.Count > 0)
			{
				//DumpHt( gameHT );

				var writer = new
					XmlTextWriter(string.Format("{0}\\xml\\NFLGame.xml", Utility.OutputDirectory()), null);

				writer.WriteStartDocument();
				writer.WriteComment("Comments: Game Metrics");
				writer.WriteStartElement("game-list");

				var myEnumerator = _gameHt.GetEnumerator();
				while (myEnumerator.MoveNext())
				{
					var g = (NFLGame) myEnumerator.Value;
					WriteGameNode(writer, g);
					if (g.GameDate >= DateTime.Now) continue;
					if (g.AwayScore + g.HomeScore == 0)
						Utility.Announce(string.Format("Suspect game [{0}]:", g.ScoreOut()));
				}
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Close();
				Utility.Announce(string.Format("   {0}\\xml\\NFLGame.xml created", Utility.OutputDirectory()));
			}
		}

		private static void WriteGameNode(XmlTextWriter writer, NFLGame g)
		{
			writer.WriteStartElement("game");
			WriteElement(writer, "season", g.Season);
			WriteElement(writer, "week", g.Week);
			WriteElement(writer, "gamecode", g.GameCode);
			WriteElement(writer, "date", g.GameDate.ToShortDateString());
			WriteElement(writer, "hometeamcode", g.HomeTeam);
			WriteElement(writer, "awayteamcode", g.AwayTeam);
			WriteMetricsNode("home", writer, g);
			WriteMetricsNode("away", writer, g);
			WriteEPsNode("home", writer, g, g.HomeTeam);
			WriteEPsNode("away", writer, g, g.AwayTeam);
			writer.WriteEndElement();
		}

		private static void WriteEPsNode(string homeOrAway, XmlWriter writer, NFLGame g, string teamCode)
		{
			writer.WriteStartElement(string.Format("{0}-ep", homeOrAway));
			foreach (TeamUnit u in Utility.UnitList)
			{
				var ep = g.ExperiencePoints(u.UnitCode, teamCode);
				writer.WriteAttributeString(u.UnitCode, ep.ToString());
			}
			writer.WriteEndElement();
		}

		private static void WriteMetricsNode(string homeOrAway, XmlWriter writer, NFLGame g)
		{
			writer.WriteStartElement(string.Format("{0}-metrics", homeOrAway));
			if (homeOrAway == "home")
			{
				writer.WriteAttributeString("score", g.HomeScore.ToString());
				writer.WriteAttributeString("TDp", g.HomeTDp.ToString());
				writer.WriteAttributeString("TDr", g.HomeTDr.ToString());
				writer.WriteAttributeString("SAKa", g.HomeSaKa.ToString());
			}
			else
			{
				writer.WriteAttributeString("score", g.AwayScore.ToString());
				writer.WriteAttributeString("TDp", g.AwayTDp.ToString());
				writer.WriteAttributeString("TDr", g.AwayTDr.ToString());
				writer.WriteAttributeString("SAKa", g.AwaySaKa.ToString());
			}
			writer.WriteEndElement();
		}
	}
}