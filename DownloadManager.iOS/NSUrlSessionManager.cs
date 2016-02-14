using System;
using Fabrik.SimpleBus;
using Foundation;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DownloadManager.iOS
{

	public class NSUrlSessionManager : NSUrlSessionDownloadDelegate {

		private readonly IBus _bus;
		private readonly int _maxdownloads;
		private readonly string _sessionidentifier;
		private readonly bool _allowsCellularAccess;
		private readonly ConcurrentDictionary<string, NSUrlSession> _nsurlsession;

		public NSUrlSessionManager(IBus bus, int maxdownloads) {
			_bus = bus;
			_bus.Subscribe<CheckFreeSlot> (CheckFreeSlot_Received);
			_bus.Subscribe<ResetDownloads> (ResetDownloads_Received);
			_bus.Subscribe<QueueDownload> (QueueDownload_Received);


			_maxdownloads = maxdownloads;
			_sessionidentifier = "DownloadManager.iOS.NSUrlSession";
			_allowsCellularAccess = false;
			_nsurlsession = new ConcurrentDictionary<string, NSUrlSession> ();
		}

		private NSUrlSession NSUrlSession {
			get { 
				Trace.TraceInformation("[NSUrlSessionManager] NSUrlSession");
				return _nsurlsession.GetOrAdd (_sessionidentifier,
					(identifier) => {
						using (var config = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration (identifier)) {
							config.AllowsCellularAccess = _allowsCellularAccess;
							config.NetworkServiceType = NSUrlRequestNetworkServiceType.Background;
							config.ShouldUseExtendedBackgroundIdleMode = true;

							Trace.TraceInformation("[NSUrlSessionManager] NSUrlSession.AllowsCellularAccess                : {0}", config.AllowsCellularAccess);
							Trace.TraceInformation("[NSUrlSessionManager] NSUrlSession.NetworkServiceType                  : {0}", config.NetworkServiceType);
							Trace.TraceInformation("[NSUrlSessionManager] NSUrlSession.ShouldUseExtendedBackgroundIdleMode : {0}", config.ShouldUseExtendedBackgroundIdleMode);
							Trace.TraceInformation("[NSUrlSessionManager] NSUrlSession.FromConfiguration");

							return NSUrlSession.FromConfiguration (config, this, new NSOperationQueue());
						}
					});
			}
		}

		public void QueueDownload_Received(QueueDownload queuedownload) {

			Trace.TraceInformation("[NSUrlSessionManager] QueueDownload");

			var download = queuedownload.Download;

			Trace.TraceInformation("[NSUrlSessionManager] QueueDownload Url           : {0}", download.Url);
			Trace.TraceInformation("[NSUrlSessionManager] QueueDownload Total         : {0}", download.Total);
			Trace.TraceInformation("[NSUrlSessionManager] QueueDownload Written       : {0}", download.Written);
			Trace.TraceInformation("[NSUrlSessionManager] QueueDownload DownloadState : {0}", download.DownloadState);
			Trace.TraceInformation("[NSUrlSessionManager] QueueDownload State         : {0}", download.State);
			Trace.TraceInformation("[NSUrlSessionManager] QueueDownload LastModified  : {0}", download.LastModified);

			NSUrlSession.GetTasks2 ((dataTasks, uploadTasks, downloadTasks) => {
					
				int free = _maxdownloads - downloadTasks.Length;

				Trace.TraceInformation ("[NSUrlSessionManager] QueueDownload GetTasks2");
				Trace.TraceInformation ("[NSUrlSessionManager] QueueDownload GetTasks2 Maxdownloads : {0}", _maxdownloads);
				Trace.TraceInformation ("[NSUrlSessionManager] QueueDownload GetTasks2 Length       : {0}", downloadTasks.Length);
				Trace.TraceInformation ("[NSUrlSessionManager] QueueDownload GetTasks2 Free         : {0}", free);

				if (free <= 0) {
					_bus.SendAsync<DownloadRejected> (new DownloadRejected {
						Id = download.Id,
						Reason = RejectionEnum.QueueFull
					});
					return;
				}

				using (var url = NSUrl.FromString (download.Url))
				using (var request = NSUrlRequest.FromUrl (url)) {
					var task = NSUrlSession.CreateDownloadTask (request);
					task.TaskDescription = download.Id.ToString ();
					task.Resume ();
				}

			});

		}

		public async void ResetDownloads_Received(ResetDownloads reset) {

			Trace.TraceInformation("[NSUrlSessionManager] ResetDownloads");

			await NSUrlSession.ResetAsync ();
		}

		public void CheckFreeSlot_Received(CheckFreeSlot freeslot) {

			Trace.TraceInformation("[NSUrlSessionManager] CheckFreeSlot");

			NSUrlSession.GetTasks2((dataTasks, uploadTasks, downloadTasks) => {

				int free = _maxdownloads - downloadTasks.Length;

				Trace.TraceInformation("[NSUrlSessionManager] CheckFreeSlot GetTasks2");
				Trace.TraceInformation("[NSUrlSessionManager] CheckFreeSlot GetTasks2 Maxdownloads : {0}", _maxdownloads);
				Trace.TraceInformation("[NSUrlSessionManager] CheckFreeSlot GetTasks2 Length       : {0}", downloadTasks.Length);
				Trace.TraceInformation("[NSUrlSessionManager] CheckFreeSlot GetTasks2 Free         : {0}", free);

				_bus.SendAsync<FreeSlot> (new FreeSlot {
					Free = free
				});
			});
		}

		public override void DidBecomeInvalid (NSUrlSession session, NSError error)
		{
			Trace.TraceInformation("[NSUrlSessionManager] DidBecomeInvalid");
			Trace.TraceInformation("[NSUrlSessionManager] DidBecomeInvalid Session : {0}", session.Description);
			Trace.TraceInformation("[NSUrlSessionManager] DidBecomeInvalid Error   : {0}", error.LocalizedDescription);
		}

		public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
		{

			Trace.TraceInformation("[NSUrlSessionManager] DidCompleteWithError");
			Trace.TraceInformation("[NSUrlSessionManager] DidCompleteWithError Session : {0}", session.Description);
			Trace.TraceInformation("[NSUrlSessionManager] DidCompleteWithError Task    : {0}", task.TaskDescription);

			var staskdescription = task.TaskDescription;
			if (staskdescription == null) {
				Trace.TraceInformation("[NSUrlSessionManager] DidCompleteWithError NullTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidCompleteWithError_NullTaskId
				});
				return;
			}
			int taskid;
			if (!int.TryParse (staskdescription, out taskid)) {
				Trace.TraceInformation("[NSUrlSessionManager] DidCompleteWithError InvalidTaskId");

				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidCompleteWithError_InvalidTaskId
				});
				return;
			}

			if (error != null) {
				Trace.TraceInformation("[NSUrlSessionManager] DidCompleteWithError Error");
				Trace.TraceInformation("[NSUrlSessionManager] DidCompleteWithError Description : {0}", error.LocalizedDescription);
				Trace.TraceInformation("[NSUrlSessionManager] DidCompleteWithError TaskId      : {0}", taskid);
				_bus.SendAsync<DownloadError> (new DownloadError {
					Id = taskid,
					Error = ErrorEnum.DidCompleteWithError_Error,
					Description = error.LocalizedDescription
				});
				return;
			}

		}


		public override void DidFinishDownloading (NSUrlSession session, NSUrlSessionDownloadTask task, NSUrl location)
		{

			Trace.TraceInformation("[NSUrlSessionManager] DidFinishDownloading");
			Trace.TraceInformation("[NSUrlSessionManager] DidFinishDownloading Session  : {0}", session.Description);
			Trace.TraceInformation("[NSUrlSessionManager] DidFinishDownloading Task     : {0}", task.Description);
			Trace.TraceInformation("[NSUrlSessionManager] DidFinishDownloading Location : {0}", location.Path);

			_bus.SendAsync<FreeSlot> (new FreeSlot());

			var staskdescription = task.TaskDescription;
			if (staskdescription == null) {
				Trace.TraceInformation("[NSUrlSessionManager] DidFinishDownloading NullTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidFinishDownloading_NullTaskId
				});
				return;
			}

			int taskid;
			if (!int.TryParse (staskdescription, out taskid)) {
				Trace.TraceInformation ("[NSUrlSessionManager] DidFinishDownloading InvalidTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidFinishDownloading_InvalidTaskId
				});
				return;
			}

			var response = task.Response as NSHttpUrlResponse;
			if (response == null) {
				Trace.TraceInformation ("[NSUrlSessionManager] DidFinishDownloading NullResponse");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidFinishDownloading_NullResponse
				});
				return;
			}

			int statuscode = (int)response.StatusCode;

			if (statuscode != 200) {
				Trace.TraceInformation ("[NSUrlSessionManager] DidFinishDownloading StatusCode");
				Trace.TraceInformation ("[NSUrlSessionManager] DidFinishDownloading StatusCode : {0}", statuscode);

				_bus.SendAsync<DownloadError> (new DownloadError {
					Id = taskid,
					Error = ErrorEnum.DidFinishDownloading_ResponseError,
					StatusCode = (int)response.StatusCode,
				});

				return;
			}

			string path = location == null ? null : location.Path;
			_bus.SendAsync<FinishedDownload> (new FinishedDownload {
				Id = taskid,
				Location = path,
			});

		}

		public override void DidFinishEventsForBackgroundSession (NSUrlSession session)
		{
			Trace.TraceInformation("[NSUrlSessionManager] DidFinishEventsForBackgroundSession");
			_bus.SendAsync<FreeSlot> (new FreeSlot {
				Free = 1
			});
		}

		public override void DidReceiveChallenge (NSUrlSession session, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
		{
			Trace.TraceInformation("[NSUrlSessionManager] DidReceiveChallenge");
		}

		public override void DidReceiveChallenge (NSUrlSession session, NSUrlSessionTask task, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
		{
			Trace.TraceInformation("[NSUrlSessionManager] DidReceiveChallenge2");
		}

		public override void DidResume (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
		{
			Trace.TraceInformation("[NSUrlSessionManager] DidResume");
		}

		public override void DidSendBodyData (NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend)
		{
			Trace.TraceInformation("[NSUrlSessionManager] DidSendBodyData");
		}

		public override void DidWriteData (NSUrlSession session, NSUrlSessionDownloadTask task, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
		{
			
			Trace.TraceInformation("[NSUrlSessionManager] DidWriteData");
			Trace.TraceInformation("[NSUrlSessionManager] DidWriteData Task     : {0}", task.Description);
			Trace.TraceInformation("[NSUrlSessionManager] DidWriteData Session  : {0}", session.Description);
			Trace.TraceInformation("[NSUrlSessionManager] DidWriteData Written  : {0}", bytesWritten);
			Trace.TraceInformation("[NSUrlSessionManager] DidWriteData Total    : {0}", totalBytesWritten);
			Trace.TraceInformation("[NSUrlSessionManager] DidWriteData Expected : {0}", totalBytesExpectedToWrite);

			var staskdescription = task.TaskDescription;
			if (staskdescription == null) {
				Trace.TraceInformation("[NSUrlSessionManager] DidWriteData NullTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidWriteData_NullTaskId
				});
				return;
			}

			int taskid;
			if (!int.TryParse (staskdescription, out taskid)) {
				Trace.TraceInformation("[NSUrlSessionManager] DidWriteData InvalidTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidWriteData_InvalidTaskId
				});
				return;
			}

			_bus.SendAsync<ProgressDownload> (new ProgressDownload {
				Id = taskid,
				Total = totalBytesExpectedToWrite,
				Written = totalBytesWritten
			});
		}

		public override void NeedNewBodyStream (NSUrlSession session, NSUrlSessionTask task, Action<NSInputStream> completionHandler)
		{
			Trace.TraceInformation("[NSUrlSessionManager] NeedNewBodyStream");
		}

		public override void WillPerformHttpRedirection (NSUrlSession session, NSUrlSessionTask task, NSHttpUrlResponse response, NSUrlRequest newRequest, Action<NSUrlRequest> completionHandler)
		{
			Trace.TraceInformation("[NSUrlSessionManager] WillPerformHttpRedirection");
		}
	}
}

