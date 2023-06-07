using System;

namespace EXRCore.Services {
	internal sealed class ServiceWrapperBasedFactory : IServiceWrapper {
		private readonly Func<IService> creator;
		private IService instance;
		
		public ServiceWrapperBasedFactory(Func<IService> creator) => this.creator = creator;
		public IService Resolve() => instance ??= creator();
		public void DeleteInstance() => instance = null;
	}
}