using System;
using System.Collections.Generic;
using UnityEngine;

namespace EXRCore.EcsFramework {
	public abstract class EcsProvider<TSubject> where TSubject: IEcsSubject {
		protected readonly IDictionary<Type, TSubject> subjects;
		
		protected EcsProvider(IDictionary<Type, TSubject> subjects, bool needToCopy) {
			if (!needToCopy) {
				this.subjects = subjects;
				return;
			}
			
			this.subjects = new Dictionary<Type, TSubject>(subjects.Count);
			foreach (var kvp in subjects) {
				subjects[kvp.Key] = kvp.Value;
			}
		}

		public T Get<T>() where T : TSubject {
			var key = typeof(T);
			return subjects.TryGetValue(key, out var subject)
				? (T)subject
				: throw new NullReferenceException($"Subject of type {key} doesn't registered in entity!");
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
		
		public bool Contains<T>() where T : TSubject => subjects.ContainsKey(typeof(T));
	}
}