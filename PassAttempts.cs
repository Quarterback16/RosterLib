namespace RosterLib
{
   public class PassAttempts : BaseStat
    {

        #region IStat Members

        public override string Name
        {
            get
            {
                return "Pass Attempts";
            }
        }

        public override bool IsReasonable()
        {

            if ( (Quantity >= 60) || ( Quantity < 0 ) )
                return false;
            else
                return true;

        }

        #endregion
    }
}
