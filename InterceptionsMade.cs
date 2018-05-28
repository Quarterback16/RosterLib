namespace RosterLib
{
   public class InterceptionsMade : BaseStat
    {

        #region IStat Members

        public override string Name
        {
            get
            {
                return "Interceptions Made";
            }
        }

        public override bool IsReasonable()
        {
            if ((Quantity >= 5) || (Quantity < 0))
                return false;
            else
                return true;
        }

        #endregion
    }
}
