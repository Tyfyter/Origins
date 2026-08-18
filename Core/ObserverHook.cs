#if false // can't be done because hooks need a declaring type and dynamic methods don't have declaring types
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Terraria.ModLoader;

namespace Origins.Core; 
internal static class ObserverHook {
	/*public static void Add<TOrig, THook>(Delegate createObserver, int argIndex, params Span<int> extraArgIndex) where TOrig : Delegate where THook : Delegate {
		if (typeof(TOrig).DeclaringType != typeof(THook).DeclaringType) throw new Exception("TOrig and THook must be from the same class");
		if (!typeof(TOrig).Name.StartsWith("orig_")) throw new Exception("TOrig and THook must use hookgen naming convention for automatic ");
		MethodInfo add_Hook = typeof(TOrig).DeclaringType.GetEvent()?.GetAddMethod();
	}*/
	public static void Add<TOrig, THook>(MethodInfo method, Delegate createObserver, int argIndex, params Span<int> extraArgIndex) where TOrig : Delegate where THook : Delegate {
		if (createObserver.Target is not null) throw new ArgumentException($"Observer hooks must be static", nameof(createObserver));
		if (createObserver.Method.GetParameters().Length != 1 + extraArgIndex.Length) throw new ArgumentException($"Observer hooks must have exactly the amount of parameters provided", nameof(createObserver));
		if (createObserver.Method.ReturnType.GetMethod("Dispose", []) is not MethodInfo dispose) throw new ArgumentException($"Observer hooks must return a disposable", nameof(createObserver));
		Type[] methodParams = Enumerable.Repeat(method.DeclaringType, method.IsStatic ? 0 : 1).Concat(method.GetParameters().Select(p => p.ParameterType)).ToArray();
		ParameterInfo[] observerParams = createObserver.Method.GetParameters();
		if (observerParams[0].ParameterType != methodParams[argIndex]) {
			throw new Exception($"Observer hook argument (0:{observerParams[0]}) does not match type of parameter {argIndex} {methodParams[argIndex]}");
		}
		for (int i = 0; i < extraArgIndex.Length; i++) {
			if (observerParams[i + 1].ParameterType != methodParams[argIndex]) {
				throw new Exception($"Observer hook argument ({i}:{observerParams[i + 1]}) does not match type of parameter {extraArgIndex[i]} {methodParams[extraArgIndex[i]]}");
			}
		}
		argIndex++;
		for (int i = 0; i < extraArgIndex.Length; i++) extraArgIndex[i]++;
		DynamicMethod dmd = new($"Observer Hook: {method.Name}", method.ReturnType, [typeof(TOrig), ..methodParams]);
		ILGenerator gen = dmd.GetILGenerator();
		gen.DeclareLocal(createObserver.Method.ReturnType);

		gen.Emit(OpCodes.Ldarg, argIndex);
		for (int i = 0; i < extraArgIndex.Length; i++) gen.Emit(OpCodes.Ldarg, extraArgIndex[i]);
		gen.Emit(OpCodes.Call, createObserver.Method);
		gen.Emit(OpCodes.Stloc_0);

		Label label = gen.BeginExceptionBlock();
		gen.Emit(OpCodes.Ldarg_0);
		for (int i = 0; i < methodParams.Length; i++) gen.Emit(OpCodes.Ldarg, i + 1);
		gen.Emit(OpCodes.Callvirt, typeof(TOrig).GetMethod("Invoke", methodParams));
		gen.Emit(OpCodes.Leave, label);

		gen.BeginFinallyBlock();
		if (createObserver.Method.ReturnType.IsValueType) {
			gen.Emit(OpCodes.Ldloca_S, 0);
		} else {
			gen.Emit(OpCodes.Ldloc_0);
		}
		gen.Emit(OpCodes.Call, dispose);
		gen.Emit(OpCodes.Endfinally);
		gen.EndExceptionBlock();
		gen.Emit(OpCodes.Ret);
		MonoModHooks.Add(method, dmd.CreateDelegate<THook>());
	}
}
#endif