using UnityEngine;

namespace EXRCore.Services {
	public abstract class Installer : MonoBehaviour {
		public abstract void Install(IServiceContainer container);
	}
}