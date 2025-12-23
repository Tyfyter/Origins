using Microsoft.CodeAnalysis;
using Origins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Word_Snippet_Handler : ITagHandler {
		public static Dictionary<string, Func<string>> variableTags = new() {
			["LocalPlayerGender"] = () => Main.LocalPlayer.Male ? "Male" : "Female"
		};
		public class Word_Snippet : TextSnippet {
			readonly LocalizedText l10n;
			public Word_Snippet(string _tags, Color baseColor) : base(_tags, baseColor) {
				float bestTagCount = float.NegativeInfinity;
				List<WordTag> tags = WordTag.ParseList(_tags);
				LanguageTree tree = TextUtils.LanguageTree.Find($"Mods.Origins.Words.SelectByTag");
				LocalizedText[] options = tree.GetDescendants(false).Select(tree => tree.value).ToArray();
				//StringBuilder stringBuilder = new();
				for (int i = 0; i < options.Length; i++) {
					List<WordTag> optionTags = WordTag.ParseList(options[i].Key);
					float matchQuality = WordTag.GetMatch(tags, optionTags, out float match, out float mismatch);
					if (bestTagCount < matchQuality) {
						bestTagCount = matchQuality;
						l10n = options[i];
					}
					//stringBuilder.Append($"[{options[i].Value}:{matchQuality};{match}/{mismatch}]");
				}
				//Text = l10n.Value + ";" + stringBuilder.ToString();
				Text = l10n.Value;
			}
			public override void Update() {
				Text = l10n.Value;
			}
		}
		public readonly struct WordTag(string text, WordTag.TagImportance importance) {
			public readonly string Text { get; } = text;
			public readonly TagImportance Importance { get; } = importance;
			static readonly Regex tagRegex = new("({(\\w+)}|\\w+)(.*)");
			public static List<WordTag> ParseList(string text) {
				return text.Split("SelectByTag.")[^1].Split([';', '.'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Parse).ToList();
			}
			public static WordTag Parse(string text) {
				Match match = tagRegex.Match(text);
				string tagText;
				TagImportance importance = TagImportance.Normal;
				if (match.Groups[2].Success) tagText = variableTags[match.Groups[2].Value]();
				else tagText = match.Groups[1].Value;

				switch (match.Groups[3].Value) {
					case "":
					break;
					case "?":
					importance = TagImportance.Optional;
					break;
					case "!":
					importance = TagImportance.Required;
					break;
					default:
					throw new FormatException($"Invalid tag importance data \"{match.Groups[3].Value}\"");
				}
				return new(tagText, importance);
			}
			public static float GetMatch(IEnumerable<WordTag> a, IEnumerable<WordTag> b, out float match, out float mismatch) {
				match = 0;
				mismatch = 0;
				foreach (WordTag tag in a) {
					if (b.Contains(tag)) match += 1;
					else switch (tag.Importance) {
						case TagImportance.Required:
						mismatch = float.PositiveInfinity;
						break;

						case TagImportance.Optional:
						break;

						default:
						case TagImportance.Normal:
						mismatch += 1;
						break;
					}
				}
				foreach (WordTag tag in b) {
					if (a.Contains(tag)) match += 1;
					else switch (tag.Importance) {
						case TagImportance.Required:
						mismatch = float.PositiveInfinity;
						break;

						case TagImportance.Optional:
						break;

						default:
						case TagImportance.Normal:
						mismatch += 1;
						break;
					}
				}
				match /= 2;
				mismatch /= 2;
				return match - mismatch;
			}
			public readonly bool Equals(WordTag other) => Text == other.Text;
			public override readonly bool Equals(object obj) => obj is WordTag other && Equals(other);
			public override readonly int GetHashCode() => Text.GetHashCode();
			public static bool operator ==(WordTag left, WordTag right) => left.Equals(right);
			public static bool operator !=(WordTag left, WordTag right) => !left.Equals(right);
			public enum TagImportance {
				Normal,
				Required,
				Optional
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) baseColor = Color.White;
			else if (baseColor.A == Main.mouseTextColor) baseColor *= 255f / Main.mouseTextColor;
			return new Word_Snippet(text, baseColor);
		}
	}
}
