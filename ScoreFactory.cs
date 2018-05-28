using System.Collections.Generic;

namespace RosterLib
{
   public class ScoreFactory
    {

        public IScore CreateScore( string scoreType)
        {
            IScore score = null;

            switch (scoreType)
            {
                case "P":
                    score = new TdPass();
                    break;
                case "R":
                    score = new TdRun();
                    break;
                case "3":
                    score = new FieldGoal();
                    break;
                case "1":
                    score = new PointAfter();
                    break;
                case "F":
                    score = new FumbleReturn();
                    break;
                case "I":
                    score = new InterceptReturn();
                    break;
                case "K":
                    score = new KickOffReturn();
                    break;
                case "T":
                    score = new PuntReturn();
                    break;
                case "S":
                    score = new Safety();
                    break;
                case "2":
                    score = new TwoPointConversionPass();
                    break;
                case "N":
                    score = new TwoPointConversionRun();
                    break;
                default:
                    RosterLib.Utility.Announce( string.Format("A score of type {0} cannot be found", scoreType ) );
                    break;
            }

            if (score != null) score.ScoreType = scoreType;

            return score;
        }

        public List<BaseScore> GetAllScoreTypes()
        {
            var sl = new List<BaseScore>();
            sl.Add( new TdPass() );
            sl.Add(new TdRun());
            sl.Add(new FieldGoal());
            sl.Add(new PointAfter());
            sl.Add(new FumbleReturn());
            sl.Add(new InterceptReturn());
            sl.Add(new KickOffReturn());
            sl.Add(new PuntReturn());
            sl.Add(new Safety());
            sl.Add(new TwoPointConversionPass());
            sl.Add(new TwoPointConversionRun());

            RosterLib.Utility.Announce(string.Format("There are {0} score types", sl.Count ) );

            return sl;
        }
    }
}

