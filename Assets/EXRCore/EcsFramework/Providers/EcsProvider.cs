using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public abstract class EcsProvider<TSubject> where TSubject: IEcsSubject {
		private readonly IDictionary<Type, TSubject> subjectsMap;
		
		protected EcsProvider(IReadOnlyDictionary<Type, Func<TSubject>> subjects) {
			subjectsMap = new Dictionary<Type, TSubject>(subjects.Count);
			foreach (var kvp in subjects) {
				subjectsMap[kvp.Key] = kvp.Value.Invoke();
			}
		}
		
		protected EcsProvider(IDictionary<Type, TSubject> subjectsMap) {
			this.subjectsMap = subjectsMap;
		}
		
		protected void ExecuteFor<T>(Action<TSubject> callback) where T: TSubject {
			if (subjectsMap.TryGetValue(typeof(T), out var subject)) {
				callback(subject);
			}
		}
		
		protected void ExecuteForAll(Action<TSubject> callback) {
			foreach (var subject in subjectsMap.Values) {
				callback(subject);
			}
		}

		public T Get<T>() where T : TSubject {
			var key = typeof(T);
			return subjectsMap.TryGetValue(key, out var subject)
				? (T)subject
				: throw new NullReferenceException($"{typeof(TSubject).Name} of type {key} doesn't registered!");
		}

		public bool TryGet<T>(out T subject) where T : TSubject {
			try {
				subject = Get<T>();
			} catch (NullReferenceException exception) {
				Debug.LogError(exception.Message);
				subject = default;
				return false;
			}
			
			return true;
		}
		
		public bool Contains<T>() where T : TSubject => subjectsMap.ContainsKey(typeof(T));
	}
}