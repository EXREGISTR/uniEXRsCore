using UnityEngine;

namespace EXRCore.Services {
	public sealed class ServiceContainerInstaller : MonoBehaviour {
		[SerializeField] private Installer[] installers;
		[SerializeField] private bool initializeOnAwake = true;
		
		private IServiceContainer container;

		private void Awake() {
			if (!initializeOnAwake) return;
			Initialize();
		}

		private void OnDestroy() => container?.Dispose();
		
		public void Initialize() {
			if (container != null) {
				Debug.LogWarning("Container already created!");
				return;
			}
			
			container = ServiceContainer.Create();
			foreach (var installer in installers) {
				installer.Install(container);
			}
		}
	}
}