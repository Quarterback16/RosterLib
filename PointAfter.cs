namespace RosterLib
{
   public class PointAfter : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "PAT";
            }
        }
        public PointAfter()
        {
            ScoreType = "1";
        }
    }
}
