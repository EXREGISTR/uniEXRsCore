using System;

namespace EXRCore.Services {
	public interface IServiceContainer : IDisposable {
		public IServiceContainer RegisterFromInstance<TInterface, TService>(TService service)
			where TInterface : IService
			where TService : class, TInterface;
		
		public IServiceContainer RegisterDisposableFromInstance<TInterface, TService>(TService service)
			where TInterface : IDisposableService
			where TService : class, TInterface;

		public IServiceContainer Register<TInterface, TService>(Func<TService> creator)
			where TInterface : IService
			where TService : class, TInterface;
		
		public IServiceContainer RegisterDisposable<TInterface, TService>(Func<TService> creator)
			where TInterface : IDisposableService
			where TService : class, TInterface;

		public IServiceContainer RegisterAsPersistent<TInterface, TService>(Func<TService> creator)
			where TInterface : IService
			where TService : class, TInterface;

		public IServiceContainer RegisterFromInstanceAsPersistent<TInterface, TService>(TService instance)
			where TInterface : IService
			where TService : class, TInterface;
	}
}