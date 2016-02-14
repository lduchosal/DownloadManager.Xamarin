using System;
using DownloadManager.iOS.Bo;

namespace DownloadManager.iOS
{
	public class DownloadRejected
	{
		public int Id { get; set; }
		public RejectionEnum Reason { get; set; }
	}

	public enum RejectionEnum {
		QueueFull
	}
}

