using Foundation;
using UIKit;
using MonoTouch.Dialog;
using System.Linq;
using DownloadManager.iOS;
using System;

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

		public override void HandleEventsForBackgroundUrl (UIApplication application, string sessionIdentifier, System.Action completionHandler)
		{
			_downloader.Completion = completionHandler;
		}

		public override void DidEnterBackground (UIApplication application)
		{
			Console.WriteLine ("[AppDelegate] DidEnterBackground");
		}

		public override void WillEnterForeground (UIApplication application)
		{
			Console.WriteLine ("[AppDelegate] WillEnterForeground");
		}

		public override void PerformFetch (UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
		{
			Console.WriteLine ("[AppDelegate] PerformFetch");

			var result = UIBackgroundFetchResult.NoData;

			try 
			{
				_downloader.Run();
				result = UIBackgroundFetchResult.NewData;
			}
			catch 
			{
				result = UIBackgroundFetchResult.Failed;
			}
			finally
			{
				completionHandler (result);
			}
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			Console.WriteLine ("[AppDelegate] FinishedLaunching");

			UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval (UIApplication.BackgroundFetchIntervalMinimum);

			Window = new UIWindow (UIScreen.MainScreen.Bounds);

			_downloader = new Downloader ();
			_downloads = new Section ("Downloads") {
			}; 

			string templateurl = "http://pokeprice.local/api/v1/card/image/BLW/{0}";
			string zipurl = "http://pokeprice.local/api/v1/card/zip/{0}";
			string httpurl = "http://pokeprice.local/api/v1/http/{0}";
			string redirecturl = "http://pokeprice.local/api/v1/http/redirect/infinite/0";
			string scollections = "AOR,AQ,AR,B2,BCR,BEST,BKT,BLW,BS,CG,CL,DCR,DEX,DF,DP,DR,DRV,DRX,DS,DX,EM,EPO,EX,FFI,FLF,FO,G1,G2,GE,HL,HP,HS,JU,KSS,LA,LC,LM,LTR,MA,MCD2011,MCD2012,MD,MT,N1,N2,N3,N4,NINTENDOBLACK,NVI,NXD,PHF,PK,PL,PLB,PLF,PLS,POP1,POP2,POP3,POP4,POP5,POP6,POP7,POP8,POP9,PR-BLW,PR-DP,PR-HS,PR-XY,PRC,RG,ROS,RR,RS,RU,SF,SI,SK,SS,SV,SW,TM,TR,TRR,UD,UF,UL,VICTORY,WIZARDSBLACK,XY";
			string[] collections = scollections.Split (',');

			var root = new RootElement ("Root") {
				new Section ("Management"){
					new BooleanElement ("Enabled", true),
					//new EntryElement ("MaxDownloads", "Simultaneous", "4"),
					new StringElement ("Add", async delegate {
						string url = string.Format(templateurl, 1);
						await _downloader.Queue (url);
						Sync();

					}),
					new StringElement ("AddAll",  delegate {
						for(int i=1; i<80; i++) {
							string url = string.Format(templateurl, i);
							_downloader.Queue (url);
						}
						Sync();
					}),
					new StringElement ("AddZip",  delegate {
						foreach(string coll in collections) {
							string url = string.Format(zipurl, coll);
							_downloader.Queue (url);
						}
						Sync();
					}),
					new StringElement ("Add 404",  delegate {
						string url = string.Format(httpurl, 404);
						_downloader.Queue (url);
						Sync();
					}),
					new StringElement ("Add 500",  delegate {
						string url = string.Format(httpurl, 500);
						_downloader.Queue (url);
						Sync();
					}),
					new StringElement ("Add 301 Invalid Redirect",  delegate {
						string url = string.Format(httpurl, 301);
						_downloader.Queue (url);
						Sync();
					}),
					new StringElement ("Add 301 Infinite Redirect",  delegate {
						string url = string.Format(redirecturl, 301);
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
			Console.WriteLine ("[AppDelegate] DidEnterBackground");

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


