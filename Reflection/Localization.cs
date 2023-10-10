using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Reflection {
	public class LocalizationMethods : ILoadable {
		public static FastFieldInfo<LanguageManager, Dictionary<string, LocalizedText>> _localizedTexts;
		public static FastFieldInfo<LocalizedText, string> _value;
		public static FastFieldInfo<LocalizedText, bool?> _hasPlurals;
		public static FastFieldInfo<LocalizedText, object[]> BoundArgs;
		public void Load(Mod mod) {
			_localizedTexts = new(nameof(_localizedTexts), BindingFlags.NonPublic);
			_value = new(nameof(_value), BindingFlags.NonPublic);
			_hasPlurals = new(nameof(_hasPlurals), BindingFlags.NonPublic);
			BoundArgs = new($"<{nameof(BoundArgs)}>k__BackingField", BindingFlags.NonPublic);
		}
		public void Unload() {
			_localizedTexts = null;
			_value = null;
			_hasPlurals = null;
			BoundArgs = null;
		}
		public static void BindArgs(LocalizedText text, params object[] args) {
			BoundArgs.SetValue(text, args);
		}
	}
}