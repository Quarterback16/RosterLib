namespace RosterLib
{
   public interface IHtmlReport
	{
		string FileOut { get; set; }

		void Render();
	}
}
