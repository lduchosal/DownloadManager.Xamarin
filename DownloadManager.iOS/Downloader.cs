using System;
using Fabrik.SimpleBus;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using DownloadManager.iOS.Bo;
using System.Collections.Concurrent;

namespace DownloadManager.iOS
{
	public class Downloader
	{
		
		private readonly NSUrlSessionManager _service;
		private readonly DownloadRepository _repository;
		private readonly DownloadManager _manager;
		private readonly InProcessBus _bus;
		private readonly ProgressManager _progress;

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

			int maxdownloads = 50;
			_bus = new InProcessBus ();
			_repository = new DownloadRepository (db);
			_manager = new DownloadManager (_bus, _repository, maxdownloads);
			_service = new NSUrlSessionManager (_bus, maxdownloads);
			_progress = new ProgressManager (_bus, _repository);

			_bus.Subscribe<DownloadError> (DownloadError_Received);
			_bus.Subscribe<TaskError> (TaskError_Received);
			_bus.Subscribe<QueueEmpty> (QueueEmpty_Received);
			_bus.Subscribe<GlobalProgress> (GlobalProgress_Received);

		}

		private void QueueEmpty_Received(QueueEmpty empty) {

			Console.WriteLine ("[Downloader] QueueEmpty");


		}
		private void GlobalProgress_Received(GlobalProgress progress) {

			Console.WriteLine ("[Downloader] GlobalProgress");
			Console.WriteLine ("[Downloader] GlobalProgress Total : {0}", progress.Total);
			Console.WriteLine ("[Downloader] GlobalProgress Written : {0}", progress.Written);

			_progressevent (progress);
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

		public void Queue (string url, Action<Download> action)
		{
			Console.WriteLine("[Downloader] Queue");
			Console.WriteLine("[Downloader] Queue url : {0}", url);

			var progress = _progress.Queue (url, action);
			_bus.SendAsync<QueueUrl> (new QueueUrl {
				Url = url
			});

		}


		public void Run ()
		{
			Console.WriteLine("[Downloader] Run");
			var task = _bus.SendAsync<CheckFreeSlot> (new CheckFreeSlot() {
			});
			task.RunSynchronously ();
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

