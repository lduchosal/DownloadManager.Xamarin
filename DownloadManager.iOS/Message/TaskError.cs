using System;
using DownloadManager.iOS.Bo;

namespace DownloadManager.iOS
{
	public class TaskError
	{
		public int Id {
			get ;
			set;
		}

		public TaskErrorEnum Error {
			get ;
			set;
		}

		public string Description {
			get ;
			set;
		}

		public int StatusCode {
			get ;
			set;
		}
	}

	public enum TaskErrorEnum : int {
		Empty = 0,
		DownloadError,
		InvalidResponse,
		Timeout,

	}
}

