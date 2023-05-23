using System;

namespace EXRCore.UIFramework {
	public class ReactiveProperty<T> {
		private T value;

		public T Value {
			get => value;
			set {
				if (this.value != null) {
					if (this.value.Equals(value)) return;
				}
				
				this.value = value;
				Changed?.Invoke(value);
			}
		}

		public event Action<T> Changed;

		public ReactiveProperty(T value = default) => this.value = value;
		
		public static implicit operator T(ReactiveProperty<T> property) => property.value;
	}
}