using System;
using Fabrik.SimpleBus;
using DownloadManager.iOS.Bo;
using System.Threading;
using DownloadManager.iOS;
using NUnit.Framework;
using System.Threading.Tasks;

namespace DownloadManager.Test
{
	[TestFixture]
	public class DownloadManagerTest_DownloadFinished
	{

		[Test]
		public async Task Test_FinishDownload_DownloadError1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum downloaderror = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				downloaderror = p.Error;
				wait1.Set();
			});

			manager.FinishedDownload (new FinishedDownload {
				Id = 1
			});

			wait1.WaitOne ();

			Assert.AreEqual (ErrorEnum.FinishedDownload_IdentifierNotFound, downloaderror, "Downloaderror");

		}

		[Test]
		public async Task Test_FinishDownload_Finish1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);

			ErrorEnum downloaderror = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				downloaderror = p.Error;
			});

			var download = new Download {
				State = State.Downloading,
			};
			repo.Insert (download);

			manager.FinishedDownload (new FinishedDownload {
				Id = download.Id,
			});

			Assert.AreEqual (ErrorEnum.Empty, downloaderror, "Downloaderror");
			Assert.AreEqual (State.Finished, download.State, "Finished");

		}


		[Test]
		public async Task Test_FinishDownload_ErrorNotFound2 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum downloaderror = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				downloaderror = p.Error;
				wait1.Set();
			});

			var download = new Download {
				State = State.Downloading,
			};
			repo.Insert (download);

			manager.FinishedDownload (new FinishedDownload {
				Id = 666,
			});

			wait1.WaitOne ();

			Assert.AreEqual (ErrorEnum.FinishedDownload_IdentifierNotFound, downloaderror, "Downloaderror");

		}


		[Test]
		public async Task Test_FinishDownload_ErrorInvalidState1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum downloaderror = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				downloaderror = p.Error;
				wait1.Set();
			});

			var download = new Download {
				State = State.Finished,
			};
			repo.Insert (download);

			manager.FinishedDownload (new FinishedDownload {
				Id = download.Id,
			});

			wait1.WaitOne ();

			Assert.AreEqual (ErrorEnum.FinishedDownload_InvalidState, downloaderror, "DownloadError");

		}



	}
}

