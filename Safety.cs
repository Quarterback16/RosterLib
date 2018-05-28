namespace RosterLib
{
   public class Safety : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "Safety";
            }
        }
        public Safety()
        {
            ScoreType = "S";
        }

    }
}
