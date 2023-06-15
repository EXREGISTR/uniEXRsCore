namespace EXRCore.EcsFramework {
	public interface IEcsSystem {
		internal void Initialize(Entity context, EcsProvider<IPersistentComponent> components, EcsProvider<IEcsSystem> systems);
		internal void InitializeAndEnable(Entity context, EcsProvider<IPersistentComponent> components, EcsProvider<IEcsSystem> systems);
		public void Enable();
		public void Disable();
		internal void OnDestroy();
	}
}