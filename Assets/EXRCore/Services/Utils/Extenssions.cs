using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EXRCore.Services {
	public static class Extenssions {
		public static IServiceContainer Register<TService>(this IServiceContainer container, Func<TService> creator)
			where TService : class, IService {
			return container.Register<TService, TService>(creator);
		}
		
		public static IServiceContainer RegisterDisposable<TService>(this IServiceContainer container, Func<TService> creator)
			where TService : class, IDisposableService {
			return container.RegisterDisposable<TService, TService>(creator);
		}

		public static IServiceContainer Register<TService>(this IServiceContainer container) 
			where TService : class, IService, new() {
			return container.Register(() => new TService());
		}

		public static IServiceContainer RegisterDisposable<TService>(this IServiceContainer container)
			where TService : class, IDisposableService, new() {
			return container.RegisterDisposable(() => new TService());
		}
		
		public static IServiceContainer Register<TInterface, TService>(this IServiceContainer container)
			where TInterface : IService
			where TService : class, TInterface, new() {
			return container.Register<TInterface, TService>(() => new TService());
		}
		
		public static IServiceContainer RegisterDisposable<TInterface, TService>(this IServiceContainer container)
			where TInterface : IDisposableService
			where TService : class, TInterface, new() {
			return container.RegisterDisposable<TInterface, TService>(() => new TService());
		}

		public static IServiceContainer RegisterFromInstance<TService>(this IServiceContainer container, TService service)
			where TService : class, IService {
			return container.RegisterFromInstance<TService, TService>(service);
		}
		
		public static IServiceContainer RegisterDisposableFromInstance<TService>(this IServiceContainer container, TService service)
			where TService : class, IDisposableService {
			return container.RegisterDisposableFromInstance<TService, TService>(service);
		}
		
		public static IServiceContainer RegisterAsPersistent<TService>(this IServiceContainer container, Func<TService> creator)
			where TService : class, IService {
			return container.RegisterAsPersistent<TService, TService>(creator);
		}

		public static IServiceContainer RegisterAsPersistent<TService>(this IServiceContainer container) 
			where TService : class, IService, new() {
			return container.RegisterAsPersistent(() => new TService());
		}
		
		public static IServiceContainer RegisterAsPersistent<TInterface, TService>(this IServiceContainer container)
			where TInterface : IService
			where TService : class, TInterface, new() {
			return container.RegisterAsPersistent<TInterface, TService>(() => new TService());
		}

		public static IServiceContainer RegisterMonoAsPersistent<TService>
			(this IServiceContainer container, TService prefab) 
			where TService : MonoBehaviour, IService {
			return container.RegisterMonoAsPersistent<TService, TService>(prefab);
		}
		
		public static IServiceContainer RegisterMonoFromInstanceAsPersistent<TService>
			(this IServiceContainer container, TService instance)
			where TService : MonoBehaviour, IService {
			return container.RegisterMonoFromInstanceAsPersistent<TService, TService>(instance);
		}

		public static IServiceContainer RegisterMonoAsPersistent<TInterface, TService>
			(this IServiceContainer container, TService prefab)
			where TInterface : IService
			where TService : MonoBehaviour, TInterface {
			return container.RegisterAsPersistent<TInterface, TService>(() => {
				var instance = Object.Instantiate(prefab);
				Object.DontDestroyOnLoad(instance.gameObject);
				return instance;
			});
		}

		public static IServiceContainer RegisterMonoFromInstanceAsPersistent<TInterface, TService>
			(this IServiceContainer container, TService instance)
			where TInterface : IService
			where TService : MonoBehaviour, TInterface {
			Object.DontDestroyOnLoad(instance.gameObject);
			return container.RegisterFromInstanceAsPersistent<TInterface, TService>(instance);
		}
	}
}