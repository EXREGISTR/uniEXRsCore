using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace EXRCore.EcsFramework {
	public class EcsSystemsProvider : EcsProvider<IEcsSystem>, IDisposable {
		public EcsSystemsProvider(IReadOnlyDictionary<Type, Func<IEcsSystem>> systems) : base(systems) { }
		public EcsSystemsProvider(IDictionary<Type, IEcsSystem> systems) : base(systems) { }

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

			ExecuteForAll(action);
		}

		public void EnableAll() => ExecuteForAll(system => system.Enable());
		public void DisableAll() => ExecuteForAll(system => system.Disable());
		
		public void Enable<T>() where T : IEcsSystem => ExecuteFor<T>(system => system.Enable());
		public void Disable<T>() where T : IEcsSystem => ExecuteFor<T>(system => system.Disable());
		
		public void FixedUpdate() => ExecuteForAll(system => system.FixedUpdate());
		public void Update() => ExecuteForAll(system => system.Update());
		public void Dispose() => ExecuteForAll(system => system.OnDestroy());
	}
}