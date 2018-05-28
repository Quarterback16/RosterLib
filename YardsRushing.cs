namespace RosterLib
{
   public class YardsRushing : BaseStat
    {


        #region IStat Members

        public override string Name
        {
            get
            {
                return "Rushing Yards";
            }
        }

        public override bool IsReasonable()
        {

            if ( (Quantity >= 300) || (Quantity < -50) )
                return false;
            else
                return true;

        }

        #endregion
    }
}
