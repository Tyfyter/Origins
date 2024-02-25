using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
		public static T MakeInstanceCaller<T>(Type type, string name) where T : Delegate {
			return MakeInstanceCaller<T>(type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
		}
		public static T MakeInstanceCaller<T>(MethodInfo method) where T : Delegate {
			string methodName = method.ReflectedType.FullName + ".call_" + method.Name;
			MethodInfo invoke = typeof(T).GetMethod("Invoke");
			ParameterInfo[] parameters = invoke.GetParameters();
			DynamicMethod getterMethod = new DynamicMethod(methodName, invoke.ReturnType, parameters.Select(p => p.ParameterType).ToArray(), true);
			ILGenerator gen = getterMethod.GetILGenerator();

			for (int i = 0; i < parameters.Length; i++) {
				gen.Emit(OpCodes.Ldarg_S, i);
			}
			gen.Emit(OpCodes.Call, method);
			gen.Emit(OpCodes.Ret);

			return getterMethod.CreateDelegate<T>();
		}
		public static void LoadReflections(Type type) {
			foreach (var item in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				string name = item.GetCustomAttribute<ReflectionMemberNameAttribute>()?.MemberName ?? item.Name;
				if (item.FieldType.IsAssignableTo(typeof(Delegate))) {
					Type parentType = item.GetCustomAttribute<ReflectionParentTypeAttribute>().ParentType;
					MethodInfo info = parentType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, item.GetCustomAttribute<ReflectionParameterTypesAttribute>()?.Types ?? Array.Empty<Type>());
					if (info.IsStatic) {
						item.SetValue(null, info.CreateDelegate(item.FieldType));
					} else {
						item.SetValue(null, info.CreateDelegate(item.FieldType, Activator.CreateInstance(parentType)));
					}
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