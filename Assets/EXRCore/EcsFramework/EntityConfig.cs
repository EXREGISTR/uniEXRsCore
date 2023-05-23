using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public abstract class EntityConfig : ScriptableObject {
		private readonly Dictionary<Type, IPersistentComponent> components = new();
		private readonly Dictionary<Type, IEcsSystem> systems = new();
		
		protected virtual bool needToCopyComponentsMap => true;
		protected virtual bool needToCopySystemsMap => true;
		
		[field: SerializeField] public GameObject Prefab { get; private set; }
		public EcsComponentsProvider Components => new(components, needToCopyComponentsMap);
		public EcsSystemsProvider Systems => new(systems, needToCopySystemsMap);
		
		public abstract void Initialize();
		
		public void RegisterComponent<T>(T component) where T: IPersistentComponent => Register(typeof(T), components, component);
		public void RegisterSystem<T>(T system) where T : IEcsSystem => Register(typeof(T), systems, system);
		
		public void ReplaceComponent<T>(T component) where T : IPersistentComponent => Replace(typeof(T), components, component);
		public void ReplaceSystem<T>(T system) where T: IEcsSystem => Replace(typeof(T), systems, system);
		
		public void UnregisterComponent<T>() where T: IPersistentComponent => components.Remove(typeof(T));
		public void UnregisterSystem<T>() where T: IEcsSystem => systems.Remove(typeof(T));
		
		private static void Register<T>(Type subjectType, IDictionary<Type, T> target, T subject) where T: IEcsSubject {
			if (target.ContainsKey(subjectType)) {
				Debug.LogWarning($"Subject {subjectType} already registered");
				return;
			}
			
			Replace(subjectType, target, subject, false);
		}
		
		private static void Replace<T>(Type subjectType, IDictionary<Type, T> target, T subject, bool needToRemove = true) where T: IEcsSubject {
			if (subjectType.IsAbstract) {
				throw new ArgumentException("Impossible to register key of abstract subject!");
			}
			
			if (needToRemove) {
				target.Remove(subjectType);
			}
			
			target[subjectType] = subject;
		}
	}
}