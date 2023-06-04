using System;
using System.Collections.Generic;
using EXRCore.DIContainer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.EcsFramework {
	public sealed class EntityBuilder {
		private readonly GameObject prefab;
		
		private IDictionary<Type, IPersistentComponent> components;
		private IDictionary<Type, IEcsSystem> systems;
		
		[InjectService] private EcsWorld world;
		
		public EntityBuilder(GameObject prefab) => this.prefab = prefab;

		public EntityBuilder AddComponent<T>(T component) where T : class, IPersistentComponent {
			var key = typeof(T);
			
			components ??= new Dictionary<Type, IPersistentComponent>();
			if (components.ContainsKey(key)) {
				Debug.LogWarning($"Component {key} already registered in builder!");
				return this;
			}
			
			components[key] = component;
			return this;
		}

		public EntityBuilder AddSystem<T>(T system) where T : class, IEcsSystem {
			var key = typeof(T);
			
			systems ??= new Dictionary<Type, IEcsSystem>();
			if (systems.ContainsKey(key)) {
				Debug.LogWarning($"System {key} already registered in builder!");
				return this;
			}
			
			ServiceContainer.ExecuteInjection(system);
			systems[key] = system;
			return this;
		}
		
		public Entity Create(Vector3 position, Quaternion rotation, Transform parent = null) {
			var componentsProvider = components != null ? new EcsComponentsProvider(components) : null;
			var systemsProvider = systems != null ? new EcsSystemsProvider(systems) : null;

			GameObject owner = Object.Instantiate(prefab, position, rotation, parent);
			return world.CreateEntity(owner, componentsProvider, systemsProvider);
		}
	}
}