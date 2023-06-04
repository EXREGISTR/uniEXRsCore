using System;
using System.Collections.Generic;

namespace EXRCore.DIContainer {
	public partial class ServiceContainer : IDisposable {
		private readonly IDictionary<Type, object> aliveServicesByType = new Dictionary<Type, object>();
		private readonly IDictionary<Type, IDisposable> disposableServicesByType = new Dictionary<Type, IDisposable>();
		private readonly IDictionary<Type, IScope> servicesScopes = new Dictionary<Type, IScope>();

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
		
		public void Register<T>(T service) where T : class {
			var type = typeof(T);
			if (ContainsService(type)) return;
			
			RegisterService(type, service);
			ExecuteInjectionInternal(type, service);
		}
		
		public void RegisterLazy<T>(Func<T> serviceCreator) where T: class {
			var key = typeof(T);
			if (servicesScopes.ContainsKey(key)) return;

			var scope = new SingletonScope(CreateScopeWrapper(serviceCreator));
			servicesScopes[key] = scope;
		}
		
		private static Func<object> CreateScopeWrapper<T>(Func<T> creator) where T: class {
			object Wrapper() {
				var type = typeof(T);
				var service = creator();
				ExecuteInjectionInternal(type, service);
				return service;
			}
			
			return Wrapper;
		}

		public bool TryGetService<T>(out T service) where T: class {
			if (TryGetServiceInternal(typeof(T), out var founded)) {
				service = founded as T;
			}

			service = null;
			return false;
		}

		private bool ContainsService(Type key) => TryGetServiceInternal(key, out _);
		
		private bool TryGetServiceInternal(Type key, out object service) {
			if (aliveServicesByType.TryGetValue(key, out service)) return true;
			if (disposableServicesByType.TryGetValue(key, out var disposable)) {
				service = disposable;
				return true;
			}

			return false;
		}

		public bool TryResolveService(Type type, out object service) {
			service = null;
			if (ContainsService(type)) return false;
			if (!servicesScopes.ContainsKey(type)) return false;
			
			service = Resolve(type);
			return true;
		}

		private object Resolve(Type type) {
			var service = servicesScopes[type].Resolve();
			RegisterService(type, service);
			return service;
		}

		private void RegisterService(Type type, object service) {
			if (service is IDisposable disposable) {
				disposableServicesByType[type] = disposable;
			} else {
				aliveServicesByType[type] = service;
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
}