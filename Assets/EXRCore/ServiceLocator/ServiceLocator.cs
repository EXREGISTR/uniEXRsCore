using System;
using System.Collections.Generic;

namespace EXRCore.EcsFramework.ServiceLocator {
	public static class ServiceLocator {
		private static Dictionary<Type, IService> services;

		static ServiceLocator() => Reinitialize();

		public static void Register<T>(T service) where T : IService {
			var key = typeof(T);
			if (services.ContainsKey(key)) {
				throw new ServiceException($"Service {key} already registered!");
			}
			
			services[key] = service;
		}

		public static void Replace<T>(T service) where T : IService {
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