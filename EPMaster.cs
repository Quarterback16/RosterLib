using System;
using System.Collections;
using System.IO;
using System.Xml.XPath;

namespace RosterLib
{
	/// <summary>
	///   The object that handles the Experience points.
	///   could create a super class for XML cache
	/// </summary>
	public class EpMaster : XmlCache
	{
		public EpMaster( string name ) : base( name )
		{
			try
			{
				Utility.Announce( "Creating epXmlDoc" );
				EpXmlDoc = new XPathDocument(
					string.Format("{0}xml\\ep.xml", Utility.OutputDirectory() ) );
				Utility.Announce("ep.xml loaded OK!");
				Nav = EpXmlDoc.CreateNavigator();
				Utility.Announce(string.Format("EPMaster constructed : {0}", name ));			   
			}
			catch (IOException e)
			{
				Utility.Announce(string.Format( "EPMaster : Unable to open xmlfile - {0}", e.Message ) );
			}
		}

		
		public void AddExpPts( string playerId, string gameCode, decimal ep )
		{
			
		}

		/// <summary>
		/// Gets the EP from the XML file.
		/// Huge list of players and their EP output for each game they played.
		/// Updated ..
		/// Needs the nav navigator
		/// </summary>
		/// <param name="id">The player id.</param>
		/// <returns></returns>
		public decimal GetEp( string id )
		{
			Utility.Announce(string.Format( "EPMaster.GetEP : {0}", id ) );
			var isCached = false;
			var ep = -1.0M;
			Utility.Announce(string.Format( "EPMaster.GetEP : Testing epXmlDoc {0}", id ) );
			if ( EpXmlDoc != null )
			{
				var expr = string.Format("/playerList/player/ep[../id=\"{0}\"]", id );
				Utility.Announce(string.Format( "EPMaster.GetEP : {0}", expr ) );
				if (Nav == null)
					Utility.Announce(string.Format("EPMaster.GetEP : navigator is null {0}", id));
				else
				{
					var nodeIter = Nav.Select( expr );
					while ( nodeIter.MoveNext() )
					{
						if (nodeIter.Current != null)
						{
							Utility.Announce( 
								string.Format( "EPMaster.GetEP : Lookup Result for {1}: {0} ep", 
									nodeIter.Current.Value, id ) );
							ep = Convert.ToDecimal( nodeIter.Current.Value );
						}
						isCached = true;
						break;
					}
				}
			}
			else
				Utility.Announce(string.Format( "EPMaster.GetEP : XmlDoc is null {0}", id ) );
				
			if ( isCached ) CacheHits++;
			else CacheMisses++;
			return ep;
		}
		
		public ArrayList GetGameList( string id )
		{
			var gameList = new ArrayList();
			if ( EpXmlDoc != null )
			{
				var expr = string.Format("/playerList/player/ep-list[../id='{0}']", id );
				var nodeIter = Nav.Select( expr );
				while (nodeIter.MoveNext())
				{
					//  ep-list node has a bunch of week nodes
					if ( nodeIter.Current != null && nodeIter.Current.HasChildren )
					{
						//  bunch of week nodes				
						var copy = nodeIter.Current.Clone();
						var weekList = copy.SelectChildren( "week", "" );
						while (weekList.MoveNext())
						{
							if (weekList.Current != null)
							{
								var theWeek = weekList.Current.Clone();
								theWeek.MoveToFirstChild();
								var season = theWeek.Value.Substring( 0, 4 );
								var week = theWeek.Value.Substring( 5, 2 );
								theWeek.MoveToNext();
								var ep = Decimal.Parse( theWeek.Value );
								gameList.Add( new NflPerformance( season, week, ep ) );
							}
						}
					}
				}
			}
			return gameList;
		}

	}
}
