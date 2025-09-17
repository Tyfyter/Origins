using System.Collections.Generic;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader;
using PegasusLib;
using System;
using System.Reflection.Emit;

namespace Origins.Reflection {
	public class LocalizationMethods : ILoadable {
		public static FastFieldInfo<LanguageManager, Dictionary<string, LocalizedText>> _localizedTexts;
		public static FastFieldInfo<LocalizedText, string> _value;
		public static FastFieldInfo<LocalizedText, bool?> _hasPlurals;
		public static FastFieldInfo<LocalizedText, object[]> BoundArgs;
		public static Func<string, LocalizedText> createLocalizedText;
		public void Load(Mod mod) {
			_localizedTexts = new(nameof(_localizedTexts), BindingFlags.NonPublic);
			_value = new(nameof(_value), BindingFlags.NonPublic);
			_hasPlurals = new(nameof(_hasPlurals), BindingFlags.NonPublic);
			BoundArgs = new($"<{nameof(BoundArgs)}>k__BackingField", BindingFlags.NonPublic);
			DynamicMethod getterMethod = new DynamicMethod($"{nameof(LocalizationMethods)}.{nameof(createLocalizedText)}", typeof(LocalizedText), [typeof(string)], true);
			ILGenerator gen = getterMethod.GetILGenerator();

			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Newobj, typeof(LocalizedText).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [typeof(string), typeof(string)]));
			gen.Emit(OpCodes.Ret);

			createLocalizedText = getterMethod.CreateDelegate<Func<string, LocalizedText>>();
		}
		public void Unload() {
			_localizedTexts = null;
			_value = null;
			_hasPlurals = null;
			BoundArgs = null;
			createLocalizedText = null;
		}
		public static void BindArgs(LocalizedText text, params object[] args) {
			BoundArgs.SetValue(text, args);
		}
	}
}