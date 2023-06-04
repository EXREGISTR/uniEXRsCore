using System;

namespace EXRCore.DIContainer {
	internal interface IScope {
		public object Resolve();
	}

	internal class SingletonScope : IScope {
		private readonly Func<object> creator;
		private object instance;
		
		public SingletonScope(Func<object> creator) {
			this.creator = creator;
		}
		
		public object Resolve() => instance ??= creator();
	}
}