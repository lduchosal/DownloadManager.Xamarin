using DownloadManager.iOS.Bo;
using System;
using NUnit.Framework;
using System.Diagnostics;
using DownloadManager.iOS;
using Foundation;
using Fabrik.SimpleBus;
using System.Threading;

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
			progressmanager.Queue ("url", (download) => {

			});

		}

		[Test]
		public void Queue_2 ()
		{
			Console.WriteLine ("Queue_2");

			var bus = new InProcessBus ();
			var repo = new DownloadRepositoryMock ();

			long total = 0;
			long written = 0;
			var wait1 = new AutoResetEvent (false);

			var download = new Download {
				Url = "url",
				Total = 0,
				Written = 0
			};
			repo.Insert (download);

			var progressmanager = new ProgressManager (bus, repo);
			progressmanager.Queue (download.Url, (d) => {
				total = d.Total;
				written = d.Written;
				wait1.Set();
			});

			download.Total = 100;
			download.Written = 10;
				
			progressmanager.NotifyProgress (new NotifyProgress {
				Url = download.Url,
				Download = download
			});	

			download.Written = 50;

			progressmanager.NotifyProgress (new NotifyProgress {
				Url = download.Url,
				Download = download
			});

			download.Written = 100;

			progressmanager.NotifyProgress (new NotifyProgress {
				Url = download.Url,
				Download = download
			});

			wait1.WaitOne ();
			wait1.WaitOne ();
			wait1.WaitOne ();

			Assert.AreEqual (100, written, "Written");
			Assert.AreEqual (100, total, "Total");

		}



	}
}
