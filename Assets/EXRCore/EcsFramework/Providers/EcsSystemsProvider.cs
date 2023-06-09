﻿using System;
using System.Collections.Generic;

namespace EXRCore.EcsFramework {
	public sealed class EcsSystemsProvider : EcsProvider<IEcsSystem>, IDisposable {
		public EcsSystemsProvider(IReadOnlyDictionary<int, Func<IEcsSystem>> systems) : base(systems) { }
		public EcsSystemsProvider(IDictionary<int, IEcsSystem> systems) : base(systems) { }
		
		public void Initialize(Entity context, EcsProvider<IPersistentComponent> components, bool enableSystemsNow) {
			Action<IEcsSystem> action;
			if (enableSystemsNow) {
				action = system => {
					system.InitializeAndEnable(context, components, this);
				};
			} else {
				action = system => {
					system.Initialize(context, components, this);
				};
			}
			ExecuteForAll(action);
		}
		
		public void EnableAll() => ExecuteForAll(system => system.Enable());
		public void DisableAll() => ExecuteForAll(system => system.Disable());
		
		public void Enable<T>() where T : class, IEcsSystem => ExecuteFor<T>(system => system.Enable());
		public void Disable<T>() where T : class, IEcsSystem => ExecuteFor<T>(system => system.Disable());
		
		public void Dispose() => ExecuteForAll(system => system.OnDestroy());
	}
}