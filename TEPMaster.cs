
using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace RosterLib
{
	/// <summary>
	///   The object that handles the Experience points.
	/// </summary>
	public class TEPMaster
	{
		private XmlDocument epDoc;
		private Hashtable matrixHT;
			
		public UnitMatrix GetMatrixFor( string teamCode )
		{
			UnitMatrix u = null;
			if ( matrixHT == null ) LoadState();
		   if ( matrixHT != null )
		      if ( matrixHT.ContainsKey( teamCode ) )
		         u = (UnitMatrix) matrixHT[ teamCode ];

		   return u;
      }
		
		private void LoadState()
		{
			matrixHT = new Hashtable();
			UnitMatrix um = null;
			NflTeam team;
			RosterLib.Utility.Announce( "Loading saved Team EP" );
         bool exists = File.Exists("teamEP.xml");
         if (exists)
         {
            epDoc = new XmlDocument();
            epDoc.Load("teamEP.xml");
            XmlNode root = epDoc.ChildNodes[2];  // skip node 1 as it is a comment
            foreach (XmlNode playerNode in root.ChildNodes)
            {
               string teamCode = "";
               foreach (XmlNode n in playerNode.ChildNodes)
               {
                  switch (n.Name)
                  {
                     case "teamcode":
                        teamCode = n.InnerText;
                        break;
                     case "unit-list":
                        team = new NflTeam(teamCode);
                        um = new UnitMatrix(team);
                        ProcessUnitList(n, um);
                        break;
                     default:
                        break;
                  }
               }
               if (um != null)
                  AddTeam(teamCode, um);
            }
            PrintIndexAndKeysAndValues(matrixHT);
         }
		}
		
		private void ProcessUnitList( XmlNode n, UnitMatrix um )
		{
			string unitCode = "";
			decimal ep = 0.0M;
			foreach ( XmlNode unitNode in n.ChildNodes )
			{
				switch (unitNode.Name)
				{
					case "unitcode":
						if ( unitCode.Length > 0 ) um.AddUnit( unitCode, ep );
						unitCode = unitNode.InnerText;
						break;
					case "ep":
						ep = Decimal.Parse( unitNode.InnerText );
						break;
					default:
						break;
				}
			}
			um.AddUnit( unitCode, ep );
		}
		
		private void AddTeam( string id, UnitMatrix um )
		{
			if ( id.Length > 0 )
				matrixHT.Add( id, um );
		}
		
		private void PrintIndexAndKeysAndValues( Hashtable myList )
		{
			IDictionaryEnumerator myEnumerator = myList.GetEnumerator();
			int i = 0;
			RosterLib.Utility.Announce( "\t-INDEX-\t-KEY-\t-VALUE-" );
			while ( myEnumerator.MoveNext() )
				RosterLib.Utility.Announce( string.Format( "\t[{0}]:\t{1}\t{2}", i++, myEnumerator.Key, myEnumerator.Value ) );
		}

	}
}

