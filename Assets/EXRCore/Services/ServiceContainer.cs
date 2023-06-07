using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.Services {
	public sealed class ServiceContainer : IServiceContainer {
		private static ServiceContainer current;
		
		private ServiceContainer() { }
		public static IServiceContainer Create() {
			current?.Dispose();
			current = new ServiceContainer();
			return current;
		}

		private static readonly Dictionary<Type, IServiceWrapper> persistentServiceWrappers = new();

		private readonly Dictionary<Type, IServiceWrapper> serviceWrappers = new();
		private List<IDisposableService> disposableServices;
		
		#region API
		public IServiceContainer RegisterDisposableFromInstance<TInterface, TService>(TService service) 
			where TInterface : IDisposableService
			where TService : class, TInterface {
			disposableServices ??= new List<IDisposableService>();
			disposableServices.Add(service);
			return RegisterFromInstance<TInterface, TService>(service);
		}
		
		public IServiceContainer RegisterDisposable<TInterface, TService>(Func<TService> creator) 
			where TInterface : IDisposableService
			where TService : class, TInterface {
			var wrapper = new ServiceWrapperBasedFactory(() => { 
				var instance = creator();
				disposableServices ??= new List<IDisposableService>();
				disposableServices.Add(instance);
				return instance;
			});
			return RegisterDestroyableWrapper<TInterface, TService>(wrapper);
		}
		
		public IServiceContainer Register<TInterface, TService>(Func<TService> creator)
			where TInterface : IService
			where TService : class, TInterface {
			var wrapper = new ServiceWrapperBasedFactory(creator);
			return RegisterDestroyableWrapper<TInterface, TService>(wrapper);
		}
		
		public IServiceContainer RegisterAsPersistent<TInterface, TService>(Func<TService> creator) 
			where TInterface : IService
			where TService : class, TInterface {
			var wrapper = new ServiceWrapperBasedFactory(creator);
			return RegisterPersistentWrapper<TInterface, TService>(wrapper);
		}
		
		public IServiceContainer RegisterFromInstance<TInterface, TService>(TService service) 
			where TInterface : IService
			where TService : class, TInterface {
			var wrapper = new ServiceWrapperBasedInstance(service);
			return RegisterDestroyableWrapper<TInterface, TService>(wrapper);
		}
		
		public IServiceContainer RegisterFromInstanceAsPersistent<TInterface, TService>(TService instance) 
			where TInterface : IService
			where TService : class, TInterface {
			var wrapper = new ServiceWrapperBasedInstance(instance);
			return RegisterPersistentWrapper<TInterface, TService>(wrapper);
		}
		#endregion

		private IServiceContainer RegisterPersistentWrapper<TInterface, TService>(IServiceWrapper wrapper)
			where TInterface : IService
			where TService : class, TInterface {
			return RegisterWrapper<TInterface, TService>(persistentServiceWrappers, wrapper);
		}

		private IServiceContainer RegisterDestroyableWrapper<TInterface, TService>(IServiceWrapper wrapper)
			where TInterface : IService
			where TService : class, TInterface {
			return RegisterWrapper<TInterface, TService>(serviceWrappers, wrapper);
		}
		
		private IServiceContainer RegisterWrapper<TInterface, TService>(IDictionary<Type, IServiceWrapper> target, IServiceWrapper wrapper)
			where TInterface : IService
			where TService : class, TInterface {
			var interfaceType = typeof(TInterface);
			var serviceType = typeof(TService);
			
			if (interfaceType.IsAbstract) {
				if (!MayRegisterWrapper(interfaceType)) return this;
			}

			if (!MayRegisterWrapper(serviceType)) return this;
			
			target[serviceType] = wrapper;
			if (interfaceType.IsAbstract) target[interfaceType] = wrapper;
			return this;
		}
		
		private bool MayRegisterWrapper(Type key) {
			if (serviceWrappers.ContainsKey(key)) {
				Debug.LogWarning($"Failed to create wrapper for service {key}: " +
				                 $"wrapper already registered for type {key}.");
				return false;
			}

			if (persistentServiceWrappers.ContainsKey(key)) {
				Debug.LogWarning($"Failed to create wrapper for service {key}: " +
				                 $"wrapper already registered for type {key} as persistent.");
				return false;
			}

			return true;
		}

		public static TService ResolveService<TService>() where TService: class, IService {
			if (current == null) {
				Debug.LogError("Failed to resolve service: ServiceContainer not created!");
				return null;
			}
			
			var serviceType = typeof(TService);
			if (!current.serviceWrappers.TryGetValue(serviceType, out var wrapper)) {
				Debug.LogError($"Failed to resolve service: Wrapper for service type {serviceType} not created!");
				return null;
			}
			
			return wrapper.Resolve() as TService;
		}

		public static bool TryResolveService<TService>(out TService service) where TService: class, IService {
			service = ResolveService<TService>();
			return service != null;
		}

		public void Dispose() {
			if (disposableServices == null) {
				current = null;
				return;
			}
			
			foreach (var service in disposableServices) {
				service.Dispose();
			}
			
			disposableServices.Clear();
			current = null;
		}
	}
}