using System;
using MonoTouch.Dialog;
using DownloadManager.iOS.Bo;
using UIKit;

namespace DownloadManager.Sample
{
	public class ProgressElement : StringElement
	{
		
		private Progress Progress { get; set; }
		public ProgressElement (string caption, Action tapped, Progress progress) : base(caption, tapped) {

			Progress = progress;

		}


		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (Progress == null) {
				return;
			}
			Progress.Dispose ();
		}

	}
}

