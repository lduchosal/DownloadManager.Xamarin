using Foundation;
using UIKit;
using MonoTouch.Dialog;
using System.Linq;
using DownloadManager.iOS;

namespace DownloadManager.Sample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window {
			get;
			set;
		}

		Section _downloads;
		Downloader _downloader;

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			Window = new UIWindow (UIScreen.MainScreen.Bounds);

			_downloader = new Downloader ();
			_downloads = new Section ("Downloads") {
			}; 

			string templateurl = "http://pokeprice.local/api/v1/card/image/BLW/{0}";

			var root = new RootElement ("Root") {
				new Section ("Management"){
					new BooleanElement ("Enabled", true),
					//new EntryElement ("MaxDownloads", "Simultaneous", "4"),
					new StringElement ("Add", async delegate {
						string url = string.Format(templateurl, 1);
						await _downloader.Queue (url);
						Sync();

					}),
					new StringElement ("AddAll", async delegate {
						for(int i=1; i<80; i++) {
							string url = string.Format(templateurl, i);
							_downloader.Queue (url);
						}
						Sync();

					}),
					new StringElement ("Add 404", async delegate {
						string url = string.Format(templateurl, 404);
						_downloader.Queue (url);
						Sync();

					}),
					new StringElement ("Reset", async delegate {
						await _downloader.Reset();
						Sync();
					}),
					new StringElement ("Sync", delegate {
						Sync();
					}),
				},
				_downloads
			};

			var sample = new DialogViewController (root);
			var nav = new UINavigationController (sample);
			Window.RootViewController = nav;
			Window.MakeKeyAndVisible ();

			return true;
		}

		void Sync ()
		{
			var list = _downloader.List();
			var slist = list.Select (item => {
				var url = item.Url;
				var surl = url.Substring(url.Length -10, 10);
				var s = string.Format ("{0} {1} {2} {3} {4}", item.Id, surl, item.State, item.Temporary, item.StatusCode);
				return s;
			});
			
			var elements = from s in slist
				select new StringElement(s);
			_downloads.Clear();
			_downloads.AddAll(elements);
		}
	}
}


