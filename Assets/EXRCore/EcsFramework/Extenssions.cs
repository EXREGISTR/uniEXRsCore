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
			where T : IEntityMessage {
			entity.RegisterHandler(onAddCallback);
			entity.RegisterHandler<T>(onRemoveCallback);
		}

		public static bool TryAddComponent<T>(this EcsWorld world, T component, GameObject target) where T : IDynamicComponent {
			return world.TryGetEntity(target, out var entity) && entity.AddComponent(component);
		}
		
		public static bool TryRemoveComponent<T>(this EcsWorld world, GameObject target) where T : IDynamicComponent {
			return world.TryGetEntity(target, out var entity) && entity.RemoveComponent<T>();
		}

		public static bool TrySendMessage<T>(this EcsWorld world, T message, GameObject receiver) where T : IEntityMessage {
			if (!world.TryGetEntity(receiver, out var entity)) return false;
			entity.SendMessage(message);
			return true;
		}

		public static void RegisterComponent<TComponent, TConfig>(this EcsWorld world, Func<TComponent> component) 
			where TComponent: class, IPersistentComponent 
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.RegisterComponent(component);
			}
		}
		
		public static void ReplaceComponent<TComponent, TConfig>(this EcsWorld world, Func<TComponent> component) 
			where TComponent: class, IPersistentComponent 
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.ReplaceComponent(component);
			}
		}

		public static void UnregisterComponent<TComponent, TConfig>(this EcsWorld world)
			where TComponent : class, IPersistentComponent
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.UnregisterComponent<TComponent>();
			}
		}
		
		public static void RegisterSystem<TSystem, TConfig>(this EcsWorld world, Func<TSystem> system) 
			where TSystem : class, IEcsSystem
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.RegisterSystem(system);
			}
		}
		
		public static void ReplaceSystem<TSystem, TConfig>(this EcsWorld world, Func<TSystem> system) 
			where TSystem : class, IEcsSystem
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.ReplaceSystem(system);
			}
		}
		
		public static void UnregisterSystem<TSystem, TConfig>(this EcsWorld world)
			where TSystem : class, IEcsSystem
			where TConfig : EntityConfig {
			if (EcsWorld.TryGetConfig<TConfig>(out var config)) {
				config.UnregisterSystem<TSystem>();
			}
		}
		
		public static Entity CreateEntity<T>(this EcsWorld world, Vector3 position, Quaternion rotation, Transform parent = null, 
			bool enableSystemsByDefault = true) where T: EntityConfig {
			if (!EcsWorld.TryGetConfig<T>(out var config)) return null;

			var data = config.CreateEntityData();
			GameObject owner = Object.Instantiate(data.Prefab, position, rotation, parent);
			return world.CreateEntity(owner, data.Components, data.Systems, enableSystemsByDefault);
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