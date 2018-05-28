namespace RosterLib
{
   public class InterceptReturn : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "Interception Return";
            }
        }

        public InterceptReturn()
        {
            ScoreType = "I";
        }
    }
}
