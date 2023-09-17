using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace Origins.Reflection {
	public class UIModConfig : ILoadable {
		public delegate void _SwitchToSubConfig(UIPanel panel);
		public delegate UIPanel _MakeSeparateListPanel(object item, object subitem, PropertyFieldWrapper memberInfo, IList array, int index, Func<string> AbridgedTextDisplayFunction);
		public static _SwitchToSubConfig SwitchToSubConfig { get; private set; }
		public static _MakeSeparateListPanel MakeSeparateListPanel { get; private set; }
		public void Load(Mod mod) {
			Type type = typeof(ModConfig).Assembly.GetType("Terraria.ModLoader.Config.UI.UIModConfig");
			SwitchToSubConfig = type.GetMethod(nameof(SwitchToSubConfig), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<_SwitchToSubConfig>();
			MakeSeparateListPanel = type.GetMethod(nameof(MakeSeparateListPanel), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate<_MakeSeparateListPanel>();
		}
		public void Unload() {
			SwitchToSubConfig = null;
			MakeSeparateListPanel = null;
		}
	}
}