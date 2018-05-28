namespace RosterLib
{
   public class StatFactory
    {

        public IStat CreateStat( string statType )
        {
            IStat stat = null;

            switch (statType)
            {
                case "Q":
                    stat = new Sack();
                    break;
                case "Y":
                    stat = new YardsRushing();
                    break;
                case "R":
                    stat = new Carries();
                    break;
                case "C":
                    stat = new Completions();
                    break;
                case "A":
                    stat = new PassAttempts();
                    break;
                case "S":
                    stat = new YardsPassing();
                    break;
                case "Z":
                    stat = new PassesIntercepted();
                    break;
                case "P":
                    stat = new PassesCaught();
                    break;
                case "G":
                    stat = new YardsReceiving();
                    break;
                case "M":
                    stat = new InterceptionsMade();
                    break;
                default:
                    RosterLib.Utility.Announce( string.Format("A stat of type {0} cannot be found", statType ) );
                    break;
            }

            return stat;
        }

    }
}
