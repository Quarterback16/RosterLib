using System;
using System.Collections;
using System.Data;

namespace RosterLib
{
   /// <summary>
   ///   Represents a teams lineup for a game
   /// </summary>
   class NFLLineup
   {
      private ArrayList m_PlayerList;

      //  Units
      //  player + Pos + Start

      public NFLLineup( DataSet lineupDs, string teamCode, string season, string week )
      {
         PlayerList = new ArrayList();
         lineupDs = Utility.TflWs.GetLineup( teamCode, season, Int32.Parse(week));
         DataTable dt = lineupDs.Tables["lineup"];
         foreach (DataRow dr in dt.Rows)
         {
            NFLPlayer p = Masters.Pm.GetPlayer(dr["PLAYERID"].ToString());
            string pos = dr[ "POS" ].ToString();
            bool isStarter = dr[ "START" ].ToString().ToUpper().Equals( "TRUE" ) ? true : false;
            LineupRole r = new LineupRole( p, pos, isStarter );
            PlayerList.Add( p );
         }
      }

      #region  Accessors

      public ArrayList PlayerList
      {
         get { return m_PlayerList; }
         set { m_PlayerList = value; }
      }

      #endregion

   }

   class LineupRole
   {
      private NFLPlayer m_Player;
      private string m_Pos;
      private bool m_Starter;

      public LineupRole( NFLPlayer p, string pos, bool isStarter )
      {
         m_Player = p;
         m_Pos = pos;
         m_Starter = isStarter;
      }
   }
}
