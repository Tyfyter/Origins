using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Journal_Control_Handler : ITagHandler {
		public class Journal_Control_Snippet : TextSnippet {
			public Journal_Control_Snippet(string text, Color color = default) : base(text) {
				Color = color;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			return new Journal_Control_Snippet(text, baseColor);
		}
	}
}
