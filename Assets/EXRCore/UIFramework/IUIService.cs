using System;
using JetBrains.Annotations;

namespace EXRCore.UIFramework {
	public interface IUIService : IDisposable {
		public void Initialize([NotNull] IScreensContainer container);
		public void Show<THandler>() where THandler : IPersistentViewHandler;
		public void Show<THandler>([NotNull] THandler handler) where THandler : IOneTimeHandler;
		public void Close<THandler>(bool showWarning = true) where THandler : IViewHandler;
		public void BindHandler<THandler>([NotNull] THandler handler) where THandler : IPersistentViewHandler;
	}
}