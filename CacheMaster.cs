using System.Collections;

namespace RosterLib
{
   public class CacheMaster
   {
      public Hashtable TheHt { get; set; }

      public int CacheHits { get; set; }

      public int CacheMisses { get; set; }

      public bool IsDirty { get; set; }

      public CacheMaster() 
      {
         TheHt = new Hashtable();
      }
   }
}
