namespace EXRCore.Services {
	internal sealed class ServiceWrapperBasedInstance : IServiceWrapper {
		private IService service;
		public ServiceWrapperBasedInstance(IService instance) => service = instance;

		public IService Resolve() => service;
		public void DeleteInstance() => service = null;
	}
}