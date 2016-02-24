using System;

namespace DownloadManager.iOS.Bo
{
	public class Progress
	{

		private readonly Progress _parent;
		public Progress(Progress parent) {
			_parent = parent;
		}

		public Progress() {
			_parent = null;
		}

		public void Notify(Download download) {
			_changed (download);
		}

		public void Reset() {
			_changed = delegate { };
		}


		private Action<Download> _changed = delegate { };
		public event Action<Download> Changed
		{
			add { _changed += value; }
			remove { _changed -= value; }
		}

	}
}

