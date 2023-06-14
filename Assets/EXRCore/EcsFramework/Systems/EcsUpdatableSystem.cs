using System;
using EXRCore.Observers;

namespace EXRCore.EcsFramework {
	public abstract class EcsUpdatableSystem : EcsSystem, IUpdatable {
		private IDisposable subscriber;
		
		public sealed override void Enable() {
			if (subscriber != null) return;
			subscriber = GameDispatcher.AddOnUpdate(this);
			OnEnable();
		}
		
		public sealed override void Disable() {
			if (subscriber == null) return;
			subscriber.Dispose();
			subscriber = null;
			OnDisable();
		}

		public abstract void Update();
		
		protected virtual void OnEnable() { }
		protected virtual void OnDisable() { }
	}
}