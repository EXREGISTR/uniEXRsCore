﻿using System;
using EXRCore.DIContainer;
using UnityEngine;

namespace EXRCore.Pools {
	public interface IPoolProvider {
		public Type Type { get; }
	}

	public abstract class PoolProvider<T> : MonoBehaviour, IPoolProvider where T : IPoolObject {
		[SerializeField] private int count;
		
		[InjectService] private PoolService poolService;
		
		private Pool<T> pool;

		public Type Type { get; }
		
		protected PoolProvider() => Type = GetType();
		
		private void Awake() {
			poolService.Register(this);
			pool = new Pool<T>(Factory, Resetter, count);
		}
		
		private void OnDestroy() => poolService.Unregister(this);

		public T Get() => pool.Get();
		public void Return(T @object) => pool.Return(@object);

		protected abstract T Factory(); 
		protected abstract void Resetter(T @object);
		
		public override string ToString() => Type.ToString();
	}
}
