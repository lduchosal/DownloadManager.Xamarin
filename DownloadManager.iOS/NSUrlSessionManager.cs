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

			Console.WriteLine("[NSUrlSessionManager] ctor");

			_bus = bus;
			_bus.Subscribe<CheckFreeSlot> (CheckFreeSlot_Received);
			_bus.Subscribe<ResetDownloads> (ResetDownloads_Received);
			_bus.Subscribe<QueueDownload> (QueueDownload_Received);


			_maxdownloads = maxdownloads;
			_sessionidentifier = "DownloadManager.iOS.NSUrlSession";
			_allowsCellularAccess = false;
			_nsurlsession = new ConcurrentDictionary<string, NSUrlSession> ();

			// Initialize NSUrlSession as soon as possible.
			var identifier = NSUrlSession.Configuration.Identifier;
			Console.WriteLine("[NSUrlSessionManager] ctor identifier : {0}", identifier);

		}

		private NSUrlSession NSUrlSession {
			get { 
				Console.WriteLine("[NSUrlSessionManager] NSUrlSession");
				return _nsurlsession.GetOrAdd (_sessionidentifier,
					(identifier) => {
						using (var config = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration (identifier)) {
							config.AllowsCellularAccess = _allowsCellularAccess;
							config.NetworkServiceType = NSUrlRequestNetworkServiceType.Background;
							config.ShouldUseExtendedBackgroundIdleMode = true;

							Console.WriteLine("[NSUrlSessionManager] NSUrlSession.AllowsCellularAccess                : {0}", config.AllowsCellularAccess);
							Console.WriteLine("[NSUrlSessionManager] NSUrlSession.NetworkServiceType                  : {0}", config.NetworkServiceType);
							Console.WriteLine("[NSUrlSessionManager] NSUrlSession.ShouldUseExtendedBackgroundIdleMode : {0}", config.ShouldUseExtendedBackgroundIdleMode);
							Console.WriteLine("[NSUrlSessionManager] NSUrlSession.FromConfiguration");

							return NSUrlSession.FromConfiguration (config, this, new NSOperationQueue());
						}
					});
			}
		}

		public void QueueDownload_Received(QueueDownload queuedownload) {

			Console.WriteLine("[NSUrlSessionManager] QueueDownload");

			var download = queuedownload.Download;

			Console.WriteLine("[NSUrlSessionManager] QueueDownload Url           : {0}", download.Url);
			Console.WriteLine("[NSUrlSessionManager] QueueDownload Total         : {0}", download.Total);
			Console.WriteLine("[NSUrlSessionManager] QueueDownload Written       : {0}", download.Written);
			Console.WriteLine("[NSUrlSessionManager] QueueDownload DownloadState : {0}", download.DownloadState);
			Console.WriteLine("[NSUrlSessionManager] QueueDownload State         : {0}", download.State);
			Console.WriteLine("[NSUrlSessionManager] QueueDownload LastModified  : {0}", download.LastModified);

			NSUrlSession.GetTasks2 ((dataTasks, uploadTasks, downloadTasks) => {
					
				int free = _maxdownloads - downloadTasks.Length;

				Console.WriteLine ("[NSUrlSessionManager] QueueDownload GetTasks2");
				Console.WriteLine ("[NSUrlSessionManager] QueueDownload GetTasks2 Maxdownloads : {0}", _maxdownloads);
				Console.WriteLine ("[NSUrlSessionManager] QueueDownload GetTasks2 Length       : {0}", downloadTasks.Length);
				Console.WriteLine ("[NSUrlSessionManager] QueueDownload GetTasks2 Free         : {0}", free);

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

			Console.WriteLine("[NSUrlSessionManager] ResetDownloads");

			await NSUrlSession.ResetAsync ();
		}

		public void CheckFreeSlot_Received(CheckFreeSlot freeslot) {

			Console.WriteLine("[NSUrlSessionManager] CheckFreeSlot");

			NSUrlSession.GetTasks2((dataTasks, uploadTasks, downloadTasks) => {

				int free = _maxdownloads - downloadTasks.Length;

				Console.WriteLine("[NSUrlSessionManager] CheckFreeSlot GetTasks2");
				Console.WriteLine("[NSUrlSessionManager] CheckFreeSlot GetTasks2 Maxdownloads : {0}", _maxdownloads);
				Console.WriteLine("[NSUrlSessionManager] CheckFreeSlot GetTasks2 Length       : {0}", downloadTasks.Length);
				Console.WriteLine("[NSUrlSessionManager] CheckFreeSlot GetTasks2 Free         : {0}", free);

				_bus.SendAsync<FreeSlot> (new FreeSlot {
					Free = free
				});
			});
		}

		public override void DidBecomeInvalid (NSUrlSession session, NSError error)
		{
			Console.WriteLine("[NSUrlSessionManager] DidBecomeInvalid");
			Console.WriteLine("[NSUrlSessionManager] DidBecomeInvalid Session : {0}", session.Description);
			Console.WriteLine("[NSUrlSessionManager] DidBecomeInvalid Error   : {0}", error.LocalizedDescription);
		}

		public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
		{

			Console.WriteLine("[NSUrlSessionManager] DidCompleteWithError");
			Console.WriteLine("[NSUrlSessionManager] DidCompleteWithError Session : {0}", session.Description);
			Console.WriteLine("[NSUrlSessionManager] DidCompleteWithError Task    : {0}", task.TaskDescription);

			var staskdescription = task.TaskDescription;
			if (staskdescription == null) {
				Console.WriteLine("[NSUrlSessionManager] DidCompleteWithError NullTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidCompleteWithError_NullTaskId
				});
				return;
			}
			int taskid;
			if (!int.TryParse (staskdescription, out taskid)) {
				Console.WriteLine("[NSUrlSessionManager] DidCompleteWithError InvalidTaskId");

				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidCompleteWithError_InvalidTaskId
				});
				return;
			}

			if (error != null) {
				Console.WriteLine("[NSUrlSessionManager] DidCompleteWithError Error");
				Console.WriteLine("[NSUrlSessionManager] DidCompleteWithError Description : {0}", error.LocalizedDescription);
				Console.WriteLine("[NSUrlSessionManager] DidCompleteWithError TaskId      : {0}", taskid);
				_bus.SendAsync<TaskError> (new TaskError {
					Id = taskid,
					Error = TaskErrorEnum.DownloadError,
					Description = error.LocalizedDescription
				});
				return;
			}

		}


		public override void DidFinishDownloading (NSUrlSession session, NSUrlSessionDownloadTask task, NSUrl location)
		{

			Console.WriteLine("[NSUrlSessionManager] DidFinishDownloading");
			Console.WriteLine("[NSUrlSessionManager] DidFinishDownloading Session  : {0}", session.Description);
			Console.WriteLine("[NSUrlSessionManager] DidFinishDownloading Task     : {0}", task.Description);
			Console.WriteLine("[NSUrlSessionManager] DidFinishDownloading Location : {0}", location.Path);

			_bus.SendAsync<FreeSlot> (new FreeSlot());

			var staskdescription = task.TaskDescription;
			if (staskdescription == null) {
				Console.WriteLine("[NSUrlSessionManager] DidFinishDownloading NullTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidFinishDownloading_NullTaskId
				});
				return;
			}

			int taskid;
			if (!int.TryParse (staskdescription, out taskid)) {
				Console.WriteLine ("[NSUrlSessionManager] DidFinishDownloading InvalidTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidFinishDownloading_InvalidTaskId
				});
				return;
			}

			var response = task.Response as NSHttpUrlResponse;
			if (response == null) {
				Console.WriteLine ("[NSUrlSessionManager] DidFinishDownloading NullResponse");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidFinishDownloading_NullResponse
				});
				return;
			}

			int statuscode = (int)response.StatusCode;
			if (statuscode != 200) {
				
				Console.WriteLine ("[NSUrlSessionManager] DidFinishDownloading StatusCode");
				Console.WriteLine ("[NSUrlSessionManager] DidFinishDownloading StatusCode : {0}", statuscode);

				_bus.SendAsync<TaskError> (new TaskError {
					Id = taskid,
					Error = TaskErrorEnum.InvalidResponse,
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
			Console.WriteLine("[NSUrlSessionManager] DidFinishEventsForBackgroundSession");
			_bus.SendAsync<BackgroundSessionEnded> (new BackgroundSessionEnded {
			});
		}

//		public override void DidReceiveChallenge (NSUrlSession session, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
//		{
//			Console.WriteLine("[NSUrlSessionManager] DidReceiveChallenge");
//			Console.WriteLine ("[NSUrlSessionManager] DidReceiveChallenge");
//			Console.WriteLine("[NSUrlSessionManager] DidReceiveChallenge");
//
//			completionHandler (challenge, NSUrlCredential.);
//		}

		public override void DidReceiveChallenge (NSUrlSession session, NSUrlSessionTask task, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
		{
			Console.WriteLine("[NSUrlSessionManager] DidReceiveChallenge2");
		}

		public override void DidResume (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
		{
			Console.WriteLine("[NSUrlSessionManager] DidResume");
		}

		public override void DidSendBodyData (NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend)
		{
			Console.WriteLine("[NSUrlSessionManager] DidSendBodyData");
		}

		public override void DidWriteData (NSUrlSession session, NSUrlSessionDownloadTask task, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
		{
			
			Console.WriteLine("[NSUrlSessionManager] DidWriteData");
			Console.WriteLine("[NSUrlSessionManager] DidWriteData Task     : {0}", task.Description);
			Console.WriteLine("[NSUrlSessionManager] DidWriteData Session  : {0}", session.Description);
			Console.WriteLine("[NSUrlSessionManager] DidWriteData Written  : {0}", bytesWritten);
			Console.WriteLine("[NSUrlSessionManager] DidWriteData Total    : {0}", totalBytesWritten);
			Console.WriteLine("[NSUrlSessionManager] DidWriteData Expected : {0}", totalBytesExpectedToWrite);

			var staskdescription = task.TaskDescription;
			if (staskdescription == null) {
				Console.WriteLine("[NSUrlSessionManager] DidWriteData NullTaskId");
				_bus.SendAsync<DownloadError> (new DownloadError {
					Error = ErrorEnum.DidWriteData_NullTaskId
				});
				return;
			}

			int taskid;
			if (!int.TryParse (staskdescription, out taskid)) {
				Console.WriteLine("[NSUrlSessionManager] DidWriteData InvalidTaskId");
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
			Console.WriteLine("[NSUrlSessionManager] NeedNewBodyStream");
		}

		public override void WillPerformHttpRedirection (NSUrlSession session, NSUrlSessionTask task, NSHttpUrlResponse response, NSUrlRequest newRequest, Action<NSUrlRequest> completionHandler)
		{
			Console.WriteLine("[NSUrlSessionManager] WillPerformHttpRedirection");
		}
	}
}

