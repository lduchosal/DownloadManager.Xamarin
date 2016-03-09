using System;

namespace DownloadManager.iOS.Bo
{
	public class Progress
	{

		public void Notify(Download download) {
			_changed (download);
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

