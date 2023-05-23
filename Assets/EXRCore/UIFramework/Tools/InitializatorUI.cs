using UnityEngine;

namespace EXRCore.UIFramework {
	public abstract class InitializatorUI : MonoBehaviour {
		[SerializeField] private ScreenContainer container;

		private readonly IUIService service = new UIService();
		
		private void OnEnable() {
			service.Initialize(container);
			Initialize(service);
		}
		
		private void OnDisable() => service.Dispose();
		
		protected abstract void Initialize(IUIService service);
	}
}