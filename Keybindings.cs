using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins {
	public class Keybindings : ILoadable {
		[Keybind("Trigger Set Bonus", "Q")]
		public static ModKeybind TriggerSetBonus { get; private set; }
		[Keybind("Inspect Item", "Mouse3")]
		public static ModKeybind InspectItem { get; private set; }
		public void Load(Mod mod) {
			Type type = typeof(ModKeybind);
			foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Static)) {
				if (field.FieldType == type && field.GetCustomAttribute<KeybindAttribute>() is KeybindAttribute data) {
					field.SetValue(null, KeybindLoader.RegisterKeybind(mod, data.Name, data.DefaultBinding));
				}
			}
			foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Static)) {
				if (property.PropertyType == type && property.GetCustomAttribute<KeybindAttribute>() is KeybindAttribute data) {
					property.SetValue(null, KeybindLoader.RegisterKeybind(mod, data.Name, data.DefaultBinding));
				}
			}
		}
		public void Unload() {
			Type type = typeof(ModKeybind);
			foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Static)) {
				if (field.FieldType == type && field.GetCustomAttribute<KeybindAttribute>() is KeybindAttribute data) {
					field.SetValue(null, null);
				}
			}
			foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Static)) {
				if (property.PropertyType == type && property.GetCustomAttribute<KeybindAttribute>() is KeybindAttribute data) {
					property.SetValue(null, null);
				}
			}
		}
		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
		protected sealed class KeybindAttribute(string name, string defaultBinding = "None") : Attribute {
			public string Name { get; } = name;
			public string DefaultBinding { get; } = defaultBinding;
		}
	}
}
