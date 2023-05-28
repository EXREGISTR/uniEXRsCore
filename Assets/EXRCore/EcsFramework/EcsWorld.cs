using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EXRCore.Pools;
using EXRCore.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.EcsFramework {
	public sealed class EcsWorld : IService {
		private readonly Dictionary<GameObject, IEntity> spawnedEntitiesByObject = new();
		private static Dictionary<Type, EntityConfig> configsMap;
		
		private EcsWorld() { }
		
		public static EcsWorld Create() {
			if (ServiceLocator.TryGetService(out EcsWorld world)) {
				world.spawnedEntitiesByObject.Clear();
			}
			
			world = new EcsWorld();
			ServiceLocator.Replace(world);
			return world;
		}

		public static Task InitializeConfigsAsync(IReadOnlyCollection<EntityConfig> configs) {
			var task = new Task(() => {
				configsMap = new Dictionary<Type, EntityConfig>(configs.Count);
				foreach (var config in configs) {
					configsMap[config.GetType()] = config;
					config.Initialize();
				}
			});
			task.Start();
			return task;
		}
		
		public static bool TryGetConfig<T>(out EntityConfig config) where T : EntityConfig {
			var configType = typeof(T);
			if (configType.IsAbstract) {
				throw new ArgumentException($"Invalid config type {configType}! It is abstract");
			}
			
			if (!configsMap.TryGetValue(configType, out var founded)) {
				Debug.LogError($"Config {configType} doesn't exist!");
				config = null;
				return false;
			}

			config = founded;
			return true;
		}
		
		public Entity CreateEntity(GameObject owner, EcsComponentsProvider components = null, EcsSystemsProvider systems = null, 
			bool enableSystemsByDefault = true) {
			var entity = new Entity(owner, components, systems, enableSystemsByDefault);
			if (spawnedEntitiesByObject.ContainsKey(owner)) { 
				throw new InvalidOperationException($"Entity for game object {owner} already exists!");
			}

			spawnedEntitiesByObject[owner] = entity;
			return entity;
		}
		
		public bool TryGetEntity(GameObject other, out IEntity entity) => spawnedEntitiesByObject.TryGetValue(other, out entity);

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
			var entity = ServiceLocator.GetService<PoolService>().Get<TPool, Entity>();
			((IEntity)entity).OnEnable();
			spawnedEntitiesByObject[entity.Owner] = entity;
			return entity;
		}
		
		public void ReturnToPool<TPool>(IEntity entity) where TPool: PoolProvider<Entity> {
			entity.OnDisable();
			spawnedEntitiesByObject.Remove(entity.Owner);
			ServiceLocator.GetService<PoolService>().Return<TPool, Entity>((Entity)entity);
		} 
		
		public void Destroy(IEntity entity) {
			entity.OnDisable();
			entity.OnDestroy();
			spawnedEntitiesByObject.Remove(entity.Owner);
			if (entity.Owner != null) Object.Destroy(entity.Owner);
		}
	}
}