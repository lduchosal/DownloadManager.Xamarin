using System;
using Fabrik.SimpleBus;
using DownloadManager.iOS.Bo;
using System.Threading;
using DownloadManager.iOS;
using System.Diagnostics;
using Fabrik.SimpleBus;
using NUnit.Framework;
using System.Threading.Tasks;

namespace DownloadManager.Test
{
	[TestFixture]
	public class DownloadManagerTest
	{
		[Test]
		public async Task Test_QueueUrl_CheckFreeSlot1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait = new AutoResetEvent (false);

			bool alreadyQueued = false;
			bool checkFreeSlot = false;
			bus.Subscribe<AlreadyQueued> (p => {
				try {
					alreadyQueued = true;
					wait.Set();
				} catch(Exception e) {
					Console.WriteLine (e);
				}
			});

			bus.Subscribe<CheckFreeSlot> (p => {
				try{
					checkFreeSlot = true;
					wait.Set();
				} catch(Exception e) {
					Console.WriteLine (e);
				}
			});

			manager.QueueUrl (new QueueUrl {
				Url = "http://url.com/download/file.zip"
			});

			wait.WaitOne ();

			Assert.AreEqual (true, checkFreeSlot);
			Assert.AreEqual (false, alreadyQueued);

		}

		[Test]
		public async Task Test_QueueUrl_AlreadyQueued1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			bool alreadyQueued = false;
			bool checkFreeSlot = false;

			bus.Subscribe<AlreadyQueued> (p => {
				alreadyQueued = true;
				wait1.Set();
			});

			bus.Subscribe<CheckFreeSlot> (p => {
				checkFreeSlot = true;
				wait1.Set();
			});

			repo.Insert(new Download {
				Url = "http://url.com/download/file.zip"
			});

			manager.QueueUrl (new QueueUrl {
				Url = "http://url.com/download/file.zip"
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, checkFreeSlot);
			Assert.AreEqual (true, alreadyQueued);

		}

		[Test]
		public async Task  Test_FreeSlot_Nowait1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			bool queueDownload = false;
			bool queueEmpty = false;

			bus.Subscribe<QueueDownload> (p => {
				queueDownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueEmpty = true;
				wait1.Set();
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queueDownload);
			Assert.AreEqual (true, queueEmpty);

		}

		[Test]
		public async Task  Test_FreeSlot_Queued1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			bool queueDownload = false;
			bool queueEmpty = false;

			bus.Subscribe<QueueDownload> (p => {
				queueDownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueEmpty = true;
				wait1.Set();
			});


			repo.Insert(new Download {
				Url = "http://url.com/download/file.zip",
				State = State.Waiting
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (true, queueDownload);
			Assert.AreEqual (false, queueEmpty);

		}


		[Test]
		public async Task  Test_FreeSlot_Queued_Finished ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			bool queueDownload = false;
			bool queueEmpty = false;

			bus.Subscribe<QueueDownload> (p => {
				queueDownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueEmpty = true;
				wait1.Set();
			});

			repo.Insert(new Download {
				Url = "http://url.com/download/file.zip",
				State = State.Finished
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queueDownload);
			Assert.AreEqual (true, queueEmpty);

		}



		[Test]
		public async Task  Test_FreeSlot_Queued_Error ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			bool queueDownload = false;
			bool queueEmpty = false;

			bus.Subscribe<QueueDownload> (p => {
				queueDownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueEmpty = true;
				wait1.Set();
			});

			repo.Insert(new Download {
				Url = "http://url.com/download/file.zip",
				State = State.Error
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queueDownload);
			Assert.AreEqual (true, queueEmpty);

		}



		[Test]
		public async Task Test_FreeSlot_Queued_Waiting ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			bool queueDownload = false;
			bool queueEmpty = false;

			bus.Subscribe<QueueDownload> (p => {
				queueDownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueEmpty = true;
				wait1.Set();
			});

			repo.Insert(new Download {
				Url = "http://url.com/download/file.zip",
				State = State.Error
			});
			repo.Insert(new Download {
				Url = "http://url.com/download/file2.zip",
				State = State.Waiting
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (true, queueDownload);
			Assert.AreEqual (false, queueEmpty);

		}

		[Test]
		public async Task Test_ProgressDownload_DownloadError1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum downloaderror = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				downloaderror = p.Error;
				wait1.Set();
			});

			manager.ProgressDownload (new ProgressDownload {
				Id = 1,
				Total = 1000,
				Written = 10
			});

			wait1.WaitOne ();

			Assert.AreEqual (ErrorEnum.ProgressDownload_IdentifierNotFound, downloaderror, "Downloaderror");

		}


		[Test]
		public async Task Test_ProgressDownload_Progress1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);

			ErrorEnum downloaderror = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				downloaderror = p.Error;
			});

			var download = new Download {
				State = State.Downloading,
				Total = 0,
				Written = 0
			};
			repo.Insert (download);

			manager.ProgressDownload (new ProgressDownload {
				Id = download.Id,
				Total = 1000,
				Written = 10
			});

			Assert.AreEqual (10, download.Written, "Written");
			Assert.AreEqual (1000, download.Total, "Total");
			Assert.AreEqual (ErrorEnum.Empty, downloaderror, "Downloaderror");

		}


		[Test]
		public async Task Test_ProgressDownload_ErrorNotFound2 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum downloaderror = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				downloaderror = p.Error;
				wait1.Set();
			});

			var download = new Download {
				State = State.Downloading,
				Total = 0,
				Written = 0
			};
			repo.Insert (download);

			manager.ProgressDownload (new ProgressDownload {
				Id = 666,
				Total = 1000,
				Written = 10
			});

			wait1.WaitOne ();

			Assert.AreEqual (0, download.Written, "Written");
			Assert.AreEqual (0, download.Total, "Total");
			Assert.AreEqual (ErrorEnum.ProgressDownload_IdentifierNotFound, downloaderror, "Downloaderror");

		}


		[Test]
		public async Task Test_ProgressDownload_ErrorInvalidState1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum downloaderror = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				downloaderror = p.Error;
				wait1.Set();
			});

			var download = new Download {
				State = State.Finished,
				Total = 0,
				Written = 0
			};
			repo.Insert (download);

			manager.ProgressDownload (new ProgressDownload {
				Id = download.Id,
				Total = 1000,
				Written = 10
			});

			wait1.WaitOne ();

			Assert.AreEqual (0, download.Written, "Written");
			Assert.AreEqual (0, download.Total, "Total");
			Assert.AreEqual (ErrorEnum.ProgressDownload_InvalidState, downloaderror, "DownloadError");

		}



	}
}

