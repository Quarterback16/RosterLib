namespace RosterLib
{
   public class TdRun : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "TD Run";
            }
        }

        public TdRun()
        {
            ScoreType = "R";
        }
    }
}
