using System;

namespace EXRCore.Services {
	public static class Service<T> where T: class, IService {
		public static void Execute(Action<T> callback) {
			if (ServiceContainer.TryResolveService(out T service)) callback(service);
		}

		public static T Instance => ServiceContainer.ResolveService<T>();
	}
}