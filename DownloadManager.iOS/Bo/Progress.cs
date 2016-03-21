using System;

namespace DownloadManager.iOS.Bo
{
	public class Progress : IDisposable
	{
		private event Action<Download> _changed = delegate { };

		public Progress(Action<Download> action) {
			Bind (action);
		}

		public void Bind(Action<Download> action) {
			_changed += action;
		}

		public void Notify(Download download) {
			try {
				_changed (download);
			} catch (Exception e) {
				// disposed object crash..
				Console.WriteLine(e.ToString());
			}
		}

		public void Dispose ()
		{
			_changed = delegate { };
		}
	}
}

