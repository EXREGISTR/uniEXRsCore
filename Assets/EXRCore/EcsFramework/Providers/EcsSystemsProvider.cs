using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace EXRCore.EcsFramework {
	public class EcsSystemsProvider : EcsProvider<IEcsSystem> {
		public EcsSystemsProvider(IDictionary<Type, IEcsSystem> systems, bool needToCopy = true) : base(systems, needToCopy) { }
		
		public void Initialize(Entity context, [CanBeNull] EcsProvider<IPersistentComponent> components) {
			foreach (var system in subjects.Values) {
				system.Initialize(context, components);
				system.Enable();
			}
		}

		public void Enable<T>() where T: IEcsSystem {
			if (subjects.TryGetValue(typeof(T), out var system)) {
				system.Enable();
			}
		}

		public void Disable<T>() where T: IEcsSystem {
			if (subjects.TryGetValue(typeof(T), out var system)) {
				system.Disable();
			}
		}

		public void FixedUpdate() {
			foreach (var system in subjects.Values) {
				system.FixedUpdate();
			}
		}
		
		public void Update() {
			foreach (var system in subjects.Values) {
				system.Update();
			}
		}
	}
}