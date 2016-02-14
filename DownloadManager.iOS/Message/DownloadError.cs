using System;
using DownloadManager.iOS.Bo;

namespace DownloadManager.iOS
{
	public class DownloadError
	{
		public int Id {
			get ;
			set;
		}

		public State State {
			get;
			set;
		}

		public ErrorEnum Error {
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

	public enum ErrorEnum : int {

		Empty = 0,

		DidFinishDownloading_InvalidTaskId,
		DidFinishDownloading_NullTaskId,
		DidFinishDownloading_NullResponse,
		DidFinishDownloading_ResponseError,

		DidWriteData_NullTaskId,
		DidWriteData_InvalidTaskId,

		DidCompleteWithError_NullTaskId,
		DidCompleteWithError_InvalidTaskId,
		DidCompleteWithError_Error,

		ProgressDownload_IdentifierNotFound,
		ProgressDownload_InvalidState,
		ProgressDownload_OutOfOrder,

		FinishedDownload_IdentifierNotFound,
		FinishedDownload_InvalidState,

		DownloadRejected_InvalidState,
		DownloadRejected_IdentifierNotFound,

		DownloadError_InvalidState,
		DownloadError_IdentifierNotFound,

		FreeSlot_InvalidState,

	}
}

