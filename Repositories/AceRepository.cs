using RosterLib.Models;
using RosterLib.Interfaces;

namespace RosterLib.Repositories
{
   public class AceRepository : IAceRepository
   {
      public void Update(Ace ace)
      {
         var aceDs = Utility.TflWs.GetAce(ace.Season, ace.Week, ace.PlayerId);

         if (aceDs.Tables[0].Rows.Count == 1)
         //  if yes just update
            Utility.TflWs.UpdateAce(ace.Season, ace.Week, ace.PlayerId, ace.PlayerCat, ace.TeamCode,
               ace.Load, ace.Touches);
         else
            Add(ace);
      }

      public void Add( Ace ace )
      {
         Utility.TflWs.InsertAce(ace.Season, ace.Week, ace.TeamCode, ace.PlayerId, ace.PlayerCat, 
            ace.Load, ace.Touches);
      }
   }
}
