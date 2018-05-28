namespace RosterLib
{
   public class TwoPointConversionPass : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "Pass for a 2 point conversion";
            }
        }

        public TwoPointConversionPass()
        {
            ScoreType = "N";
        }

    }
}
