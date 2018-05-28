namespace RosterLib
{
   public class PgmMaster : CacheMaster, IPgmMaster
   {

      public PlayerGameMetrics GetPgm(string playerCode, string gameCode)
      {
         PlayerGameMetrics pgm;
         var key = string.Format("{0}:{1}", playerCode, gameCode);

         if (this.TheHt.ContainsKey(key))
         {
            pgm = (PlayerGameMetrics)TheHt[key];
            CacheHits++;
         }
         else
         {
            pgm = new PlayerGameMetrics();
            PutPgm(pgm);
            CacheMisses++;
         }
         return pgm;
      }

      public void PutPgm(PlayerGameMetrics pgm)
      {
         var key = string.Format("{0}:{1}", pgm.PlayerId, pgm.GameKey );
			if (this.TheHt.ContainsKey(key))
			{
				TheHt[key] = pgm;
			}
			else
            TheHt.Add( key, pgm);
         IsDirty = true;
      }
   }
}