﻿using System;
using System.Collections.Generic;
using EXRCore.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.EcsFramework {
	public sealed class EntityBuilder {
		private readonly GameObject prefab;
		
		private IDictionary<Type, IPersistentComponent> components;
		private IDictionary<Type, IEcsSystem> systems;
		
		private EntityBuilder(GameObject prefab) => this.prefab = prefab;

		public EntityBuilder AddComponent<T>(T component) where T : IPersistentComponent {
			var key = typeof(T);

			components ??= new Dictionary<Type, IPersistentComponent>();
			if (components.ContainsKey(key)) {
				Debug.LogWarning($"Component {key} already registered in builder!");
				return this;
			}

			components[key] = component;
			return this;
		}

		public EntityBuilder AddSystem<T>(T system) where T : IEcsSystem {
			var key = typeof(T);
			
			systems ??= new Dictionary<Type, IEcsSystem>();
			if (systems.ContainsKey(key)) {
				Debug.LogWarning($"System {key} already registered in builder!");
				return this;
			}

			systems[key] = system;
			return this;
		}
		
		public Entity Create(Vector3 position, Quaternion rotation, Transform parent = null) {
			var componentsProvider = new EcsComponentsProvider(components, false);
			var systemsProvider = new EcsSystemsProvider(systems, false);
			
			GameObject owner = Object.Instantiate(prefab, position, rotation, parent);
			return Service<EcsWorld>.Instance.CreateEntity(owner, componentsProvider, systemsProvider);
		}
	}
}