using System;
using Fabrik.SimpleBus;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using DownloadManager.iOS.Bo;
using Foundation;
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

			_bus.Subscribe<ProgressDownload> (ProgressDownload_Received);
			_bus.Subscribe<FinishedDownload> (FinishedDownload_Received);
			_bus.Subscribe<DownloadError> (DownloadError_Received);
			_bus.Subscribe<TaskError> (TaskError_Received);
			_bus.Subscribe<QueueEmpty> (QueueEmpty_Received);

		}

		private void ProgressDownload_Received(ProgressDownload progress) {

			Console.WriteLine("[Downloader] ProgressDownload");
			Console.WriteLine("[Downloader] ProgressDownload Id : {0}", progress.Id);
			Console.WriteLine("[Downloader] ProgressDownload Total : {0}", progress.Total);
			Console.WriteLine("[Downloader] ProgressDownload Written : {0}", progress.Written);

			Download download;
			bool found = _repository.TryById (progress.Id, out download);
			if (!found) {
				return;
			}

		}

		private void FinishedDownload_Received(FinishedDownload finished) {

			Console.WriteLine("[Downloader] FinishedDownload");
			Console.WriteLine("[Downloader] FinishedDownload Id : {0}", finished.Id);
			Console.WriteLine("[Downloader] FinishedDownload Location : {0}", finished.Location);

		}

		private void QueueEmpty_Received(QueueEmpty empty) {
		
			Console.WriteLine ("[Downloader] QueueEmpty");

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

		public async Task<NSProgress> Queue (string url)
		{
			Console.WriteLine("[Downloader] Queue");
			Console.WriteLine("[Downloader] Queue url : {0}", url);

			await _bus.SendAsync<QueueUrl> (new QueueUrl {
				Url = url
			});

			return _progress.Queue (url);

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

