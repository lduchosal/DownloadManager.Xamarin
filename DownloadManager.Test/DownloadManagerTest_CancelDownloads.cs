using System;
using Fabrik.SimpleBus;
using DownloadManager.iOS.Bo;
using System.Threading;
using DownloadManager.iOS;
using System.Diagnostics;
using NUnit.Framework;
using System.Threading.Tasks;

namespace DownloadManager.Test
{
	[TestFixture]
	public class DownloadManagerTest_CancelDownloads
	{

		[Test]
		public async Task Test_CancelDownloads_Empty ()
		{
			Console.WriteLine ("Test_CancelDownloads_Empty");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);

			manager.CancelDownloads (new CancelDownloads {
			});

			var all = repo.All ();
			Assert.AreEqual (0, all.Count);
		}


		[Test]
		public async Task Test_CancelDownloads_Single ()
		{
			Console.WriteLine ("Test_CancelDownloads_Single");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);

			var download = new Download { State = State.Waiting };
			repo.Insert (download);

			manager.CancelDownloads (new CancelDownloads {
			});

			Assert.AreEqual (State.Finished, download.State, "First");
		}


		[Test]
		public async Task Test_CancelDownloads_Multiple ()
		{
			Console.WriteLine ("Test_CancelDownloads_Multiple");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);

			var waiting = new Download { State = State.Waiting };
			var error = new Download { State = State.Error };
			var downloading = new Download { State = State.Downloading };
			var finished = new Download { State = State.Finished };

			repo.Insert (waiting);
			repo.Insert (downloading);
			repo.Insert (error);
			repo.Insert (finished);

			manager.CancelDownloads (new CancelDownloads {
			});

			Assert.AreEqual (State.Finished, waiting.State, "Waiting");
			Assert.AreEqual (State.Finished, downloading.State, "Downloading");
			Assert.AreEqual (State.Finished, error.State, "Error");
			Assert.AreEqual (State.Finished, finished.State, "Finished");
		}


		[Test]
		public async Task Test_CancelDownloads_Multiple2 ()
		{
			Console.WriteLine ("Test_CancelDownloads_Multiple2");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);

			var waiting1 = new Download { State = State.Waiting };
			var waiting2 = new Download { State = State.Waiting };
			var waiting3 = new Download { State = State.Waiting };
			var waiting4 = new Download { State = State.Waiting };
			var waiting5 = new Download { State = State.Waiting };

			repo.Insert (waiting1);
			repo.Insert (waiting2);
			repo.Insert (waiting3);
			repo.Insert (waiting4);
			repo.Insert (waiting5);

			manager.CancelDownloads (new CancelDownloads {
			});

			Assert.AreEqual (State.Finished, waiting1.State, "Waiting1");
			Assert.AreEqual (State.Finished, waiting2.State, "Waiting2");
			Assert.AreEqual (State.Finished, waiting3.State, "Waiting3");
			Assert.AreEqual (State.Finished, waiting4.State, "Waiting4");
			Assert.AreEqual (State.Finished, waiting5.State, "Waiting5");
		}

	}
}

