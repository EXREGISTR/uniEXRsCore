using System;
using System.Threading;
using System.Threading.Tasks;

namespace EXRCore.UIFramework {
	public static class Extenssions {
		public static async Task ShowAsync<THandler>(this IUIService uiService, THandler handler, float lifeTimeInSeconds, 
			CancellationToken token, bool closeOnCancelledOperation = true)
			where THandler : IOneTimeHandler {
			uiService.Show(handler);
			await uiService.CloseAsync<THandler>(lifeTimeInSeconds, token, closeOnCancelledOperation);
		}
		
		public static async Task CloseAsync<THandler>(this IUIService uiService, float closeDelay, CancellationToken token, 
			bool closeOnCancelledOperation = true)
			where THandler : IOneTimeHandler {
			try {
				await Task.Delay(TimeSpan.FromSeconds(closeDelay), token);
			} catch (OperationCanceledException) {
				if (closeOnCancelledOperation) uiService.Close<THandler>(false);
				return;
			} 
			
			uiService.Close<THandler>(false);
		}
	}
}
