using System;
using System.Linq;

namespace EXRCore.Events {
	internal sealed class EventHandlersList<T> : IEventHandlersList where T: IMessage {
		private Action<T> listeners;
		
		internal void Raise(T message) {
			try {
				listeners?.Invoke(message);
			} catch (Exception e) {
				EventBus.ErrorLogger(e.Message);
			}
		}

		internal void AddListener(Action<T> callback) {
			if (listeners != null) {
				if (listeners.GetInvocationList().Contains(callback)) {
					EventBus.WarningLogger($"Method {callback.Method.Name} from object {callback.Target.GetType()} " +
					                       $"already registered for message {typeof(T)}!");
					return;
				}
			}

			listeners += callback;
		}

		internal void RemoveListener(Action<T> callback) => listeners -= callback;
		void IEventHandlersList.Clear() => listeners = null;
	}
}