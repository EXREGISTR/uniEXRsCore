using UnityEngine;

namespace EXRCore.Services {
	public abstract class MonoInstaller : MonoBehaviour, IInstaller {
		public abstract void Install(IServiceContainer container);
	}
}