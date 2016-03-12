using System;

namespace DownloadManager.iOS.Bo
{
	public class Progress
	{
		public void Notify(Download download) {
			try {
				_changed (download);
			} catch (Exception e) {
				Console.WriteLine (e.ToString());
			}
		}

		public void Reset() {
			_changed = delegate { };
		}

		private event Action<Download> _changed = delegate { };
		public event Action<Download> Changed
		{
			add { _changed += value; }
			remove { _changed -= value; }
		}

	}
}

