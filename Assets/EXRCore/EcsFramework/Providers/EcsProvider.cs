using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public abstract class EcsProvider<TBaseSubject> {
		private readonly IDictionary<int, TBaseSubject> subjectsMap;

		protected EcsProvider() => subjectsMap = null;
		protected EcsProvider(IReadOnlyDictionary<int, Func<TBaseSubject>> subjects) {
			subjectsMap = new Dictionary<int, TBaseSubject>(subjects.Count);
			foreach (var kvp in subjects) {
				subjectsMap[kvp.Key] = kvp.Value();
			}
		}
		
		protected EcsProvider(IDictionary<int, TBaseSubject> subjectsMap) {
			this.subjectsMap = subjectsMap;
		}
		
		protected void ExecuteFor<T>(Action<T> callback) where T: class, TBaseSubject {
			if (subjectsMap.TryGetValue(TypeHelper<T>.Identity, out var subject)) {
				callback(subject as T);
			}
		}
		
		protected void ExecuteForAll(Action<TBaseSubject> callback) {
			foreach (var subject in subjectsMap.Values) {
				callback(subject);
			}
		}

		public T Get<T>() where T : TBaseSubject {
			return subjectsMap.TryGetValue(TypeHelper<T>.Identity, out var subject)
				? (T)subject
				: throw new NullReferenceException($"{typeof(TBaseSubject).Name} of type {typeof(T)} doesn't registered!");
		}

		public bool TryGet<T>(out T subject) where T : TBaseSubject {
			try {
				subject = Get<T>();
			} catch (NullReferenceException exception) {
				Debug.LogError(exception.Message);
				subject = default;
				return false;
			}
			
			return true;
		}
		
		public bool Contains<T>() where T : TBaseSubject => subjectsMap.ContainsKey(TypeHelper<T>.Identity);
	}
}