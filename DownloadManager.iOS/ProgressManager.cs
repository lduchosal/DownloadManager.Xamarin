using System;
using System.Collections.Concurrent;
using Fabrik.SimpleBus;
using DownloadManager.iOS.Bo;
using System.Threading.Tasks;
using System.Threading;

namespace DownloadManager.iOS
{
	public class ProgressManager
	{

		private readonly ConcurrentDictionary<string, Progress> _progresses;
		private readonly ConcurrentDictionary<string, Download> _notificationqueue;
		private readonly IDownloadRepository _repository;
		private readonly IBus _bus;

		public ProgressManager (IBus bus, IDownloadRepository repo)
		{
			_progresses = new ConcurrentDictionary<string, Progress> ();
			_notificationqueue = new ConcurrentDictionary<string, Download> ();
			_repository = repo;
			_bus = bus;

			_bus.Subscribe<NotifyGlobalProgress> (NotifyGlobalProgress);
			_bus.Subscribe<NotifyProgress> (NotifyProgress);
			_bus.Subscribe<ResetDownloads> (ResetDownloads);
			_bus.Subscribe<ThrottleNotifyProgress> (ThrottleNotifyProgress);

		}

		private void Progress(Download download) {
			Progress progress; 
			bool hassubscriber = _progresses.TryGetValue (download.Url, out progress);
			if (hassubscriber) {
				progress.Notify (download);
			}

		}

		public Progress Queue(string url, Action<Download> progressevent) {

			var progress = _progresses.GetOrAdd (url, new Progress(progressevent));
			return progress;
		}

		public void NotifyProgress(NotifyProgress notify) 
		{
			Console.WriteLine("[ProgressManager] NotifyProgress");
			Console.WriteLine("[ProgressManager] NotifyProgress Url     : {0}", notify.Url);
			Console.WriteLine("[ProgressManager] NotifyProgress Id      : {0}", notify.Download.Id);
			Console.WriteLine("[ProgressManager] NotifyProgress Total   : {0}", notify.Download.Total);
			Console.WriteLine("[ProgressManager] NotifyProgress Written : {0}", notify.Download.Written);

			_notificationqueue.AddOrUpdate (notify.Url, notify.Download, (url, download) => {
				return download.LastModified > notify.Download.LastModified
					? download : notify.Download;
			});

		}

		public void ThrottleNotifyProgress(ThrottleNotifyProgress notify)  {
			Console.WriteLine("[ProgressManager] ThrottleNotifyProgress");
			var keys = _notificationqueue.Keys;
			foreach (string key in keys) {
				Download download;
				if (!_notificationqueue.TryRemove (key, out download)) {
					continue;
				}
				Progress progress;
				if (!_progresses.TryGetValue (download.Url, out progress)) {
					continue;
				}
				progress.Notify (download);
			}
		}

		private async void NotifyGlobalProgress(NotifyGlobalProgress notify) {

			Console.WriteLine("[ProgressManager] NotifyGlobalProgress");

			long total = 100;
			long written = 100;
			foreach (var item in _repository.All()) {

				long itotal = item.State == State.Finished ? 100
					: item.State == State.Error ? 100
					: item.State == State.Waiting ? 100
					: item.State == State.Downloading 
					? (item.Total == 0 ? 100 : item.Total)
					: 100;


				long iwritten = item.State == State.Finished ? 100
					: item.State == State.Error ? 100
					: item.State == State.Waiting ? 0
					: item.State == State.Downloading 
						? item.Written : 0;

				itotal = Math.Max(itotal, iwritten);

				int percent = (int)((iwritten / (float)itotal) * 100);
				written += percent;
				total += 100;
			}

			await _bus.SendAsync<GlobalProgress> (new GlobalProgress {
				Total = total,
				Written = written
			});

		}

		public void ResetDownloads(ResetDownloads reset) 
		{
			Console.WriteLine("[ProgressManager] ResetDownloads");

			var progresses = _progresses.Values;
			_progresses.Clear ();
			foreach (var progress in progresses) {
				progress.Dispose ();
			}
		}
	}
}

