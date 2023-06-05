using System;

namespace EXRCore.DIContainer {
	internal class ServiceDescriptor {
		private readonly Func<object> creator;
		public Type ImplementationType { get; }

		public ServiceDescriptor(Func<object> creator, Type concreteType) {
			this.creator = creator;
			ImplementationType = concreteType;
		}

		public object Resolve() => creator();
	}
}