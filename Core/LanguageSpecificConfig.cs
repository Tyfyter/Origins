using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace Origins.Core {
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class LanguageSpecificAttribute(params IEnumerable<string> cultures) : Attribute, IMoveToPegFlag {
		static readonly Dictionary<string, string> languageRedirects;
		static LanguageSpecificAttribute() {
			languageRedirects = [];
			LanguageOverrideMod("tmodkor1449", GameCulture.CultureName.English, "ko-KR");
			static void LanguageOverrideMod(string modName, GameCulture.CultureName vanillaCulture, string modCulture) {
				if (ModLoader.HasMod(modName)) languageRedirects[GameCulture.FromCultureName(vanillaCulture).Name] = modCulture;
			}
		}
		readonly HashSet<string> showIn = cultures.ToHashSet();
		public LanguageSpecificAttribute(params GameCulture.CultureName[] cultures) : this(cultures.Select(GameCulture.FromCultureName).Select(culture => culture.Name)) { }
		public bool Hide => !showIn.Contains(languageRedirects.TryGetValue(Language.ActiveCulture.Name, out string newCulture) ? newCulture : Language.ActiveCulture.Name);
		class Loader : ILoadable {
			delegate Tuple<UIElement, UIElement> orig_WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1);
			delegate Tuple<UIElement, UIElement> hook_WrapIt(orig_WrapIt orig, UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1);
			void ILoadable.Load(Mod mod) {
				try {
					Type type = typeof(ModConfig).Assembly.GetType("Terraria.ModLoader.Config.UI.UIModConfig");
					MonoModHooks.Add(type.GetMethod("WrapIt"), (orig_WrapIt orig, UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1) => {
						if (memberInfo.MemberInfo.GetCustomAttribute<LanguageSpecificAttribute>()?.Hide ?? false) return null;
						return orig(parent, ref top, memberInfo, item, order, list, arrayType, index);
					});
				} catch (Exception e) {
					if (Origins.LogLoadingError("DetourBindFail", "LanguageSpecificConfig", e)) throw;
				}
			}
			void ILoadable.Unload() { }
		}
	}
}
