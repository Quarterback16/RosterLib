namespace RosterLib
{
   public class Completions : BaseStat
    {

        #region IStat Members

        public override string Name
        {
            get
            {
                return "Completions";
            }
        }

        public override bool IsReasonable()
        {
            if ( ( Quantity >= 50 ) || (Quantity < 0 ) )
                return false;
            else
                return true;
        }

        #endregion
    }
}
