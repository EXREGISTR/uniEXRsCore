using System;
using EXRCore.Observers;

namespace EXRCore.EcsFramework {
	public abstract class EcsFullUpdatableSystem : EcsSystem, IUpdatable, IFixedUpdatable {
		private IDisposable subscriber;
		
		public sealed override void Enable() {
			if (subscriber != null) return;
			subscriber = GameDispatcher.AddOnAllUpdate(this);
			OnEnable();
		}

		public sealed override void Disable() {
			if (subscriber == null) return;
			subscriber.Dispose();
			subscriber = null;
			OnDisable();
		}

		public abstract void Update();
		public abstract void FixedUpdate();
		
		protected virtual void OnEnable() { }
		protected virtual void OnDisable() { }
	}
}