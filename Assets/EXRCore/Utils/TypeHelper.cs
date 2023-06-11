namespace EXRCore.Utils {
	public static class TypeHelper<T> {
		public static int Identity { get; } = typeof(T).GetHashCode();
	}
}