using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EXRCore.Observers {
	public sealed class GameDispatcher : MonoBehaviour {
		private class FullObserver : IDisposable {
			private readonly int updateId;
			private readonly int fixedUpdateId;

			public FullObserver(int updateId, int fixedUpdateId) {
				this.updateId = updateId;
				this.fixedUpdateId = fixedUpdateId;
			}
			
			public void Dispose() {
				instance.updateObservers.Remove(updateId);
				instance.fixedUpdateObservers.Remove(fixedUpdateId);
			}
		}
		private class SoloObserver : IDisposable {
			private readonly int id;

			public SoloObserver(int id) => this.id = id;
			
			public void Dispose() {
				if (id < 1000) {
					instance.fixedUpdateObservers.Remove(id);
				} else {
					instance.updateObservers.Remove(id);
				}
			}
		}
		
		private static GameDispatcher instance;
		
		private CancellationTokenSource cts;
		private IDictionary<int, IUpdatable> updateObservers;
		private IDictionary<int, IFixedUpdatable> fixedUpdateObservers;
		
		private static GameDispatcher GetInstance() {
			if (instance == null) {
				instance = new GameObject().AddComponent<GameDispatcher>();
			}

			return instance;
		}

		private void Awake() {
			cts = new CancellationTokenSource();
		}
		
		private void OnDestroy() {
			cts.Cancel();
		}

		public static IDisposable AddOnAllUpdate<T>(T observer) where T : IUpdatable, IFixedUpdatable {
			var dispatcher = GetInstance();
			dispatcher.fixedUpdateObservers ??= new Dictionary<int, IFixedUpdatable>();
			dispatcher.updateObservers ??= new Dictionary<int, IUpdatable>();
			
			int fixedUpdateId = dispatcher.fixedUpdateObservers.Count;
			int updateId = 1000 + dispatcher.updateObservers.Count;
			
			dispatcher.fixedUpdateObservers[fixedUpdateId] = observer;
			dispatcher.updateObservers[updateId] = observer;
			
			return new FullObserver(updateId, fixedUpdateId);
		}
		
		public static IDisposable AddOnFixedUpdate(IFixedUpdatable observer) {
			var dispatcher = GetInstance();
			dispatcher.fixedUpdateObservers ??= new Dictionary<int, IFixedUpdatable>();
			int id = dispatcher.fixedUpdateObservers.Count;
			dispatcher.fixedUpdateObservers[id] = observer;
			return new SoloObserver(id);
		}
		
		public static IDisposable AddOnUpdate(IUpdatable observer) {
			var dispatcher = GetInstance();
			dispatcher.updateObservers ??= new Dictionary<int, IUpdatable>();
			int id = 1000 + dispatcher.updateObservers.Count;
			dispatcher.updateObservers[id] = observer;
			return new SoloObserver(id);
		}
		
		public static Coroutine ExecuteCoroutine(IEnumerator routine) {
			return GetInstance().StartCoroutine(routine);
		}
		
		public static void StopExecutingCoroutine(Coroutine coroutine) {
			GetInstance().StopCoroutine(coroutine);
		}
		
		public static void ExecuteAfterTime(Action callback, in float secondsDelay, CancellationToken token) {
			var timer = new Timer(callback, secondsDelay);
			timer.Start(token);
		}
		
		public static void ExecuteAfterTime(Action callback, in float secondsDelay) => ExecuteAfterTime(callback, secondsDelay, GetInstance().cts.Token);
	}
}