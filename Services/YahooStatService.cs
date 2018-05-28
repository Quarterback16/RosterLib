using RosterLib.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RosterLib.Services
{
   public class YahooStatService : IYahooStatService
   {
      public XDocument Xdoc { get; set; }
      public string XmlFile { get; set; }

      public YahooStatService()
      {
         XmlFile = string.Format( "{0}XML\\{1}", Utility.OutputDirectory(), 
            Constants.DefaultFileName.YahooXml);
         Xdoc = XDocument.Load( XmlFile );
      }

      public YahooStatService(string xmlFile)
      {
         XmlFile = string.Format( "{0}XML\\{1}", Utility.OutputDirectory(), xmlFile );
         Xdoc = XDocument.Load( XmlFile );
      }

      public decimal GetStat( string playerId, string season, string week )
      {
         var results = LoadStats( playerId, season, week );

         var theStat = results.FirstOrDefault();
         if ( theStat == null )
            return 0.0M;
         else
            return theStat.Qty;
      }

      public IEnumerable<YahooStat> LoadStats( 
         string playerId, string season, string week )
      {
         return Xdoc.Element( "stat-list" )
            .Elements( "stat" )
            .Where( e => e.Attribute( "id" ).Value == playerId
                       && e.Attribute( "season" ).Value == season
                       && e.Attribute( "week" ).Value == week )
            .Select( r => new YahooStat
            {
               Id = ( string ) r.Attribute( "id" ),
               Season = ( string ) r.Attribute( "season" ),
               Week = ( string ) r.Attribute( "week" ),
               Qty = ( decimal ) r.Attribute( "qty" )
            }
            );
      }

      public bool IsStat( string playerId, string season, string week )
      {
         var results = LoadStats( playerId, season, week );
         return results.Any();
      }
   }

}
