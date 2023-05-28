using System;
using System.Threading.Tasks;
using EXRCore.Pools;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.EcsFramework {
	public static class Extenssions {
		public static void RegisterHandler<T>(this Entity entity, [NotNull] Action<T> onAddCallback,
			[NotNull] Action onRemoveCallback)
			where T : IDynamicComponent {
			entity.RegisterHandler(onAddCallback);
			entity.RegisterHandler<T>(onRemoveCallback);
		}
		
		public static void RegisterComponent<TComponent, TConfig>(this EcsWorld world, TComponent component) 
			where TComponent: IPersistentComponent 
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.RegisterComponent(component);
			}
		}
		
		public static void UnregisterComponent<TComponent, TConfig>(this EcsWorld world)
			where TComponent : IPersistentComponent
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.UnregisterComponent<TComponent>();
			}
		}
		
		public static void RegisterSystem<TSystem, TConfig>(this EcsWorld world, TSystem system) 
			where TSystem : IEcsSystem
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.RegisterSystem(system);
			}
		}
		
		public static void UnregisterSystem<TSystem, TConfig>(this EcsWorld world)
			where TSystem : IEcsSystem
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.UnregisterSystem<TSystem>();
			}
		}
		
		public static Entity CreateEntity<T>(this EcsWorld world, Vector3 position, Quaternion rotation, Transform parent = null, 
			bool enableSystemsByDefault = true) where T: EntityConfig {
			if (!EcsWorld.TryGetConfig<T>(out var config)) return null;

			GameObject owner = Object.Instantiate(config.Prefab, position, rotation, parent);
			var providers = config.CreateProviders();
			return world.CreateEntity(owner, providers.components, providers.systems, enableSystemsByDefault);
		}
		
		public static async Task DestroyAsync(this EcsWorld world, GameObject other, float delayInSeconds) {
			await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
			world.Destroy(other);
		}
		
		public static async Task DestroyAsync(this EcsWorld world, IEntity other, float delayInSeconds) {
			await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));
			world.Destroy(other);
		}
		
		public static void ReturnToPool<TPool>(this EcsWorld world, GameObject other) where TPool: PoolProvider<Entity> {
			if (!world.TryGetEntity(other, out IEntity entity)) {
				return;
			}
			
			world.ReturnToPool<TPool>(entity);
		}

		public static void Destroy(this EcsWorld world, GameObject other) {
			if (!world.TryGetEntity(other, out var entity)) {
				Object.Destroy(other);
				return;
			}
			
			world.Destroy(entity);
		}
	}
}