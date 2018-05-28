using System;
using System.Data;
using System.Linq;

namespace RosterLib
{
   public class DanGordan
   {

      private const decimal KStrongAgainstSpread = 0.6M;
      private const decimal KWeekAgainstSpread = 0.4M;
      private const int KLargePointSwingMargin = 50;
      private const int KBigScore = 30;
      private const decimal KClipOfNoReturn = 0.375M;
      private const decimal KClipOfSomeChanceStill = 0.500M;

      #region  The weekly Prediction

      public void ProjectLine( string season, string week )
      {
         DataTable dt = DefineData();
         LoadData( dt, season, week );
         Render( season, week, dt );
      }

      private static void Render( string season, string week, DataTable dt )
      {
      	var str = new SimpleTableReport
      	                        	{
      	                        		ReportHeader = "Gordan Line : Week " + week,
                                 		ColumnHeadings = true,
                                 		DoRowNumbers = true
                                 	};
      	str.AddColumn( new ReportColumn( "Away Team", "AWAY", "{0,-20}" ) );
         str.AddColumn( new ReportColumn( "ALR", "AWAYLETTER", "{0,-2}" ) );
         str.AddColumn( new ReportColumn( "Home Team", "HOME", "{0,-20}" ) );
         str.AddColumn( new ReportColumn( "HLR", "HOMELETTER", "{0,-2}" ) );
         str.AddColumn( new ReportColumn( "Spread", "SPREAD", "{0:#0.0}" ) );
         str.AddColumn( new ReportColumn( "GordLine", "GLINE", "{0:#0.0}" ) );
         str.AddColumn(new ReportColumn("MyTip", "MYTIP", "{0:#0.0}"));
         str.AddColumn(new ReportColumn("Diff", "DIFF", "{0:#0.0}"));
         str.AddColumn( new ReportColumn( "AwSR", "ASR", "{0}" ) );
         str.AddColumn( new ReportColumn( "HmSR", "HSR", "{0}" ) );

         str.AddColumn( new ReportColumn( "Bet", "BET", "{0,-20}" ) );
         str.AddColumn( new ReportColumn( "Away", "AWAYSCORE", "{0}" ) );
         str.AddColumn( new ReportColumn( "Home", "HOMESCORE", "{0}" ) );
         str.AddColumn( new ReportColumn( "Result", "RESULT", "{0}" ) );
         str.AddColumn( new ReportColumn( "Adjustment", "ADJUST", "{0}" ) );
         str.LoadBody( dt );
         str.RenderAsHtml(
            string.Format("{0}GordanLine{1}{2}.htm", Utility.OutputDirectory(), season, week),
            true );
      }

      private static void LoadData( DataTable dt, string season, string weekIn )
      {
         var week = new NFLWeek( season, weekIn );
         //  carry forward all the old ratings to cover the bye situatuins,
         //  but dont override adjustments
         var s = Masters.Sm.GetSeason( season );
         foreach ( string key in s.TeamKeyList )
         {
            NflTeam t = Masters.Tm.GetTeam( key );
            t.LetterRating[ Int32.Parse( weekIn ) ] = t.LetterRating[ Int32.Parse( weekIn ) - 1 ];
            t.NumberRating[ Int32.Parse( weekIn ) ] = t.NumberRating[ Int32.Parse( weekIn ) - 1 ];
            Masters.Sm.IsDirty = true;
         }
         var previousWeek = Int32.Parse( weekIn ) - 1;
         foreach ( NFLGame g in week.GameList() )
         {
            var dr = dt.NewRow();

            if (g.HomeNflTeam == null) g.HomeNflTeam = Masters.Tm.GetTeam( g.Season, g.HomeTeam );
            if (g.AwayNflTeam == null) g.AwayNflTeam = Masters.Tm.GetTeam(g.Season, g.AwayTeam);
            
            dr[ "HOME" ] = g.HomeTeamName + " " + HtmlLib.Bold( Emoticons( g.HomeNflTeam, g.AwayNflTeam, g ) );
            dr[ "AWAY" ] = g.AwayTeamName + " " + HtmlLib.Bold( Emoticons( g.AwayNflTeam, g.HomeNflTeam, g ) );
            dr[ "SPREAD" ] = g.Spread;
            var gline = GordanLine( g );
            var hLetter = g.HomeNflTeam.LetterRating[ previousWeek ];
            dr[ "HOMELETTER" ] = hLetter;
            var aLetter = g.AwayNflTeam.LetterRating[ previousWeek ];
            dr[ "AWAYLETTER" ] = aLetter;
            dr[ "HSR" ] = g.HomeNflTeam.SpreadRange( season, string.Format( "{0:00}", previousWeek ) );
            dr[ "ASR" ] = g.AwayNflTeam.SpreadRange( season, string.Format( "{0:00}", previousWeek ) );
            dr[ "GLINE" ] = gline;
            dr[ "MYTIP" ] = g.MyLine();
            dr[ "DIFF" ] = g.Spread - gline;
            dr[ "BET" ] = WhatsTheBet( g, gline );
            if ( g.Played() )
            {
               dr[ "RESULT" ] = GordanResult( g );
               dr[ "HOMESCORE" ] = g.HomeScore;
               dr[ "AWAYSCORE" ] = g.AwayScore;
               dr[ "ADJUST" ] = NumberAdjustment( g );
            }
            dt.Rows.Add( dr );
            if ( g.Played() )
            {
               //  update rankings
               g.HomeNflTeam.NumberRating[ Int32.Parse( weekIn ) ] =
                  HomeAdjustment( g.HomeScore, g.AwayScore, g.GordanLine ) +
                  g.HomeNflTeam.NumberRating[ Int32.Parse( weekIn ) - 1 ];
               g.AwayNflTeam.NumberRating[ Int32.Parse( weekIn ) ] =
                  AwayAdjustment( g.AwayScore, g.HomeScore, g.GordanLine ) +
                  g.AwayNflTeam.NumberRating[ Int32.Parse( weekIn ) - 1 ];
               // carry the letter ratings too
               g.HomeNflTeam.LetterRating[ Int32.Parse( weekIn ) ] =
                  g.HomeNflTeam.LetterRating[ Int32.Parse( weekIn ) - 1 ];
               g.AwayNflTeam.LetterRating[ Int32.Parse( weekIn ) ] =
                  g.AwayNflTeam.LetterRating[ Int32.Parse( weekIn ) - 1 ];
               Masters.Sm.IsDirty = true;
            }
         }
      }

      private static DataTable DefineData()
      {
         var dt = new DataTable();
         var cols = dt.Columns;
         cols.Add( "AWAY", typeof ( String ) );
         cols.Add( "HOME", typeof ( String ) );
         cols.Add( "AWAYLETTER", typeof ( String ) );
         cols.Add( "HOMELETTER", typeof ( String ) );
         cols.Add( "SPREAD", typeof ( Decimal ) );
         cols.Add( "GLINE", typeof ( Decimal ) );
         cols.Add("MYTIP", typeof(Decimal));
         cols.Add( "DIFF", typeof ( Decimal ) );
         cols.Add( "BET", typeof ( String ) );
         cols.Add( "HOMESCORE", typeof ( Int32 ) );
         cols.Add( "AWAYSCORE", typeof ( Int32 ) );
         cols.Add( "RESULT", typeof ( String ) );
         cols.Add( "ADJUST", typeof ( String ) );
         cols.Add( "ASR", typeof ( Int32 ) );
         cols.Add( "HSR", typeof ( Int32 ) );
         return dt;
      }

      #endregion

      private static string NumberAdjustment( NFLGame g )
      {
         decimal homeAdj = HomeAdjustment( g.HomeScore, g.AwayScore, g.GordanLine );
         decimal awayAdj = AwayAdjustment( g.AwayScore, g.HomeScore, g.GordanLine );
         return string.Format( "{0}:{1}", awayAdj, homeAdj );
      }

      private static decimal HomeAdjustment( int homeScore, int awayScore, decimal spread )
      {
         decimal actMargin = homeScore - awayScore; //  if +ve then a home win
         if ( spread > 0.0M )
         {
            //  home was favoured
            actMargin -= spread;
         }
         else
         {
            actMargin += Math.Abs( spread );
         }
         decimal adj = Adjustment( actMargin );
         return adj;
      }

      private static decimal AwayAdjustment( int awayScore, int homeScore, decimal spread )
      {
         decimal actMargin = awayScore - homeScore; //  if +ve then a away win
         if ( spread > 0.0M )
         {
            //  home was favoured
            actMargin += spread; //  add the spread to the margin in favour of away team
         }
         else
         {
            actMargin -= Math.Abs( spread ); //  take the points off the away team
         }
         decimal adj = Adjustment( actMargin );
         return adj;
      }

      private static decimal Adjustment( decimal margin )
      {
         decimal adjustment;

         decimal absMargin = Math.Abs( margin );
         if ( absMargin < 5.0M )
            adjustment = 0.0M;
         else if ( absMargin < 10.0M )
            adjustment = 1.0M;
         else if ( absMargin < 15.0M )
            adjustment = 2.0M;
         else
            adjustment = 3.0M;

         if ( margin < 0.0M ) adjustment *= -1; //  negative adjustment

         return adjustment;
      }

      private static string GordanResult( NFLGame g )
      {
         string outcome = "  ";

         decimal result = g.HomeScore - g.AwayScore - g.Spread; //  Spread is positive for HT
         if ( g.BetHome )
         {
            if ( result > 0 )
               outcome = "Win!";
            else outcome = result.Equals( 0 ) ? "Push" : "Loss";
         }
         if ( g.BetAway )
         {
            if ( result > 0 )
               outcome = "Loss";
            else outcome = result.Equals( 0 ) ? "Push" : "Win!";
         }
         g.SpreadResult = result; // - is Away win, + is home win
         return outcome;
      }

      private static string WhatsTheBet( NFLGame g, decimal gLine )
      {
         decimal diff = PointsDiff( g.Spread, gLine );
         string bet;

         if ( Math.Abs( diff ) < 4.0M )
            bet = "   ";
         else
         {
            string spread;
            //  we have a bet as we are over the 4pt threshold
            if ( g.Spread > 0 )
            {
               //  home favoured
               if ( diff > 0 )
               {
                  //  spread is too high - bet Away with the points
                  spread = " +";
                  g.BetAway = true;
                  bet = "Bet away " + g.AwayNflTeam.Nick() + spread;
               }
               else
               {
                  //  spread is too low - bet Home minus the low points
                  spread = " -";
                  bet = "Bet home " + g.HomeNflTeam.Nick() + spread;
                  g.BetHome = true;
               }
            }
            else
            {
               //  away is favoured
               if ( diff > 0 )
               {
                  //  away team is to win by more
                  spread = " -";
                  bet = "Bet away " + g.AwayNflTeam.Nick() + spread;
                  g.BetAway = true;
               }
               else
               {
                  //  away team is giving up too many points
                  spread = " +";
                  bet = "Bet home " + g.HomeNflTeam.Nick() + spread;
                  g.BetHome = true;
               }
            }
            bet += string.Format( "{0:#0.#}", Math.Abs( g.Spread ) );
         }
         return bet;
      }

      #region  Pre-season tasks

      public void GenerateNumberRatings( string season )
      {
         //  At the begining of every season I assign each
         //  team a number rating, a more objective rating
         //  wholly controlled in its movement by team 
         //  Scores.  Number ratings provide a kind of
         //  balance against the subjectivity of my
         //  letter ratings.  They are also used as 
         //  important indicators that a team is "due up"
         //  or "due down".
         //
         NflSeason s = Masters.Sm.GetSeason( season );

         //  To assign each team a number I mathematically
         //  play out the season using my letter power
         //  ratings, which I convert into projected point
         //  spreads using my Letter Rating Point Differences
         //  algorithm.  In addition, every home club gets
         //  2.5 points.
         foreach ( string key in s.TeamKeyList )
         {
            NflTeam team = Masters.Tm.GetTeam( key );
            decimal projWins = ProjectWinsFor( team, season );
            decimal powerNumber = ConvertWinsToProjectedWinNumber( projWins );
            // 
            //  I then average this projected win number value with a
            //  value for a teams letter power value.
            //
            //  Convert the starting subjective letter rating to a number
            string startingLetter = team.LetterRating[ 0 ].Trim();
            decimal startingNumber = ConvertLetterIntoNumber( startingLetter );
            //
            //  the average of these two numbers becomes a team's starting
            //  number power rating.
            decimal startingNumberPowerRating = ( ( startingNumber + powerNumber )/2.0M );
            if ( startingNumberPowerRating > 21.0M )
               // round down
               startingNumberPowerRating -= 0.5M;
            else
               //round up
               startingNumberPowerRating += 0.5M;
            startingNumberPowerRating = Math.Round( startingNumberPowerRating, 0 );
            team.NumberRating[ 0 ] = startingNumberPowerRating;
            RosterLib.Utility.Announce( string.Format( "{0} starting number is {1:00}, letter is {2,-2}, proj M_wins {3:00.###}",
                                                team.Name, startingNumberPowerRating, startingLetter, projWins ) );
            RosterLib.Utility.Announce( " " );
            //break;  //TODO: remove
         }
         Masters.Sm.IsDirty = true; //  to force re-writing the xml
      }

      private static decimal ProjectWinsFor( NflTeam team, string season )
      {
         //  Using the starting Letter power ratings playy all games
         //  on the schedule, using letter to predict result
         var gameNo = 0;

         var projectedWins = 0.0M;

         var gp = new GordanPredictor();
         if ( team.GameList == null ) team.LoadGames( team.TeamCode, season );
         foreach ( NFLGame g in team.GameList )
         {
            NflTeam opponent;
            gameNo++;
            gp.PredictGame( g, new FakePredictionStorer(), DateTime.Now);
            decimal decimalEquiv;
            string haInd;
            if ( g.IsHome( team.TeamCode ) )
            {
               decimalEquiv = g.HomeDecEquivalent;
               haInd = "host";
               opponent = g.AwayNflTeam;
            }
            else
            {
               decimalEquiv = g.AwayDecEquivalent;
               haInd = "at";
               opponent = g.HomeNflTeam;
            }
            projectedWins += decimalEquiv;
            var ratingGap = RatingGap( team.LetterRating[ 0 ].Trim(), opponent.LetterRating[ 0 ].Trim() );
         	var nLine = g.IsHome( team.TeamCode ) ? ratingGap + 2.5M : ratingGap - 2.5M;

         	var line = nLine < 0
         	              	? string.Format( "+{0:0.0}", Math.Abs( nLine ) )
         	              	: string.Format( "-{0:0.0}", Math.Abs( nLine ) );

            var locOpp = string.Format( "{5:00} {0} ({1}) {2} {3} ({4})",
                                           team.Nick(), team.LetterRating[ 0 ], haInd, opponent.Nick(),
                                           opponent.LetterRating[ 0 ], gameNo );
            var projLine = string.Format( "{0} {1}", team.Nick(), line );
            var gap = string.Format( "{0}", ratingGap );
            Utility.Announce( string.Format( "{0,-40}  {1,7}      {2,-15}   {3:#.000}",
                                                locOpp, gap, projLine, decimalEquiv ) );
         }
         Utility.Announce( string.Format( "{0,76:0.00} wins", projectedWins ) );

         return projectedWins;
      }

      #endregion

      #region  Emotional Pointers

      //  After week 4 the emotional pointers come into play
      private static string Emoticons( NflTeam t, NflTeam opp, NFLGame g )
      {
         string emos = "";
         if ( WantsRevenge( t, opp, g.GameDate ) ) emos += "R";
         if ( TeamEmbarrased( t, g.GameDate ) ) emos += "H";
         if ( HomeDogBounce( t, g ) ) emos += "B";
         if ( TeamSandwiched( t, g ) ) emos += "S";
         if ( OutOfDivisionFavourites( t, g ) ) emos += "o";
         if ( MondayNightFollowUp( t, g ) ) emos += "M";
         if ( LargePointSwing( t, g ) ) emos += "L";
         if ( SundayOrMondayNightUnderdog( t, g ) ) emos += "D";
         if ( SundayOrMondayNightRout( t, g ) ) emos += "G";
         if ( BigScoringTeam( t, g ) ) emos += "9";
         if ( PlayoffPotential( t ) ) emos += "P";
         if ( SpecialRivalries( t, g ) ) emos += "&";
         if ( DueUp( t, g ) ) emos += "^";
         if ( DueDown( t, g ) ) emos += "v";
         if (UnderratedDog(t, g)) emos += "*";
         if (LastGamePhonyWin(t, g)) emos += "p";
         if (LastGamePhonyLoss(t, g)) emos += "l";
         
         emos += FollowupPerformance( t, g );

         return emos;
      }

      /// <summary>
      /// Segment I (Weeks 1-4)
      /// Pointer One:
      ///   Underrated Dog
      ///     Bet on an Underdog when it is underrated in the spread by 4 or more.
      ///     This is tied to the power ratings.  
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool UnderratedDog( NflTeam team, NFLGame upcomingGame )
      {
         bool isUnderratedDog = false;
         if ( upcomingGame.IsDog( team ) )
         {
            decimal gl = GordanLine( upcomingGame );
            if (PointsDiff( upcomingGame.Spread, gl ) >= 4.0M)
            {
               isUnderratedDog = true;
            }
         }
         return isUnderratedDog;
      }

      //  the magic number
      internal static decimal GordanLine(NFLGame g)
      {
         var gp = new GordanWeeklyPredictor();
         gp.PredictGame( g, new FakePredictionStorer(), DateTime.Now);
         return g.GordanLine;
      }
      
      internal static decimal PointsDiff(decimal spread, decimal gline)
      {
         //  -11  5  = 16 (-16)
         //  -11 -5  =  6 (+ 6)
         //   11  5  =  6
         //   11 -5  = 16
         return spread - gline;
      }

      /// <summary>
      /// Segment I (weeks 1-4)
      /// Pointer Ten: (p 67)
      ///   Due-Ups
      ///     When a team first gets 5 points or more above its starting number power rating, 
      ///     it becomes due down.  I look to bet against such a team.  When a team first gets 
      ///     5 points or more below its starting number power rating, it becomes a team that 
      ///     is due-up, and one I look to bet.
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool DueUp( NflTeam team, NFLGame upcomingGame )
      {
         bool isDueUp = false;
         if ( team.DueUpWeek().Equals( upcomingGame.Week ) )
            isDueUp = true;
         return isDueUp;
      }

      public static bool StartDueUp(NflTeam team, NFLGame upcomingGame)
      {
         bool isDueUp = false;
         if (team.StarDueUpWeek().Equals(upcomingGame.Week))
            isDueUp = true;
         return isDueUp;
      }
      
      /// <summary>
      /// Segment I (weeks 1-4)
      /// Pointer Ten: (p 67)
      ///   Due-Ups
      ///     When a team first gets 5 points or more above its starting number power rating, 
      ///     it becomes due down.  I look to bet against such a team.  When a team first gets 
      ///     5 points or more below its starting number power rating, it becomes a team that 
      ///     is due-up, and one I look to bet.
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool DueDown( NflTeam team, NFLGame upcomingGame )
      {
         bool isDueDown = false;
         if ( team.DueDownWeek().Equals( upcomingGame.Week ) )
            isDueDown = true;
         return isDueDown;
      }
      
       public static bool StarDueDown(NflTeam team, NFLGame upcomingGame)
      {
         bool isDueDown = false;
         if (team.StarDueDownWeek().Equals(upcomingGame.Week))
            isDueDown = true;
         return isDueDown;
      }

      /// <summary>
      /// Segment I (Weeks 1-4)
      /// Pointer Two: (p 63, 73)
      ///   Home Dog After Home Loss as Favourite.
      ///   When a team loses as a favourite at home and is the home dog next week, 
      ///   look to bet on the underdog 
      /// </summary>
      public static bool HomeDogBounce( NflTeam team, NFLGame nextGame )
      {
         bool bounce = false;
         NFLGame lastGame = team.PreviousGame( nextGame.GameDate );
         if ( lastGame.IsFavourite( team ) )
            if ( lastGame.Lost( team ) )
            {
               if ( nextGame.IsDog( team ) )
                  if ( nextGame.IsHome( team.TeamCode ) )
                     bounce = true;
            }
         return bounce;
      }

      /// <summary>
      /// Segment I (Weeks 1-4)
      /// Pointer Eleven: (p 70)
      ///   Sandwich Game
      ///   I look to wager against a team ina  divisional series "sandwich game".
      ///   A sandwich game is a non-divisional game that takes placewhen a team has just played 
      ///   at least two straight divisional games and has at least 2 more straight divisional
      ///   games coming up after the non divisional contest.
      /// </summary>
      public static bool TeamSandwiched( NflTeam t, NFLGame nextGame )
      {
         bool sandwichGame = false;
         if ( ! nextGame.IsDivisionalGame() )
         {
            NFLGame lastGame = t.PreviousGame( nextGame.GameDate );
            if ( lastGame.IsDivisionalGame() )
            {
               NFLGame lastLastGame = t.PreviousGame( lastGame.GameDate );
               if ( lastLastGame.IsDivisionalGame() )
               {
                  //  now what about the next game?
                  NFLGame nextNextGame = t.NextGame( nextGame.GameDate );
                  if (nextNextGame != null)
                  {
                     if (nextNextGame.IsDivisionalGame())
                     {
                        // finally another one
                        NFLGame nextNextNextgame = t.NextGame(nextNextGame.GameDate);
                        if (nextNextNextgame.IsDivisionalGame())
                           sandwichGame = true;
                     }
                  }
               }
            }
         }
         return sandwichGame;
      }
      
      /// <summary>
      /// Segment I (Weeks 1-4)
      /// Pointer Twelve:  (p 71)
      ///   PhonyWins and Phony Losses.
      ///     I look to bet against a team coming off a phony win and on a team coming
      ///     off a phony loss.
      ///     In most games the team with the higher-average-gain per pass wins.  If a
      ///     team wins a game having the lower average and the losing team committed more 
      ///     turnovers, the team that won has gained a phony win.
      /// </summary>
      /// <returns></returns>
      public static bool LastGamePhonyWin(NflTeam team, NFLGame upcomingGame)
      {
         bool isLastGamePhonyWin = false;
         NFLGame prevGame = team.PreviousGame( upcomingGame.GameDate );
 
         if ( prevGame.WasPhonyWin() )
         {
            if ( prevGame.Won( team ) )
            {
               isLastGamePhonyWin = true;
            }
         }
         return isLastGamePhonyWin;
      }
      internal static bool LastGamePhonyLoss(NflTeam team, NFLGame upcomingGame)
      {
         bool isLastGamePhonyLoss = false;
         NFLGame prevGame = team.PreviousGame(upcomingGame.GameDate);

         if (prevGame.WasPhonyWin())
         {
            if (prevGame.Lost(team))
            {
               isLastGamePhonyLoss = true;
            }
         }
         return isLastGamePhonyLoss;
      }

      /// <summary>
      /// Segment II (Weeks 5-8)
      /// Pointer: One (p 72)
      ///   In revenge games, a team is trying to avenge a loss to an opponent.  This loss must
      ///   have taken place in the last year.
      ///   Note:  does not have to be the last time they played - any loss will qualify for 
      ///          revenge.
      /// </summary>
      /// <param name="t"></param>
      /// <param name="opposingTeam"></param>
      /// <param name="when"></param>/// 
      /// <returns></returns>      
      public static bool WantsRevenge( NflTeam t, NflTeam opposingTeam, DateTime when )
      {
      	var aYear = new TimeSpan( 365, 0, 0, 0 );
         //  get all the games betwen these 2 in the last year
         var ds = Utility.TflWs.GetGamesBetween( t.TeamCode, opposingTeam.TeamCode, when.Subtract( aYear ) );
         var dt = ds.Tables[ "SCHED" ];
         dt.DefaultView.Sort = "GAMEDATE ASC";
      	return (from DataRow dr in dt.Rows 
					     where dr.RowState != DataRowState.Deleted 
					        select new NFLGame(dr)).Any( g => g.Lost(t) );
      }

      /// <summary>
      /// Segment II (Weeks 5-8)
      /// Pointer Two A: (p 72)
      ///   Embarrasing Performances.  I look to wager on a team that has _just_ suffered a humiliating loss
      ///   either by a big score or to a bad club.  I also look to bet a team that has very poor offensive 
      ///   or defensive numbers in their last game, or to wager against a team that has just put up 
      ///   outstanding offensive or defensive numbers.
      /// </summary>
      /// <param name="t"></param>
      /// <param name="when"></param>
      /// <returns></returns>
      public static bool TeamEmbarrased( NflTeam t, DateTime when )
      {
         //  get their previous game
         NFLGame prevGame = t.PreviousGame( when );
         if ( prevGame != null )
            if ( prevGame.WasRout() )
               if ( prevGame.Lost( t ) )
                  return true;

         return false;
      }

      /// <summary>
      /// Segment II (Weeks 5-8)
      /// Pointer: Six (p 73)
      ///   Out-of-Division Favourites.
      ///     As in games one through four, if a team has played 2 or more consecutive divisional games
      ///     and is now playing a nondivisional game, I look to wager against the team if they are 
      ///     favoured by 7 or more.
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool OutOfDivisionFavourites( NflTeam team, NFLGame upcomingGame )
      {
         var isOutOfDivisionFavourite = false;
         if ( ! upcomingGame.IsDivisionalGame() )
         {
            if ( upcomingGame.IsFavourite( team ) )
            {
               if ( Math.Abs( upcomingGame.Spread ) >= 7 )
               {
                  var lastGame = team.PreviousGame( upcomingGame.GameDate );
                  if ( lastGame.IsDivisionalGame() )
                  {
                     var previousToLastGame = team.PreviousGame( lastGame.GameDate );
                  	if (previousToLastGame.IsDivisionalGame())
                  		isOutOfDivisionFavourite = true;
                  }
               }
            }
         }
         return isOutOfDivisionFavourite;
      }

      /// <summary>
      /// Segment II ( Weeks 5-8)
      /// Pointer Seven: (p73)
      ///   Follow-up Performances.
      ///     If a team is strong against the spread after an outright loss (I check back one year on this)
      ///     I will look to bet them after a loss.  If a team plays poorly after a win, I will look to bet 
      ///     against them after a win.  Getting to know each team's personality is important using 
      ///     this model.
      ///     Interpretation:  Team is strong if they cover the spread >= 60%, week if they lose to the
      ///                      spread >= 60%.
      /// 
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static string FollowupPerformance( NflTeam team, NFLGame upcomingGame )
      {
         string followupCode = "";
         decimal spreadRecord;
         NFLGame previousGame = team.PreviousGame( upcomingGame.GameDate );
         if ( previousGame.Won( team ) )
         {
            spreadRecord = team.SpreadRecordAfterWin( upcomingGame.GameDate );
            if ( spreadRecord >= KStrongAgainstSpread )
               followupCode = "s";
            else if ( spreadRecord <= KWeekAgainstSpread )
               followupCode = "w";
         }
         else
         {
            spreadRecord = team.SpreadRecordAfterLoss( upcomingGame.GameDate );
            if ( spreadRecord >= KStrongAgainstSpread )
               followupCode = "w";
            else if ( spreadRecord <= KWeekAgainstSpread )
               followupCode = "s";
         }
         return followupCode;
      }

      /// <summary>
      /// Segment II ( Weeks 5-8)
      /// Pointer Eight: (p73)
      ///   Monday Night Followup.
      ///     As in the first four games, I look to bet
      ///     against a team that has won and covered a Monday night game against a 
      ///     divisional opponent, and who is now playing a non divisional team.
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool MondayNightFollowUp( NflTeam team, NFLGame upcomingGame )
      {
         var isMnfFollowup = false;
         if ( ! upcomingGame.IsDivisionalGame() )
         {
            var previousGame = team.PreviousGame( upcomingGame.GameDate );
            if ( previousGame.IsMondayNight() )
            {
               if ( previousGame.IsDivisionalGame() )
                  if ( previousGame.WonVsSpread( team ) )
                     isMnfFollowup = true;
            }
         }
         return isMnfFollowup;
      }

      /// <summary>
      /// Segment II (Weeks 5-8)
      /// Pointer Nine: (p73-4)
      ///   Large Point Swings.
      ///     As in the first four games, I use the 50-point swing model.  In other words,
      ///     if Team A's margin of win and Team B's margin of loss the previous week add up
      ///     to 50 points or more, I am looking to bet Team B this week.
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool LargePointSwing( NflTeam team, NFLGame upcomingGame )
      {
         bool largePointSwing = false;
         NFLGame prevGame = team.PreviousGame( upcomingGame.GameDate );
         if ( prevGame.Lost( team ) )
         {
            NflTeam opponent = upcomingGame.OpponentTeam( team.TeamCode );
            NFLGame prevOpponentGame = opponent.PreviousGame( upcomingGame.GameDate );
            if ( prevOpponentGame.Won( opponent ) )
            {
               int margin = prevGame.MarginOfVictory() + prevOpponentGame.MarginOfVictory();
               if ( margin >= KLargePointSwingMargin )
                  largePointSwing = true;
            }
         }

         return largePointSwing;
      }

      /// <summary>
      /// Segment II (Week 5-8)
      /// Pointer Ten: (p 74)
      ///   Sunday or Monday Night Home Underdogs.
      ///     As in the first four games, I look to bet Sunday or Monday night home underdogs
      ///     when the power ratings favour them.
      /// </summary>
      /// <returns></returns>
      public static bool SundayOrMondayNightUnderdog( NflTeam team, NFLGame upcomingGame )
      {
         bool isSundayOrMondayNightUnderdog = false;

         if ( upcomingGame.IsPrimeTime() )
         {
            if ( upcomingGame.Dog().Equals( team.TeamCode ) )
               if ( upcomingGame.HomeDog() )
                  isSundayOrMondayNightUnderdog = true;
         }
         return isSundayOrMondayNightUnderdog;
      }

      /// <summary>
      /// Segment II (Weeks 5-8)
      /// Pointer Eleven: (p 74)
      ///   Under-the-Lights followup.
      ///     As in the first four games, I look to wager on teams that lost a rout on Sunday or 
      ///     Monday night and look to wager against teams that won a rout under the night
      ///     lights of national television.
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool SundayOrMondayNightRout( NflTeam team, NFLGame upcomingGame )
      {
         bool isSundayOrMondayNightRout = false;
         NFLGame prevGame = team.PreviousGame( upcomingGame.GameDate );

         if ( prevGame.IsPrimeTime() )
         {
            if ( prevGame.WasRout() )
               if ( prevGame.Lost( team ) )
                  isSundayOrMondayNightRout = true;
         }
         return isSundayOrMondayNightRout;
      }

      /// <summary>
      /// Segment II (weeks 5-8)
      /// Pointer Twelve: (p 74)
      ///   Big Scoring Teams
      ///     As in the first 4 games, I look to bet against a favourite that scored a total of
      ///     30 or more points in each of the last two weeks in winning efforts.
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool BigScoringTeam( NflTeam team, NFLGame upcomingGame )
      {
         bool isBigScorer = false;
         if ( upcomingGame.IsFavourite( team ) )
         {
            NFLGame prevGame = team.PreviousGame( upcomingGame.GameDate );
            if ( prevGame.Won( team ) )
               if ( prevGame.ScoreFor( team ) >= KBigScore )
               {
                  NFLGame prevPrevGame = team.PreviousGame( prevGame.GameDate );
                  if ( prevGame.Won( team ) )
                  {
                     if ( prevPrevGame.ScoreFor( team ) >= KBigScore )
                        isBigScorer = true;
                  }
               }
         }
         return isBigScorer;
      }

      /// <summary>
      /// Segment II 
      /// Pointer Thirteen: (p 74)
      ///   Playoff Potential
      ///     I look to wager on must-win underdogs if they are usually in contention teams.
      ///     What I mean by a must-win underdog is a team that would be virtually out of the 
      ///     playoff picture or placed in a very disadvatageous spot concerning the playoff
      ///     race with a loss.  And again, the team must be one that usually makes the playoffs
      ///     and thus one for which a playoff absence would be a huge failure for the franchise.
      ///     Note:  quick way to work out loss of playoff contention is if a loss would drop you
      ///            to 500 and we are already eight weeks on then you are then "on wood".
      /// </summary>
      /// <param name="team"></param>
      /// <returns></returns>
      public static bool PlayoffPotential( NflTeam team )
      {
         bool isPlayoffPotential = false;
         decimal currentClip = Utility.Clip( team.Wins, team.Losses++, team.MTies );
         if ( ( currentClip <= KClipOfSomeChanceStill ) && ( currentClip >= KClipOfNoReturn ) )

            if ( team.GamesPlayed() > 8 )
            {
               if ( PerenialPlayoffContender( team ) )
               {
                  isPlayoffPotential = true;
               }
            }

         return isPlayoffPotential;
      }

      /// <summary>
      /// Segment II
      /// Pointer 14: (p 74)
      ///   Special Rivalries
      ///     I look to wager on an out-of-contention underdog in a nationally telecast game
      ///     if it is one in which they are playing a longtime rival.  These are games in which 
      ///     out-of-contention teams look to get very fired up.
      /// 
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool SpecialRivalries( NflTeam team, NFLGame upcomingGame )
      {
         bool isSpecialRivalry = false;
         if ( upcomingGame.IsOnTv )
         {
            if ( team.IsRival( upcomingGame.Opponent( team.TeamCode ) ) )
            {
               if ( upcomingGame.IsDog( team ) )
               {
                  if ( team.IsOutOfContention() )
                     isSpecialRivalry = true;
               }
            }
         }
         return isSpecialRivalry;
      }

      /// <summary>
      /// Segment II
      /// Pointer 15: (p 74)
      ///   Big Game Ahead
      ///     If a team has a big game coming up the next week and might be overlooking the 
      ///     present game, I will be considering a bet against them.
      /// </summary>
      /// <param name="team"></param>
      /// <param name="upcomingGame"></param>
      /// <returns></returns>
      public static bool BigGameAhead( NflTeam team, NFLGame upcomingGame )
      {
      	const bool isBigGameAhead = false;
      	return isBigGameAhead;
      }

   	/// <summary>
      /// Segment II
      /// Pointer 17: (p 75)
      ///   Phoney Win Follow-ups
      ///     As in games one through four, when a team is a favourite after a phony win
      ///     I look to wager against them.
      /// </summary>
      /// <returns></returns>
      public static bool PhonyWinFollowup()
   	{
   		const bool isPhonyWinFollowup = false;
   		return isPhonyWinFollowup;
   	}

   	/// <summary>
      /// Segment II
      /// Pointer 17: (p 75)
      ///   Phoney Loss Follow-ups
      ///     As in games one through four, if a team is an underdog after a phony loss, 
      ///     I look to wager on them.
      /// </summary>
      /// <returns></returns>
      public static bool PhonyLossFollowup()
   	{
   		const bool isPhonyLossFollowup = false;
   		return isPhonyLossFollowup;
   	}

   	#endregion

      public static bool PerenialPlayoffContender(NflTeam team)
      {
         bool isContender = false;
         //  These franchises always expect to be in the playoffs
         if ( team.TeamCode.Equals( "IC" ) )
         {
            isContender = true;
         }
         else if ( team.TeamCode.Equals( "PS" ) )
         {
            isContender = true;
         }
         else if ( team.TeamCode.Equals( "SS" ) )
         {
            isContender = true;
         }
         else if ( team.TeamCode.Equals( "NE" ) )
         {
            isContender = true;
         }
         else if ( team.TeamCode.Equals( "PS" ) )
         {
            isContender = true;
         }
         else if ( team.TeamCode.Equals( "DB" ) )
         {
            isContender = true;
         }
         else if ( team.TeamCode.Equals( "PE" ) )
         {
            isContender = true;
         }
         return isContender;
      }

      public static decimal ConvertWinsToProjectedWinNumber(decimal projWins)
      {
         //  from page 52
         decimal powerNumber = 0.0M;
         if ( projWins < 2.0M )
            powerNumber = 4.0M;
         else if ( projWins < 3.0M )
            powerNumber = 8.0M;
         else if ( projWins < 4.0M )
            powerNumber = 10.5M;
         else if ( projWins < 5.0M )
            powerNumber = 13.0M;
         else if ( projWins < 6.0M )
            powerNumber = 15.5M;
         else if ( projWins < 7.0M )
            powerNumber = 18.0M;
         else if ( projWins < 8.0M )
            powerNumber = 19.5M;
         else if ( projWins < 9.0M )
            powerNumber = 21.0M;
         else if ( projWins < 10.0M )
            powerNumber = 22.5M;
         else if ( projWins < 11.0M )
            powerNumber = 24.0M;
         else if ( projWins < 12.0M )
            powerNumber = 26.5M;
         else if ( projWins < 13.0M )
            powerNumber = 29.0M;
         else if ( projWins < 14.0M )
            powerNumber = 31.5M;
         else if ( projWins < 15.0M )
            powerNumber = 34.0M;

         return powerNumber;
      }

      public static decimal ConvertLetterIntoNumber(string letter)
      {
         decimal number = 0.0M;
         //  from chart page 53
         if ( letter.Equals( "E-" ) )
            number = 4.0M;
         else if ( letter.Equals( "E" ) )
            number = 8.0M;
         else if ( letter.Equals( "E+" ) )
            number = 10.5M;
         else if ( letter.Equals( "D-" ) )
            number = 13.0M;
         else if ( letter.Equals( "D" ) )
            number = 15.5M;
         else if ( letter.Equals( "D+" ) )
            number = 18.0M;
         else if ( letter.Equals( "C-" ) )
            number = 19.5M;
         else if ( letter.Equals( "C" ) )
            number = 21.0M;
         else if ( letter.Equals( "C+" ) )
            number = 22.5M;
         else if ( letter.Equals( "B-" ) )
            number = 24.0M;
         else if ( letter.Equals( "B" ) )
            number = 26.5M;
         else if ( letter.Equals( "B+" ) )
            number = 29.0M;
         else if ( letter.Equals( "A-" ) )
            number = 31.5M;
         else if ( letter.Equals( "A" ) )
            number = 34.0M;
         else if ( letter.Equals( "A" ) )
            number = 38.0M;
         return number;
      }

      //private string ConvertProjectedWinNumberToLetter(decimal powerNumber)
      //{
      //   string letter = "?";
      //   //  from chart page 53
      //   if (powerNumber.Equals( 4.0M ) )
      //      letter = "E-";
      //   else if (powerNumber.Equals( 8.0M ) )
      //      letter = "E";
      //   else if (powerNumber.Equals( 10.5M ) )
      //      letter = "E+";
      //   else if (powerNumber.Equals( 13.0M ) )
      //      letter = "D-";
      //   else if (powerNumber.Equals( 15.5M ) )
      //      letter = "D";
      //   else if (powerNumber.Equals( 18.0M ) )
      //      letter = "D+";
      //   else if (powerNumber.Equals( 19.5M ) )
      //      letter = "C-";
      //   else if (powerNumber.Equals( 21.0M ) )
      //      letter = "C";
      //   else if (powerNumber.Equals( 22.5M ) )
      //      letter = "C+";
      //   else if (powerNumber.Equals( 24.0M ) )
      //      letter = "B-";
      //   else if (powerNumber.Equals( 26.5M ) )
      //      letter = "B";
      //   else if (powerNumber.Equals( 29.0M ) )
      //      letter = "B+";
      //   else if (powerNumber.Equals( 31.5M ) )
      //      letter = "A-";
      //   else if (powerNumber.Equals( 34.0M ) )
      //      letter = "A";
      //   else if (powerNumber.Equals( 38.0M ) )
      //      letter = "A";
      //   return letter;
      //}

      public static decimal RatingGap( string homeLetter, string awayLetter )
      {
         //  the rating gap is the difference between 2 letter ratings
         decimal homeNumber = ConvertLetterIntoNumber( homeLetter );
         decimal awayNumber = ConvertLetterIntoNumber( awayLetter );
         return homeNumber - awayNumber;
      }

      public static decimal CalculateSpreadsAsDecimals(decimal ratingGap)
      {
         //  from table on page 51
         decimal absVal = Math.Abs( ratingGap );
         decimal decEquivalent;
         if ( absVal < 1.5M )
            decEquivalent = .507M;
         else if ( absVal < 2.0M )
            decEquivalent = .512M;
         else if ( absVal < 2.5M )
            decEquivalent = .519M;
         else if ( absVal < 3.0M )
            decEquivalent = .524M;
         else if ( absVal < 3.5M )
            decEquivalent = .600M;
         else if ( absVal < 4.0M )
            decEquivalent = .615M;
         else if ( absVal < 4.5M )
            decEquivalent = .630M;
         else if ( absVal < 5.0M )
            decEquivalent = .643M;
         else if ( absVal < 5.5M )
            decEquivalent = .655M;
         else if ( absVal < 6.0M )
            decEquivalent = .667M;
         else if ( absVal < 6.5M )
            decEquivalent = .688M;
         else if ( absVal < 7.0M )
            decEquivalent = .706M;
         else if ( absVal < 7.5M )
            decEquivalent = .722M;
         else if ( absVal < 8.0M )
            decEquivalent = .737M;
         else if ( absVal < 8.5M )
            decEquivalent = .750M;
         else if ( absVal < 9.0M )
            decEquivalent = .762M;
         else if ( absVal < 9.5M )
            decEquivalent = .773M;
         else if ( absVal < 10.0M )
            decEquivalent = .783M;
         else if ( absVal < 10.5M )
            decEquivalent = .792M;
         else if ( absVal < 11.0M )
            decEquivalent = .800M;
         else if ( absVal < 11.5M )
            decEquivalent = .810M;
         else if ( absVal < 12.0M )
            decEquivalent = .818M;
         else if ( absVal < 12.5M )
            decEquivalent = .833M;
         else if ( absVal < 13.0M )
            decEquivalent = .846M;
         else if ( absVal < 13.5M )
            decEquivalent = .857M;
         else if ( absVal < 14.0M )
            decEquivalent = .875M;
         else if ( absVal < 14.5M )
            decEquivalent = .889M;
         else if ( absVal < 15.0M )
            decEquivalent = .900M;
         else if ( absVal < 15.5M )
            decEquivalent = .917M;
         else if ( absVal < 16.0M )
            decEquivalent = .933M;
         else if ( absVal < 16.5M )
            decEquivalent = .952M;
         else if ( absVal < 17.0M )
            decEquivalent = .952M;
         else if ( absVal < 17.5M )
            decEquivalent = .962M;
         else
            decEquivalent = .962M;

         return decEquivalent;
      }

      public static decimal SpreadRangeScore(int marginOfVictory, decimal expectedMargin)
      {
      	decimal spreadRangeScore;
         var coverMargin = Convert.ToDecimal( marginOfVictory ) - expectedMargin;
         var cover = !(coverMargin < 0);
         coverMargin = Math.Abs( coverMargin );

         if ( coverMargin < 3.5M )
            spreadRangeScore = 1.0M;
         else if ( coverMargin < 6.5M )
            spreadRangeScore = 2.0M;
         else if ( coverMargin < 9.5M )
            spreadRangeScore = 3.0M;
         else if ( coverMargin < 12.5M )
            spreadRangeScore = 4.0M;
         else if ( coverMargin < 15.5M )
            spreadRangeScore = 5.0M;
         else
            spreadRangeScore = 6.0M;

         if ( !cover ) spreadRangeScore *= -1;

         return spreadRangeScore;
      }

   }
}