using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class Basic : ILoadable {
		public static FastFieldInfo<Delegate, object> _target;
		internal static List<ReflectionLoader> reflectionLoaders = new();
		public void Load(Mod mod) {
			_target = new("_target", BindingFlags.NonPublic | BindingFlags.Instance);
		}
		public void Unload() {
			_target = null;
			reflectionLoaders = null;
		}
		public static void LoadReflections(Type type) {
			foreach (var item in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				string name = item.GetCustomAttribute<ReflectionMemberNameAttribute>()?.MemberName ?? item.Name;
				if (item.FieldType.IsAssignableTo(typeof(Delegate))) {
					Type parentType = item.GetCustomAttribute<ReflectionParentTypeAttribute>().ParentType;
					item.SetValue(null, parentType.GetMethod(name, item.GetCustomAttribute<ReflectionParameterTypesAttribute>()?.Types ?? Array.Empty<Type>()).CreateDelegate(item.FieldType));
				} else if (item.FieldType.IsGenericType) {
					Type genericType = item.FieldType.GetGenericTypeDefinition();
					if (genericType == typeof(FastFieldInfo<,>) || genericType == typeof(FastStaticFieldInfo<,>)) {
						item.SetValue(
							null,
							item.FieldType.GetConstructor(new Type[] { typeof(string), typeof(BindingFlags), typeof(bool) })
							.Invoke(new object[] { name, BindingFlags.Public | BindingFlags.NonPublic, true })
						);
					} else if (genericType == typeof(FastStaticFieldInfo<>)) {
						item.SetValue(
							null,
							item.FieldType.GetConstructor(new Type[] { typeof(Type), typeof(string), typeof(BindingFlags), typeof(bool) })
							.Invoke(new object[] { item.GetCustomAttribute<ReflectionParentTypeAttribute>().ParentType, name, BindingFlags.Public | BindingFlags.NonPublic, true })
						);
					}
				}
			}
		}
		public static void UnloadReflections(Type type) {
			foreach (var item in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				if (!item.FieldType.IsValueType) {
					item.SetValue(null, null);
				}
			}
		}
	}
	public abstract class ReflectionLoader : ILoadable {
		public abstract Type ParentType { get; }
		public void Load(Mod mod) {
			Basic.reflectionLoaders.Add(this);
			Basic.LoadReflections(ParentType);
		}
		public void Unload() {
			Basic.UnloadReflections(ParentType);
		}
		~ReflectionLoader() {
			Unload();
		}
	}
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ReflectionMemberNameAttribute : Attribute {
		public string MemberName { get; init; }
		public ReflectionMemberNameAttribute(string memberName) {
			MemberName = memberName;
		}
	}
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ReflectionParentTypeAttribute : Attribute {
		public Type ParentType { get; init; }
		public ReflectionParentTypeAttribute(Type type) {
			ParentType = type;
		}
	}
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class ReflectionParameterTypesAttribute : Attribute {
		public Type[] Types { get; init; }
		public ReflectionParameterTypesAttribute(params Type[] types) {
			Types = types;
		}
	}
}