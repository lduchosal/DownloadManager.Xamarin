using System;
using Fabrik.SimpleBus;
using DownloadManager.iOS.Bo;
using System.Diagnostics;

namespace DownloadManager.iOS
{
	public class DownloadManager
	{
		private readonly IDownloadRepository _repo;
		private readonly IBus _bus;
		private readonly int _maxdownload;

		public DownloadManager (IBus bus, IDownloadRepository repo, int maxdownloads)
		{
			_repo = repo;
			_maxdownload = maxdownloads;
			_bus = bus;
			_bus.Subscribe<QueueUrl> (QueueUrl);
			_bus.Subscribe<ResetDownloads> (ResetDownloads);
			_bus.Subscribe<ProgressDownload> (ProgressDownload);
			_bus.Subscribe<FinishedDownload> (FinishedDownload);
			_bus.Subscribe<CancelDownloads> (CancelDownloads);
			_bus.Subscribe<DownloadRejected> (DownloadRejected);
			_bus.Subscribe<DownloadError> (DownloadError);
			_bus.Subscribe<FreeSlot> (FreeSlot);
		}

		public async void QueueUrl(QueueUrl qurl) {

			Trace.TraceInformation("[Downloadanager] QueueUrl");
			Trace.TraceInformation("[Downloadanager] QueueUrl Url : {0}", qurl.Url);

			string url = qurl.Url;
			Download result = null;
			bool exists = _repo.TryByUrl (url, out result);
			if (exists) {
				await _bus.SendAsync<AlreadyQueued> (new AlreadyQueued { 
					Url = url
				});
				return;
			}

			var insert = new Bo.Download {
				State = Bo.State.Waiting,
				Url = url,
			};
			_repo.Insert(insert);
			await _bus.SendAsync<CheckFreeSlot> (new CheckFreeSlot ());
			return;
		}

		public async void FreeSlot(FreeSlot freeslot) {

			Trace.TraceInformation("[Downloadanager] FreeSlot");

			int downloading = _repo.CountByState (State.Downloading);
			if (downloading >= _maxdownload) {
				await _bus.SendAsync<QueueFull> (new QueueFull{
					Downloading = downloading,
					MaxDownload = _maxdownload
				});
				return;
			}

			Download waiting;
			bool dequeued = _repo.FirstByState (State.Waiting, out waiting);
			if (!dequeued) {
				await _bus.SendAsync<QueueEmpty> (new QueueEmpty());
				return;
			}

			bool resumed = waiting.TryResume ();
			if (!resumed) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = waiting.Id,
					State = waiting.State,
					Error = ErrorEnum.FreeSlot_InvalidState
				});
				return;
			}
			_repo.Update (waiting);

			var queuedownload = new QueueDownload {
				Download = waiting
			};
			await _bus.SendAsync<QueueDownload> (queuedownload);
		}

		public async void ProgressDownload (ProgressDownload progress) {

			Trace.TraceInformation("[Downloadanager] ProgressDownload");
			Trace.TraceInformation("[Downloadanager] ProgressDownload Id : {0}", progress.Id);
			Trace.TraceInformation("[Downloadanager] ProgressDownload Written : {0}", progress.Written);
			Trace.TraceInformation("[Downloadanager] ProgressDownload Total : {0}", progress.Total);

			Download download;
			bool found = _repo.TryById (progress.Id, out download);

			if (!found) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = progress.Id,
					Error = ErrorEnum.ProgressDownload_IdentifierNotFound

				});
				return;
			}

			bool progressed = download.TryProgress (progress.Written, progress.Total);
			if (!progressed) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = progress.Id,
					Error = ErrorEnum.ProgressDownload_OutOfOrder
				});
				return;
			}
			_repo.Update (download);

			

		}

		public void ResetDownloads (ResetDownloads reset)
		{
			Trace.TraceInformation("[Downloadanager] ResetDownloads");

			_repo.DeleteAll ();
		}

		public async void FinishedDownload (FinishedDownload finished) {

			Trace.TraceInformation("[Downloadanager] FinishedDownload");
			Trace.TraceInformation("[Downloadanager] FinishedDownload Id : {0}", finished.Id);
			Trace.TraceInformation("[Downloadanager] FinishedDownload Location : {0}", finished.Location);

			Download result;
			bool found = _repo.TryById(finished.Id, out result);

			if (!found) {
				var error = new DownloadError {
					Id = finished.Id,
					Error = ErrorEnum.FinishedDownload_IdentifierNotFound
				};
				await _bus.SendAsync<DownloadError> (error);
				return;
			}

			bool progressed = result.TryFinish (finished.Location);
			if (!progressed) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = result.Id,
					State = result.State,
					Error = ErrorEnum.FinishedDownload_InvalidState
				});
				return;
			}

			_repo.Update (result);

		}

		public async void CancelDownloads(CancelDownloads cancel) {
			
			Trace.TraceInformation("[Downloadanager] CancelDownloads");

			var queued = _repo.ByState (new [] { 
				State.Downloading, 
				State.Waiting, 
				State.Error
			});

			foreach (var queue in queued) {
				queue.Cancel ();
			}

			_repo.UpdateAll (queued);
		}

		public async void DownloadRejected(DownloadRejected rejected) {

			Trace.TraceInformation("[Downloadanager] DownloadRejected");
			Trace.TraceInformation("[Downloadanager] DownloadRejected Id     : {0}", rejected.Id);
			Trace.TraceInformation("[Downloadanager] DownloadRejected Reason : {0}", rejected.Reason);

			Download download;
			bool found = _repo.TryById(rejected.Id, out download);

			if (!found) {
				var error = new DownloadError {
					Id = rejected.Id,
					Error = ErrorEnum.DownloadRejected_IdentifierNotFound
				};
				await _bus.SendAsync<DownloadError> (error);
				return;
			}

			bool paused = download.TryPause ();
			if (!paused) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = download.Id,
					State = download.State,
					Error = ErrorEnum.DownloadRejected_InvalidState
				});
				return;
			}
			_repo.Update (download);

		}


		public async void DownloadError(DownloadError error) {

			Trace.TraceInformation("[Downloadanager] DownloadError");
			Trace.TraceInformation("[Downloadanager] DownloadError Id          : {0}", error.Id);
			Trace.TraceInformation("[Downloadanager] DownloadError Error       : {0}", error.Error);
			Trace.TraceInformation("[Downloadanager] DownloadError Description : {0}", error.Description);
			Trace.TraceInformation("[Downloadanager] DownloadError State       : {0}", error.State);
			Trace.TraceInformation("[Downloadanager] DownloadError StatusCode  : {0}", error.StatusCode);

			if (error.Error != ErrorEnum.DidFinishDownloading_ResponseError) {
				return;
			}

			Download download;
			bool found = _repo.TryById(error.Id, out download);

			if (!found) {
				var invalididentifier = new DownloadError {
					Id = error.Id,
					Error = ErrorEnum.DownloadError_IdentifierNotFound
				};
				await _bus.SendAsync<DownloadError> (invalididentifier);
				return;
			}

			bool failed = download.TryFail (error.StatusCode);
			if (!failed) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = download.Id,
					State = download.State,
					Error = ErrorEnum.DownloadError_InvalidState
				});
				return;
			}
			_repo.Update (download);

		}


	}
}

