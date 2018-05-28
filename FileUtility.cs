using System;
using System.IO;
using System.Linq;

namespace RosterLib
{
	public static class FileUtility
	{
		public static DateTime DateOf( string fileName )
		{
			if ( !File.Exists( fileName ) )
				return new DateTime( 1, 1, 1 );

			var fileInfo = new FileInfo( fileName );
			return fileInfo.LastWriteTime;
		}

		public static string CopyDirectory( string Src, string Dst, NLog.Logger logger = null )
		{
			try
			{
				if ( Dst[ Dst.Length - 1 ] != Path.DirectorySeparatorChar )
					Dst += Path.DirectorySeparatorChar;

				if ( !Directory.Exists( Dst ) )
					Directory.CreateDirectory( Dst );

				string[] SourceFiles = Directory.GetFileSystemEntries( Src );
				foreach ( var SourceElement in SourceFiles )
				{
					if ( Directory.Exists( SourceElement ) )
					{
						var destDir = Dst + Path.GetFileName( SourceElement );
						if ( !Directory.Exists( destDir ) )
							Directory.CreateDirectory( destDir );

						CopyDirectory( SourceElement, destDir, logger );
					}
					else
					{
						// Files in directory
						File.Copy( SourceElement, Dst + Path.GetFileName( SourceElement ), true );
						if ( logger != null )
						{
							logger.Trace( $"Copied file: {SourceElement}" );
						}
					}
				}
				return string.Empty;
			}
			catch ( IOException ex )
			{
				return ex.Message;
			}
		}

		public static bool TryToDelete( string f )
		{
			try
			{
				// A.
				// Try to delete the file.
				File.Delete( f );
				return true;
			}
			catch ( IOException )
			{
				// B.
				// We could not delete the file.
				return false;
			}
		}

		public static void DeleteAllFilesInDirectory( string dir )
		{
			var downloadedMessageInfo = new DirectoryInfo( dir );

			foreach ( var file in downloadedMessageInfo.GetFiles() )
			{
				file.Delete();
			}
			foreach ( var d in downloadedMessageInfo.GetDirectories() )
			{
				d.Delete( true );
			}
		}

		public static int CountFilesInDirectory( string dir )
		{
			var downloadedMessageInfo = new DirectoryInfo( dir );
			var files = downloadedMessageInfo.GetFiles();
			return files.Count();
		}
	}
}