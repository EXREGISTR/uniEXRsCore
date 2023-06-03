using JetBrains.Annotations;

namespace EXRCore.EcsFramework {
	public interface IEcsSystem : IEcsSubject {
		internal void Initialize(Entity context, [CanBeNull] EcsProvider<IPersistentComponent> components);
		internal void FixedUpdate();
		internal void Update();
		internal void OnDestroy();
		public void Enable();
		public void Disable();
	}
}