using System;

namespace DownloadManager.iOS
{
	public class GlobalProgress
	{
		public long Written { get; set; }
		public long Total { get; set; }
		public StateEnum State { get; set; }

	}

	public enum StateEnum {
		Running,
		Stopped,
		Paused
	}
}

