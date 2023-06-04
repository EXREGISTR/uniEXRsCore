using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EXRCore.DIContainer {
	public partial class ServiceContainer {
		private static IReadOnlyDictionary<Type, Descriptor> descriptorsByObjectType;
		
		[InitializeOnEnterPlayMode]
		private static void CreateDescriptors() {
			var allTypes = Assembly.GetExecutingAssembly().GetTypes();
			var descriptors = new Dictionary<Type, Descriptor>();
			var attributeType = typeof(InjectServiceAttribute);
			foreach (var type in allTypes) {
				var descriptor = new Descriptor();
				foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)) {
					if (HandleField(field, attributeType)) {
						descriptor.AddField(field);
					}
				}
				
				if (!descriptor.IsEmpty) {
					descriptors[type] = descriptor;
				}
			}
			
			descriptorsByObjectType = descriptors;
		}
		
		private static bool HandleField(FieldInfo field, Type attributeType) {
			if (field.GetCustomAttribute(attributeType) == null) return false;
			if (field.FieldType.IsAbstract) {
				Debug.LogError("Field with inject attribute cannot be with abstract type!");
				return false;
			}

			return true;
		}
		
		public static void ExecuteInjection<T>(T target) where T : class => ExecuteInjectionInternal(typeof(T), target);
		
		private static void ExecuteInjectionInternal(Type type, object target) {
			if (descriptorsByObjectType.TryGetValue(type, out var descriptor)) {
				descriptor.Inject(target, current);
			}
		}
	}
}