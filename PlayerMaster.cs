using System;
using System.Collections;
using System.Xml;

namespace RosterLib
{
   /// <summary>
   /// Summary description for PlayerMaster a memory cache.
   /// Probably not much value in persisting the data as
   /// roles are too dynamic.
   /// </summary>
   public class PlayerMaster : XmlCache
   {
      #region  Constructors

      public PlayerMaster( string name, string fileName ) : base( name )
      {
         //  The caching is not working well because any role update
         //  will invalidate the XML
         //try
         //{
         //   //  load HT from the xml
         //   Filename = fileName;
         //   XmlDoc = new XmlDocument();
          //   XmlDoc.Load( Utility.OutputDirectory() + Filename);
         //   XmlNode listNode = XmlDoc.ChildNodes[ 2 ];
         //   foreach ( XmlNode node in listNode.ChildNodes )
         //      AddXmlPlayer( node );
         //}
         //catch ( IOException e )
         //{
         //   RosterLib.Utility.Announce( string.Format( "Unable to open {1} xmlfile - {0}", e.Message, fileName ) );
         //}
      }

      #endregion

      #region  Reading

      public NFLPlayer GetPlayer( string playerId )
      {
         NFLPlayer p;
         if ( TheHt.ContainsKey( playerId ) )
         {
            p = (NFLPlayer) TheHt[ playerId ];
            CacheHits++;
#if DEBUG
            //RosterLib.Utility.Announce( "PlayerMaster.GetPlayer: Player " + playerId + " got from cache");
#endif
         }
         else
         {
            //  new it up
#if DEBUG
            //RosterLib.Utility.Announce( string.Format( "PlayerMaster.GetPlayer: Player {0} not in cache", playerId ));
#endif
            p = new NFLPlayer( playerId );
            PutPlayer( p );
            CacheMisses++;
         }
         return p;
      }

      #endregion

      public decimal InjuryRatio()
      {
         var players = 0;
         var injuries = 0;

         var myEnumerator = TheHt.GetEnumerator();

         while ( myEnumerator.MoveNext() )
         {
            var p = (NFLPlayer) myEnumerator.Value;
            if ( p.IsActive() )
               players++;
            if ( p.IsInjured() )
               injuries++;
         }

         decimal ratio = Ratio( injuries, players );
         RosterLib.Utility.Announce( string.Format( "Injury Ratio:- Injuries {0} players {1} Ratio {2:###.#}%",
                                             injuries, players, ratio*100.0M ) );
         return ratio;
      }

      private static decimal Ratio( int i, int p )
      {
         return Convert.ToDecimal( i )/Convert.ToDecimal( p );
      }

      #region  Writing

      public void PutPlayer( NFLPlayer p )
      {
         if ( p.PlayerCode != null )
         {
            if ( ! TheHt.ContainsKey( p.PlayerCode ) )
            {
               TheHt.Add( p.PlayerCode, p );
               IsDirty = true;
            }
         }
      }

      private void AddXmlPlayer( XmlNode playerNode )
      {
         AddPlayer( new NFLPlayer( playerNode ) );
      }

      public void AddPlayer( NFLPlayer p )
      {
         TheHt.Add( p.PlayerCode, p );
      }

      #endregion

      #region  Persistence

      public void Dump2Xml()
      {
         if ( ( TheHt.Count > 0 ) && IsDirty )
         {
            XmlTextWriter writer = new
               XmlTextWriter(string.Format("{0}{1}", Utility.OutputDirectory(), Filename), null);

            writer.WriteStartDocument();
            writer.WriteComment( "Comments: " + Name );
            writer.WriteStartElement( "player-list" );

            IDictionaryEnumerator myEnumerator = TheHt.GetEnumerator();
            while ( myEnumerator.MoveNext() )
            {
               NFLPlayer p = (NFLPlayer) myEnumerator.Value;
               if ( p.PlayerCode.Length > 6 ) WritePlayerNode( writer, p );
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            RosterLib.Utility.Announce( string.Format( "   {0} created", Filename ) );
         }
      }

      private static void WritePlayerNode( XmlWriter writer, NFLPlayer p )
      {
         writer.WriteStartElement( "player" );
         writer.WriteAttributeString( "id", p.PlayerCode );
         writer.WriteAttributeString( "name", p.PlayerName );
         writer.WriteAttributeString( "rookie-year", p.RookieYear );
         writer.WriteAttributeString( "pos", p.PlayerPos );
         writer.WriteAttributeString( "role", p.PlayerRole );
         writer.WriteAttributeString( "star", p.StarRating );
         writer.WriteAttributeString( "cat", p.PlayerCat );
         writer.WriteAttributeString( "bio", p.Bio );
         writer.WriteAttributeString( "jersey", p.JerseyNo );
         writer.WriteAttributeString( "dob", p.DBirth );
         writer.WriteAttributeString( "owner", p.Owner );
         writer.WriteAttributeString( "currteam", p.CurrTeam.TeamCode );
         writer.WriteAttributeString( "Scores", p.Scores.ToString() );
         writer.WriteAttributeString( "currscores", p.CurrScores.ToString() );
         writer.WriteEndElement();
      }

      #endregion

      #region  Logging

      public void DumpSeasons()
      {
         IDictionaryEnumerator myEnumerator = TheHt.GetEnumerator();
         while ( myEnumerator.MoveNext() )
         {
            NflSeason s = (NflSeason)myEnumerator.Value;
            RosterLib.Utility.Announce( string.Format( "Season {0}:- ", myEnumerator.Key ) );
            s.DumpTeams();
         }
      }

      #endregion
   }
}