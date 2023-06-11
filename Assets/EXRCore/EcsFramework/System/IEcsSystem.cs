namespace EXRCore.Utils {
	public interface IEcsSystem : IEcsSubject {
		public bool EnableAfterInitialize { get; }
		internal void Initialize(Entity context, EcsProvider<IPersistentComponent> components, EcsProvider<IEcsSystem> systems);
		internal void FixedUpdate();
		internal void Update();
		internal void OnDestroy();
		public void Enable();
		public void Disable();
	}
}