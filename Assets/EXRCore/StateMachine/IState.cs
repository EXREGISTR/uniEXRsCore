namespace EXRCore.StateMachine {
	public interface IState {
		public bool CanEnter { get; }
		public bool CanExit { get; }
		public void Enter();
		public void Exit();
	}
}