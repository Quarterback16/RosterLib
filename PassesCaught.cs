namespace RosterLib
{
   public class PassesCaught : BaseStat
    {

        #region IStat Members

        public override string Name
        {
            get
            {
                return "Passes Caught";
            }
        }

        public override bool IsReasonable()
        {
            if ((Quantity >= 15) || (Quantity < 0))
                return false;
            else
                return true;
        }

        #endregion
    }
}
