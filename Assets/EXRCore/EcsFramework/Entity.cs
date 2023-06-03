using System;
using System.Collections.Generic;
using EXRCore.Pools;
using JetBrains.Annotations;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public sealed class Entity : IEntity, IPoolObject {
		private readonly EcsComponentsProvider persistentComponents;
		private readonly EcsSystemsProvider systems;
		
		private IDictionary<Type, IDynamicComponent> dynamicComponents;
		private IDictionary<Type, ICallbacksWrapper> onReceiveMessageCallbacks;
		private IDictionary<Type, ICallbacksWrapper> onRemoveCallbacks;
		
		public GameObject Owner { get; }
		public Transform Transform => Owner.transform;
		public Vector3 Position => Transform.position;
		public Quaternion Rotation => Transform.rotation;

		public Entity(
			GameObject owner,
			[CanBeNull] EcsComponentsProvider persistentComponents,
			[CanBeNull] EcsSystemsProvider systems,
			bool enableSystemsNow) {
			
			this.persistentComponents = persistentComponents;
			this.systems = systems;
			this.Owner = owner;
			
			systems?.Initialize(this, persistentComponents, enableSystemsNow);
		}
		
		public void SendMessage<T>(T message) where T : IEntityMessage {
			if (onReceiveMessageCallbacks == null) return;
			var messageType = typeof(T);
			
			if (onReceiveMessageCallbacks.TryGetValue(messageType, out var callbacks)) {
				((OnReceivedMessagesCallbacks<T>)callbacks).Invoke(message);
			}
		}
		
		public void EnableSystem<T>() where T : IEcsSystem => systems.Enable<T>();
		public void DisableSystem<T>() where T : IEcsSystem => systems.Disable<T>();
		
		public void RegisterHandler<T>(Action<T> onAddedCallback) where T : IEntityMessage {
			var key = typeof(T);
			onReceiveMessageCallbacks ??= new Dictionary<Type, ICallbacksWrapper>();
			if (!onReceiveMessageCallbacks.TryGetValue(key, out var callbacks)) {
				var list = new OnReceivedMessagesCallbacks<T>();
				list.RegisterCallback(onAddedCallback);
				onReceiveMessageCallbacks[key] = list;
			} else {
				((OnReceivedMessagesCallbacks<T>)callbacks).RegisterCallback(onAddedCallback);
			}
			
			if (dynamicComponents.TryGetValue(key, out var component)) {
				onAddedCallback((T)component);
			}
		}
		public void RegisterHandler<T>(Action onRemovedCallback) where T : IEntityMessage {
			var key = typeof(T);
			onRemoveCallbacks ??= new Dictionary<Type, ICallbacksWrapper>();
			if (onRemoveCallbacks.TryGetValue(key, out var callbacks)) {
				((OnRemovedComponentCallbackList)callbacks).RegisterCallback(onRemovedCallback);
				return;
			}

			var list = new OnRemovedComponentCallbackList();
			list.RegisterCallback(onRemovedCallback);
			onRemoveCallbacks[key] = list;
		}
		
		#region Components
        public bool AddComponent<T>(T component) where T: IDynamicComponent {
        	var key = typeof(T);
        	if (dynamicComponents != null) {
        	     if (dynamicComponents.ContainsKey(key)) return false;
        	} else {
        		dynamicComponents = new Dictionary<Type, IDynamicComponent>();
        	}
        	dynamicComponents[key] = component;
        	SendMessage(component);
        	return true;
        }
        		
		public bool RemoveComponent<T>() where T : IDynamicComponent { 
			var key = typeof(T);
			if (dynamicComponents == null) return false;
        
			var result = dynamicComponents.Remove(key);
			if (!result) return false;
        			
			if (onRemoveCallbacks != null) { 
				if (onRemoveCallbacks.TryGetValue(key, out var callbacks)) {
					((OnRemovedComponentCallbackList)callbacks).Invoke();
				}
			}
			
			return true;
		}
        
		public bool ContainsPersistentComponent<T>() where T: IPersistentComponent {
			return persistentComponents != null && persistentComponents.Contains<T>();
        }
        
		public bool ContainsDynamicComponent<T>() where T: IDynamicComponent {
			return dynamicComponents != null && dynamicComponents.ContainsKey(typeof(T));
		}
		#endregion
                
		#region Explicitly
		void IEntity.OnEnable() => systems.EnableAll();
		void IEntity.OnDisable() => systems.DisableAll();
		void IEntity.FixedUpdate() => systems?.FixedUpdate();
		void IEntity.Update() => systems?.Update();
		
		void IEntity.OnDestroy() {
			systems.Dispose();
			if (onReceiveMessageCallbacks != null) {
				foreach (var callbacksList in onReceiveMessageCallbacks.Values) {
					callbacksList.Clear();
				}

				onReceiveMessageCallbacks.Clear();
			}

			if (onRemoveCallbacks != null) {
				foreach (var callbacksList in onRemoveCallbacks.Values) {
					callbacksList.Clear();
				}

				onRemoveCallbacks.Clear();
			}
		}
		#endregion
		
		#region Operators
		public static implicit operator GameObject(Entity entity) => entity.Owner;

		public static bool operator ==(GameObject a, Entity b) {
			if (a is null || b is null) return false;
			
			return a.GetHashCode() != b.GetHashCode();
		}
		public static bool operator !=(GameObject a, Entity b) {
			return !(a == b);
		}

		public static bool operator ==(Entity a, GameObject b) {
			if (a is null || b is null) return false;
			
			return a.GetHashCode() != b.GetHashCode();
		}
		public static bool operator !=(Entity a, GameObject b) {
			return !(a == b);
		}
		
		public static bool operator ==(Entity a, Entity b) {
			if (a is null || b is null) return false;
			
			return a.GetHashCode() == b.GetHashCode();
		}
		public static bool operator !=(Entity a, Entity b) {
			return !(a == b);
		}
		#endregion
		
		public override bool Equals(object obj) {
			if (obj is not Entity other) return false;
			return GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode() => Owner.GetHashCode();
		public override string ToString() => $"Entity {Owner.name}";
	}
}