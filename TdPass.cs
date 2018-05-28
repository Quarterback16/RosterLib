namespace RosterLib
{
   public class TdPass : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "TD Pass";
            }
        }

        public TdPass()
        {
            ScoreType = "P";
        }
    }
}
