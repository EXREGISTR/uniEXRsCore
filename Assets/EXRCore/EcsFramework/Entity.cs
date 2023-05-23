using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public sealed class Entity {
		private readonly EcsComponentsProvider persistentComponents;
		private readonly EcsSystemsProvider systems;
		
		private IDictionary<Type, IDynamicComponent> dynamicComponents;
		private IDictionary<Type, ICallbacksWrapper> onAddCallbacks;
		private IDictionary<Type, ICallbacksWrapper> onRemoveCallbacks;
		public GameObject Owner { get; }
		
		public Entity(
			GameObject owner,
			[CanBeNull] EcsComponentsProvider persistentComponents,
			[CanBeNull] EcsSystemsProvider systems) {

			this.persistentComponents = persistentComponents;
			this.systems = systems;
			this.Owner = owner;

			systems?.Initialize(this, persistentComponents);
		}

		public bool AddComponent<T>(T component) where T: IDynamicComponent {
			var key = typeof(T);
			if (dynamicComponents.ContainsKey(key)) return false;
			dynamicComponents ??= new Dictionary<Type, IDynamicComponent>();
			dynamicComponents[key] = component;
			
			if (onAddCallbacks != null) {
				if (onAddCallbacks.TryGetValue(key, out var callbacks)) {
					((OnAddComponentCallbackList<T>)callbacks).OnAddComponent(component);
				}
			}
			
			return true;
		}
		
		public bool RemoveComponent<T>() where T : IDynamicComponent {
			var key = typeof(T);
			if (!dynamicComponents.ContainsKey(key)) return false;
			
			var result = dynamicComponents.Remove(key);
			if (onRemoveCallbacks != null) {
				if (onRemoveCallbacks.TryGetValue(key, out var callbacks)) {
					((OnRemovedComponentCallbackList)callbacks).OnRemovedComponent();
				}
			}

			return result;
		}
		
		internal void FixedUpdate() => systems?.FixedUpdate();
		internal void Update() => systems?.Update();

		public bool ContainsPersistentComponent<T>() where T: IPersistentComponent {
			return persistentComponents != null && persistentComponents.Contains<T>();
		}

		public bool ContainsDynamicComponent<T>() where T: IDynamicComponent {
			return dynamicComponents != null && dynamicComponents.ContainsKey(typeof(T));
		}

		public void EnableSystem<T>() where T : IEcsSystem => systems.Enable<T>();
		public void DisableSystem<T>() where T : IEcsSystem => systems.Disable<T>();
		
		public void RegisterHandler<T>([NotNull] Action<T> onAddCallback) where T : IDynamicComponent {
			var key = typeof(T);
			onAddCallbacks ??= new Dictionary<Type, ICallbacksWrapper>();
			if (!onAddCallbacks.TryGetValue(key, out var callbacks)) {
				var list = new OnAddComponentCallbackList<T>();
				list.RegisterCallback(onAddCallback);
				onAddCallbacks[key] = list;
			} else {
				((OnAddComponentCallbackList<T>)callbacks).RegisterCallback(onAddCallback);
			}
			
			if (dynamicComponents.TryGetValue(key, out var component)) {
				onAddCallback((T)component);
			}
		}
		
		public void RegisterHandler<T>([NotNull] Action onRemoveCallback) where T : IDynamicComponent {
			var key = typeof(T);
			onRemoveCallbacks ??= new Dictionary<Type, ICallbacksWrapper>();
			if (onRemoveCallbacks.TryGetValue(key, out var callbacks)) {
				((OnRemovedComponentCallbackList)callbacks).RegisterCallback(onRemoveCallback);
				return;
			}

			var list = new OnRemovedComponentCallbackList();
			list.RegisterCallback(onRemoveCallback);
			onRemoveCallbacks[key] = list;
		}
		
		internal void OnDestroy() {
			if (onAddCallbacks != null) {
				foreach (var callbacksList in onAddCallbacks.Values) {
					callbacksList.Clear();
				}

				onAddCallbacks.Clear();
			}

			if (onRemoveCallbacks != null) {
				foreach (var callbacksList in onRemoveCallbacks.Values) {
					callbacksList.Clear();
				}

				onRemoveCallbacks.Clear();
			}
			
			persistentComponents?.ResetAll();
		}

		public override bool Equals(object obj) {
			if (obj is not Entity other) return false;
			return GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode() => Owner.GetHashCode();
		public override string ToString() => $"Entity {Owner.name}";
	}
}