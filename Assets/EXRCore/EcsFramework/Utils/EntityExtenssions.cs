﻿using System;

namespace EXRCore.EcsFramework {
	public static class EntityExtenssions {
		public static void RegisterHandler<T>(this Entity entity, Action<T> onAddCallback, Action onRemoveCallback)
			where T : IDynamicComponent {
			entity.RegisterHandler(onAddCallback);
			entity.RegisterHandler<T>(onRemoveCallback);
		}
	}
}