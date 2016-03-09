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
		private Action _completion;

		public Action BackgroundCompletionHandler {
			set {
				_completion = value;
			} 
		}

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
			_bus.Subscribe<TaskError> (TaskError);
			_bus.Subscribe<FreeSlot> (FreeSlot);
			_bus.Subscribe<BackgroundSessionEnded> (BackgroundSessionEnded);
		}

		public void BackgroundSessionEnded(BackgroundSessionEnded ended) {

			Console.WriteLine("[Downloadanager] BackgroundSessionEnded");

			if (_completion == null) {
				return;
			}
			_completion ();
			_completion = null;
		}

		public async void QueueUrl(QueueUrl qurl) {

			Console.WriteLine("[Downloadanager] QueueUrl");
			Console.WriteLine("[Downloadanager] QueueUrl Url : {0}", qurl.Url);

			string url = qurl.Url;
			Download result = null;
			bool exists = _repo.TryByUrl (url, out result);
			if (exists) {

				await _bus.SendAsync<NotifyProgress> (new NotifyProgress {
					Url = result.Url,
					Download = result
				});

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
			await _bus.SendAsync<NotifyProgress> (new NotifyProgress {
				Url = insert.Url,
				Download = insert
			});

			return;
		}

		public async void FreeSlot(FreeSlot freeslot) {

			Console.WriteLine("[Downloadanager] FreeSlot");

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

			Console.WriteLine("[Downloadanager] ProgressDownload");
			Console.WriteLine("[Downloadanager] ProgressDownload Id : {0}", progress.Id);
			Console.WriteLine("[Downloadanager] ProgressDownload Written : {0}", progress.Written);
			Console.WriteLine("[Downloadanager] ProgressDownload Total : {0}", progress.Total);

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
			_repo.Update (download);

			await _bus.SendAsync<NotifyProgress> (new NotifyProgress {
				Url = download.Url,
				Download = download
			});

			if (!progressed) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = progress.Id,
					Error = ErrorEnum.ProgressDownload_OutOfOrder
				});
				return;
			}

		}

		public void ResetDownloads (ResetDownloads reset)
		{
			Console.WriteLine("[Downloadanager] ResetDownloads");

			_repo.DeleteAll ();
		}

		public async void FinishedDownload (FinishedDownload finished) {

			Console.WriteLine("[Downloadanager] FinishedDownload");
			Console.WriteLine("[Downloadanager] FinishedDownload Id : {0}", finished.Id);
			Console.WriteLine("[Downloadanager] FinishedDownload Location : {0}", finished.Location);

			Download download;
			bool found = _repo.TryById(finished.Id, out download);

			if (!found) {
				var error = new DownloadError {
					Id = finished.Id,
					Error = ErrorEnum.FinishedDownload_IdentifierNotFound
				};
				await _bus.SendAsync<DownloadError> (error);
				return;
			}

			bool progressed = download.TryFinish (finished.Location);
			if (!progressed) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = download.Id,
					State = download.State,
					Error = ErrorEnum.FinishedDownload_InvalidState
				});
				return;
			}

			_repo.Update (download);

			await _bus.SendAsync<NotifyProgress> (new NotifyProgress {
				Url = download.Url,
				Download = download
			});
		}

		public async void CancelDownloads(CancelDownloads cancel) {
			
			Console.WriteLine("[Downloadanager] CancelDownloads");

			var queued = _repo.ByState (new [] { 
				State.Downloading, 
				State.Waiting, 
				State.Error
			});

			foreach (var queue in queued) {
				queue.Cancel ();
			}

			_repo.UpdateAll (queued);


			foreach (var queue in queued) {
				await _bus.SendAsync<NotifyProgress> (new NotifyProgress {
					Url = queue.Url,
					Download = queue
				});
			}
		}

		public async void DownloadRejected(DownloadRejected rejected) {

			Console.WriteLine("[Downloadanager] DownloadRejected");
			Console.WriteLine("[Downloadanager] DownloadRejected Id     : {0}", rejected.Id);
			Console.WriteLine("[Downloadanager] DownloadRejected Reason : {0}", rejected.Reason);

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

			Console.WriteLine("[Downloadanager] DownloadError");
			Console.WriteLine("[Downloadanager] DownloadError Id          : {0}", error.Id);
			Console.WriteLine("[Downloadanager] DownloadError Error       : {0}", error.Error);
			Console.WriteLine("[Downloadanager] DownloadError Description : {0}", error.Description);
			Console.WriteLine("[Downloadanager] DownloadError State       : {0}", error.State);

		}

		public async void TaskError(TaskError error) {

			Console.WriteLine("[Downloadanager] TaskError");
			Console.WriteLine("[Downloadanager] TaskError Id          : {0}", error.Id);
			Console.WriteLine("[Downloadanager] TaskError Error       : {0}", error.Error);
			Console.WriteLine("[Downloadanager] TaskError Description : {0}", error.Description);
			Console.WriteLine("[Downloadanager] TaskError StatusCode  : {0}", error.StatusCode);

			Download download;
			bool found = _repo.TryById(error.Id, out download);

			if (!found) {
				var invalididentifier = new DownloadError {
					Id = error.Id,
					Error = ErrorEnum.TaskError_IdentifierNotFound
				};
				await _bus.SendAsync<DownloadError> (invalididentifier);
				return;
			}

			bool failed = download.TryFail (error.StatusCode);
			if (!failed) {
				await _bus.SendAsync<DownloadError> (new DownloadError {
					Id = download.Id,
					State = download.State,
					Error = ErrorEnum.TaskError_InvalidState
				});
				return;
			}
			_repo.Update (download);

			await _bus.SendAsync<NotifyProgress> (new NotifyProgress {
				Url = download.Url,
				Download = download
			});

		}


	}
}

