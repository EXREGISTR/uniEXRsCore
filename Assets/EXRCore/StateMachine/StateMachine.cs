using System;
using System.Collections.Generic;
using EXRCore.Utils;

namespace EXRCore.StateMachine {
	public enum TransitionFailReason {
		StateAlreadyActive, StateDoesntRegistered, ImpossibleExitFromCurrentState, ImpossibleEnterToNewState,
	}
	
	public sealed class StateMachine<TBaseState> where TBaseState : IState {
		private readonly Dictionary<int, TBaseState> states = new(8);

		public TBaseState Current { get; private set; }
		private int currentStateIdentity;
		
		public void Register<T>(T state) where T : TBaseState {
			var identity = TypeHelper<T>.Identity;
			if (states.ContainsKey(identity)) {
				throw new InvalidOperationException($"State for type {typeof(T)} already registered");
			}
			
			states[identity] = state;
		}

		public bool SetState<T>(Action<TransitionFailReason> executeIfChangeFailed = null) where T : TBaseState {
			var identity = TypeHelper<T>.Identity;
			
			if (Current != null && !Current.CanExit) {
				executeIfChangeFailed?.Invoke(TransitionFailReason.ImpossibleExitFromCurrentState);
				return false;
			}
			
			if (currentStateIdentity == identity) {
				executeIfChangeFailed?.Invoke(TransitionFailReason.StateAlreadyActive);
				return false;
			}

			if (!states.TryGetValue(identity, out var state)) {
				executeIfChangeFailed?.Invoke(TransitionFailReason.StateDoesntRegistered);
				return false;
			}

			if (!state.CanEnter) {
				executeIfChangeFailed?.Invoke(TransitionFailReason.ImpossibleEnterToNewState);
				return false;
			}
			
			ExecuteTransition(identity, state);
			return true;
		}
		
		private void ExecuteTransition(int identity, TBaseState newState) {
			Current?.Exit();
			Current = newState;
			currentStateIdentity = identity;
			Current.Enter();
		}
	}
}