using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EXRCore.EcsFramework {
	internal interface ICallbacksWrapper {
		public void Clear();
	}
	
	internal class OnAddComponentCallbackList<T> : ICallbacksWrapper where T: IDynamicComponent {
		private Action<T> callbacks;

		public void RegisterCallback(Action<T> callback) => callbacks += callback;
		
		public void OnAddComponent(T component) {
			try {
				callbacks.Invoke(component);
			} catch (NullReferenceException) {
				Debug.LogWarning("Callback is null");
			} catch (Exception e) {
				Debug.LogWarning($"Exception in callback {callbacks} from {callbacks.Target}: {e.Message}");
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() => callbacks = null;
	}

	internal class OnRemovedComponentCallbackList : ICallbacksWrapper {
		private Action callbacks;

		public void RegisterCallback(Action callback) => callbacks += callback;
		public void OnRemovedComponent() {
			try {
				callbacks.Invoke();
			} catch (NullReferenceException) {
				Debug.LogWarning("Callback is null");
			} catch (Exception e) {
				Debug.LogWarning($"Exception in callback {callbacks} from {callbacks.Target}: {e.Message}");
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() => callbacks = null;
	}
}