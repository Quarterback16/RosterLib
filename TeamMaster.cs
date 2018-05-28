using System.IO;
using System.Xml;

namespace RosterLib
{
   /// <summary>
   ///   TeamMaster will keep a hash table of Teams so they dont get re-created all the time.
   ///   Key is season + teamcode
   /// </summary>
   public class TeamMaster : XmlCache
   {
      public TeamMaster( string name, string fileName ) : base( name )
      {
         try
         {
            //  load HT from the xml
            Filename = fileName;
            XmlDoc = new XmlDocument();
            XmlDoc.Load(Utility.XmlDirectory() + Filename);
            var listNode = XmlDoc.ChildNodes[ 2 ];
            foreach ( XmlNode node in listNode.ChildNodes )
               AddXmlTeam( node );
         }
         catch ( IOException e )
         {
            Utility.Announce( string.Format( "Unable to open {1} xmlfile - {0}", e.Message, fileName ) );
         }
      }

      private void AddXmlTeam( XmlNode teamNode )
      {
         PutTeam( new NflTeam( teamNode ) );
      }


      private static string Key( NflTeam team )
      {
         return team.Season + team.TeamCode;
      }

      public void PutTeam( NflTeam team )
      {
         if ( ! TheHt.ContainsKey( Key( team ) ) )
            TheHt.Add( Key( team ), team );
      }

      public NflTeam GetTeam( string teamKey )
      {
         var seasonpart = teamKey.Substring( 0, 4 );
         var teampart = teamKey.Substring( 4, 2 );
         return GetTeam( seasonpart, teampart );
      }

      public NflTeam GetTeam( string season, string teamCode )
      {
#if DEBUG
         //Utility.Announce( string.Format( "TeamMaster:Getting Team {0} for {1}", teamCode, season ) );
#endif
         NflTeam team;
         var key = season + teamCode;
         if ( TheHt.ContainsKey( key ) )
         {
            team = (NflTeam) TheHt[ key ];
            team.SetRecord(season, skipPostseason: false);
            CacheHits++;
         }
         else
         {
            //  new it up
#if DEBUG
            //Utility.Announce( string.Format( "TeamMaster:Instantiating Team {0} for {1}", teamCode, season ) );
#endif
            team = new NflTeam( teamCode, season );
            PutTeam( team );
            IsDirty = true;
            CacheMisses++;
         }
         return team;
      }

      public string TeamFor( string teamCode, string season )
      {
         var team = GetTeam( season, teamCode );
         return team.Name;
      }

      #region  Persistence

      public void Dump2Xml()
      {
         if ( ( TheHt.Count > 0 ) && IsDirty )
         {
            var writer = new
               XmlTextWriter(string.Format("{0}{1}", Utility.OutputDirectory(), Filename), null);

            writer.WriteStartDocument();
            writer.WriteComment( "Comments: " + Name );
            writer.WriteStartElement( "team-list" );

            var myEnumerator = TheHt.GetEnumerator();
            while ( myEnumerator.MoveNext() )
            {
               var t = (NflTeam) myEnumerator.Value;
               WriteTeamNode( writer, t );
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            Utility.Announce( Filename + " created" );
         }
      }

      private static void WriteTeamNode( XmlTextWriter writer, NflTeam t )
      {
         writer.WriteStartElement( "team" );
         writer.WriteAttributeString( "season", t.Season );
         writer.WriteAttributeString( "id", t.TeamCode );
         writer.WriteAttributeString( "name", t.Name );
         writer.WriteAttributeString( "division", t.Division() );

         writer.WriteEndElement();
      }

      #endregion
   }
}