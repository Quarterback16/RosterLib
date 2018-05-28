namespace RosterLib
{
	/// <summary>
	/// Summary description for SeasonOpponent.
	/// </summary>
   public class SeasonOpposition
   {
      private readonly bool bHome;
      private string opponent;
      private int metric;

      public SeasonOpposition( string opponentIn, bool bHomeIn, int metricIn )
      {
         Opponent = opponentIn;
         bHome = bHomeIn;
         Metric = metricIn;
      }

      public int Metric
      {
         get { return metric; }
         set { metric = value; }
      }

      public string Opponent
      {
         get { return opponent; }
         set { opponent = value; }
      }

      public bool IsHome()
      {
         return bHome;
      }

   }

}
