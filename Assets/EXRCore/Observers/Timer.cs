using System;
using System.Threading;
using System.Threading.Tasks;

namespace EXRCore.Observers {
	public sealed class Timer {
		private readonly Action callback;
		private readonly float secondsDelay;
		
		public Timer(Action callback, in float secondsDelay) {
			this.callback = callback;
			this.secondsDelay = secondsDelay;
		}
		
		public async void Start(CancellationToken token) {
			await Task.Delay(TimeSpan.FromSeconds(secondsDelay), token);
			callback();
		}
	}
}