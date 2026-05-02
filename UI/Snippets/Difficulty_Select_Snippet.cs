using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Difficulty_Select_Handler : AdvancedTextSnippetHandler<Difficulty_Select_Handler.Nothing> {
		public struct Nothing;
		public override IEnumerable<string> Names => ["diffselect"];
		public override IEnumerable<SnippetOption> GetOptions() => [];
		public class Difficulty_Select_Snippet(string[] strings, Color color = default) : WrappingTextSnippet(GetText(strings), color) {
			public override void Update() => Text = GetText(strings);
			static string GetText(string[] strings) {
				switch (strings.Length) {
					case 4:
					if (Main.LocalPlayer.difficulty == PlayerDifficultyID.Creative) return strings[3];
					goto case 3;
					case 3:
					if (Main.masterMode) return strings[2];
					goto case 2;
					case 2:
					return Main.expertMode ? strings[1] : strings[0];
					case 0:
					return strings[0];
					default:
					return string.Empty;
				}
			}
		}
		public override TextSnippet Parse(string text, Color baseColor, Nothing options) {
			return new Difficulty_Select_Snippet(text.Split('|'), baseColor);
		}
	}
}
