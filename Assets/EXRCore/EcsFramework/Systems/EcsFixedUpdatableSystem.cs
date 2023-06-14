using System;
using EXRCore.Observers;

namespace EXRCore.EcsFramework {
	public abstract class EcsFixedUpdatableSystem : EcsSystem, IFixedUpdatable {
		private IDisposable subscriber;

		public sealed override void Enable() {
			if (subscriber != null) return;
			subscriber = GameDispatcher.AddOnFixedUpdate(this);
			OnEnable();
		}
		
		public sealed override void Disable() {
			if (subscriber == null) return;
			subscriber.Dispose();
			subscriber = null;
			OnDisable();
		}
		
		public abstract void FixedUpdate();

		protected virtual void OnEnable() { }
		protected virtual void OnDisable() { }
	}
}