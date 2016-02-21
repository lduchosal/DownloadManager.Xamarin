using System;
using System.Collections.Concurrent;
using Fabrik.SimpleBus;
using Foundation;
using DownloadManager.iOS.Bo;

namespace DownloadManager.iOS
{
	public class ProgressManager
	{

		private ConcurrentDictionary<string, Foundation.NSProgress> _progresses;
		private readonly IDownloadRepository _repository;
		private readonly IBus _bus;
		private readonly NSProgress _rootprogress;

		public ProgressManager (IBus bus, IDownloadRepository repo)
		{
			_progresses = new ConcurrentDictionary<string, Foundation.NSProgress> ();
			_repository = repo;
			_bus = bus;
			_rootprogress = new NSProgress ();
			_rootprogress.BecomeCurrent (100);

			_bus.Subscribe<ProgressDownload> (ProgressDownload);
			_bus.Subscribe<FinishedDownload> (FinishedDownload);
			_bus.Subscribe<TaskError> (TaskError);

			Init ();

		}

		private void Init() {
			var all = _repository.All ();
			foreach (var item in all) {
				_progresses.GetOrAdd (item.Url, NSProgress);
			}
		}

		private void Progress(Download download) {
			var progress = _progresses.GetOrAdd (download.Url, NSProgress);
			progress.TotalUnitCount = download.Total;
			progress.CompletedUnitCount = download.Written;

		}

		private NSProgress NSProgress(string url) {
			var progress = new NSProgress (_rootprogress, new NSDictionary()) {
				Pausable = true,
				Cancellable = true,
			};

			return progress;
		}

		public NSProgress Queue(string url) {

			var progress = _progresses.GetOrAdd (url, NSProgress);
			return progress;
		}


		public void TaskError(TaskError error) 
		{
			Console.WriteLine("[Downloader] TaskError");
			Console.WriteLine("[Downloader] TaskError Id : {0}", error.Id);
			Console.WriteLine("[Downloader] TaskError Error : {0}", error.Error);
			Console.WriteLine("[Downloader] TaskError Description : {0}", error.Description);

			Download download;
			bool found = _repository.TryById (error.Id, out download);
			if (!found) {
				return;
			}
			NSProgress progress;
			_progresses.TryGetValue (download.Url, out progress);
			progress.Cancel ();
		}

		public void ProgressDownload(ProgressDownload progress) {

			Console.WriteLine("[Downloader] ProgressDownload");
			Console.WriteLine("[Downloader] ProgressDownload Id : {0}", progress.Id);
			Console.WriteLine("[Downloader] ProgressDownload Total : {0}", progress.Total);
			Console.WriteLine("[Downloader] ProgressDownload Written : {0}", progress.Written);

			Download download;
			bool found = _repository.TryById (progress.Id, out download);
			if (!found) {
				return;
			}

			Progress (download);

		}

		public void FinishedDownload(FinishedDownload finished) {

			Console.WriteLine("[Downloader] FinishedDownload");
			Console.WriteLine("[Downloader] FinishedDownload Id : {0}", finished.Id);
			Console.WriteLine("[Downloader] FinishedDownload Location : {0}", finished.Location);

			Download download;
			bool found = _repository.TryById (finished.Id, out download);
			if (!found) {
				return;
			}
			Progress (download);

		}

	}
}

