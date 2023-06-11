using System;
using System.Threading.Tasks;
using EXRCore.Pools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.Utils {
	public static class WorldExtenssions {
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
		
		public static void RegisterComponent<TComponent, TFactory>(this EcsWorld world, Func<TComponent> component) 
			where TComponent: class, IPersistentComponent 
			where TFactory : EntityFactory {
			if (EcsWorld.TryGetFactory<TFactory>(out var factory)) {
				factory.RegisterComponent(component);
			}
		}
		
		public static void ReplaceComponent<TComponent, TFactory>(this EcsWorld world, Func<TComponent> component) 
			where TComponent: class, IPersistentComponent 
			where TFactory : EntityFactory {
			if (EcsWorld.TryGetFactory<TFactory>(out var factory)) {
				factory.ReplaceComponent(component);
			}
		}
		
		public static void UnregisterComponent<TComponent, TFactory>(this EcsWorld world)
			where TComponent : class, IPersistentComponent
			where TFactory : EntityFactory {
			if (EcsWorld.TryGetFactory<TFactory>(out var factory)) {
				factory.UnregisterComponent<TComponent>();
			}
		}
		
		public static void RegisterSystem<TSystem, TFactory>(this EcsWorld world, Func<TSystem> system) 
			where TSystem : class, IEcsSystem
			where TFactory : EntityFactory {
			if (EcsWorld.TryGetFactory<TFactory>(out var factory)) {
				factory.RegisterSystem(system);
			}
		}
		
		public static void ReplaceSystem<TSystem, TFactory>(this EcsWorld world, Func<TSystem> system) 
			where TSystem : class, IEcsSystem
			where TFactory : EntityFactory {
			if (EcsWorld.TryGetFactory<TFactory>(out var factory)) {
				factory.ReplaceSystem(system);
			}
		}
		
		public static void UnregisterSystem<TSystem, TFactory>(this EcsWorld world)
			where TSystem : class, IEcsSystem
			where TFactory : EntityFactory {
			if (EcsWorld.TryGetFactory<TFactory>(out var factory)) {
				factory.UnregisterSystem<TSystem>();
			}
		}

		public static Entity CreateEntity<TFactory>(this EcsWorld world, Vector3 position, Quaternion rotation, 
			Transform parent = null, bool enableSystemsByDefault = true) where TFactory: EntityFactory {
			if (!EcsWorld.TryGetFactory<TFactory>(out var factory)) return null;
			return factory.CreateEntity(world, position, rotation, parent);
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