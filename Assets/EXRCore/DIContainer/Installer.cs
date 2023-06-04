using UnityEngine;

namespace EXRCore.DIContainer {
	public abstract class Installer : MonoBehaviour {
		public abstract void Install(ServiceContainer container);
	}
}