using System;
using Foundation;
using System.Threading;

namespace DownloadManager.iOS
{
	public class FinishedDownload
	{
		public int Id { get; set; }
		public string Location { get; set; }
		public AutoResetEvent FileLock { get; set; }
		 
	}
}

