namespace RosterLib
{
   public class GetGamePrediction
   {
      public GetGamePrediction( PlayerGameProjectionMessage input )
      {
         Process( input );
      }

      private static void Process( PlayerGameProjectionMessage input )
      {
         input.Prediction = input.Game.GetPrediction( "unit" );
      }
   }
}