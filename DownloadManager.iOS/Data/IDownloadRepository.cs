using System;
using DownloadManager.iOS.Bo;
using System.Collections.Generic;

namespace DownloadManager.iOS
{
	public interface IDownloadRepository
	{
		bool TryById (int id, out Bo.Download result);
		bool TryByUrl(string url, out Download result) ;
		bool FirstByState(State state, out Download result);

		int CountByState(State state);

		void Insert (Download insert);
		void Update (Download download);
		void UpdateAll (List<Download> queued);
		void DeleteAll ();

		List<Bo.Download> ByState (State[] states);
		List<Bo.Download> All ();

	}
}

