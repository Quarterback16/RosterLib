namespace RosterLib
{
   public class Carries : BaseStat
    {

        #region IStat Members

        public override string Name
        {
            get
            {
                return "Carries";
            }
        }

        public override bool IsReasonable()
        {
            if ( ( Quantity >= 40) || (Quantity < 0 ) )
                return false;
            else
                return true;
        }

        #endregion
    }
}

