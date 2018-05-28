using System;

namespace RosterLib
{
   /// <summary>
   /// Applys the GS4 scoring rules to a player.
   /// </summary>
   public class SimpleDefencePredictor : IPredictDefensiveTeams
   {

      public Int32 PredictSacks( NflTeam team, string season, int week )
      {
         //  Predict the number of Sacks the team will make
         int sacks = 0;
         //  who are the opponents
         NflTeam opponent = team.OpponentFor( season, week );
         if (opponent != null)
         {
            //  not on a bye
            sacks += 1;
            int prSacks = ConvertRating( team.PrRating() );
            int ppSacks = ConvertRating( opponent.PpRating() );
            sacks += ( prSacks - ppSacks );
         }
         
         //  What is the Game
         NFLGame game = team.GameFor(season, week);
         if (game != null)
         {
            if (game.IsHome(team.TeamCode)) sacks += 1;
            if (game.IsBadWeather()) sacks += 1;
         }

         if (sacks < 0) sacks = 0;
         
         return sacks;
      }

      public int PredictSteals( NflTeam team, string season, int week )
      {
         //  Predict the number of Interceptions the team will make
         int ints = 0;
         //  who are the opponents
         NflTeam opponent = team.OpponentFor(season, week);
         if (opponent != null)
         {
            //  not on a bye
            int pdInts = ConvertRating(team.PrRating());
            int poInts = ConvertRating(opponent.PpRating());
            ints += (pdInts - poInts) - 1; //  will range from 3 to -5
         }

         //  What is the Game
         NFLGame game = team.GameFor(season, week);
         if ( game != null )
            if ( game.IsHome( team.TeamCode ) ) ints += 1;


         if (ints < 0) ints = 0;

         return ints;
      }

      private int ConvertRating(string rating)
      {
         int val;
         switch (rating)
         {
            case "A":
               val = 5;
               break;
            case "B":
               val = 4;
               break;
            case "C":
               val = 4;
               break;
            case "D":
               val = 4;
               break;
            default:
               val = 1;
               break;
         }
         return val;
      }
      
   }
}



