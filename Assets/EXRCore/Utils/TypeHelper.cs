namespace EXRCore.EcsFramework {
	public static class TypeHelper<T> {
		public static int Identity { get; } = typeof(T).GetHashCode();
	}
}