using System;

namespace EXRCore.DIContainer {
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class InjectServiceAttribute : Attribute { }
}