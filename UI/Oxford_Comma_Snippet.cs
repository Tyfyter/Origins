using Microsoft.CodeAnalysis;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Localization;
using Terraria.UI.Chat;
using static Origins.UI.Word_Snippet_Handler;

namespace Origins.UI {
	public class Oxford_Comma_Handler : ITagHandler {
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) => new Oxford_Comma_Snippet(text, baseColor);
	}
	public class Not_Oxford_Comma_Handler : ITagHandler {
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) => new Oxford_Comma_Snippet(text, baseColor, true);
	}
#pragma warning disable CS9107
	public class Oxford_Comma_Snippet(string text, Color baseColor, bool inverted = false) : TextSnippet(text, baseColor) {
#pragma warning restore CS9107
		public override void Update() {
			if (OriginClientConfig.Instance.OxfordComma == inverted) {
				Text = "";
			} else {
				Text = text;
			}
		}
	}
}
