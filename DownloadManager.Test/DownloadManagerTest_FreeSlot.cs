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
	public class DownloadManagerTest_FreeSlot
	{

		[Test]
		public async Task  Test_FreeSlot_Empty1 ()
		{
			Console.WriteLine ("Test_FreeSlot_Empty1");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			bool queuedownload = false;
			bool queueempty = false;
			bool queuefull = false;

			bus.Subscribe<QueueDownload> (p => {
				queuedownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueFull> (p => {
				queuefull = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueempty = true;
				wait1.Set();
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queuedownload);
			Assert.AreEqual (false, queuefull);
			Assert.AreEqual (true, queueempty);

		}

		[Test]
		public async Task  Test_FreeSlot_Empty2 ()
		{
			Console.WriteLine ("Test_FreeSlot_Empty2");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			bool queuedownload = false;
			bool queueempty = false;
			bool queuefull = false;

			bus.Subscribe<QueueDownload> (p => {
				queuedownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueFull> (p => {
				queuefull = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueempty = true;
				wait1.Set();
			});

			repo.Insert(new Download { Url = "http://url.com/download/file.zip", State = State.Finished });

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queuedownload);
			Assert.AreEqual (false, queuefull);
			Assert.AreEqual (true, queueempty);

		}

		[Test]
		public async Task  Test_FreeSlot_Empty3 ()
		{

			Console.WriteLine ("Test_FreeSlot_Empty3");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			bool queuedownload = false;
			bool queueempty = false;
			bool queuefull = false;

			bus.Subscribe<QueueDownload> (p => {
				queuedownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueFull> (p => {
				queuefull = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueempty = true;
				wait1.Set();
			});

			repo.Insert(new Download { Url = "http://url.com/download/file1.zip", State = State.Finished });
			repo.Insert(new Download { Url = "http://url.com/download/file2.zip", State = State.Finished });
			repo.Insert(new Download { Url = "http://url.com/download/file3.zip", State = State.Finished });
			repo.Insert(new Download { Url = "http://url.com/download/file4.zip", State = State.Finished });
			repo.Insert(new Download { Url = "http://url.com/download/file5.zip", State = State.Finished });

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queuedownload);
			Assert.AreEqual (false, queuefull);
			Assert.AreEqual (true, queueempty);

		}

		[Test]
		public async Task  Test_FreeSlot_Queued1 ()
		{
			Console.WriteLine ("Test_FreeSlot_Queued1");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			bool queuedownload = false;
			bool queueempty = false;
			bool queuefull = false;

			bus.Subscribe<QueueDownload> (p => {
				queuedownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueFull> (p => {
				queuefull = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueempty = true;
				wait1.Set();
			});


			repo.Insert(new Download {
				Url = "http://url.com/download/file.zip",
				State = State.Waiting
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (true, queuedownload);
			Assert.AreEqual (false, queueempty);
			Assert.AreEqual (false, queuefull);

		}


		[Test]
		public async Task  Test_FreeSlot_Queued_Finished ()
		{

			Console.WriteLine ("Test_FreeSlot_Queued_Finished");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			bool queuedownload = false;
			bool queueempty = false;
			bool queuefull = false;

			bus.Subscribe<QueueDownload> (p => {
				queuedownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueFull> (p => {
				queuefull = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueempty = true;
				wait1.Set();
			});

			repo.Insert(new Download {
				Url = "http://url.com/download/file.zip",
				State = State.Finished
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queuedownload);
			Assert.AreEqual (true, queueempty);
			Assert.AreEqual (false, queuefull);

		}

		[Test]
		public async Task  Test_FreeSlot_Queued_Empty3 ()
		{

			Console.WriteLine ("Test_FreeSlot_Queued_Empty3");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			bool queuedownload = false;
			bool queueempty = false;
			bool queuefull = false;

			bus.Subscribe<QueueDownload> (p => {
				queuedownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueFull> (p => {
				queuefull = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueempty = true;
				wait1.Set();
			});

			repo.Insert(new Download {
				Url = "http://url.com/download/file.zip",
				State = State.Error
			});

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queuedownload);
			Assert.AreEqual (true, queueempty);
			Assert.AreEqual (false, queuefull);

		}

		[Test]
		public async Task Test_FreeSlot_Queued_Waiting ()
		{

			Console.WriteLine ("Test_FreeSlot_Queued_Waiting");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			bool queuedownload = false;
			bool queueempty = false;
			bool queuefull = false;

			bus.Subscribe<QueueDownload> (p => {
				queuedownload = true;
				wait1.Set();
			});

			bus.Subscribe<QueueFull> (p => {
				queuefull = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueempty = true;
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

			Assert.AreEqual (true, queuedownload);
			Assert.AreEqual (false, queueempty);
			Assert.AreEqual (false, queuefull);

		}


		[Test]
		public async Task Test_FreeSlot_Queued_Full ()
		{
			Console.WriteLine ("Test_FreeSlot_Queued_Full");
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			bool queuedownload = false;
			bool queueempty = false;
			bool queuefull = false;

			bus.Subscribe<QueueDownload> (p => {
				queuedownload = true;
				wait1.Set();

			});

			bus.Subscribe<QueueFull> (p => {
				queuefull = true;
				wait1.Set();
			});

			bus.Subscribe<QueueEmpty> (p => {
				queueempty = true;
				wait1.Set();
			});

			repo.Insert(new Download { Url = "http://url.com/download/file1.zip", State = State.Downloading });
			repo.Insert(new Download { Url = "http://url.com/download/file3.zip", State = State.Downloading });
			repo.Insert(new Download { Url = "http://url.com/download/file4.zip", State = State.Downloading });
			repo.Insert(new Download { Url = "http://url.com/download/file5.zip", State = State.Downloading });

			manager.FreeSlot (new FreeSlot {
			});

			wait1.WaitOne ();

			Assert.AreEqual (false, queuedownload);
			Assert.AreEqual (false, queueempty);
			Assert.AreEqual (true, queuefull);

		}

	}
}

