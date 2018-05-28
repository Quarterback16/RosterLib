using RosterLib.Models;

namespace RosterLib.Interfaces
{
   public interface IAceRepository
   {
      void Update( Ace ace );

      void Add( Ace ace );
   }
}