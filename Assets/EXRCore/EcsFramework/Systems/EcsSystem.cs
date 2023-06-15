namespace EXRCore.EcsFramework {
	public abstract class EcsSystem : IEcsSystem {
		private readonly bool enableOnInitialize;
		protected Entity context { get; private set; }
		
		protected EcsSystem() => enableOnInitialize = true;
		protected EcsSystem(in bool enable) => enableOnInitialize = enable;
		
		void IEcsSystem.Initialize(Entity context, EcsProvider<IPersistentComponent> components, EcsProvider<IEcsSystem> systems) {
			this.context = context;
			Initialize(components, systems);
		}
		
		void IEcsSystem.InitializeAndEnable(Entity context, EcsProvider<IPersistentComponent> components, EcsProvider<IEcsSystem> systems) {
			this.context = context;
			Initialize(components, systems);
			if (enableOnInitialize) Enable();
		}
		
		protected abstract void Initialize(in EcsProvider<IPersistentComponent> components, in EcsProvider<IEcsSystem> systems);
		
		public virtual void Enable() { }
		public virtual void Disable() { }
		
		void IEcsSystem.OnDestroy() => OnDestroy();
		protected virtual void OnDestroy() { }
	}
}