using DownloadManager.iOS.Bo;
using System;
using NUnit.Framework;
using System.Diagnostics;

namespace DownloadManager.Test
{
	[TestFixture]
	public class DownloadTest
	{
		
		[Test]
		public void Waiting_Fail ()
		{
			Console.WriteLine ("Waiting_Fail");

			var download = new Download ();
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			Assert.AreEqual (State.Error, download.State, "State");
		}

		[Test]
		public void Waiting_Resume ()
		{
			Console.WriteLine ("Waiting_Resume");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			Assert.AreEqual (State.Downloading, download.State, "State");
		}

		[Test]
		public void Waiting_Cancel ()
		{
			Console.WriteLine ("Waiting_Cancel");

			var download = new Download ();
			download.Cancel ();
			Assert.AreEqual (State.Finished, download.State, "State");
		}




		[Test]
		public void Waiting_Invalid1 ()
		{
			Console.WriteLine ("Waiting_Invalid1");

			var download = new Download ();
			bool paused = download.TryPause ();
			Assert.AreEqual (false, paused, "Paused");
		}

		[Test]
		public void Waiting_Invalid2 ()
		{
			Console.WriteLine ("Waiting_Invalid2");

			var download = new Download ();
			Assert.Throws<InvalidOperationException> (() => download.Retry ());
		}

		[Test]
		public void Waiting_Invalid3 ()
		{
			Console.WriteLine ("Waiting_Invalid3");

			var download = new Download ();
			bool progressed = download.TryProgress (0, 0);
			Assert.AreEqual (false, progressed, "Progressed");
		}

		[Test]
		public void Waiting_Invalid4 ()
		{
			Console.WriteLine ("Waiting_Invalid4");

			var download = new Download ();
			bool finished = download.TryFinish ("location");
			Assert.AreEqual (false, finished, "Finished");
		}

		[Test]
		public void Downloading_Pause ()
		{
			Console.WriteLine ("Downloading_Pause");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool paused = download.TryPause ();
			Assert.AreEqual (true, paused, "Paused");
			Assert.AreEqual (State.Waiting, download.State, "State");
		}

		[Test]
		public void Downloading_Progress ()
		{
			Console.WriteLine ("Downloading_Progress");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool progressed = download.TryProgress (0,0);
			Assert.AreEqual (true, progressed, "Progressed");
			Assert.AreEqual (State.Downloading, download.State, "State");
		}

		[Test]
		public void Downloading_Cancel ()
		{
			Console.WriteLine ("Downloading_Cancel");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			download.Cancel ();
			Assert.AreEqual (State.Finished, download.State, "State");
		}

		[Test]
		public void Downloading_Fail ()
		{
			Console.WriteLine ("Downloading_Fail");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			Assert.AreEqual (State.Error, download.State, "State");
		}

		[Test]
		public void Downloading_Invalid1 ()
		{
			Console.WriteLine ("Downloading_Invalid1");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			Assert.Throws<InvalidOperationException> (() => download.Retry ());
		}

		[Test]
		public void Downloading_Invalid2 ()
		{
			Console.WriteLine ("Downloading_Invalid2");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool resumed2 = download.TryResume ();
			Assert.AreEqual (false, resumed2, "Resumed2");
		}


		[Test]
		public void Error_Retry ()
		{
			Console.WriteLine ("Error_Retry");

			var download = new Download ();
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			download.Retry ();
			Assert.AreEqual (State.Waiting, download.State, "State");
		}

		[Test]
		public void Error_Cancel ()
		{
			Console.WriteLine ("Error_Cancel");

			var download = new Download ();
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			download.Cancel ();
			Assert.AreEqual (State.Finished, download.State, "State");
		}

		[Test]
		public void Error_Fail ()
		{
			Console.WriteLine ("Error_Fail");

			var download = new Download ();
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			bool failed2 = download.TryFail (404);
			Assert.AreEqual (true, failed2, "Failed2");
			Assert.AreEqual (State.Finished, download.State, "State");
		}


		[Test]
		public void Error_Invalid1 ()
		{
			Console.WriteLine ("Error_Invalid1");

			var download = new Download ();
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			bool resumed = download.TryResume ();
			Assert.AreEqual (false, resumed, "Resumed");
		}

		[Test]
		public void Error_Invalid2 ()
		{
			Console.WriteLine ("Error_Invalid2");

			var download = new Download ();
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			bool paused = download.TryPause ();
			Assert.AreEqual (false, paused, "Paused");
		}

		[Test]
		public void Error_Invalid3 ()
		{
			Console.WriteLine ("Error_Invalid3");

			var download = new Download ();
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			bool progressed = download.TryProgress (0,0);
			Assert.AreEqual (false, progressed, "Progressed");
		}

		[Test]
		public void Error_Invalid4 ()
		{
			Console.WriteLine ("Error_Invalid4");

			var download = new Download ();
			bool failed = download.TryFail (404);
			Assert.AreEqual (true, failed, "Failed");
			bool finished = download.TryFinish ("location");
			Assert.AreEqual (null, download.Temporary, "Temporary");
			Assert.AreEqual (false, finished, "Finished");
		}

		[Test]
		public void Finished_Invalid1 ()
		{
			Console.WriteLine ("Finished_Invalid1");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool finished = download.TryFinish ("location");
			Assert.AreEqual (true, finished, "Finished");
			Assert.AreEqual ("location", download.Temporary, "Temporary");
			Assert.Throws<InvalidOperationException> (() => download.Cancel());
		}


		[Test]
		public void Finished_Invalid2 ()
		{
			Console.WriteLine ("Finished_Invalid2");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool finished = download.TryFinish ("location");
			Assert.AreEqual (true, finished, "Finished");
			Assert.AreEqual ("location", download.Temporary, "Temporary");
			bool failed = download.TryFail (404);
			Assert.AreEqual (false, failed, "Failed");
		}

		[Test]
		public void Finished_Invalid3 ()
		{
			Console.WriteLine ("Finished_Invalid3");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool finished = download.TryFinish ("location");
			Assert.AreEqual (true, finished, "Finished");
			Assert.AreEqual ("location", download.Temporary, "Temporary");
			bool finished2 = download.TryFinish ("location2");
			Assert.AreEqual (false, finished2, "Finished2");
			Assert.AreEqual ("location", download.Temporary, "Temporary2");
		}

		[Test]
		public void Finished_Invalid4 ()
		{
			Console.WriteLine ("Finished_Invalid4");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool finished = download.TryFinish ("location");
			Assert.AreEqual (true, finished, "Finished");
			Assert.AreEqual ("location", download.Temporary, "Temporary2");
			bool paused = download.TryPause ();
			Assert.AreEqual (false, paused, "Paused");
		}

		[Test]
		public void Finished_Invalid5 ()
		{
			Console.WriteLine ("Finished_Invalid5");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool finished = download.TryFinish ("location");
			Assert.AreEqual (true, finished, "Finished");
			Assert.AreEqual ("location", download.Temporary, "Temporary");
			bool progressed = download.TryProgress (0, 0);
			Assert.AreEqual (false, progressed, "Progressed");
		}

		[Test]
		public void Finished_Invalid6 ()
		{
			Console.WriteLine ("Finished_Invalid6");

			var download = new Download ();
			bool resumed = download.TryResume ();
			Assert.AreEqual (true, resumed, "Resumed");
			bool finished = download.TryFinish ("location");
			Assert.AreEqual (true, finished, "Finished");
			Assert.AreEqual ("location", download.Temporary, "Temporary");
			bool resumed2 = download.TryResume ();
			Assert.AreEqual (false, resumed2, "Resumed2");
		}

	}
}
