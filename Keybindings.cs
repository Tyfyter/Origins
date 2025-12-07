using Microsoft.Xna.Framework.Input;
using PegasusLib;
using System;
using System.Reflection;
using Terraria.ModLoader;

namespace Origins {
	public class Keybindings : ILoadable {
		[Keybind("Trigger Set Bonus", "Q")]
		public static ModKeybind TriggerSetBonus { get; private set; }
		[Keybind("Forbidden Voice", "F")]
		public static ModKeybind ForbiddenVoice { get; private set; }
		[Keybind(Keys.H)]
		public static ModKeybind GoldenLotus { get; private set; }
		[Keybind(Keys.J)]
		public static ModKeybind UseMojoFlask { get; private set; }
		/*[Keybind("mouse3")]
		public static ModKeybind OpenJournal { get; private set; }*/
		[Keybind(Keys.Up)]
		public static ModKeybind JournalBack { get; private set; }
		[Keybind("Mouse2")]
		public static ModKeybind StressBall { get; private set; }
		public static ModKeybind WishingGlass => ModContent.GetInstance<SyncedKeybinds>().WishingGlass.keybind;
#if DEBUG
		[Keybind("Debug Screen Shader", Keys.OemQuotes)]
		public static ModKeybind DebugScreenShader { get; private set; }
#endif
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
				if (field.FieldType == type && field.GetCustomAttribute<KeybindAttribute>() is not null) {
					field.SetValue(null, null);
				}
			}
			foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Static)) {
				if (property.PropertyType == type && property.GetCustomAttribute<KeybindAttribute>() is not null) {
					property.SetValue(null, null);
				}
			}
		}
		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
		protected sealed class KeybindAttribute(string name, string defaultBinding) : Attribute {
			public KeybindAttribute(string defaultBinding = "None") : this(null, defaultBinding) { }
			public KeybindAttribute(string name, Keys key) : this(name, key.ToString()) { }
			public KeybindAttribute(Keys defaultKey) : this(null, defaultKey) {}
			public string Name { get; } = name;
			public string DefaultBinding { get; } = defaultBinding;
		}
	}
	public class SyncedKeybinds : KeybindHandlerPlayer {
		[Keybind(Keys.V)]
		public AutoKeybind WishingGlass;
	}
}
