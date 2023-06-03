using UnityEngine;

namespace EXRCore.UIFramework {
	public abstract class InitializatorUI : MonoBehaviour {
		[SerializeField] private ScreenContainer container;
		
		private UIService service;
		
		private void OnEnable() {
			service = new UIService();
			service.Initialize(container);
			Initialize(service);
		}
		
		private void OnDisable() => service.Dispose();
		
		protected abstract void Initialize(IUIService service);
	}
}