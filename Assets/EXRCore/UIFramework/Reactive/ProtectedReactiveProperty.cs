using System;

namespace EXRCore.UIFramework {
	public class ProtectedReactiveProperty<T> where T: struct {
		private readonly Guid password;
		
		public delegate void OnValueChanged(in T value);
		public event OnValueChanged Changed;

		public ProtectedReactiveProperty(T value, Guid password) : this(password) => this.value = value;
		public ProtectedReactiveProperty(Guid password) => this.password = password;
		
		private T value;
		public T Value => value;

		public bool SetValue(T newValue, Guid password) {
			if (this.password != password) return false;
			if (value.Equals(newValue)) return false;
		
			value = newValue;
			Changed?.Invoke(value);
			return true;
		}
		
		public static implicit operator T(ProtectedReactiveProperty<T> property) => property.value;
	}
}