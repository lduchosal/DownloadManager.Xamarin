using DownloadManager.iOS;
using System.Threading.Tasks;
using Foundation;
using Fabrik.SimpleBus;

namespace DownloadManager.Test
{
	public class NSUrlSessionManagerMock 
	{
		private readonly IBus _bus;
		public NSUrlSessionManagerMock (IBus bus)
		{
			_bus = bus;
		}

		public async Task<bool> FreeSlot() {
			await _bus.SendAsync<FreeSlot> (new FreeSlot());
			return true;
		}

		public async Task<bool> StartedDownload(string url) {
			await _bus.SendAsync<StartedDownload> (new StartedDownload {
				Url = url
			});

			return true;
		}

		public async Task<bool> ProgressDownload (int id, int bytes, int total)
		{
			await _bus.SendAsync<ProgressDownload> (new ProgressDownload {
				Id = id,
				Written = bytes,
				Total = total,			
			});
			return true;
		}

		public async Task<bool> DownloadFinished (int id, string location)
		{
			await _bus.SendAsync<FinishedDownload> (new FinishedDownload {
				Id = id,
				Location = location,
			});
			return true;

		}

		public async Task<bool> DownloadError (ErrorEnum error, string description = null)
		{
			await _bus.SendAsync<DownloadError> (new DownloadError {
				Error = error,
				Description = description
			});
			return true;

		}

	}
}

