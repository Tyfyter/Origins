using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod;
public interface IModIntegration : IAutoload<IModIntegration.Loader> {
	public static abstract IEnumerable<string> RequiredMods { get; }
	public static virtual void OnLoad() { }
	public static virtual void LateLoad() { }
	public static virtual void Cleanup() { }
	public static virtual void Recommend() { }
	class Loader : IAutoloader {
		#region Events
		public static Event LateLoad;
		#endregion
		#region handling
		static readonly MethodInfo load = typeof(Loader).GetMethod("Load");
		static void IAutoloader.Autoload(Mod mod, Type type) => load.MakeGenericMethod(type).Invoke(null, []);
		public static void Load<T>() where T : IModIntegration {
			foreach (string mod in T.RequiredMods) {
				if (!ModLoader.HasMod(mod)) {
					T.Recommend();
					return;
				}
			}
			T.OnLoad();
			LateLoad.Add<T>(T.LateLoad);
			ModContent.GetInstance<Origins>().Logger.Info($"Loaded {typeof(T).Name} integrations");
		}
		static bool IsImplemented(Delegate method) => method.Method.DeclaringType != typeof(IModIntegration);
		static Loader() {
			foreach (FieldInfo item in typeof(Loader).GetFields()) {
				if (item.FieldType == typeof(Event)) item.SetValue(null, new Event());
			}
		}
		public class Event {
			Action action;
			Dictionary<Type, Action> associations = [];
			public void Do() {
				associations = null;
				action?.Invoke();
				action = null;
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0039:Use local function", Justification = "<Pending>")]
			public void Add<T>(Action action) where T : IModIntegration {
				if (!IsImplemented(action)) return;
				Action toAdd = () => {
					try {
						action();
					} catch (Exception) {
						T.Cleanup();
						LateLoad.Error<T>();
						throw;
					}
				};
				associations.Add(typeof(T), toAdd);
				this.action += toAdd;
			}
			public void Error<T>() where T : IModIntegration {
				if (associations?.TryGetValue(typeof(T), out Action action) ?? false) this.action -= action;
			}
		}
		#endregion
	}
	internal static void DoLateLoad() => Loader.LateLoad.Do();
}
