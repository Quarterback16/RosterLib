using System.Collections;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace RosterLib
{
	public class XmlCache : ICache
	{
      public Logger Logger { get; set; }

      protected XPathDocument EpXmlDoc;
		protected XPathNavigator Nav;

		public XmlCache( string entityName )
		{
         Logger = NLog.LogManager.GetCurrentClassLogger();

         CacheMisses = 0;
			CacheHits = 0;
			IsDirty = false;
			Logger.Trace(string.Format("XmlCache.Init Constructing {0} master", entityName ) );
			Name = entityName;
			TheHt = new Hashtable();
		}

		public int CacheHits { get; set; }

		public int CacheMisses { get; set; }

		public bool IsDirty { get; set; }

		public XmlDocument XmlDoc { get; set; }

		public Hashtable TheHt { get; set; }

		public string Filename { get; set; }

		public string Name { get; set; }

		public virtual decimal GetStat( string theKey )
		{
			return 0.0M;
		}

		public string StatsMessage()
		{
			 return string.Format( "{2} Cache hits {0} misses {1}", CacheHits, CacheMisses, Name );
		}
		
		public static void WriteElement( XmlTextWriter writer, string name, string text )
		{
			writer.WriteStartElement( name );
			writer.WriteString( text);
			writer.WriteEndElement();				
		}
		
		public void DumpHt()
		{
			var myEnumerator = TheHt.GetEnumerator();
			var i = 0;
			Logger.Trace( "\t-INDEX-\t-KEY-\t-VALUE-" );
			while ( myEnumerator.MoveNext() )
				Logger.Trace( string.Format( "\t[{0}]:\t{1}\t{2}", i++, myEnumerator.Key, myEnumerator.Value ) );			
		}

      public void Announce( string message )
      {
         Logger.Trace( message );
      }
	}
}