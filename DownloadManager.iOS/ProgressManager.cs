using System;
using System.Collections.Concurrent;
using Fabrik.SimpleBus;
using DownloadManager.iOS.Bo;

namespace DownloadManager.iOS
{
	public class ProgressManager
	{

		private ConcurrentDictionary<string, Progress> _progresses;
		private readonly IDownloadRepository _repository;
		private readonly IBus _bus;
		private readonly Progress _rootprogress;

		public ProgressManager (IBus bus, IDownloadRepository repo)
		{
			_progresses = new ConcurrentDictionary<string, Progress> ();
			_repository = repo;
			_bus = bus;
			_rootprogress = new Progress ();

			_bus.Subscribe<NotifyProgress> (NotifyProgress);
			_bus.Subscribe<ResetDownloads> (ResetDownloads);

			Init ();

		}

		private void Init() {
			var all = _repository.All ();
			foreach (var item in all) {
				_progresses.GetOrAdd (item.Url, Progress);
			}
		}

		private void Progress(Download download) {
			var progress = _progresses.GetOrAdd (download.Url, Progress);
			progress.Notify (download);

		}

		private Progress Progress(string url) {
			var progress = new Progress (_rootprogress);
			return progress;
		}

		public Progress Queue(string url, Action<Download> action) {

			var progress = _progresses.GetOrAdd (url, Progress);
			progress.Changed += action;
			return progress;
		}

		public void NotifyProgress(NotifyProgress notify) 
		{
			Console.WriteLine("[ProgressManager] NotifyProgress");
			Console.WriteLine("[ProgressManager] NotifyProgress Url     : {0}", notify.Url);
			Console.WriteLine("[ProgressManager] NotifyProgress Id      : {0}", notify.Download.Id);
			Console.WriteLine("[ProgressManager] NotifyProgress Total   : {0}", notify.Download.Total);
			Console.WriteLine("[ProgressManager] NotifyProgress Written : {0}", notify.Download.Written);

			Progress progress;
			_progresses.TryGetValue (notify.Url, out progress);

			progress.Notify (notify.Download);
		}

		public void ResetDownloads(ResetDownloads reset) 
		{
			Console.WriteLine("[ProgressManager] ResetDownloads");
			var progresses = _progresses.Values;
			_progresses.Clear ();
			foreach (var progress in progresses) {
				progress.Reset ();
			}
		}

//
//		public void TaskError(TaskError error) 
//		{
//			Console.WriteLine("[ProgressManager] TaskError");
//			Console.WriteLine("[ProgressManager] TaskError Id : {0}", error.Id);
//			Console.WriteLine("[ProgressManager] TaskError Error : {0}", error.Error);
//			Console.WriteLine("[ProgressManager] TaskError Description : {0}", error.Description);
//
//			Download download;
//			bool found = _repository.TryById (error.Id, out download);
//			if (!found) {
//				return;
//			}
//			Progress progress;
//			_progresses.TryGetValue (download.Url, out progress);
//
//			if (progress.Changed != null) {
//				progress.Changed (download);
//			}
//		}
//
//		public void ProgressDownload(ProgressDownload progress) {
//
//			Console.WriteLine("[ProgressManager] ProgressDownload");
//			Console.WriteLine("[ProgressManager] ProgressDownload Id : {0}", progress.Id);
//			Console.WriteLine("[ProgressManager] ProgressDownload Total : {0}", progress.Total);
//			Console.WriteLine("[ProgressManager] ProgressDownload Written : {0}", progress.Written);
//
//			Download download;
//			bool found = _repository.TryById (progress.Id, out download);
//			if (!found) {
//				return;
//			}
//
//			Progress (download);
//
//
//
//		}
//
//		public void FinishedDownload(FinishedDownload finished) {
//
//			Console.WriteLine("[ProgressManager] FinishedDownload");
//			Console.WriteLine("[ProgressManager] FinishedDownload Id : {0}", finished.Id);
//			Console.WriteLine("[ProgressManager] FinishedDownload Location : {0}", finished.Location);
//
//			Download download;
//			bool found = _repository.TryById (finished.Id, out download);
//			if (!found) {
//				return;
//			}
//			Progress (download);
//
//		}

	}
}

