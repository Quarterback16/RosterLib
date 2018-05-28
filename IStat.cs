using System.Data;

namespace RosterLib
{
   public interface IStat
    {
        string Name { get; }
        string PlayerId { get; set; }
        decimal Quantity { get; set; }


        string Error { get; set; }

        bool IsValid();
        bool IsReasonable();

        void Dump();
        void Load( DataRow dr);
    }
}
