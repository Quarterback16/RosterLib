using System.Collections.Generic;

namespace RosterLib
{
   /// <summary>
   ///   Implemented as a Singleton
   /// </summary>
   public class PlayerCache : ICachePlayers
   {
      private static class SingletonHolder
      {
         internal static readonly PlayerCache instance = new PlayerCache();
         
         static SingletonHolder()
         {
            //  empty static constructor forces laziness
         }
      }

      #region  Global State
      public List<NFLPlayer> Players { get; set; }
      #endregion

      private PlayerCache()
      {
         Players = new List<NFLPlayer>();   
      }

      public static PlayerCache Instance
      {
         get
         {
            return SingletonHolder.instance;
         }
      }

      #region  This is the raison d'etre for the class (needs to be thread safe as multiple threads will be hitting it)       

      public NFLPlayer Get( string playerId )
      {
         return Players.Find( p => p.PlayerCode == playerId );
      }

      public void Put( NFLPlayer player )
      {
         if ( ! Players.Contains( player ) )
            Players.Add( player );
      }

      #endregion

   }
}
