using System;
using DownloadManager.iOS.Bo;
using System.Threading;

namespace DownloadManager.iOS
{
	public class NotifyFinish
	{
		public string Url { get; set; }
		public Download Download { get; set; }
		public AutoResetEvent FileLock { get; set; }
	}
}

