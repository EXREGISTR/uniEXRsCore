namespace EXRCore.Services {
	internal interface IServiceWrapper {
		public IService Resolve();
		public void DeleteInstance();
	}
}