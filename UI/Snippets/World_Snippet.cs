using AltLibrary.Common.Systems;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class World_Handler : AdvancedTextSnippetHandler<World_Handler.Options> {
		public enum Options {
			Name,
			Evil,
			Hallow,
			Jungle,
			Hell,
			Size,
		}
		public override IEnumerable<string> Names => ["world"];

		public class World_Snippet : TextSnippet {
			public World_Snippet(string text, Color color = default) : base(text) {
				Color = color;
				Text = text;
			}
		}

		public override IEnumerable<SnippetOption> GetOptions() {
			yield break;
		}

		public override TextSnippet Parse(string text, Color baseColor, Options options) {
			if (!Enum.TryParse(text, true, out Options opt)) return new TextSnippet(text, baseColor);
			switch (opt) {
				case Options.Name: text = Main.worldName;
				break;

				case Options.Evil: text = WorldBiomeManager.GetWorldEvil(true).DisplayName.Value;
				break;

				case Options.Hallow: text = WorldBiomeManager.GetWorldHallow(true).DisplayName.Value;
				break;

				case Options.Jungle: text = WorldBiomeManager.GetWorldJungle(true).DisplayName.Value;
				break;

				case Options.Hell: text = WorldBiomeManager.GetWorldHell(true).DisplayName.Value;
				break;

				case Options.Size: {
					const string key = "Mods.Origins.World_Sizes.";
					text = Language.GetOrRegister($"{key}{Main.maxTilesX}x{Main.maxTilesY}", () => Language.GetTextValue(key + "Custom", Main.maxTilesX, Main.maxTilesY)).Value;
					break;
				}
			}
			return new World_Snippet(text, baseColor);
		}
	}
}
