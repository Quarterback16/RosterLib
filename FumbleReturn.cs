namespace RosterLib
{
   public class FumbleReturn : BaseScore
    {
        public override string ScoreType { get; set; }

        public override string Name
        {
            get
            {
                return "Fumble Return";
            }
        }
        
        public FumbleReturn()
        {
            ScoreType = "F";
        }
    }
}
