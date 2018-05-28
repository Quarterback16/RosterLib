using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace RosterLib
{
	/// <summary>
	///   The Season Cache for RosterGrid (season.xml).
	///   Written by : RosterGrid Cleanup
	///   Loaded by  : FreeAgent Report and UnitReport
	///                search for Masters.Sm
	/// </summary>
	public class SeasonMaster : XmlCache
	{
		#region  Constructors

		public SeasonMaster( string name, string fileName ) : base( name )
		{
			Filename = fileName;
			try
			{
				//  load HT from the xml
				XmlDoc = new XmlDocument();
				XmlDoc.Load( fileName );
				XmlNode listNode = XmlDoc.ChildNodes[ 2 ];
				foreach ( XmlNode node in listNode.ChildNodes )
					AddXmlSeason( node );

				//  Create the XML navigation objects to allow xpath queries
				//RosterLib.Utility.Announce( "SeasonMaster.Init Creating epXmlDoc" );
				EpXmlDoc = new XPathDocument( fileName );
				// bad implementation - does not throw an exeception if XML is invalid
				//RosterLib.Utility.Announce( string.Format( "{0} loaded OK!", fileName ) );
				Nav = EpXmlDoc.CreateNavigator();

				DumpHt();
				DumpSeasons();
				RosterLib.Utility.Announce( string.Format( "SeasonMaster constructed : {0}-{1}", name, fileName ) );

			}
			catch ( IOException e )
			{
				RosterLib.Utility.Announce( string.Format( "Unable to open {1} xmlfile - {0}", e.Message, fileName ) );
			}
		}

		#endregion

		#region  Reading

		public NflSeason GetSeason( string season )
		{
			NflSeason s;
			if ( TheHt.ContainsKey( season ) )
			{
				s = (NflSeason) TheHt[ season ];
				CacheHits++;
			}
			else
			{
				//  new it up
				s = new NflSeason( season );
				PutSeason( s );
				CacheMisses++;
			}
			return s;
		}

      public NflSeason GetSeason(string season, bool teamsOnly )
      {
         NflSeason s;
         if (TheHt.ContainsKey(season))
         {
            s = (NflSeason)TheHt[season];
            CacheHits++;
         }
         else
         {
            //  new it up
            s = new NflSeason( season, teamsOnly );
            PutSeason(s);
            CacheMisses++;
         }
         return s;
      }

		public String[] GetGordan( string season, string teamKey )
		{
			String[] rating = new String[RosterLib.Constants.K_WEEKS_IN_A_SEASON + 1];
			//RosterLib.Utility.Announce( string.Format( "SeasonMaster.GetGordan : {0} ", teamKey ) );

			if ( EpXmlDoc != null )
			{
				//string expr = string.Format("/playerList/player/ep[../id=\"{0}\"]", id);
				string expr = string.Format( "/season-list/season/team-list/team[@key=\"{0}\"]", teamKey );
				//RosterLib.Utility.Announce( string.Format( "SeasonMaster.GetGordan : {0}", expr ) );
				if ( Nav == null )
					RosterLib.Utility.Announce( string.Format( "SeasonMaster.GetGordan : navigator is null {0}", teamKey ) );
				XPathNodeIterator NodeIter = Nav.Select( expr );
				while ( NodeIter.MoveNext() )
				{
					//  Cursor style
					XPathNavigator ratingList = NodeIter.Current.Clone();
					// Select the <rating-list> node and go top the top
					ratingList.MoveToFirstChild();
					XPathNavigator ratings = ratingList.Clone();
					ratings.MoveToFirstChild();
					ratings.MoveToParent(); //  <ratings-list>
					XPathNodeIterator weekIterator = ratings.Select( "./at-week" ); // should be 17+1 of these
					int w = 0;
					while ( weekIterator.MoveNext() )
					{
						XPathNavigator weekNav = weekIterator.Current;
						if ( weekNav.MoveToAttribute( "letter", "" ) )
						{
							rating[ w ] = weekNav.Value; //  rating for each week
							w++;
						}
					}
				}
			}

			else
				RosterLib.Utility.Announce( string.Format( "SeasonMaster.GetGordan : XmlDoc is null {0}", teamKey ) );

			return rating;
		}

		/// <summary>
		///  Reads the Nibble Ratings out of the XML
		/// </summary>
		/// <param name="season"></param>
		/// <param name="teamKey"></param>
		/// <returns></returns>
		public int[,] GetNibbleRate( string season, string teamKey )
		{
			int[,] rating = new int[Constants.K_WEEKS_IN_REGULAR_SEASON,1];
			Utility.Announce( string.Format( "SeasonMaster.GetNibbleRate : {0} ", teamKey ) );

			if ( EpXmlDoc != null )
			{
				//string expr = string.Format("/playerList/player/ep[../id=\"{0}\"]", id);
				var expr = string.Format( "/season-list/season/team-list/team[@key=\"{0}\"]", teamKey );
				//RosterLib.Utility.Announce( string.Format( "SeasonMaster.GetGordan : {0}", expr ) );
				if ( Nav == null )
					Utility.Announce( string.Format( "SeasonMaster.GetNibbleRate : navigator is null {0}", teamKey ) );
				var nodeIter = Nav.Select( expr );
				while ( nodeIter.MoveNext() )
				{
					//  Cursor style
					var ratingList = nodeIter.Current.Clone();
					// Select the <rating-list> node and go top the top
					ratingList.MoveToFirstChild();
					var ratings = ratingList.Clone();
					ratings.MoveToFirstChild();
					ratings.MoveToParent(); //  <ratings-list>
					var weekIterator = ratings.Select( "./at-week" ); // should be 17+1 of these
					var w = 0;
					while ( weekIterator.MoveNext() )
					{
						var weekNav = weekIterator.Current;
						if (weekNav.MoveToAttribute("offence", ""))
							rating[w, 0] = Int32.Parse(weekNav.Value); //  rating for each week
						if (weekNav.MoveToAttribute("defence", ""))
							rating[w, 1] = Int32.Parse(weekNav.Value); //  rating for each week
						w++;
					}
				}
			}

			else
				Utility.Announce( string.Format( "SeasonMaster.GetNibbleRate : XmlDoc is null {0}", teamKey ) );

			return rating;
		}

		#endregion

		#region  Writing

		public
			void PutSeason( NflSeason s )
		{
			if ( ! TheHt.ContainsKey( s.Year ) )
			{
				TheHt.Add( s.Year, s );
				IsDirty = true;
			}
		}

		private
			void AddXmlSeason( XmlNode node )
		{
			AddSeason( new NflSeason( node ) );
		}

		public
			void AddSeason( NflSeason s )
		{
			TheHt.Add( s.Year, s );
		}

		#endregion

		#region  Persistence

		/// <summary>
		///   Converts the memory hash table to XML
		/// </summary>
		public void Dump2Xml()
		{
			if ( ( TheHt.Count > 0 ) && IsDirty )
			{
				XmlTextWriter writer = new
                    XmlTextWriter(string.Format("{0}{1}", Utility.OutputDirectory(), Filename), null);

				writer.WriteStartDocument();
				writer.WriteComment( "Comments: " + Name );
				writer.WriteStartElement( "season-list" );

				IDictionaryEnumerator myEnumerator = TheHt.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					NflSeason s = (NflSeason) myEnumerator.Value;
					WriteSeasonNode( writer, s );
				}
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Close();
                RosterLib.Utility.Announce(string.Format("{0}{1} created.", Utility.OutputDirectory(), Filename));
			}
		}

		private static
			void WriteSeasonNode( XmlTextWriter writer, NflSeason s )
		{
			writer.WriteStartElement( "season" );
			WriteElement( writer, "year", s.Year );
			WriteElement( writer, "weeks", s.Weeks.ToString() );
			WriteTeamList( writer, s );
			writer.WriteEndElement();
		}

		private static
			void WriteTeamList( XmlTextWriter writer, NflSeason s )
		{
			writer.WriteStartElement( "team-list" );
			foreach ( string teamKey in s.TeamKeyList )
			{
				writer.WriteStartElement( "team" );
				writer.WriteAttributeString( "key", teamKey );
				WriteRatings( writer, teamKey );
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		private static
			void WriteRatings( XmlTextWriter writer, string teamKey )
		{
			writer.WriteStartElement( "rating-list" );
			NflTeam t = Masters.Tm.GetTeam( teamKey );
			if ( t != null )
			{
				for ( int w = 0; w < RosterLib.Constants.K_WEEKS_IN_A_SEASON; w++ )
				{
					writer.WriteStartElement( "at-week" );
					writer.WriteAttributeString( "week", w.ToString() );
					writer.WriteAttributeString( "letter", t.LetterRating[ w ] );
					writer.WriteAttributeString( "number", t.NumberRating[ w ].ToString() );
					writer.WriteAttributeString( "offence", t.NibbleRating[ w, 0 ].ToString() );
					writer.WriteAttributeString( "defence", t.NibbleRating[ w, 1 ].ToString() );
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();
		}

		#endregion

		#region  Logging

		public void DumpSeasons()
		{
			IDictionaryEnumerator myEnumerator = TheHt.GetEnumerator();
			while ( myEnumerator.MoveNext() )
			{
				NflSeason s = (NflSeason) myEnumerator.Value;
				RosterLib.Utility.Announce( string.Format( "Season {0}:- ", myEnumerator.Key ) );
				s.DumpTeams();
			}
		}

		#endregion
	}
}