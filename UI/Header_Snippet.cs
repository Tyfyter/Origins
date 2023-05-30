using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Header_Snippet_Handler : ITagHandler {
		public class Header_Snippet : TextSnippet {
			public Header_Snippet(string text, Color color = default) : base(text, color, 1.1f) {}
		}
		public TextSnippet Parse(string text, Color baseColor = default(Color), string options = null) {
			return new Header_Snippet(text, baseColor);
		}
	}
}
