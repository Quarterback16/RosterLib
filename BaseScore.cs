using System;
using System.Data;

namespace RosterLib
{
   public abstract class BaseScore : IScore
    {
        public string Season { get; set; }
        public string Week { get; set; }
        public string GameNo { get; set; }

        public abstract string Name { get; }
        public abstract string ScoreType { get; set; }

        public string PlayerId1 { get; set; }
        public string PlayerId2 { get; set; }
        public string TeamId { get; set; }
        public string When { get; set; }
        public int Distance { get; set; }

        public string Error { get; set; }

        public void Dump()
        {
            Utility.Announce(string.Format("{0}:{1}-{2} {3} {4} has {5} {6}",
                Season, Week, GameNo, TeamId, PlayerId1, Name, Distance ) );
        }

        public bool IsValid()
        {
            bool isValid = true;

            string allTeams = "CH AF DC AC DL CP NG SF GB NO PE LR MV TB WR SS BR HT BB DB CI IC MD KC CL JJ NE OR PS TT NJ LC";
            isValid = (allTeams.IndexOf(TeamId) > -1);
            Error = (isValid ? string.Empty : "Invalid Team");

            if (isValid)
            {
                string allGames = "A B C D E F G H I J K L M N O P";
                isValid = (allGames.IndexOf(GameNo) > -1);
                Error = (isValid ? string.Empty : "Invalid Game code");
            }

            if (isValid)
            {
                string allScores = "FIKTSPRC312N";
                isValid = (allScores.IndexOf(ScoreType) > -1);
                Error = (isValid ? string.Empty : "Invalid Score code");
            }

            return isValid;
        }


        public void Load(DataRow dr)
        {
            Season = dr["SEASON"].ToString();
            Week = dr["WEEK"].ToString();
            GameNo = dr["GAMENO"].ToString();
            TeamId = dr["TEAM"].ToString();
            When = dr["WHEN"].ToString();
            PlayerId1 = dr["PLAYERID1"].ToString();
            PlayerId1 = dr["PLAYERID1"].ToString();
            ScoreType = dr["SCORE"].ToString();
            Distance = Int32.Parse(dr["DISTANCE"].ToString() );
        }

        public virtual bool IsReasonable()
        {
            return false;
        }

    }
}
