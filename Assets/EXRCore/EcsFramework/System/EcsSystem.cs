namespace EXRCore.EcsFramework {
	public abstract class EcsSystem : IEcsSystem {
		protected Entity context { get; private set; }
		protected bool isActive { get; private set; }
		
		void IEcsSystem.Initialize(Entity context, EcsProvider<IPersistentComponent> components) {
			this.context = context;
			if (components != null) Initialize(components);
		}
		
		protected abstract void Initialize(EcsProvider<IPersistentComponent> components);
		
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