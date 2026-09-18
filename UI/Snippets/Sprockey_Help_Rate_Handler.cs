using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Mounts.Star_Soldier;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Sprockey_Help_Rate_Handler : AdvancedTextSnippetHandler<Sprockey_Help_Rate_Handler.Options> {
		public enum Options {
			NotSelectedYet,
			Never,
			Rarely,
			Frequently,
			Selected
		}
		public override IEnumerable<string> Names => ["sprockeyhr"];
		
		public class Sprockey_Help_Rate_Snippet : TextSnippet {
			LocalizedText text;
			readonly LocalizedText tooltip;
			readonly Options options;
			public Sprockey_Help_Rate_Snippet(Options options, Color color = default) : base(default) {
				this.options = options;
				text = Language.GetText("Mods.Origins.Journal.Wire_Tutorial.HelpRate." + options);
				Text = text.Value;
				TextOriginal = text.Value;
				CheckForHover = true;
				switch (options) {
					case Options.Frequently:
					color = Color.Lerp(color, Color.Gray, 0.5f);
					tooltip = Language.GetText("Mods.Origins.Journal.Wire_Tutorial.HelpRate.UnavailableTooltip");
					break;
				}
				Color = color;
			}
			public override void Update() {
				switch (options) {
					case Options.Selected:
					text = Language.GetText("Mods.Origins.Journal.Wire_Tutorial.HelpRate." + Main.LocalPlayer.OriginPlayer().sprockeyHelpRate);
					break;
				}
				Text = text.Value;
			}
			public override void OnHover() {
				if (tooltip is not null) UICommon.TooltipMouseText(tooltip.Value);
			}
			public override void OnClick() {
				switch (options) {
					case Options.Frequently:
					case Options.Selected:
					case Options.NotSelectedYet:
					break;
					
					default:
					ref Options selectedMode = ref Main.LocalPlayer.OriginPlayer().sprockeyHelpRate;
					if (selectedMode == Options.NotSelectedYet) Journal_UI_Open.shouldRefresh = true;
					selectedMode = options;
					break;
				}
			}
			struct UpdateFlag : IBroken {
				public static string BrokenReason => "Use WrappingTextSnippet.Font";
			}
			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
				size = Star_Soldier.Font.MeasureString(Text);
				return justCheckingString;
			}
		}

		public override IEnumerable<SnippetOption> GetOptions() {
			yield break;
		}

		public override TextSnippet Parse(string text, Color baseColor, Options options) {
			if (!Enum.TryParse(text, true, out Options opt)) return new TextSnippet(text, baseColor);
			return new Sprockey_Help_Rate_Snippet(opt, baseColor);
		}
	}
}
