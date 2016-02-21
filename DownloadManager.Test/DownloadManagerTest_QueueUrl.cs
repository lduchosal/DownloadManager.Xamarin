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
	public class DownloadManagerTest_QueueUrl
	{
		[Test]
		public async Task Test_QueueUrl_CheckFreeSlot1 ()
		{

			Console.WriteLine ("Test_QueueUrl_CheckFreeSlot1");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
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
			Console.WriteLine ("Test_QueueUrl_AlreadyQueued1");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
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


	}
}

