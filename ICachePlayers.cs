namespace RosterLib
{
   public interface ICachePlayers
   {
      NFLPlayer Get( string playerId );

      void Put( NFLPlayer player );

   }
}