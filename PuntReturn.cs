namespace RosterLib
{
   public class PuntReturn : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "Punt Return";
            }
        }

        public PuntReturn()
        {
            ScoreType = "T";
        }
    }
}
