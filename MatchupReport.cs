using System;
using System.Collections;
using System.Data;
using TFLLib;

namespace RosterLib
{
	/// <summary>
	/// Summary description for MatchupReport.
	/// </summary>
	public class MatchupReport
	{
		protected DataLibrarian TflWs;
		readonly NFLGame _game;
		NFLGambler _kenny;

		public string FileOut { get; set; }
		
		private bool _showPrediction = true;
		private bool _showRecent = true;
		private bool _showFreeAgents = true;
		private bool _showTeamCards = true;
		private bool _showInjuries = true;
		private bool _showLineupCards;
		private bool _showUnits = true;
		
		public MatchupReport( NFLGame gameIn )
		{
			Utility.Announce( "Match Report for :" + gameIn.AwayTeamName + " @ " + gameIn.HomeTeamName );
			if ( gameIn.HomeNflTeam == null )
				Utility.Announce( "Teams have not been instantiated" );
			else
			{
				TflWs = Utility.TflWs;
				if ( _showTeamCards )
				{
					gameIn.AwayNflTeam.LoadTeamCard(); //  lazy load
					gameIn.AwayNflTeam.SetDefence(); //  lazy load
					gameIn.HomeNflTeam.LoadTeamCard(); //  lazy load			
					gameIn.HomeNflTeam.SetDefence(); //  lazy load			
				}
				_game = gameIn;
			}
		}
		
		public void Render( bool writeProjection )
		{
			if (_game == null)
			{
				Utility.Announce( "Game Off" );
				return;
			}

			var s = GameBlock() + "<br>\n\n";

			if ( ShowPrediction ) 
			{
				//SimplePredictor predictor = new SimplePredictor();
				var predictor = new UnitPredictor
				                	{
				                		RatingsService = new UnitRatingsService(new TimeKeeper(null)), 
											WriteProjection = writeProjection
				                	};
				//WizPredictor predictor = new WizPredictor();

				Predictions( predictor, ref s );
			}
			
			if ( ShowRecent ) RecentMeetings( ref s );

			if ( ShowBets ) Bets( ref s );

			if ( ShowFreeAgents ) FreeAgents( ref s, _game.HomeTeamName, _game.AwayTeamName,
														 _game.HomeNflTeam, _game.AwayNflTeam );

			if ( ShowTeamCards ) HomeOffenceMatchup( ref s );
			if ( ShowInjuries ) Injuries( ref s, _game.HomeNflTeam, _game.AwayNflTeam,
													_game.HomeTeamName, _game.AwayTeamName );

			if ( ShowMatrix ) HomeMatrix( ref s );
			if ( ShowUnits ) HomeOffUnits( ref s );
			
			if ( ShowTeamCards ) AwayOffenceMatchup( ref s );
			if ( ShowInjuries ) Injuries( ref s, _game.AwayNflTeam, _game.HomeNflTeam,
													_game.AwayTeamName, _game.HomeTeamName );

			if ( ShowMatrix ) AwayMatrix( ref s );			
			if ( ShowUnits ) AwayOffUnits( ref s );

			if ( ShowLineups ) LineupCards( ref s );
			RenderMatchup( s );
		}

		private void LineupCards( ref string s )
		{
#if DEBUG
			Utility.Announce("Lineup Cards");
#endif
			if ( _game.HomeNflTeam != null ) s += _game.HomeNflTeam.SpitLineups( false ) + "<br>";
			if ( _game.AwayNflTeam != null) s += _game.AwayNflTeam.SpitLineups(false);
		}
		
		private void HomeOffUnits( ref string s )
		{
#if DEBUG
			Utility.Announce("HomeOffUnits"); 
#endif
			if (_game == null)
				Utility.Announce("HomeOffUnits: null game");
			else
			{
				if ((_game.HomeNflTeam == null) || (_game.AwayNflTeam == null))
					Utility.Announce("HomeOffUnits: null team");
				else
				{
					s += HtmlLib.TableOpen( "BORDER='1'" );

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.RoList(), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.RdList(), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.RoUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.RdUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.PoList(), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.PdList(), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.PoUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.PdUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.PpUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.PrUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableClose();
				}
			}
		}

		private void AwayOffUnits(ref string s)
		{
#if DEBUG
			Utility.Announce( "AwayOffUnits" );
#endif
			if ( _game == null )
				Utility.Announce( "AwayOffUnits: null game" );
			else
			{
				if ( ( _game.HomeNflTeam == null ) || ( _game.AwayNflTeam == null ) )
					Utility.Announce( "AwayOffUnits: null team" );
				else
				{
					s += HtmlLib.TableOpen( "BORDER='1'" );

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.RoList(), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.RdList(), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.RoUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.RdUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.PoList(), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.PdList(), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.PoUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.PdUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableRowOpen();
					s += HtmlLib.TableDataAttr( _game.AwayNflTeam.PpUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableDataAttr( _game.HomeNflTeam.PrUnit( _game.Season ), "VALIGN='TOP'" );
					s += HtmlLib.TableRowClose();

					s += HtmlLib.TableClose();
				}
			}
		}

		private void RenderMatchup( string s )
		{
			if ( ( _game.HomeNflTeam == null ) || ( _game.AwayNflTeam == null ) )
				Utility.Announce( "RenderMatchup: null team" );
			else
			{
				FileOut =
                    string.Format(Utility.OutputDirectory() + "{3}\\Matchups\\Week{2}\\Matchup_Wk{2}_{0}v{1}.htm",
										_game.HomeNflTeam.TeamCode,
										_game.AwayNflTeam.TeamCode, 
										_game.Week, 
										_game.Season );

				var h = new HtmlFile( FileOut, " Matchup as of " + 
               DateTime.Now.ToString( "ddd dd MMM yy" ) );
				h.AddToBody( s );
				h.Render();
			}
		}

		private void AwayMatrix( ref string s )
		{
			if (_game.HomeNflTeam != null)
				s += _game.HomeNflTeam.GetMatrix().RenderAsHtml( _game.HomeNflTeam.TeamCode + " Defensive units", true, false, false, true  ) + "<br>\n\n";
			if (_game.AwayNflTeam != null)
				s += _game.AwayNflTeam.Matrix.RenderAsHtml( _game.AwayNflTeam.TeamCode + " Offensive units", false, true, false, true ) + "<br>\n\n";
		}

		private void HomeMatrix( ref string s )
		{
			if (_game != null)
			{
				UnitMatrix um;
				if (_game.AwayNflTeam != null)
				{
					um = _game.AwayNflTeam.GetMatrix();
					s += um.RenderAsHtml( _game.AwayNflTeam.TeamCode + " Defensive units", true, false, false, true ) + "<br>\n\n";
				}
				if (_game.HomeNflTeam != null)
				{
					um = _game.HomeNflTeam.GetMatrix();
					s += um.RenderAsHtml( _game.HomeNflTeam.TeamCode + " Offensive units", false, true, false, true ) + "<br>\n\n";
				}
			}
		}

		private void AwayOffenceMatchup( ref string s )
		{
			if (_game != null)
			{
				if (_game.HomeNflTeam != null)
					s += _game.HomeNflTeam.TeamCard.CardDefence() + "<br>\n\n";
				if (_game.AwayNflTeam != null)
					s += _game.AwayNflTeam.TeamCard.CardOffence() + "<br>\n\n";
			}
		}

		private void HomeOffenceMatchup( ref string s )
		{
			if (_game != null)
			{
				s += "<br>\n";
				//  Away Defence 
				if ( _game.AwayNflTeam != null )
				{
					if ( _game.AwayNflTeam.TeamCard != null )
					{
						s += _game.AwayNflTeam.TeamCard.CardDefence() + "<br>\n\n";
					}
				}
				//  Home Offence
				if ( _game.HomeNflTeam != null )
				{
					if ( _game.HomeNflTeam.TeamCard != null )
					{
						s += _game.HomeNflTeam.TeamCard.CardOffence() + "<br>\n\n";
					}
				}
			}
		}

		private static void FreeAgents( ref string s, string homeName, string awayName, NflTeam homeTeam, NflTeam awayTeam )
		{
			//RosterLib.Utility.Announce( "Doing FreeAgents");
			if ((homeTeam == null) || (awayTeam == null))
				Utility.Announce("FreeAgents:one or more teams are null");
			else
			{
				s += "<br>\n";
				//  Table with one row which has 2 columns Away and Home
				s += HtmlLib.TableOpen( "BORDER='1' BORDERCOLOR='BLUE'" );
				s += HtmlLib.TableRowOpen();
				s += HtmlLib.TableDataAttr( "Free Agents/Backups", "COLSPAN='2'" );
				s += HtmlLib.TableRowClose();
				s += HtmlLib.TableRowOpen();
				s += HtmlLib.TableDataAttr( "Away " + awayName, "WIDTH='400'" );
				s += HtmlLib.TableDataAttr( "Home " + homeName, "WIDTH='400'" );
				s += HtmlLib.TableRowClose();
				s += HtmlLib.TableRowOpen();
				s += HtmlLib.TableDataAttr( awayTeam.FreeAgents( true, true, false ), "VALIGN='TOP'" );
				s += HtmlLib.TableDataAttr( homeTeam.FreeAgents( true, true, false ), "VALIGN='TOP'" );
				s += HtmlLib.TableRowClose() + HtmlLib.TableClose() + "<br>\n";
			}
			//RosterLib.Utility.Announce( "Finished FreeAgents");			
		}

		private static void Injuries( ref string s, NflTeam offensiveTeam, NflTeam defensiveTeam,
									  string offName, string defName )
		{
			if ((offensiveTeam == null) || (defensiveTeam == null))
				Utility.Announce("Injuries:one or more teams are null");
			else
			{
				s += "<br>\n";
				//  Table with one row which has 2 columns
				s += HtmlLib.TableOpen( "BORDER='1' BORDERCOLOR='BLUE'" );
				s += HtmlLib.TableRowOpen();
				s += HtmlLib.TableDataAttr( "Injuries", "COLSPAN='2'" );
				s += HtmlLib.TableRowClose();
				s += HtmlLib.TableRowOpen();
				s += HtmlLib.TableDataAttr( "Offence " + offName, "WIDTH='400'" );
				s += HtmlLib.TableDataAttr( "Defence " + defName, "WIDTH='400'" );
				s += HtmlLib.TableRowClose();
				s += HtmlLib.TableRowOpen();
				s += HtmlLib.TableDataAttr( offensiveTeam.InjuredList( true ), "VALIGN='TOP'" );
				s += HtmlLib.TableDataAttr( defensiveTeam.InjuredList( false ), "VALIGN='TOP'" );
				s += HtmlLib.TableRowClose() + HtmlLib.TableClose() + "<br>\n";
			}
		}

		private void Bets( ref string s )
		{
			//RosterLib.Utility.Announce( "Doing Bets");
			if ( _kenny == null )
				_kenny = new NFLGambler( 750.00D );
			var betList = new ArrayList();
			//  pass it a game
			_kenny.ConsiderGame( _game, ref betList );
			if ( betList.Count > 0 )
				//  get the list as formatted HTML
				s += _kenny.RenderBets( betList, false, false );
			//RosterLib.Utility.Announce( "Finished Bets");			
		}

		private void RecentMeetings( ref string s )
		{
			//RosterLib.Utility.Announce( "Doing Recent Meetings");			

			var s1 = String.Empty;
			var nonDeletedRecs = 0;
			s1 += HtmlLib.TableOpen( "border='1'" );
			s1 += HtmlLib.TableHeader("Season");
			s1 += HtmlLib.TableHeader("Week");
			s1 += HtmlLib.TableHeader("AT");
			s1 += HtmlLib.TableHeader("AS");
			s1 += HtmlLib.TableHeader("HT");
			s1 += HtmlLib.TableHeader("HS");
			s1 += HtmlLib.TableHeader("Spread");
			s1 += HtmlLib.TableHeader("O/U") + "<br>\n\n";
			//RosterLib.Utility.Announce(s1);		
			var now = DateTime.Now;
			var ts = new TimeSpan( (3*365), 0, 0, 0 );
			var threeYrsAgo = now.Subtract( ts );
			//RosterLib.Utility.Announce( string.Format("3 years ago was {0}", threeYrsAgo ) );
			//RosterLib.Utility.Announce(string.Format("Getting games between {0} and {1}", game.AwayTeam, game.HomeTeam ));
			if (Utility.TflWs != null)
			{
				var ds = Utility.TflWs.GetGamesBetween(_game.AwayTeam, _game.HomeTeam, threeYrsAgo);
				if (ds != null)
				{
					// RosterLib.Utility.Announce(string.Format("found {0} Recent Meetings", ds.Tables[0].Rows.Count));
					for (var i = 0; i < ds.Tables[0].Rows.Count; i++)
					{
						if (ds.Tables[0].Rows[i].RowState != DataRowState.Deleted)
						{
							nonDeletedRecs++;
							var theGame = new NFLGame(ds.Tables[0].Rows[i]);
							s1 += theGame.ResultRow() + "<br>\n\n";
						}
					}
					s1 += HtmlLib.TableClose() + "<br>\n\n";
				}
				else
					Utility.Announce("Null game set");

				if (nonDeletedRecs > 0)
					s += s1;
				else
					s += "<br>No Previous Meetings<br>";
			}
			else
				Utility.Announce( "No Librarian!" );
		}

		private void Predictions( IPrognosticate predictor, ref string s)
		{
#if DEBUG
			Utility.Announce( "Doing predictions");			
#endif
			s += Prediction( _game.HomeNflTeam, predictor ) + "<br>\n\n";
			s += Prediction( _game.AwayNflTeam, predictor ) + "<br>\n\n";
		}

		private static string Prediction( NflTeam team, IPrognosticate predictor )
		{
			var predictionStr = String.Empty;
			if ( team != null )
				predictionStr = 
					HtmlLib.TableOpen( "BORDER='1' CELLSPACING='0' CELLPADDING='0'" ) + 
					HtmlLib.TableRowOpen() + 
					HtmlLib.TableDataAttr( team.Name, "COLSPAN='19'" ) + 
					HtmlLib.TableRowClose() + 
					team.SeasonProjection( predictor, "Spread", DateTime.Now );
			return predictionStr;
		}

		private string GameBlock()
		{
			string s;
			if (_game != null)
			{
				var weekOut = Int32.Parse(_game.Week) > 17 ? "PO" : "W" + _game.Week;
				if (_game.Week.Equals("21")) weekOut = "SB";
				s = HtmlLib.TableOpen( "BORDER='1' CELLSPACING='3' CELLPADDING='3'" ) +
					 HtmlLib.TableRowOpen() +
					 HtmlLib.TableData( _game.AussieDate() ) +
					 HtmlLib.TableData( _game.AussieHour( true ) ) +
					 HtmlLib.TableData( weekOut ) +
					 HtmlLib.TableData( _game.Season ) +
					 HtmlLib.TableRowClose() +
					 HtmlLib.TableRowOpen() +
					 HtmlLib.TableData( HtmlLib.Bold( _game.HomeTeamName ), SetColour( true ) ) +
					 HtmlLib.TableData( "vs. " + _game.AwayTeamName, SetColour( false ) ) +
					 HtmlLib.TableData( _game.Spread.ToString() ) +
					 HtmlLib.TableData( _game.Total.ToString() ) +
					 HtmlLib.TableRowClose() +
					 HtmlLib.TableRowOpen();
				if ( _game.HomeNflTeam != null )
					s += HtmlLib.TableData( _game.HomeNflTeam.RecordOut() );
				else
					s += HtmlLib.TableData( " " );
				if (_game.AwayNflTeam != null)
					s += HtmlLib.TableData(_game.AwayNflTeam.RecordOut());
				else
					s += HtmlLib.TableData(" ");               

				s += HtmlLib.TableData("") +
					  HtmlLib.TableData("") +
					  HtmlLib.TableRowClose() +
					  HtmlLib.TableClose();
			}
			else s = "no game object";
			return s;
		}

		private string SetColour( bool isHome )
		{
			var sColour = "WHITE";
			if ( isHome )
			{
				if ( _game.Spread > 0 )
					sColour = "LIME";
			}
			else
			{
				if ( _game.Spread < 0 )
					sColour = "LIME";
			}

			return sColour;
		}


		#region  Accessors

		public bool ShowLineups
		{
			get { return _showLineupCards; }
			set { _showLineupCards = value; }
		}
			 
		public bool ShowRecent
		{
			get { return _showRecent; }
			set { _showRecent = value; }
		}
				
		public bool ShowFreeAgents
		{
			get { return _showFreeAgents; }
			set { _showFreeAgents = value; }
		}

		public bool ShowMatrix { get; set; }

		public bool ShowPrediction
		{
			get { return _showPrediction; }
			set { _showPrediction = value; }
		}

		public bool ShowBets { get; set; }

		public bool ShowInjuries
		{
			get { return _showInjuries; }
			set { _showInjuries = value; }
		}
		public bool ShowTeamCards
		{
			get { return _showTeamCards; }
			set { _showTeamCards = value; }
		}
		public bool ShowLineupCards
		{
			get { return _showLineupCards; }
			set { _showLineupCards = value; }
		}

		public bool ShowUnits
		{
			get { return _showUnits; }
			set { _showUnits = value; }
		}

		#endregion
	}
}
