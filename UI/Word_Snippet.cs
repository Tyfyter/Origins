using Microsoft.CodeAnalysis;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Word_Snippet_Handler : ITagHandler {
		public static Dictionary<string, Func<string>> variableTags = new() {
			["LocalPlayerGender"] = () => Main.LocalPlayer.Male ? "Male" : "Female"
		};
		public class Word_Snippet : TextSnippet {
			readonly LocalizedText l10n;
			public Word_Snippet(string _tags) {
				int bestTagCount = int.MaxValue;
				HashSet<string> tags = _tags.Split(';').Select(tag => {
					if (tag[0] == '{' && tag[^1] == '}') {
						return variableTags[tag[1..^1]]();
					}
					return tag;
				}).ToHashSet();
				LanguageTree tree = TextUtils.LanguageTree.Find($"Mods.Origins.Words.SelectByTag");
				LocalizedText[] options = tree.Children;
				for (int i = 0; i < options.Length; i++) {
					HashSet<string> optionTag = options[i].Key.Split(';').ToHashSet();

					int mismatch = Math.Abs(optionTag.Union(tags).Count() - tags.Count);
					if (bestTagCount > mismatch) {
						bestTagCount = mismatch;
						l10n = options[i];
					}
				}
				Text = l10n.Value;
			}
			public override void Update() {
				Text = l10n.Value;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) => new Word_Snippet(text);
	}
}
