using System;
using System.Collections.Generic;

namespace EXRCore.EcsFramework {
	public class EcsComponentsProvider : EcsProvider<IPersistentComponent> {
		public EcsComponentsProvider(IReadOnlyDictionary<Type, Func<IPersistentComponent>> components)
			: base(components) { }
		
		public EcsComponentsProvider(IDictionary<Type, IPersistentComponent> components) : base(components) { }
	}
}