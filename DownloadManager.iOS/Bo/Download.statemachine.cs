using System;
using Stateless;

namespace DownloadManager.iOS.Bo
{
	public partial class Download
	{

		private readonly StateMachine<State, Transition> _machine;
		public Download ()
		{

			State = Bo.State.Waiting;
			_machine = new StateMachine<State, Transition>(() => State, s => State = s);

			_machine.Configure (Bo.State.Waiting)
				.Permit (Transition.Cancel, Bo.State.Finished)
				.Permit (Transition.Fail, Bo.State.Error)
				.Permit (Transition.Resume, Bo.State.Downloading);

			_machine.Configure (Bo.State.Downloading)
				.Permit (Transition.Pause, Bo.State.Waiting)
				.PermitReentry (Transition.Progress)
				.Permit (Transition.Cancel, Bo.State.Finished)
				.Permit (Transition.Finish, Bo.State.Finished)
				.Permit (Transition.Fail, Bo.State.Error);

			_machine.Configure (Bo.State.Finished);

			_machine.Configure (Bo.State.Error)
				.Permit (Transition.Retry, Bo.State.Waiting)
				.Permit (Transition.Cancel, Bo.State.Finished)
				.Permit (Transition.Fail, Bo.State.Finished);
		}

		public void Cancel() {
			_machine.Fire (Transition.Cancel);
		}

		public bool TryFail(int statuscode, TaskErrorEnum error, string description) {
			if (!_machine.CanFire(Transition.Fail)) {
				return false;
			}
			_machine.Fire (Transition.Fail);
			StatusCode = statuscode;
			Error = (int)error;
			Description = description;
			return true;
		}

		public bool TryResume() {
			if (!_machine.CanFire(Transition.Resume)) {
				return false;
			}
			_machine.Fire (Transition.Resume);
			LastModified = DateTime.Now;
			return true;
		}

		public bool TryPause() {
			if (!_machine.CanFire(Transition.Pause)) {
				return false;
			}
			_machine.Fire (Transition.Pause);
			return true;
		}				


		public bool TryProgress(long written, long total) {

			LastModified = DateTime.Now;
			Written = Math.Max(written, Written);
			Total = Math.Max(total, Total);

			if (!_machine.CanFire(Transition.Progress)) {
				return false;
			}
			_machine.Fire (Transition.Progress);
			return true;
		}

		public bool TryFinish (string temporary)
		{
			if (!_machine.CanFire(Transition.Finish)) {
				return false;
			}
			_machine.Fire (Transition.Finish);
			LastModified = DateTime.Now;
			Temporary = temporary;
			return true;
		}

		public void Retry() {
			_machine.Fire (Transition.Retry);
		}

		private enum Transition {
			Queue,
			Cancel,
			Pause,
			Resume,
			Progress,
			Finish,
			Fail,
			Retry
		}

	}
	public enum State : int {
		Waiting = 1,
		Downloading = 2,
		Finished = 3,
		Error = 4
	}

}

