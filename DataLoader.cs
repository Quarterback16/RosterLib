using RosterLib.Helpers;
using System.Data;
using System.Linq;
using TFLLib;

namespace RosterLib
{
	public class DataLoader
	{
		private readonly DataLibrarian OldLibrarian;

		public DataLoader()
		{
			OldLibrarian = new DataLibrarian(
				Utility.NflConnectionString(),
				Utility.TflConnectionString(),
				Utility.CtlConnectionString(),
				new NLogAdaptor());
		}

		public bool LoadPlayerData( ITflDataService dataService )
		{
			//  Get player data from the old Librarian
			var ds = OldLibrarian.GetPlayers( "*" );
			var dt = ds.Tables[ 0 ];
			foreach ( var player in from DataRow dr in dt.Rows select new NFLPlayer( dr ) )
			{
				//  Use the data service to record that data
				dataService.InsertPlayer( player );
			}

			return true;
		}
	}

}