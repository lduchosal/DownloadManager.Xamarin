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
	public class DownloadManagerTest_QueueFull
	{

		[Test]
		public async Task Test_DownloadRejected_Error1 ()
		{

			Console.WriteLine ("Test_DownloadRejected_Error1");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum error = ErrorEnum.Empty;

			bus.Subscribe<DownloadError> (p => {
				error = p.Error;
				wait1.Set();
			});

			manager.DownloadRejected (new DownloadRejected {
				Id = 0,
				Reason = RejectionEnum.QueueFull
			});

			wait1.WaitOne ();

			Assert.AreEqual (ErrorEnum.DownloadRejected_IdentifierNotFound, error);

		}

		[Test]
		public async Task Test_DownloadRejected_Error2 ()
		{
			Console.WriteLine ("Test_DownloadRejected_Error2");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum error = ErrorEnum.Empty;

			bus.Subscribe<DownloadError> (p => {
				error = p.Error;
				wait1.Set();
			});

			repo.Insert (new Download { Url = "url", State = State.Finished });
			manager.DownloadRejected (new DownloadRejected {
				Id = 1,
				Reason = RejectionEnum.QueueFull
			});

			wait1.WaitOne ();

			Assert.AreEqual (ErrorEnum.DownloadRejected_InvalidState, error);

		}


		[Test]
		public async Task Test_DownloadRejected_1 ()
		{
			Console.WriteLine ("Test_DownloadRejected_1");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum error = ErrorEnum.Empty;

			bus.Subscribe<DownloadError> (p => {
				error = p.Error;
				wait1.Set();
			});

			var download = new Download { Url = "url", State = State.Downloading };
			repo.Insert (download);
			manager.DownloadRejected (new DownloadRejected {
				Id = 1,
				Reason = RejectionEnum.QueueFull
			});

			Assert.AreEqual (State.Waiting, download.State);

		}



	}
}

