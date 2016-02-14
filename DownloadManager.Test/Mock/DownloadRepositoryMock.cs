using DownloadManager.iOS.Bo;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DownloadManager.iOS;

namespace DownloadManager.Test
{
	public class DownloadRepositoryMock : IDownloadRepository
	{

		private int _iterator = 0;
		public Dictionary<int, Download> Database = new Dictionary<int, Download> ();
		#region IDownloadRepository implementation
		public bool TryById (int id, out DownloadManager.iOS.Bo.Download result)
		{
			return Database.TryGetValue (id, out result);
		}
		public bool TryByUrl (string url, out DownloadManager.iOS.Bo.Download result)
		{
			var urls = Database.Values.ToDictionary<Download, string> (v => v.Url);
			return urls.TryGetValue (url, out result);
	 	}
		public bool FirstByState (DownloadManager.iOS.Bo.State state, out DownloadManager.iOS.Bo.Download result)
		{
			var download = Database
				.Values
				.Where (v => v.State == state)
				.FirstOrDefault ();

			result = download;
			return download != null;
		}

		public int CountByState (DownloadManager.iOS.Bo.State state)
		{
			int count = Database
				.Values
				.Where (v => v.State == state)
				.Count ();

			return count;
		}
		public void Insert (DownloadManager.iOS.Bo.Download insert)
		{
			int id = Interlocked.Increment (ref _iterator);
			insert.Id = id;
			Database.Add (id, insert);
		}
		public void Update (DownloadManager.iOS.Bo.Download download)
		{
		}

		public void UpdateAll (System.Collections.Generic.List<DownloadManager.iOS.Bo.Download> queued)
		{
			
		}
		public void DeleteAll ()
		{
			Database.Clear ();
		}

		public System.Collections.Generic.List<DownloadManager.iOS.Bo.Download> ByState (DownloadManager.iOS.Bo.State[] states)
		{
			var downloads = Database
				.Values
				.Where (v => states.Any (s => s == v.State))
				.ToList ();

			return downloads;


		}
		public System.Collections.Generic.List<DownloadManager.iOS.Bo.Download> All ()
		{
			var downloads = Database
				.Values
				.ToList ();

			return downloads;
		}
		#endregion
	}
}

