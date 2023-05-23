using System;
using JetBrains.Annotations;

namespace EXRCore.EcsFramework {
	public static class Extenssions {
		public static void RegisterHandler<T>(this Entity entity, [NotNull] Action<T> onAddedCallback, [NotNull] Action onRemovedCallback)
			where T : IDynamicComponent {
			entity.RegisterHandler(onAddedCallback);
			entity.RegisterHandler<T>(onRemovedCallback);
		}
	}
}