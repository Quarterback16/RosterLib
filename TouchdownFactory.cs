using RosterLib.Models;
using System;
using System.Data;

namespace RosterLib
{
   public static class TouchdownFactory
   {
      public static Touchdown FromDr( DataRow dr )
      {
         var scoreCode = dr[ "SCORE" ].ToString();
         if ( ! ScoreIsATouchdown( scoreCode ) )
            return null;

         var gameKey = string.Format( "{0}:{1}-{2}", dr[ "SEASON" ], dr[ "WEEK" ], dr[ "GAMENO" ] );
         var td = new Touchdown
         {
            Action = dr["SCORE"].ToString(),
            Scorer = new NFLPlayer( dr["PLAYERID1"].ToString() ),
            Distance = Int32.Parse( dr["DISTANCE"].ToString() ),
            Game = new NFLGame( gameKey ),
            ForTeamCode = dr["TEAM"].ToString()
         };

         if ( !string.IsNullOrEmpty( dr[ "PLAYERID2" ].ToString().Trim() ) )
            td.Assisting = new NFLPlayer( dr[ "PLAYERID2" ].ToString() );

         td.AgainstTeamCode = td.Game.Opponent( td.ForTeamCode );
         return td;
      }

      public static bool ScoreIsATouchdown( string scoreCode )
      {
         var isTouchdown = false;

         if ( scoreCode == Constants.K_SCORE_FUMBLE_RETURN ||
              scoreCode == Constants.K_SCORE_INTERCEPT_RETURN ||
              scoreCode == Constants.K_SCORE_KICK_RETURN ||
              scoreCode == Constants.K_SCORE_PUNT_RETURN ||
              scoreCode == Constants.K_SCORE_TD_PASS ||
              scoreCode == Constants.K_SCORE_TD_RUN
            )
            isTouchdown = true;
         return isTouchdown;
      }
   }
}
