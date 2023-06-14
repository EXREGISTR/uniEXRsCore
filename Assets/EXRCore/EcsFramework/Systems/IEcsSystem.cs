namespace EXRCore.EcsFramework {
	public interface IEcsSystem {
		internal void Initialize(Entity context, EcsProvider<IPersistentComponent> components, EcsProvider<IEcsSystem> systems, in bool enableSystems);
		public void Enable();
		public void Disable();
		internal void OnDestroy();
	}
}