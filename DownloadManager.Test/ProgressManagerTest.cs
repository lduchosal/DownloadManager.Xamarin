using DownloadManager.iOS.Bo;
using System;
using NUnit.Framework;
using System.Diagnostics;
using DownloadManager.iOS;
using Foundation;
using Fabrik.SimpleBus;

namespace DownloadManager.Test
{
	[TestFixture]
	public class ProgressManagerTest
	{

		[Test]
		public void Queue_1 ()
		{
			Console.WriteLine ("Queue_1");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();

			var progressmanager = new ProgressManager (bus, repo);
			NSProgress progress = progressmanager.Queue ("url");

			Assert.AreEqual (0, progress.CompletedUnitCount, "CompletedUnitCount");
		}


		[Test]
		public void Queue_2 ()
		{
			Console.WriteLine ("Queue_2");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();


			var download = new Download {
				Url = "url",
				Total = 100,
				Written = 100
			};
			repo.Insert (download);

			var progressmanager = new ProgressManager (bus, repo);
			NSProgress progress = progressmanager.Queue (download.Url);
			progressmanager.FinishedDownload (new FinishedDownload {
				Id = download.Id,
				Location = null,
			});

			Assert.AreEqual (100, progress.CompletedUnitCount, "CompletedUnitCount");
			Assert.AreEqual (100, progress.TotalUnitCount, "TotalUnitCount");
		}



	}
}
