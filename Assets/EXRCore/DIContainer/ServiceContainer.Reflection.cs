using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EXRCore.DIContainer {
	public partial class ServiceContainer {
		private sealed class TypeDescriptor {
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
		
		private static IReadOnlyDictionary<Type, TypeDescriptor> descriptorsByObjectType;
		
		[InitializeOnEnterPlayMode]
		private static void CreateDescriptors() {
			var allTypes = Assembly.GetExecutingAssembly().GetTypes();
			var descriptors = new Dictionary<Type, TypeDescriptor>();
			var attributeType = typeof(InjectServiceAttribute);
			foreach (var type in allTypes) {
				var descriptor = new TypeDescriptor();
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
			if (field.FieldType == typeof(object)) return false;
			
			return true;
		}
		
		public static void ExecuteInjection<T>(T target) where T : class => ExecuteInjectionInternal(typeof(T), target);
		
		private static void ExecuteInjectionInternal(Type type, object target) {
			if (type.IsAbstract) { 
				Debug.LogWarning("You trying to execute injection to abstract type!");
				return;
			}
			
			if (descriptorsByObjectType.TryGetValue(type, out TypeDescriptor descriptor)) {
				descriptor.Inject(target, current);
			}
		}
	}
}