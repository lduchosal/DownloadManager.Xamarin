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
	public class DownloadManagerTest_DownloadError
	{

		[Test]
		public async Task Test_DownloadError_Error1 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum error = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				error = p.Error;
				wait1.Set();
			});

			manager.DownloadError (new DownloadError {
				Id = 0,
				Error = ErrorEnum.Empty,
			});

			wait1.WaitOne (10);

			Assert.AreEqual (ErrorEnum.Empty, error);

		}

		[Test]
		public async Task Test_DownloadError_Error2 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum error = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				error = p.Error;
				wait1.Set();
			});

			manager.DownloadError (new DownloadError {
				Id = 0,
				Error = ErrorEnum.DidFinishDownloading_ResponseError,
			});

			wait1.WaitOne ();

			Assert.AreEqual (ErrorEnum.DownloadError_IdentifierNotFound, error);

		}

		[Test]
		public async Task Test_DownloadError_Error3 ()
		{
			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum error = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				error = p.Error;
				wait1.Set();
			});

			var download = new Download { Url = "url", State = State.Finished };
			repo.Insert (download);
			manager.DownloadError (new DownloadError {
				Id = download.Id,
				Error = ErrorEnum.DidFinishDownloading_ResponseError,
			});

			wait1.WaitOne ();
			Assert.AreEqual (ErrorEnum.DownloadError_InvalidState, error);

		}

		[Test]
		public async Task Test_DownloadError_Error404 ()
		{
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

			manager.DownloadError (new DownloadError {
				Id = download.Id,
				Error = ErrorEnum.DidFinishDownloading_ResponseError,
				StatusCode = 404
			});

			Assert.AreEqual (State.Error, download.State);
			Assert.AreEqual (404, download.StatusCode);

		}

		[Test]
		public async Task Test_DownloadError_Error500 ()
		{
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

			manager.DownloadError (new DownloadError {
				Id = download.Id,
				Error = ErrorEnum.DidFinishDownloading_ResponseError,
				StatusCode = 500
			});

			Assert.AreEqual (State.Error, download.State);
			Assert.AreEqual (500, download.StatusCode);

		}




	}
}

