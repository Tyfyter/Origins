using Microsoft.Xna.Framework.Input;
using PegasusLib;
using System;
using System.Reflection;
using Terraria.ModLoader;

namespace Origins {
	public class Keybindings : ILoadable {
		[Keybind("Trigger Set Bonus", "Q")]
		public static ModKeybind TriggerSetBonus { get; private set; }
		[Keybind(Keys.J)]
		public static ModKeybind UseMojoFlask { get; private set; }
		[Keybind(Keys.Up)]
		public static ModKeybind JournalBack { get; private set; }
		[Keybind("Forbidden Voice", "F")]
		public static ModKeybind ForbiddenVoice { get; private set; }
		[Keybind(Keys.H)]
		public static ModKeybind GoldenLotus { get; private set; }
		[Keybind("Mouse2")]
		public static ModKeybind StressBall { get; private set; }
		[Keybind(Keys.K)]
		public static ModKeybind RetoolArm { get; private set; }
		[Keybind(Keys.V)]
		public static ModKeybind SmogPod { get; private set; }
		public static ModKeybind WishingGlass => ModContent.GetInstance<SyncedKeybinds>().WishingGlass.keybind;
		public static ModKeybind LunaticsRune => ModContent.GetInstance<SyncedKeybinds>().LunaticsRune.keybind;
		[Keybind(Keys.B)]
		public static ModKeybind MultiBucket { get; private set; }
		[Keybind(Keys.Q)]
		public static ModKeybind StarSoldierLockOn { get; private set; }

#if DEBUG
		[Keybind("Debug Screen Shader", Keys.OemQuotes)]
		public static ModKeybind DebugScreenShader { get; private set; }
#endif
		#region animation editor
		[Keybind(null, "LeftControl+Space", DebugType.Animator)]
		public static ModKeybind PlayPauseAnimation { get; private set; }
		[Keybind(null, Keys.K, DebugType.Animator)]
		public static ModKeybind InsertKeyframe { get; private set; }
		[Keybind(null, Keys.Delete, DebugType.Animator)]
		public static ModKeybind DeleteKeyframe { get; private set; }
		[Keybind(null, Keys.G, DebugType.Animator)]
		public static ModKeybind GrabGizmo { get; private set; }
		[Keybind(null, Keys.R, DebugType.Animator)]
		public static ModKeybind RotateGizmo { get; private set; }
		[Keybind(null, Keys.S, DebugType.Animator)]
		public static ModKeybind ScaleGizmo { get; private set; }
		[Keybind(null, Keys.Escape, DebugType.Animator)]
		public static ModKeybind CancelGizmo { get; private set; }
		[Keybind(null, "LeftControl+Z", DebugType.Animator)]
		public static ModKeybind EditUndo { get; private set; }
		[Keybind(null, "LeftControl+Y", DebugType.Animator)]
		public static ModKeybind EditRedo { get; private set; }
		[Keybind(null, "LeftControl+C", DebugType.Animator)]
		public static ModKeybind Copy { get; private set; }
		[Keybind(null, "LeftControl+X", DebugType.Animator)]
		public static ModKeybind Cut { get; private set; }
		[Keybind(null, "LeftControl+V", DebugType.Animator)]
		public static ModKeybind Paste { get; private set; }
		[Keybind(null, "LeftControl+LeftShift+Delete", DebugType.Animator)]
		public static ModKeybind DeleteAllKeyframes { get; private set; }
		#endregion animation editor
		public void Load(Mod mod) {
			Type type = typeof(ModKeybind);
			foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Static)) {
				if (field.FieldType == type && field.GetCustomAttribute<KeybindAttribute>() is KeybindAttribute data) {
					field.SetValue(null, KeybindLoader.RegisterKeybind(mod, data.Name ?? field.Name, data.DefaultBinding));
				}
			}
			foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Static)) {
				if (property.PropertyType == type && property.GetCustomAttribute<KeybindAttribute>() is KeybindAttribute data) {
					property.SetValue(null, KeybindLoader.RegisterKeybind(mod, data.Name ?? property.Name, data.DefaultBinding));
				}
			}
		}
		public void Unload() {
			Type type = typeof(ModKeybind);
			foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Static)) {
				if (field.FieldType == type && (field.GetCustomAttribute<KeybindAttribute>()?.ShouldRegister ?? false)) {
					field.SetValue(null, null);
				}
			}
			foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Static)) {
				if (property.PropertyType == type && (property.GetCustomAttribute<KeybindAttribute>()?.ShouldRegister ?? false)) {
					property.SetValue(null, null);
				}
			}
		}
		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
		protected sealed class KeybindAttribute(string name, string defaultBinding, DebugType debugType = DebugType.None) : Attribute {
			public KeybindAttribute(string defaultBinding = "None") : this(null, defaultBinding) { }
			public KeybindAttribute(string name, Keys key, DebugType debugType = DebugType.None) : this(name, key.ToString(), debugType) { }
			public KeybindAttribute(Keys defaultKey) : this(null, defaultKey) {}
			public string Name { get; } = name;
			public string DefaultBinding { get; } = defaultBinding;
			public DebugType DebugType { get; } = debugType;
			public bool ShouldRegister => DebugType switch {
				DebugType.Animator => DebugConfig.Instance.AnimatorMode,
				_ => true
			};
		}
		protected enum DebugType {
			None,
			Animator
		}
	}
	public class SyncedKeybinds : KeybindHandlerPlayer {
		[Keybind(Keys.V)]
		public AutoKeybind WishingGlass;
		[Keybind(Keys.G)]
		public AutoKeybind LunaticsRune;
	}
}
