using System;
using Fabrik.SimpleBus;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DownloadManager.iOS
{
	public class Downloader
	{
		
		private NSUrlSessionManager _service;
		private DownloadRepository _repository;
		private DownloadManager _manager;
		private InProcessBus _bus;

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

			_bus.Subscribe<ProgressDownload> (ProgressDownload_Received);
			_bus.Subscribe<DownloadError> (DownloadError_Received);
			_bus.Subscribe<QueueEmpty> (QueueEmpty_Received);
		}

		public void ProgressDownload_Received(ProgressDownload progress) {
			Trace.TraceInformation("[Downloader] ProgressDownload");
			Trace.TraceInformation("[Downloader] ProgressDownload Id : {0}", progress.Id);
			Trace.TraceInformation("[Downloader] ProgressDownload Total : {0}", progress.Total);
			Trace.TraceInformation("[Downloader] ProgressDownload Written : {0}", progress.Written);
		}

		public void QueueEmpty_Received(QueueEmpty empty) {
			Trace.TraceInformation ("[Downloader] QueueEmpty");
		}

		public void QueueFull_Received(QueueFull full) {
			Trace.TraceInformation ("[Downloader] QueueFull");
			Trace.TraceInformation ("[Downloader] QueueFull MaxDownload : {0}", full.MaxDownload);
			Trace.TraceInformation ("[Downloader] QueueFull Downloading : {0}", full.Downloading);
		}

		public void DownloadError_Received(DownloadError error) {
			Trace.TraceInformation("[Downloader] DownloadError");
			Trace.TraceInformation("[Downloader] DownloadError Id : {0}", error.Id);
			Trace.TraceInformation("[Downloader] DownloadError Error : {0}", error.Error);
			Trace.TraceInformation("[Downloader] DownloadError Description : {0}", error.Description);
			Trace.TraceInformation("[Downloader] DownloadError (State : {0})", error.State);
		}

		public async Task Queue (string url)
		{
			Trace.TraceInformation("[Downloader] Queue");
			Trace.TraceInformation("[Downloader] Queue url : {0}", url);

			await _bus.SendAsync<QueueUrl> (new QueueUrl {
				Url = url
			});
		}

		public async Task Reset ()
		{
			Trace.TraceInformation("[Downloader] Reset");
			await _bus.SendAsync<ResetDownloads> (new ResetDownloads {});
		}

		public List<Bo.Download> List ()
		{
			Trace.TraceInformation("[Downloader] List");
			return _repository.All ();
		}
	}
}

