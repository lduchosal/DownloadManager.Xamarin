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
	public class DownloadManagerTest_ResetDownloads
	{

		[Test]
		public async Task Test_ResetDownloads_Empty ()
		{
			Console.WriteLine ("Test_ResetDownloads_Empty");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);

			manager.ResetDownloads (new ResetDownloads {
			});

			var all = repo.All ();
			Assert.AreEqual (0, all.Count);
		}


		[Test]
		public async Task Test_ResetDownloads_Single ()
		{

			Console.WriteLine ("Test_ResetDownloads_Single");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);

			var download = new Download { State = State.Waiting };
			repo.Insert (download);

			manager.ResetDownloads (new ResetDownloads {
			});

			var all = repo.All ();
			Assert.AreEqual (0, all.Count);
		}


		[Test]
		public async Task Test_ResetDownloads_Multiple ()
		{
			Console.WriteLine ("Test_ResetDownloads_Multiple");

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

			manager.ResetDownloads (new ResetDownloads {
			});

			var all = repo.All ();
			Assert.AreEqual (0, all.Count);
		}


		[Test]
		public async Task Test_ResetDownloads_Multiple2 ()
		{
			Console.WriteLine ("Test_ResetDownloads_Multiple2");

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

			manager.ResetDownloads (new ResetDownloads {
			});

			var all = repo.All ();
			Assert.AreEqual (0, all.Count);
		}

	}
}

