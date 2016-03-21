using System;
using Fabrik.SimpleBus;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloadManager.iOS.Bo;
using System.Threading;

namespace DownloadManager.iOS
{
	public class Downloader
	{
		
		private readonly NSUrlSessionManager _service;
		private readonly DownloadRepository _repository;
		private readonly DownloadManager _manager;
		private readonly InProcessBus _bus;
		private readonly ProgressManager _progress;
		private readonly Timer _timer;
		private Action<string, string, string> _finished;

		public System.Action Completion {
			set {
				_manager.BackgroundCompletionHandler = value;
			}
		}

		private event ProgressEvent _progressevent = delegate {};
		public event ProgressEvent Progress {
			add {
				_progressevent += value;
			}
			remove {
				_progressevent -= value;
			}
		}

		public Action<string, string, string>  Finished {
			set {
				_finished = value;;
			}
		}

		private bool _enabled = false;
		public bool Enabled {
			get {
				return _enabled;
			}
			set {
				_enabled = value;
			}
		}

		public bool TryDetail (string url, out Download result)
		{
			return _repository.TryByUrl (url, out result);
		}

		public delegate void ProgressEvent (GlobalProgress progress) ;

		public Downloader ()
		{
			var personal = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var dbpath = Path.Combine (personal, "download.db");
			var db = new SQLiteConnection (
				dbpath, 
				SQLiteOpenFlags.ReadWrite 
				| SQLiteOpenFlags.Create 
				| SQLiteOpenFlags.FullMutex , 
				true) {
				#if DEBUG
				Trace = true
				#endif
			};

			int maxdownloads = 3;
			_bus = new InProcessBus ();
			_repository = new DownloadRepository (db);
			_manager = new DownloadManager (_bus, _repository, maxdownloads);
			_service = new NSUrlSessionManager (_bus, maxdownloads);
			_progress = new ProgressManager (_bus, _repository);
			_timer = new Timer (TimerCallback, null, 1000, 1000);
			_timer = new Timer (TimerCallback, null, 1000, 1000);

			_bus.Subscribe<DownloadError> (DownloadError_Received);
			_bus.Subscribe<TaskError> (TaskError_Received);
			_bus.Subscribe<QueueEmpty> (QueueEmpty_Received);
			_bus.Subscribe<GlobalProgress> (GlobalProgress_Received);
			_bus.Subscribe<NotifyFinish> (NotifyFinish_Received);

		}

		private void TimerCallback(object state) {
			
			Console.WriteLine ("[Downloader] TimerCallback");

			var globalprogress = _bus.SendAsync<NotifyGlobalProgress> (new NotifyGlobalProgress ());
			var freeslot = _bus.SendAsync<CheckFreeSlot> (new CheckFreeSlot() {});
			var stopped = _bus.SendAsync<CheckStoppedDownload> (new CheckStoppedDownload() {});
			var throttlenotify = _bus.SendAsync<ThrottleNotifyProgress> (new ThrottleNotifyProgress() {});

			globalprogress.ContinueWith ((t) => {
				Console.WriteLine ("[Downloader] NotifyGlobalProgress");
			});
			freeslot.ContinueWith ((t) => {
				Console.WriteLine ("[Downloader] CheckFreeSlot");
			});
			stopped.ContinueWith ((t) => {
				Console.WriteLine ("[Downloader] CheckStoppedDownload");
			});
			throttlenotify.ContinueWith ((t) => {
				Console.WriteLine ("[Downloader] ThrottleNotifyProgress");
			});
		}

		private void QueueEmpty_Received(QueueEmpty empty) {
			Console.WriteLine ("[Downloader] QueueEmpty");
		}

		private void NotifyFinish_Received(NotifyFinish notify) {
			
			Console.WriteLine ("[Downloader] NotifyFinish");
			Console.WriteLine ("[Downloader] NotifyFinish Url         : {0}", notify.Url);
			Console.WriteLine ("[Downloader] NotifyFinish Id          : {0}", notify.Download.Id);
			Console.WriteLine ("[Downloader] NotifyFinish Temporary   : {0}", notify.Download.Temporary);
			Console.WriteLine ("[Downloader] NotifyFinish Description : {0}", notify.Download.Description);

			bool fileexists = File.Exists (notify.Download.Temporary);
			Console.WriteLine ("[Downloader] NotifyFinish fileexists : {0}", fileexists);
			if (_finished != null) {
				_finished.Invoke (notify.Url, notify.Download.Description, notify.Download.Temporary);
			}
			notify.FileLock.Set ();

		}

		private void GlobalProgress_Received(GlobalProgress progress) {

			Console.WriteLine ("[Downloader] GlobalProgress");
			Console.WriteLine ("[Downloader] GlobalProgress Total : {0}", progress.Total);
			Console.WriteLine ("[Downloader] GlobalProgress Written : {0}", progress.Written);

			_progressevent (progress);
			if (progress.Total == progress.Written) {
				_timer.Change (int.MaxValue, int.MaxValue);
			}

		}

		private void QueueFull_Received(QueueFull full) 
		{
			Console.WriteLine ("[Downloader] QueueFull");
			Console.WriteLine ("[Downloader] QueueFull MaxDownload : {0}", full.MaxDownload);
			Console.WriteLine ("[Downloader] QueueFull Downloading : {0}", full.Downloading);
		}

		private void DownloadError_Received(DownloadError error) 
		{
			Console.WriteLine("[Downloader] DownloadError");
			Console.WriteLine("[Downloader] DownloadError Error : {0}", error.Error);
			Console.WriteLine("[Downloader] DownloadError Description : {0}", error.Description);
			Console.WriteLine("[Downloader] DownloadError (State : {0})", error.State);
		}

		private void TaskError_Received(TaskError error) 
		{
			Console.WriteLine("[Downloader] TaskError");
			Console.WriteLine("[Downloader] TaskError Id : {0}", error.Id);
			Console.WriteLine("[Downloader] TaskError Error : {0}", error.Error);
			Console.WriteLine("[Downloader] TaskError Description : {0}", error.Description);
		}

		public Progress Queue (string url, string description, Action<Download> action)
		{
			Console.WriteLine("[Downloader] Queue");
			Console.WriteLine("[Downloader] Queue url         : {0}", url);
			Console.WriteLine("[Downloader] Queue description : {0}", description);
			_timer.Change (1000, 1000);

			var progress = _progress.Queue (url, action);
			_bus.SendAsync<QueueUrl> (new QueueUrl {
				Url = url,
				Description = description
			});
			return progress;
		}

		public async Task Reset ()
		{
			Console.WriteLine("[Downloader] Reset");
			await _bus.SendAsync<ResetDownloads> (new ResetDownloads {});
		}

		public List<Bo.Download> List ()
		{
			Console.WriteLine("[Downloader] List");
			return _repository.All ();
		}

	}
}

