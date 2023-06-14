using System;
using System.Collections.Generic;
using System.Threading;
using EXRCore.Pools;
using JetBrains.Annotations;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public sealed class Entity : IEntity, IPoolObject {
		private readonly EcsComponentsProvider persistentComponents;
		private readonly EcsSystemsProvider systems;
		private readonly CancellationTokenSource cts;
		
		private IDictionary<int, IDynamicComponent> dynamicComponents;
		private IDictionary<int, ICallbacksWrapper> onReceiveMessageCallbacks;
		private IDictionary<int, ICallbacksWrapper> onRemoveCallbacks;
		private IDictionary<int, Component> cashedComponents;
		
		public int? OwnerFactoryIdentity { get; }
		public GameObject Owner { get; }
		public Transform Transform => Owner.transform;
		public Vector3 Position => Transform.position;
		public Quaternion Rotation => Transform.rotation;
		public bool IsCreatedByFactory => OwnerFactoryIdentity != null;
		public CancellationToken DestroyToken => cts.Token;
		
		internal Entity(
			GameObject owner,
			[CanBeNull] EcsComponentsProvider persistentComponents,
			[CanBeNull] EcsSystemsProvider systems,
			in bool enableSystemsNow, int? ownerFactoryIdentity) {
			persistentComponents ??= EcsComponentsProvider.Empty;
			this.persistentComponents = persistentComponents;
			this.systems = systems;
			this.Owner = owner;
			this.OwnerFactoryIdentity = ownerFactoryIdentity;
			this.cts = new CancellationTokenSource();
			
			systems?.Initialize(this, persistentComponents, enableSystemsNow);
		}

		public void EnableSystem<T>() where T : class, IEcsSystem => systems?.Enable<T>();
		public void DisableSystem<T>() where T : class, IEcsSystem => systems?.Disable<T>();

		public T GetUnityComponent<T>() where T : Component {
			var key = TypeHelper<T>.Identity;
			if (cashedComponents != null && cashedComponents.TryGetValue(key, out var component)) {
				return (T)component;
			}

			component = Owner.GetComponent<T>();
			if (component == null) return null;
			
			cashedComponents ??= new Dictionary<int, Component>();
			cashedComponents[key] = component;
			return (T)component;
		}
		
		public bool TryGetUnityComponent<T>(out T component) where T : Component {
			component = GetUnityComponent<T>();
			return component != null;
		}
		
		#region MessageHandlers
		public void SendMessage<T>(T message) where T : IEntityMessage {
			if (onReceiveMessageCallbacks == null) return;
			var key = TypeHelper<T>.Identity;

			if (onReceiveMessageCallbacks.TryGetValue(key, out var callbacks)) {
				((OnReceivedMessagesCallbacks<T>)callbacks).Invoke(message);
			}
		}

		public void RegisterHandler<T>(Action<T> onReceivedMessage) where T : IEntityMessage {
			var key = TypeHelper<T>.Identity;
			onReceiveMessageCallbacks ??= new Dictionary<int, ICallbacksWrapper>();
			if (!onReceiveMessageCallbacks.TryGetValue(key, out var callbacks)) {
				var list = new OnReceivedMessagesCallbacks<T>();
				list.RegisterCallback(onReceivedMessage);
				onReceiveMessageCallbacks[key] = list;
			} else {
				((OnReceivedMessagesCallbacks<T>)callbacks).RegisterCallback(onReceivedMessage);
			}
			
			if (dynamicComponents.TryGetValue(key, out var component)) {
				onReceivedMessage((T)component);
			}
		}
		
		public void RegisterHandler<T>(Action onRemovedCallback) where T : IDynamicComponent {
			var key = TypeHelper<T>.Identity;
			onRemoveCallbacks ??= new Dictionary<int, ICallbacksWrapper>();
			if (onRemoveCallbacks.TryGetValue(key, out var callbacks)) {
				((OnRemovedComponentCallbackList)callbacks).RegisterCallback(onRemovedCallback);
				return;
			}

			var list = new OnRemovedComponentCallbackList();
			list.RegisterCallback(onRemovedCallback);
			onRemoveCallbacks[key] = list;
		}
		#endregion
		
		#region Components
        public bool AddComponent<T>(T component) where T: IDynamicComponent {
        	var key = TypeHelper<T>.Identity;
        	if (dynamicComponents != null) {
        	     if (dynamicComponents.ContainsKey(key)) return false;
        	} else {
        		dynamicComponents = new Dictionary<int, IDynamicComponent>();
        	}
        	dynamicComponents[key] = component;
        	SendMessage(component);
        	return true;
        }
        		
		public bool RemoveComponent<T>() where T : IDynamicComponent { 
			var key = TypeHelper<T>.Identity;
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
			return dynamicComponents != null && dynamicComponents.ContainsKey(TypeHelper<T>.Identity);
		}
		#endregion
                
		#region Explicitly
		void IEntity.OnEnable() => systems?.EnableAll();
		void IEntity.OnDisable() => systems?.DisableAll();
		
		void IEntity.OnDestroy() {
			cts.Cancel();
			systems?.Dispose();
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