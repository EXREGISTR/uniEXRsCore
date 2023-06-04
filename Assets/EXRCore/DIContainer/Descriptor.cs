using System.Collections.Generic;
using System.Reflection;

namespace EXRCore.DIContainer {
	public sealed class Descriptor {
		private readonly List<FieldInfo> fieldsToInject = new();
		public bool IsEmpty => fieldsToInject.Count == 0;
		public void AddField(FieldInfo field) => fieldsToInject.Add(field);

		public void Inject(object target, ServiceContainer container) {
			foreach (var field in fieldsToInject) {
				if (container.TryResolveService(field.FieldType, out var service)) {
					field.SetValue(target, service);
				}
			}
		}
	}
}