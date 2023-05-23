using System;
using JetBrains.Annotations;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public interface IEntity {
		public GameObject Owner { get; }

		internal void FixedUpdate();
		internal void Update();
		
		public bool AddComponent<T>(T component) where T : IDynamicComponent;
		public bool RemoveComponent<T>() where T : IDynamicComponent;
		
		public void EnableSystem<T>() where T : IEcsSystem;
		public void DisableSystem<T>() where T : IEcsSystem;

		public void RegisterHandler<T>([NotNull] Action<T> onAddCallback) where T : IDynamicComponent;
		public void RegisterHandler<T>([NotNull] Action onRemoveCallback) where T : IDynamicComponent;

		public bool ContainsPersistentComponent<T>() where T : IPersistentComponent;
		public bool ContainsDynamicComponent<T>() where T : IDynamicComponent;

		internal void OnDestroy();
	}
}