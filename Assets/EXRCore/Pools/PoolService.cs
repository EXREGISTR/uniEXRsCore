using System;
using System.Collections.Generic;
using EXRCore.Services;
using UnityEngine;

namespace EXRCore.Pools {
	public sealed class PoolService : IService {
		private readonly Dictionary<Type, IPoolProvider> poolProviders = new();

		internal void Register(IPoolProvider provider) {
			var key = provider.Type;
			if (poolProviders.ContainsKey(key)) {
				Debug.LogWarning($"Pool {provider} already registered!");
				return;
			}
			
			poolProviders[key] = provider;
			Debug.Log($"Pool {provider} was registered!");
		}

		internal void Unregister(IPoolProvider provider) => poolProviders.Remove(provider.Type);

		public TObject Get<TPool, TObject>() where TPool : PoolProvider<TObject> where TObject: IPoolObject {
			var key = typeof(TPool);
			var provider = poolProviders[key];
			var @object = ((TPool)provider).Get();
			return @object;
		}
		
		public void Return<TPool, TObject>(TObject @object) where TPool : PoolProvider<TObject> where TObject : IPoolObject {
			var key = typeof(TPool);
			var provider = poolProviders[key];
			((TPool)provider).Return(@object);
		}
	}
}