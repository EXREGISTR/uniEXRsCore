namespace EXRCore.EcsFramework {
	public static class FactoryExtenssions {
		public static void RegisterComponent<T>(this EntityFactory factory) where T : class, IPersistentComponent, new() {
			factory.RegisterComponent(() => new T());
		}
		
		public static void RegisterSystem<T>(this EntityFactory factory) where T : class, IEcsSystem, new() {
			factory.RegisterSystem(() => new T());
		}
	}
}