using System;
using SQLite;

namespace DownloadManager.iOS.Bo
{
	public partial class Download
	{
		[PrimaryKey]
		[AutoIncrement]
		public int Id
		{
			get;
			set;
		}

		[Unique]
		public string Url
		{
			get;
			set;
		}

		[Indexed]
		public int DownloadState
		{
			get { return (int)State; }
			set { State = (Bo.State)value; }
		}

		[Indexed]
		public DateTime LastModified
		{
			get;
			set;
		}

		public long Total
		{
			get;
			set;
		}

		public long Written
		{
			get;
			set;
		}

		public string Temporary
		{
			get;
			set;
		}

		public int StatusCode
		{
			get;
			set;
		}

		public int Error
		{
			get;
			set;
		}

		public string ErrorDescription
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		[Ignore]
		public Bo.State State
		{
			get;
			set;
		}

		public override string ToString ()
		{
			return string.Format ("[Download: Id={0}, Url={1}]", Id, Url);
		}

		public override bool Equals (object obj)
		{
			var remote = (Download)obj;
			if (remote == null) {
				return false;
			}
			var remoteurl = remote.Url;
			if (remoteurl == null) {
				return false;
			}
			return remoteurl.Equals (this.Url);
		}

		public override int GetHashCode ()
		{
			return this.Id;
		}
	}
}

