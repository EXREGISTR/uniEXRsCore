using System;
using System.Collections.Generic;
using EXRCore.Attributes;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public abstract class EntityFactory : ScriptableObject {
		private Dictionary<int, Func<IPersistentComponent>> components = new();
		private Dictionary<int, Func<IEcsSystem>> systems = new();

		[SerializeField] private GameObject prefab;
		
		[field: ReadOnly]
		[field: SerializeField] public int Identity { get; private set; } = -1;
		
		private void OnValidate() {
			if (Identity != -1) return;
			
			Identity = GetType().GetHashCode();
		}

		internal void Initialize() {
			components = new Dictionary<int, Func<IPersistentComponent>>();
			systems = new Dictionary<int, Func<IEcsSystem>>();
			InstallEntityConfig();
		}
		
		protected abstract void InstallEntityConfig();
		protected virtual void OnEntityCreated(Entity context) { }
		
		public Entity CreateEntity(EcsWorld world, Vector3 position, Quaternion rotation, Transform parent) {
			var owner = Instantiate(prefab, position, rotation, parent);
			var componentsProvider = new EcsComponentsProvider(components);
			var systemsProvider = new EcsSystemsProvider(systems);
			var entity = world.CreateEntity(owner, componentsProvider, systemsProvider, true, Identity);
			OnEntityCreated(entity);
			return entity;
		}
		
		public void RegisterComponent<T>(Func<T> creator) where T: class, IPersistentComponent {
			Register(components, creator);
		}
		
		public void RegisterSystem<T>(Func<T> creator) where T : class, IEcsSystem {
			Register(systems, creator);
		}

		public void ReplaceComponent<T>(Func<T> creator) where T : class, IPersistentComponent {
			Replace(components, creator, true);
		}
		
		public void ReplaceSystem<T>(Func<T> creator) where T : class, IEcsSystem {
			Replace(systems, creator, true);
		}
		
		public void UnregisterComponent<T>() where T : class, IPersistentComponent => components.Remove(TypeHelper<T>.Identity);
		public void UnregisterSystem<T>() where T: class, IEcsSystem => systems.Remove(TypeHelper<T>.Identity);
		
		private static void Register<T>(IDictionary<int, Func<T>> target, Func<T> creator) where T: class {
			if (target.ContainsKey(TypeHelper<T>.Identity)) {
				Debug.LogWarning($"Creator for subject {typeof(T)} already registered");
				return;
			}
			
			Replace(target, creator, false);
		}
		
		private static void Replace<T>(IDictionary<int, Func<T>> target, Func<T> creator, bool needToRemoveOld) 
			where T: class {
			if (typeof(T).IsAbstract) {
				throw new ArgumentException("Impossible to register key of abstract subject!");
			}
			
			var identity = TypeHelper<T>.Identity;
			if (needToRemoveOld) target.Remove(identity);
			target[identity] = creator;
		}
	}
}