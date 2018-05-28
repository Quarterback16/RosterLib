
using System.Collections.Generic;
using System.Xml;


namespace RosterLib
{
	internal class NFL
	{
		//  SeasonList
		private readonly List< NflSeason > m_SeasonList;


		//  Constructor
		public NFL()
		{
			m_SeasonList = new List< NflSeason >(); //  .NET 2.0         
			//m_SeasonList.Add(new NflSeason("1994"));
			//m_SeasonList.Add(new NflSeason("1995"));
			//m_SeasonList.Add(new NflSeason("1996"));
			//m_SeasonList.Add(new NflSeason("1997"));
			//m_SeasonList.Add(new NflSeason("1998"));
			//m_SeasonList.Add(new NflSeason("1999"));
			//m_SeasonList.Add(new NflSeason("2000"));
			//m_SeasonList.Add(new NflSeason("2001"));
			//m_SeasonList.Add(new NflSeason("2002"));
			//m_SeasonList.Add(new NflSeason("2003"));
			//m_SeasonList.Add(new NflSeason("2004"));
			//m_SeasonList.Add(new NflSeason("2005"));
			//m_SeasonList.Add(new NflSeason("2006"));
			m_SeasonList.Add( new NflSeason( "2007" ) );
			m_SeasonList.Add( new NflSeason("2008"));
		}

		#region  NFL.XML

		public void CreateXml()
		{
			if ( ( m_SeasonList.Count > 0 ) )
			{
				const string fileName = "NFL.xml";
				XmlTextWriter writer = new
                    XmlTextWriter(string.Format("{0}{1}", Utility.OutputDirectory() + "xml\\", fileName), null);
				writer.Formatting = Formatting.Indented;

				writer.WriteStartDocument();
				writer.WriteComment( "Comments: NFL Season List" );
				writer.WriteStartElement( "nfl" );
				writer.WriteStartElement("season-list");

				foreach ( NflSeason s in m_SeasonList )
				{
					WriteSeasonNode( writer, s );
				}
				writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Close();
				RosterLib.Utility.Announce( fileName + " created" );
			}
		}

		private static void WriteSeasonNode( XmlWriter writer, NflSeason s )
		{
			writer.WriteStartElement( "season" );
			WriteElement( writer, "year", s.Year );
			WriteElement( writer, "weeks-in-season", s.Weeks.ToString() );

			WriteConferenceList( writer, s );

			WriteTeamList( writer, s );

			writer.WriteEndElement();
		}


		private static void WriteElement( XmlWriter writer, string name, string text)
		{
			writer.WriteStartElement(name);
			writer.WriteString(text);
			writer.WriteEndElement();
		}

		private static void WriteConferenceList(XmlWriter writer, NflSeason s)
		{
			writer.WriteStartElement("conference-list");
			//  Division List
			foreach (NflConference c in s.ConferenceList)
			{
				WriteConferenceNode(writer, c);
			}
			writer.WriteEndElement();
		}

		private static void WriteConferenceNode(XmlWriter writer, NflConference c)
		{
			writer.WriteStartElement("conference");
			WriteElement(writer, "name", c.NameOut() );

			WriteDivisionList(writer, c);

			writer.WriteEndElement();
		}

		private static void WriteDivisionList(XmlWriter writer, NflConference c)
		{
			writer.WriteStartElement("division-list");
			//  Division List
			foreach (NFLDivision d in c.DivList)
			{
				WriteDivisionNode(writer, d);
			}
			writer.WriteEndElement();
		}


		private static void WriteDivisionNode(XmlWriter writer, NFLDivision d)
		{
			writer.WriteStartElement("division");
			WriteElement(writer, "name", d.NameOut());
			WriteElement(writer, "id", d.Code);

			writer.WriteEndElement();
		}

		private static void WriteTeamList(XmlWriter writer, NflSeason s)
		{
			writer.WriteStartElement("team-list");
			foreach (NflTeam t in s.TeamList )
			{
				writer.WriteStartElement("team");
				WriteElement( writer, "id", t.TeamCode );
				WriteElement( writer, "name", t.Name);
				WriteElement( writer, "division", t.Division());

				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		#endregion

		public void SpitSchedules()
		{
			foreach (NflSeason s in m_SeasonList)
				s.SpitSchedule();
		}

		public void SpitSeasons()
		{
			foreach (NflSeason s in m_SeasonList)
				s.SpitSeason();
		}
	}
}