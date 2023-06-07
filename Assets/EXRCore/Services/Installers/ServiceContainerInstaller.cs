using System;
using UnityEngine;

namespace EXRCore.Services {
	public sealed class ServiceContainerInstaller : MonoBehaviour {
		[SerializeField] private MonoInstaller[] monoInstallers;
		[SerializeField] private ScriptableInstaller[] scriptableInstallers;
		[Space, SerializeField] private bool initializeOnAwake = true;
		
		private IServiceContainer container;

		private void Awake() {
			if (!initializeOnAwake) return;
			Initialize();
		}

		private void OnValidate() {
			monoInstallers ??= Array.Empty<MonoInstaller>();
			scriptableInstallers ??= Array.Empty<ScriptableInstaller>();
		}

		private void OnDestroy() => container?.Dispose();
		
		public void Initialize() {
			if (container != null) {
				Debug.LogWarning("Container already created!");
				return;
			}
			
			container = ServiceContainer.Create();
			foreach (var installer in monoInstallers) {
				installer.Install(container);
			}

			foreach (var installer in scriptableInstallers) {
				installer.Install(container);
			}
		}
	}
}