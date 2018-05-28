namespace RosterLib
{
   public class KickOffReturn : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "Kickoff Return";
            }
        }

        public KickOffReturn()
        {
            ScoreType = "K";
        }
    }
}
