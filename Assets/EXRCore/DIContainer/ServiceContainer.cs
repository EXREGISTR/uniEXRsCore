using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.DIContainer {
	public partial class ServiceContainer : IDisposable {
		private readonly IDictionary<Type, object> aliveServicesByType = new Dictionary<Type, object>();
		private readonly IDictionary<Type, IDisposable> disposableServicesByType = new Dictionary<Type, IDisposable>();
		private readonly IDictionary<Type, ServiceDescriptor> serviceDescriptors = new Dictionary<Type, ServiceDescriptor>();
		
		private static readonly object syncObject = new();
		private static ServiceContainer current;
		
		private ServiceContainer() { }

		public static ServiceContainer Create() {
			lock (syncObject) {
				current?.Dispose();
				current = new ServiceContainer();
			}
			
			return current;
		}
		
		#region API
		public void Register<TService>(Func<TService> creator) where TService : class {
			RegisterInternal(typeof(TService), typeof(TService), creator);
		}
		
		public void Register<TService>() where TService : class, new() {
			RegisterInternal(typeof(TService), typeof(TService), () => new TService());
		} 
		
		public void Register<TInterface, TService>(Func<TService> creator) where TService : class, TInterface {
			RegisterInternal(typeof(TInterface), typeof(TService), creator);
		}
		
		public void Register<TInterface, TService>() where TService : class, TInterface, new() {
			RegisterInternal(typeof(TInterface), typeof(TService), () => new TService());
		}

		public void RegisterNonLazy<TService>() where TService : class, new() {
			RegisterInstanceInternal(typeof(TService), typeof(TService), new TService());
		}

		public void RegisterNonLazy<TInterface, TService>() where TService : class, TInterface, new() {
			RegisterInstanceInternal(typeof(TInterface), typeof(TService), new TService());
		}
		
		public void RegisterNonLazy<TService>(TService service) where TService : class {
			RegisterInstanceInternal(typeof(TService), typeof(TService), service);
		}
		
		public void RegisterNonLazy<TInterface, TService>(TService service) where TService : class, TInterface {
			RegisterInstanceInternal(typeof(TInterface), typeof(TService), service);
		}
		
		/// <summary>
		/// Returns or creates a service of T type
		/// </summary>
		/// <typeparam name="T">the type for which the service was registered (can be base class, interface or implementation type) </typeparam>
		/// <exception cref="ServiceContainerException">if the operation was failed</exception>
		public T ResolveService<T>() where T : class {
			return TryResolveServiceInternal(typeof(T), out var service)
				? (T)service
				: throw new ServiceContainerException("Failed to create service");
		}

		/// <summary>
		/// Returns or creates a service of T type
		/// </summary>
		/// <typeparam name="T">the type for which the service was registered (can be base class, interface or implementation type) </typeparam>
		/// <returns>true if the operation was successful</returns>
		public bool TryResolveService<T>(out T service) where T : class {
			if (TryResolveServiceInternal(typeof(T), out var founded)) {
				service = founded as T;
				return true;
			}
			
			service = null;
			return false;
		}
		#endregion

		// параметр type может быть типом базового класса
		private bool TryResolveServiceInternal(Type type, out object service) {
			// пробуем получить сервис, зарегистрированный на переданный тип
			if (TryGetServiceInternal(type, out service)) return true;
			
			if (!serviceDescriptors.TryGetValue(type, out var descriptor)) {
				Debug.LogError($"Creator for {type} doesn't registered in container!");
				return false;
			}

			if (type.IsAbstract) {
				// если тип абстрактный, то проверим его наличие по конкретному типу
				if (TryGetServiceInternal(descriptor.ImplementationType, out service)) return true;
			}

			// сервис не зарегистрирован значит создаем его
			service = descriptor.Resolve();
			RegisterInstanceInternal(type, descriptor.ImplementationType, service);
			return true;
		}

		// параметр key может быть типом базового класса, от которого наследован serviceType, либо же совпадать с ним
		private void RegisterInternal(Type key, Type serviceType, Func<object> creator) {
			if (key == typeof(object)) {
				throw new ServiceContainerException("Impossible register creator for object type");
			}

			if (serviceDescriptors.ContainsKey(key)) {
				Debug.LogWarning($"Service creator already registered by key {key}!");
				return;
			}
			
			if (TryGetServiceInternal(key, out var existing)) {
				Debug.LogWarning($"You trying to register creator for already existing service {existing}!");
				return;
			}
			
			var serviceCreator = new ServiceDescriptor(creator, serviceType);
			serviceDescriptors[key] = serviceCreator;
		}
		
		private bool TryGetServiceInternal(Type key, out object service) {
			if (aliveServicesByType.TryGetValue(key, out service)) return true;
			if (disposableServicesByType.TryGetValue(key, out var disposable)) {
				service = disposable;
				return true;
			}

			return false;
		}

		private void RegisterInstanceInternal(Type key, Type implementationType, object service) {
			// если сервис содержит атрибуты инъекции, то зависимости будут переданы
			ExecuteInjectionInternal(implementationType, service);
			
			// регистрация
			if (service is IDisposable disposable) {
				disposableServicesByType[key] = disposable;
			} else {
				aliveServicesByType[key] = service;
			}
		}

		public void Dispose() {
			foreach (var disposable in disposableServicesByType.Values) {
				disposable.Dispose();
			}
			
			aliveServicesByType.Clear();
			disposableServicesByType.Clear();
			current = null;
		}
	}

	public class ServiceContainerException : Exception {
		public ServiceContainerException(string message) : base(message) { }
	}
}