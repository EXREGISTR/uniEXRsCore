using System;
using System.Collections.Generic;
using EXRCore.Pools;
using EXRCore.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.Utils {
	public sealed class EcsWorld : IService {
		private readonly Dictionary<int, IEntity> spawnedEntitiesByObject = new();
		private static IReadOnlyDictionary<int, EntityFactory> entityFactories;
		
		public static void InstallFactories(IReadOnlyCollection<EntityFactory> factoriesCollection) {
			var factories = new Dictionary<int, EntityFactory>(factoriesCollection.Count);
			foreach (var config in factoriesCollection) { 
				factories[config.Identity] = config; 
				config.Initialize();
			}

			entityFactories = factories;
		}

		public static bool TryGetFactory(int identity, out EntityFactory factory) {
			if (entityFactories == null) {
				throw new NullReferenceException("Factories map is not initialized!");
			}
			
			if (!entityFactories.TryGetValue(identity, out var founded)) {
				factory = null;
				return false;
			}

			factory = founded;
			return true;
		}
		
		public static bool TryGetFactory<T>(out EntityFactory factory) where T : EntityFactory {
			var configType = typeof(T);
			if (configType.IsAbstract) {
				throw new ArgumentException($"Invalid config type {configType}! It is abstract");
			}

			if (!TryGetFactory(configType.GetHashCode(), out factory)) {
				Debug.LogError("");
				return false;
			}
			
			return true;
		}
		
		public Entity CreateEntity(GameObject owner, EcsComponentsProvider components = null, EcsSystemsProvider systems = null, 
			bool enableSystemsByDefault = true, int? factoryIdentity = null) {
			var entity = new Entity(owner, components, systems, enableSystemsByDefault, factoryIdentity);
			var key = owner.GetHashCode();
			if (spawnedEntitiesByObject.ContainsKey(key)) { 
				throw new InvalidOperationException($"Entity for game object {owner} already exists!");
			}
			
			spawnedEntitiesByObject[key] = entity;
			return entity;
		}
		
		public bool TryGetEntity(GameObject other, out IEntity entity) => spawnedEntitiesByObject.TryGetValue(other.GetHashCode(), out entity);

		public void Update() {
			foreach (var entity in spawnedEntitiesByObject.Values) {
				entity.Update();
			}
		}
		
		public void FixedUpdate() {
			foreach (var entity in spawnedEntitiesByObject.Values) {
				entity.FixedUpdate();
			}
		}
		
		public Entity GetFromPool<TPool>() where TPool: PoolProvider<Entity> {
			var entity = Service<PoolService>.Instance.Get<TPool, Entity>();
			((IEntity)entity).OnEnable();
			spawnedEntitiesByObject[entity.Owner.GetHashCode()] = entity;
			return entity;
		}
		
		public void ReturnToPool<TPool>(IEntity entity) where TPool: PoolProvider<Entity> {
			entity.OnDisable();
			spawnedEntitiesByObject.Remove(entity.Owner.GetHashCode());
			Service<PoolService>.Instance.Return<TPool, Entity>((Entity)entity);
		} 
		
		public void Destroy(IEntity entity) {
			entity.OnDisable();
			entity.OnDestroy();
			spawnedEntitiesByObject.Remove(entity.Owner.GetHashCode());
			if (entity.Owner != null) Object.Destroy(entity.Owner);
		}
	}
}