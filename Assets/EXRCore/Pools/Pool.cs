using System;
using System.Collections.Generic;

namespace EXRCore.Pools {
	public class Pool<T> {
		private readonly Queue<T> pool;
		private readonly Func<T> factory;
		private readonly Action<T> resetter;
		
		public Pool(Func<T> factory, Action<T> resetter, int startCount = 8) {
			pool = new Queue<T>(startCount);
			this.factory = factory;
			this.resetter = resetter;
			
			for (int i = 0; i < startCount; i++) {
				pool.Enqueue(factory());
			}
		}
		
		public T Get() => pool.TryDequeue(out T @object) ? @object : factory();
		public void Return(T @object) {
			resetter(@object);
			pool.Enqueue(@object);
		}
	}
}