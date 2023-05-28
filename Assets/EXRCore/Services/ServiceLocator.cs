using System;
using System.Collections.Generic;

namespace EXRCore.Services {
	public static class Service<T> where T : class, IService {
		public static T Instance => ServiceLocator.GetService<T>();
	}

	public static class ServiceLocator {
		private static Dictionary<Type, IService> services;

		static ServiceLocator() => Reinitialize();

		public static void Register<T>(T service) where T : class, IService {
			var key = typeof(T);
			if (services.ContainsKey(key)) {
				throw new ServiceException($"Service {key} already registered!");
			}
			services[key] = service;
		}
		
		public static void Unregister<T>() where T : class, IService => services.Remove(typeof(T));
		
		public static bool TryGetService<T>(out T service) where T : class, IService {
			try {
				service = GetService<T>();
			} catch (ServiceException) {
				service = null;
				return false;
			}
			
			return true;
		}
		
		public static T GetService<T>() where T : class, IService {
			var key = typeof(T);
			if (!services.TryGetValue(key, out var service)) {
				throw new ServiceException($"Service {key} doesn't registered!");
			}
			
			return service as T;
		}

		public static void Replace<T>(T service) where T : class, IService {
			var key = typeof(T);
			services.Remove(key);
			services[key] = service;
		}
		
		public static void Reinitialize(int capacity = 6) {
			services?.Clear();
			services = new Dictionary<Type, IService>(capacity);
		}
	}
	
	public class ServiceException : Exception {
		public ServiceException(string message) : base(message) { }
	}
}