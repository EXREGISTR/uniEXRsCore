using UnityEngine;

namespace EXRCore.Utils {
	public interface IEntity {
		public GameObject Owner { get; }
		
		public T GetUnityComponent<T>() where T : Component;
		public bool TryGetUnityComponent<T>(out T component) where T : Component;
		public void SendMessage<T>(T message) where T : IEntityMessage;
		public bool AddComponent<T>(T component) where T : IDynamicComponent;
		public bool RemoveComponent<T>() where T : IDynamicComponent;
		internal void FixedUpdate();
		internal void Update();
		internal void OnDestroy();
		internal void OnEnable();
		internal void OnDisable();
	}
}