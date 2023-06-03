using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public abstract class EntityConfig : ScriptableObject {
		internal struct EntityData {
			public GameObject Prefab { get; }
			public EcsComponentsProvider Components { get; }
			public EcsSystemsProvider Systems { get; }
			
			internal EntityData(GameObject prefab, EcsComponentsProvider components, EcsSystemsProvider systems) {
				Prefab = prefab;
				Components = components;
				Systems = systems;
			}
		}
		
		private readonly Dictionary<Type, Func<IPersistentComponent>> components = new();
		private readonly Dictionary<Type, Func<IEcsSystem>> systems = new();
		
		[SerializeField] 
		private GameObject prefab;
		
		internal abstract void Initialize();
		
		internal EntityData CreateEntityData() {
			var componentsProvider = new EcsComponentsProvider(components);
			var systemsProvider = new EcsSystemsProvider(systems);
			return new EntityData(prefab, componentsProvider, systemsProvider);
		}

		public void RegisterComponent<T>(Func<T> creator) where T: IPersistentComponent {
			Register(typeof(T), components, () => creator());
		}

		public void RegisterSystem<T>(Func<T> creator) where T : IEcsSystem {
			Register(typeof(T), systems, () => creator());
		}
		
		public void ReplaceComponent<T>(Func<T> creator) where T : IPersistentComponent {
			Replace(typeof(T), components, () => creator(), true);
		}
		
		public void ReplaceSystem<T>(Func<T> creator) where T : IEcsSystem {
			Replace(typeof(T), systems, () => creator(), true);
		}

		public void UnregisterComponent<T>() where T: IPersistentComponent => components.Remove(typeof(T));
		public void UnregisterSystem<T>() where T: IEcsSystem => systems.Remove(typeof(T));
		
		private static void Register<T>(Type subjectType, IDictionary<Type, Func<T>> target, Func<T> creator) where T: IEcsSubject {
			if (target.ContainsKey(subjectType)) {
				Debug.LogWarning($"Creator for subject {subjectType} already registered");
				return;
			}
			
			Replace(subjectType, target, creator, false);
		}
		private static void Replace<T>(Type subjectType, IDictionary<Type, Func<T>> target, Func<T> creator, bool needToRemove) 
			where T: IEcsSubject {
			if (subjectType.IsAbstract) {
				throw new ArgumentException("Impossible to register key of abstract subject!");
			}

			if (needToRemove) target.Remove(subjectType);
			target[subjectType] = creator;
		}
	}
}