namespace RosterLib
{
   public class YardsReceiving : BaseStat
    {

        #region IStat Members

        public override string Name
        {
            get
            {
                return "Yards Receiving";
            }
        }

        public override bool IsReasonable()
        {

            if ((Quantity >= 250) || (Quantity < -20))
                return false;
            else
                return true;

        }

        #endregion
    }
}
