using System;
using System.Collections.Generic;

namespace EXRCore.Events {
	public static class EventBus {
		public static Action<string> WarningLogger { get; set; }
		public static Action<string> ErrorLogger { get; set; }

		static EventBus() {
			WarningLogger = Console.WriteLine;
			ErrorLogger = Console.Error.WriteLine;
		}
		
		private static readonly Dictionary<Type, IEventHandlersList> eventsMap = new(8);

		public static void Subscribe<T>(Action<T> callback) where T: IMessage {
			var messageType = typeof(T);

			if (TryGetHandlersList<T>(messageType, out var handlers)) {
				handlers.AddListener(callback);
				return;
			}

			handlers = new EventHandlersList<T>();
			eventsMap[messageType] = handlers;
			handlers.AddListener(callback);
		}
		
		public static void Unsubscribe<T>(Action<T> callback) where T: IMessage {
			var messageType = typeof(T);

			if (TryGetHandlersList<T>(messageType, out var handlers)) {
				handlers.RemoveListener(callback);
			}
		}

		public static void RaiseEvent<T>(T message) where T: IMessage {
			var messageType = typeof(T);

			if (!TryGetHandlersList<T>(messageType, out var handlers)) {
				WarningLogger($"There are no subscribed handlers for message {messageType}");
				return;
			}
			
			handlers.Raise(message);
		}

		public static void ClearHandlers<T>() where T: IMessage {
			if (eventsMap.TryGetValue(typeof(T), out IEventHandlersList founded)) {
				founded.Clear();
			}
		}

		public static void Reset() {
			foreach (var handlersList in eventsMap.Values) {
				handlersList.Clear();
			}
			
			eventsMap.Clear();
		}
		
		private static bool TryGetHandlersList<T>(Type messageType, out EventHandlersList<T> founded) where T: IMessage {
			if (eventsMap.TryGetValue(messageType, out IEventHandlersList list)) {
				if (list is not EventHandlersList<T> handlers) {
					throw new InvalidCastException(
						$"Failed to convert type {list.GetType()} to type {typeof(EventHandlersList<T>)}");
				}

				founded = handlers;
				return true;
			}

			founded = null;
			return false;
		}
	}
}