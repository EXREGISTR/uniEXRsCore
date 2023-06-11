using System;
using System.Collections.Generic;

namespace EXRCore.Utils {
	public class EcsComponentsProvider : EcsProvider<IPersistentComponent> {
		public static EcsComponentsProvider Empty => new();
		
		private EcsComponentsProvider() { }
		public EcsComponentsProvider(IReadOnlyDictionary<int, Func<IPersistentComponent>> components)
			: base(components) { }
		
		public EcsComponentsProvider(IDictionary<int, IPersistentComponent> components) : base(components) { }
	}
}