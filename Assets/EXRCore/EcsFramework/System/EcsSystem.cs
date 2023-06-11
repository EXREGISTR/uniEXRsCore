namespace EXRCore.Utils {
	public abstract class EcsSystem : IEcsSystem {
		public bool EnableAfterInitialize { get; }
		
		protected Entity context { get; private set; }
		protected bool isActive { get; private set; }
		
		protected EcsSystem() => EnableAfterInitialize = true;
		protected EcsSystem(bool enableAfterInitialize) => EnableAfterInitialize = enableAfterInitialize;
		
		void IEcsSystem.Initialize(Entity context, EcsProvider<IPersistentComponent> components, EcsProvider<IEcsSystem> systems) {
			this.context = context;
			Initialize(components, systems);
		}
		
		protected abstract void Initialize(EcsProvider<IPersistentComponent> components, EcsProvider<IEcsSystem> systems);
		
		void IEcsSystem.OnDestroy() => OnDestroy();

		public void Enable() {
			if (isActive) return;
			isActive = true;
			OnEnabled();
		}

		public void Disable() {
			if (!isActive) return;
			isActive = false;
			OnDisabled();
		}
		
		void IEcsSystem.FixedUpdate() {
			if (!isActive) return;
			FixedUpdate();
		}
		
		void IEcsSystem.Update() {
			if (!isActive) return;
			Update();
		}
		
		protected virtual void FixedUpdate() { }
		protected virtual void Update() { }
		protected virtual void OnEnabled() { }
		protected virtual void OnDisabled() { }
		protected virtual void OnDestroy() { }
	}
}