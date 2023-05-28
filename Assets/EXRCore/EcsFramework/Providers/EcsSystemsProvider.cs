using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace EXRCore.EcsFramework {
	public class EcsSystemsProvider : EcsProvider<IEcsSystem> {
		public EcsSystemsProvider(IDictionary<Type, IEcsSystem> systems, bool needToCopy = true) : base(systems, needToCopy) { }
		
		public void Initialize(Entity context, [CanBeNull] EcsProvider<IPersistentComponent> components, bool enableSystemsNow) {
			Action<IEcsSystem> action;
			if (enableSystemsNow) {
				action = system => {
					system.Initialize(context, components);
					system.Enable();
				};
			}
			else {
				action = system => system.Initialize(context, components);
			}

			foreach (var system in subjects.Values) {
				action(system);
			}
		}
		
		public void EnableAll() {
			foreach (var system in subjects.Values) {
				system.Enable();
			}
		}

		public void DisableAll() {
			foreach (var system in subjects.Values) {
				system.Disable();
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