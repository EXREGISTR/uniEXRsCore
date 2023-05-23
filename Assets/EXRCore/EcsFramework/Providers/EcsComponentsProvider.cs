using System;
using System.Collections.Generic;

namespace EXRCore.EcsFramework {
	public class EcsComponentsProvider : EcsProvider<IPersistentComponent> {
		public EcsComponentsProvider(IDictionary<Type, IPersistentComponent> components, bool needToCopy)
			: base(components, needToCopy) { }

		public void ResetAll() {
			foreach (var component in subjects.Values) {
				component.Reset();
			}
		}
	}
}