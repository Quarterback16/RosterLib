namespace RosterLib.Models
{
   public class Ace
   {
      public string PlayerId { get; set; }
      public string PlayerCat { get; set; }
      public string TeamCode { get; set; }
      public string Season { get; set; }
      public string Week { get; set; }

      public decimal Load { get; set; }

      public int Touches { get; set; }
   }
}
