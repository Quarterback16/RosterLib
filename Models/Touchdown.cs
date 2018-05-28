namespace RosterLib.Models
{
   public class Touchdown
   {
      public string Action { get; set; }
      public int Distance { get; set; }
      public NFLPlayer Scorer { get; set; }
      public NFLPlayer Assisting { get; set; }

      public NFLGame Game { get; set; }

      public string ForTeamCode { get; set; }
      public string AgainstTeamCode { get; set; }

      public override string ToString()
      {
         var s = string.Empty;
         switch (Action)
         {
            case Constants.K_SCORE_TD_PASS:
               s = $@"{ForTeamCode}: {Distance} yd Touchdown pass to {Scorer} from {Assisting} - {Action} {
				   Game.ResultOut( ForTeamCode, abbreviate: true )
				   }";
               break;

            case Constants.K_SCORE_TD_RUN:
               s = string.Format(" Touchdown run by {0}", Scorer);
               s = $@"{ForTeamCode}: {Distance} yd Touchdown run by {Scorer} - {Action} {
				   Game.ResultOut( ForTeamCode, abbreviate: true )
				   }";
               break;

            default:
               s = string.Format(" Touchdown by {0}", Scorer);
               s = $@"{ForTeamCode}: {Distance} yd Touchdown by {Scorer} - {Action} {
				   Game.ResultOut( ForTeamCode, abbreviate: true )
				   }";
               break;
         }
         return s;
      }

   }
}
