using UnityEngine;

namespace EXRCore.Services {
	public abstract class ScriptableInstaller : ScriptableObject, IInstaller {
		public abstract void Install(IServiceContainer container);
	}
}