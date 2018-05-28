namespace RosterLib
{
   public interface IPgmMaster
   {
      PlayerGameMetrics GetPgm(string playerCode, string gameCode );

      void PutPgm(PlayerGameMetrics pgm);
   }
}
