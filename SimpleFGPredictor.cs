using System;

namespace RosterLib
{
   /// <summary>
   /// Applys the GS4 scoring rules to a player.
   /// </summary>
   public class SimpleFGPredictor : IPredictFGs
   {

      public Int32 PredictFGs(NFLPlayer plyr, string season, int week )
      {
         //  Predict the number of FGs this player will kick
         int fg = 0;
         //  starters only
         if ( plyr.IsStarter() )
         {
            //  who does he play for
            NflTeam kickersTeam = plyr.CurrTeam;
            
            //  who are the opponents
            NflTeam opponent = kickersTeam.OpponentFor( season, week );
            if (opponent != null)
            {
               //  not on a bye
               fg += 1;
               if (opponent.IsGoodDefence()) fg += 1;
            }
            
            //  What is the Game
            NFLGame game = kickersTeam.GameFor(season, week);
            if (game != null)
            {
               if (game.IsHome(kickersTeam.TeamCode)) fg += 1;
               if (game.IsDomeGame()) fg += 1;
               if (game.IsBadWeather()) fg -= 1;
            }

         }
         return fg;
      }

   }
}


