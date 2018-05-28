namespace RosterLib
{
   public class PassesIntercepted : BaseStat
    {

        #region IStat Members

        public override string Name
        {
            get
            {
                return "Passes Intercepted";
            }
        }

        public override bool IsReasonable()
        {

            if ( ( Quantity >= 5 ) || ( Quantity < 0 ) )
                return false;
            else
                return true;

        }

        #endregion
    }
}
