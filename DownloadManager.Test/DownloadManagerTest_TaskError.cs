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
	public class DownloadManagerTest_TaskError
	{

		[Test]
		public async Task Test_DownloadError_Error1 ()
		{

			Console.WriteLine ("Test_DownloadError_Error1");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum error = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				error = p.Error;
				wait1.Set();
			});

			manager.TaskError (new TaskError {
				Id = 0,
				Error = TaskErrorEnum.Empty,
			});

			wait1.WaitOne (10);

			Assert.AreEqual (ErrorEnum.TaskError_IdentifierNotFound, error);

		}

		[Test]
		public async Task Test_DownloadError_Error2 ()
		{
			Console.WriteLine ("Test_DownloadError_Error2");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();
			var manager = new DownloadManager.iOS.DownloadManager (bus, repo, 3);
			var wait1 = new AutoResetEvent (false);

			ErrorEnum error = ErrorEnum.Empty;
			bus.Subscribe<DownloadError> (p => {
				error = p.Error;
				wait1.Set();
			});

			manager.TaskError (new TaskError {
				Id = 0,
				Error = TaskErrorEnum.InvalidResponse,
			});

			wait1.WaitOne ();

			Assert.AreEqual (ErrorEnum.TaskError_IdentifierNotFound, error);

		}

		[Test]
		public async Task Test_DownloadError_Error3 ()
		{

			Console.WriteLine ("Test_DownloadError_Error3");

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
			manager.TaskError (new TaskError {
				Id = download.Id,
				Error = TaskErrorEnum.DownloadError,
			});

			wait1.WaitOne ();
			Assert.AreEqual (ErrorEnum.TaskError_InvalidState, error);

		}

		[Test]
		public async Task Test_DownloadError_Error404 ()
		{

			Console.WriteLine ("Test_DownloadError_Error404");

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

			manager.TaskError (new TaskError {
				Id = download.Id,
				Error = TaskErrorEnum.InvalidResponse,
				StatusCode = 404
			});

			Assert.AreEqual (State.Error, download.State);
			Assert.AreEqual (404, download.StatusCode);

		}

		[Test]
		public async Task Test_DownloadError_Error500 ()
		{

			Console.WriteLine ("Test_DownloadError_Error500");

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

			manager.TaskError (new TaskError {
				Id = download.Id,
				Error = TaskErrorEnum.InvalidResponse,
				StatusCode = 500
			});

			Assert.AreEqual (State.Error, download.State);
			Assert.AreEqual (500, download.StatusCode);

		}




	}
}

