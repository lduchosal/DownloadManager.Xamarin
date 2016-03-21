using Foundation;
using UIKit;
using MonoTouch.Dialog;
using System.Linq;
using DownloadManager.iOS;
using System;
using ObjCRuntime;
using System.Threading;
using System.Threading.Tasks;
using DownloadManager.iOS.Bo;
using System.IO;

namespace DownloadManager.Sample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{

		public override UIWindow Window {
			get;
			set;
		}

		public UINavigationController Navigation {
			get;
			set;
		}

		Section _downloads;
		Downloader _downloader;
		DialogViewController _sample;

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
				//_downloader.Run();
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

		public void FinishedDownload(string url, string description, string temporaryfile) {
			
			Console.WriteLine ("[AppDelegate] FinishedDownload");
			Console.WriteLine ("[AppDelegate] FinishedDownload Url           : {0}", url);
			Console.WriteLine ("[AppDelegate] FinishedDownload Description   : {0}", description);
			Console.WriteLine ("[AppDelegate] FinishedDownload Temporaryfile : {0}", temporaryfile);

			bool exists = File.Exists (temporaryfile);
			Console.WriteLine ("[AppDelegate] FinishedDownload Exists        : {0}", exists);
			File.Delete (temporaryfile);

		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			Console.WriteLine ("[AppDelegate] FinishedLaunching");

			UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval (UIApplication.BackgroundFetchIntervalMinimum);
			Window = new UIWindow (UIScreen.MainScreen.Bounds);

			_downloader = new Downloader ();
			_downloader.Finished = FinishedDownload;
			_downloads = new Section ("Downloads") {
			}; 

			string templateurl = "http://pokeprice.local/api/v1/card/image/BLW/{0}";
			string zipurl = "http://pokeprice.local/api/v1/card/zip/{0}";
			string httpurl = "http://pokeprice.local/api/v1/http/{0}";
			string redirecturl = "http://pokeprice.local/api/v1/http/redirect/infinite/0";
			string scollections = "AOR,AQ,AR,B2,BCR,BEST,BKT,BLW,BS,CG,CL,DCR,DEX,DF,DP,DR,DRV,DRX,DS,DX,EM,EPO,EX,FFI,FLF,FO,G1,G2,GE,HL,HP,HS,JU,KSS,LA,LC,LM,LTR,MA,MCD2011,MCD2012,MD,MT,N1,N2,N3,N4,NINTENDOBLACK,NVI,NXD,PHF,PK,PL,PLB,PLF,PLS,POP1,POP2,POP3,POP4,POP5,POP6,POP7,POP8,POP9,PR-BLW,PR-DP,PR-HS,PR-XY,PRC,RG,ROS,RR,RS,RU,SF,SI,SK,SS,SV,SW,TM,TR,TRR,UD,UF,UL,VICTORY,WIZARDSBLACK,XY";
			string[] collections = scollections.Split (',');

			#if REMOTE
			templateurl = templateurl.Replace("http://pokeprice.local", "https://pokeprice.com");
			zipurl = zipurl.Replace("http://pokeprice.local", "https://pokeprice.com");
			httpurl = httpurl.Replace("http://pokeprice.local", "https://pokeprice.com");
			redirecturl = redirecturl.Replace("http://pokeprice.local", "https://pokeprice.com");
			#endif

			var enable = new BooleanElement ("Enabled", true);
			enable.ValueChanged += (sender, e) => {
				_downloader.Enabled = enable.Value;
			};

			var globalprogress = new StringElement ("");
			var root = new RootElement ("Root") {
				new Section ("Management"){
					enable,
					globalprogress
				},
				_downloads
			};

			_downloader.Progress += (progress) => {
				float percent = (progress.Written / (float)progress.Total) * 100;
				int ipercent = (int)percent;
				string caption = string.Format("{0} {1} {2}% ({3} / {4})", 
					"Global", 
					progress.State.ToString(),
					ipercent,
					progress.Written,
					progress.Total
				);
				InvokeOnMainThread(() => {
					globalprogress.Caption = caption;
					globalprogress
						.GetImmediateRootElement()
						.Reload(globalprogress, UITableViewRowAnimation.Automatic);
				});
			};
			
			_sample = new DialogViewController (root);

			var add = new UIBarButtonItem ("Add", 
				UIBarButtonItemStyle.Bordered, 
				 (sender, e) => {
					string url = string.Format(templateurl, 1);
				 Add(url);
			});
			
			var addall = new UIBarButtonItem ("All", 
				UIBarButtonItemStyle.Bordered, 
				 (sender, e) => {
					for(int i=1; i<80; i++) {
						string url = string.Format(templateurl, i);
					 Add(url);
					}
				});
			
			var zips = new UIBarButtonItem ("Z", 
				UIBarButtonItemStyle.Bordered, 
				 (sender, e) => {
					foreach(string coll in collections) {
						string url = string.Format(zipurl, coll);
					 Add(url);
					}
				});


			var s404 = new UIBarButtonItem ("4", 
				UIBarButtonItemStyle.Bordered, 
				 (sender, e) => {
					string url = string.Format(httpurl, 404);
					 Add(url);
				});

			var s500 = new UIBarButtonItem ("5", 
				UIBarButtonItemStyle.Bordered, 
				 (sender, e) => {
					string url = string.Format(httpurl, 500);
					 Add(url);
				});

			var s301 = new UIBarButtonItem ("3", 
				UIBarButtonItemStyle.Bordered, 
				 (sender, e) => {
					string url = string.Format(httpurl, 301);
					 Add(url);
				});


			var s301p = new UIBarButtonItem ("3+", 
				UIBarButtonItemStyle.Bordered, 
				 (sender, e) => {
					string url = string.Format(redirecturl, 301);
					 Add(url);
				});

			var reset = new UIBarButtonItem ("Reset", 
				UIBarButtonItemStyle.Bordered, 
				async (sender, e) => {
					await _downloader.Reset();
					Sync();
				});

			var sync = new UIBarButtonItem ("S", 
				UIBarButtonItemStyle.Bordered, 
				(sender, e) => {
					Sync();
				});


			Navigation = new UINavigationController ();
			Window.RootViewController = Navigation;
			Window.MakeKeyAndVisible ();

			Navigation.PushViewController (_sample, false);

			_sample.SetToolbarItems (new [] { add, addall, zips, s404, s500, s301, s301p, reset, sync}, true);
			Navigation.SetToolbarHidden (false, true);

			return true;
		}

		void Add (string url)
		{
			var s = string.Format ("{0}", 1);

			Action<Download> refresh = (download) => { 
				Console.WriteLine ("[AppDelegate] ProgressChanged {0}", download.Id);
			};
			var progress = _downloader.Queue (url, null, refresh);

			Action tapped = () => {

				Download detail;
				bool queued = _downloader.TryDetail(url, out detail);
				if (!queued) {
					return ;
				}

				var nextroot = new RootElement(s) {
					new Section ("Actions"){
						new StringElement("Back", () => {
							Navigation.PopViewController(true);
						}),
					},
					new Section ("Detail"){
						new StringElement("Id", detail.Id.ToString()),
						new StringElement("Url", detail.Url),
						new StringElement("State", detail.State.ToString()),
						new StringElement("LastModified", detail.LastModified.ToString()),
						new StringElement("DownloadState", detail.DownloadState.ToString()),
						new StringElement("Temporary", detail.Temporary),
						new StringElement("Total", detail.Total.ToString()),
						new StringElement("Written", detail.Written.ToString()),
						new StringElement("StatusCode", detail.StatusCode.ToString()),
						new StringElement("Description", detail.ErrorDescription),
						new StringElement("Error", detail.Error.ToString()),
					},
				};
				var dvc = new DialogViewController (nextroot);
				Navigation.PushViewController(dvc, true);
			};


			var element = new ProgressElement(s, tapped, progress);

			Action<Download> progressing = (d) => {
				InvokeOnMainThread (() => {
					Console.WriteLine ("[ProgressElement] Bind {0} OnMainThread", d.Id);
					element.Caption = Caption (d);
					element.GetImmediateRootElement ()
						.Reload (element, UITableViewRowAnimation.Automatic);
				}
				);
			};
			progress.Bind(progressing);

			_downloads.Insert(0, UITableViewRowAnimation.Top, element);

		}

		string Caption(Download download) {
			
			float percent = download.Total == 0 ? 0f :
				(download.Written / (float)download.Total) * 100;
			int ipercent = (int)percent;

			string caption = string.Format("{0} {1} {2}% ({3} / {4})", 
				download.Id, 
				download.State.ToString(),
				ipercent,
				download.Written,
				download.Total
			);
			return caption;

		}

		void Sync ()
		{
			Console.WriteLine ("[AppDelegate] Sync");

			var list = _downloader.List();
			_downloads.Clear();

			foreach (var item in list) { 
				Add (item.Url);
			}
		}

	}
}


