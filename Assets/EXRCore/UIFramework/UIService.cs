using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.UIFramework {
	public sealed class UIService : IUIService {
		private Dictionary<Type, IScreen> screensMap;
		
		private readonly Dictionary<Type, (IScreen screen, IViewHandler handler)> shownScreens = new();
		private readonly Dictionary<Type, IPersistentViewHandler> persistentViewHandlers = new();
		
		public void Initialize(IScreensContainer container) {
			var screens = container.GetScreens();
			screensMap = new Dictionary<Type, IScreen>(screens.Count);
            			
			foreach (var screen in screens) {
				screensMap[screen.HandlerType] = screen;
			}
		}

		public void Show<THandler>() where THandler : IPersistentViewHandler {
			var key = typeof(THandler);
			if (!screensMap.TryGetValue(key, out var screen)) {
				Debug.LogError($"No screen for handler {key.FullName}!");
				return;
			}

			screen.Show();
			shownScreens[key] = (screen, persistentViewHandlers[key]);
		}
		
		public void Show<THandler>(THandler handler) where THandler : IOneTimeHandler {
			var key = typeof(THandler);
			if (!screensMap.TryGetValue(key, out var screen)) {
				Debug.LogError($"No screen for handler {key.FullName}!");
				return;
			}
			
			Bind(screen, handler);
			screen.Show();
			shownScreens[key] = (screen, handler);
		}

		public void Close<THandler>(bool showWarning = true) where THandler : IViewHandler {
			var key = typeof(THandler);
			
			if (!shownScreens.TryGetValue(key, out var shown)) {
				if (showWarning) Debug.LogWarning($"No shown screen for handler {key.FullName}!");
				return;
			}
			
			shown.screen.Hide();
			shownScreens.Remove(key);

			if (shown.handler is IOneTimeHandler) {
				shown.screen.Dispose();
				shown.handler.Dispose();
			}
		}

		public void BindHandler<THandler>(THandler handler) where THandler : IPersistentViewHandler {
			var key = typeof(THandler);
			if (!screensMap.TryGetValue(key, out var screen)) {
				Debug.LogError($"No screen for handler {key.FullName}!");
				return;
			}
			
			Bind(screen, handler);
			persistentViewHandlers[key] = handler;
		}

		private void Bind(IScreen screen, IViewHandler handler) {
			handler.InjectUIService(this);
			screen.BindHandler(handler);
		}
		
		public void Dispose() {
			shownScreens.Clear();
			screensMap.Clear();
			persistentViewHandlers.Clear();
		}
	}
}