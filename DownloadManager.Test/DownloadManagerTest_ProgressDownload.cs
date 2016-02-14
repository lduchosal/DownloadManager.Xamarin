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
	public class DownloadManagerTest_ProgressDownload 
	{

		[Test]
		public async Task Test_ProgressDownload_DownloadError1 ()
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
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);

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
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
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
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
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
			Assert.AreEqual (ErrorEnum.ProgressDownload_OutOfOrder, downloaderror, "DownloadError");

		}



	}
}

