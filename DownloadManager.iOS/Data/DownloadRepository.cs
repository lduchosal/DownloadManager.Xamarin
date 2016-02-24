using System;
using System.Linq;
using SQLite;
using System.Collections.Generic;
using Fabrik.SimpleBus;
using DownloadManager.iOS.Bo;

namespace DownloadManager.iOS
{
	public class DownloadRepository : IDownloadRepository
	{

		private readonly SQLiteConnection _db;

		public DownloadRepository (SQLiteConnection db)
		{
			_db = db;
			_db.CreateTable<Bo.Download> ();

		}


		public bool TryByUrl(string url, out Download download) {
			string select = @"

				SELECT * 
	              FROM Download 
	             WHERE Url = ?
	                   ;
	";
			var results = _db.Query<Bo.Download>(select, url);
			if (!results.Any()) {
				download = null;
				return false;
			}

			download = results.First();
			return true;

		}

		public void Insert (Download insert)
		{
			_db.Insert (insert);
		}

		public void Update (Download download)
		{
			_db.Update (download);
		}

		public void UpdateAll (List<Download> queued)
		{
			_db.UpdateAll (queued);
		}

		public bool FirstByState(State state, out Download result) {
		
			string query = @"
			
	SELECT * 
      FROM Download 
     WHERE DownloadState = ?
  ORDER BY LastModified DESC
     LIMIT 1
           ;
";
			var downloads = _db.Query<Bo.Download> (query, state);
			if (!downloads.Any()) {
				result = null;
				return false;
			}

			result = downloads.First ();
			return true;

		}

		public int CountByState (State state)
		{
			string query = @"
			
	SELECT Count(Id) 
      FROM Download 
     WHERE DownloadState = ?
           ;
";
			var downloads = _db.ExecuteScalar<int> (query, state);
			return downloads;

		}

		public List<Bo.Download> ByState (State[] states)
		{
			var ids = from s in states
			          select (int)s;
			var sids = string.Join (",", ids);

			string query = string.Format (@"
			
	SELECT * 
      FROM Download 
     WHERE DownloadState IN ({0})
           ;
",sids);
			return _db.Query<Bo.Download> (query);
		}

		public List<Bo.Download> All ()
		{
			return _db.Query<Bo.Download> (@"

				SELECT * 
				  FROM Download 
			  ORDER BY Id DESC
				;

			");
		}

		public bool TryById (int id, out Bo.Download download)
		{
			var downloads = _db.Query<Bo.Download> (@"

				SELECT * 
 			 	  FROM Download 
				 WHERE Id = ?
 					 ;

			", id);

			if (!downloads.Any ()) {
				download = null;
				return false;
			}

			download = downloads.First ();
			return true;
		}

		public void DeleteAll ()
		{
			_db.DeleteAll<Download> ();
		}
	}
}
