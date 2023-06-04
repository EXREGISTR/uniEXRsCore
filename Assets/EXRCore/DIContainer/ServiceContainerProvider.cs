using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.DIContainer {
	[DefaultExecutionOrder(-10000)]
	public class ServiceContainerProvider : MonoBehaviour {
		[SerializeField] private List<Installer> installers;
		[SerializeField] private bool initializeOnAwake = true;
		
		private ServiceContainer container;

		private void Awake() {
			if (!initializeOnAwake) return;
			Initialize();
		}

		public void Initialize() {
			if (container != null) {
				Debug.LogError("Container already created!");
				return;
			}
			
			container = ServiceContainer.Create();
			foreach (var installer in installers) {
				installer.Install(container);
			}
		}
	}
}