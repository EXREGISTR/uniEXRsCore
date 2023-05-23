using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.EcsFramework {
	public class EcsWorld {
		public static EcsWorld Current { get; private set; }
		
		private readonly Dictionary<GameObject, IEntity> spawnedEntitiesByObject = new();
		private static Dictionary<Type, EntityConfig> configsMap;
		
		private EcsWorld() { }
		
		public static EcsWorld Create() {
			if (configsMap == null) throw new Exception("Configs not initialized!");
			Current = new EcsWorld();
			return Current;
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
		
		
		
		public void RegisterComponent<TComponent, TConfig>(TComponent component) 
			where TComponent: IPersistentComponent 
			where TConfig : EntityConfig {
			if (TryGetConfig<TConfig>(out var config)) {
				config.RegisterComponent(component);
			}
		}
		
		public void UnregisterComponent<TComponent, TConfig>()
			where TComponent : IPersistentComponent
			where TConfig : EntityConfig {
			if (TryGetConfig<TConfig>(out var config)) {
				config.UnregisterComponent<TComponent>();
			}
		}
		
		public void RegisterSystem<TSystem, TConfig>(TSystem system) 
			where TSystem : IEcsSystem
			where TConfig : EntityConfig {
			if (TryGetConfig<TConfig>(out var config)) {
				config.RegisterSystem(system);
			}
		}

		public void UnregisterSystem<TSystem, TConfig>()
			where TSystem : IEcsSystem
			where TConfig : EntityConfig {
			if (TryGetConfig<TConfig>(out var config)) {
				config.UnregisterSystem<TSystem>();
			}
		}

		private static bool TryGetConfig<T>(out EntityConfig config) where T : EntityConfig {
			config = null;
			var configType = typeof(T);
			if (configType.IsAbstract) {
				throw new ArgumentException($"Invalid config type {configType}! It is abstract");
			}
			
			if (!configsMap.TryGetValue(typeof(T), out var founded)) {
				Debug.LogError($"Config {typeof(T)} doesn't exist!");
				return false;
			}

			config = founded;
			return true;
		}
		
		public Entity CreateEntity(GameObject owner, EcsComponentsProvider components = null, EcsSystemsProvider systems = null) {
			var entity = new Entity(owner, components, systems);
			if (spawnedEntitiesByObject.ContainsKey(owner)) { 
				throw new NullReferenceException($"Entity for game object {owner} already exists!");
			}
        			
			spawnedEntitiesByObject[owner] = entity;
			return entity;
		}
		
		public Entity CreateEntity<T>(Vector3 position, Quaternion rotation, Transform parent = null) where T: EntityConfig {
			var key = typeof(T);
			if (!configsMap.TryGetValue(key, out var config)) {
				throw new NullReferenceException($"Config {key} doesn't exist!");
			}
			
			GameObject owner = Object.Instantiate(config.Prefab, position, rotation, parent);
			var providers = config.CreateProviders();
			return CreateEntity(owner, providers.components, providers.systems);
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

		public async void DestroyAsync(GameObject other, float delayInSeconds) {
			await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
			Destroy(other);
		}
		
		public void Destroy(GameObject other) {
			if (!spawnedEntitiesByObject.TryGetValue(other, out var entity)) {
				Object.Destroy(other);
				return;
			}

			Destroy(entity);
		}
		
		public void Destroy(IEntity entity) {
			entity.OnDestroy();
			spawnedEntitiesByObject.Remove(entity.Owner);
			Object.Destroy(entity.Owner);
		}
		
		public async Task DestroyAsync(Entity entity, float delayInSeconds) {
			await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
			Destroy(entity);
		}
	}
}