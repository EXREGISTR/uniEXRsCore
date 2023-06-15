using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.EcsFramework {
	public sealed class EntityBuilder {
		private readonly GameObject prefab;
		
		private IDictionary<int, IPersistentComponent> components;
		private IDictionary<int, IEcsSystem> systems;
		
		public EntityBuilder(GameObject prefab, int componentsStartCapacity = 4, int systemsStartCapacity = 4) {
			this.prefab = prefab;
			if (componentsStartCapacity != 0) components = new Dictionary<int, IPersistentComponent>(componentsStartCapacity);
			if (systemsStartCapacity != 0) systems = new Dictionary<int, IEcsSystem>(systemsStartCapacity);
		}
		
		public EntityBuilder AddComponent<T>(T component) where T : class, IPersistentComponent => RegisterSubject(ref components, component);
		public EntityBuilder AddSystem<T>(T system) where T : class, IEcsSystem => RegisterSubject(ref systems, system);
		
		private EntityBuilder RegisterSubject<T>(ref IDictionary<int, T> target, in T subject) where T : class {
			var key = TypeHelper<T>.Identity;
			
			target ??= new Dictionary<int, T>();
			if (target.ContainsKey(key)) {
				Debug.LogWarning($"Subject {key} already registered in builder!");
				return this;
			}
			
			target[key] = subject;
			return this;
		}

		public Entity Create(EcsWorld world, Vector3 position, Quaternion rotation, Transform parent = null) {
			var componentsProvider = components != null ? new EcsComponentsProvider(components) : null;
			var systemsProvider = systems != null ? new EcsSystemsProvider(systems) : null;
			
			GameObject owner = Object.Instantiate(prefab, position, rotation, parent);
			return world.CreateEntity(owner, componentsProvider, systemsProvider);
		}
	}
}