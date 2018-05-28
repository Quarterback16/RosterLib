namespace RosterLib
{
   public class Sack : BaseStat
    {
        #region IStat Members

        public override string Name
        {
            get
            {
                return "Sacks";
            }
        }

        public override bool IsReasonable()
        {
            if ( Quantity >= 7 )
                return false;
            else
                return true;

        }

        #endregion
    }
}
