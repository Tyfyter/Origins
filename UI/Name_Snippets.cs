using Origins.Items;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Player_Name_Handler : ITagHandler {
		public class Player_Name_Snippet : TextSnippet {
			public Player_Name_Snippet(Color color = default) : base() {
				Text = Main.LocalPlayer.name;
				Color = color;
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			return new Player_Name_Snippet(baseColor);
		}
	}
	public class NPC_Name_Handler : ITagHandler {
		public class NPC_Name_Snippet : TextSnippet {
			readonly int type;
			readonly bool isRealName = false;
			public NPC_Name_Snippet(int type, Color color = default) : base() {
				this.type = type;
				if (type == -1) {
					Text = "Invalid NPC type";
					return;
				}
				if (NPC.GetFirstNPCNameOrNull(type) is string name) {
					Text = name;
					isRealName = true;
				} else {
					Text = Language.GetOrRegister("Mods.Origins.Generic.Definite_Article").Format(Lang.GetNPCNameValue(type));
					isRealName = false;
				}
				CheckForHover = true;
				Color = color;
			}
			public override void OnHover() {
				Main.LocalPlayer.mouseInterface = true;
				if (isRealName) {
					UICommon.TooltipMouseText(Language.GetOrRegister("Mods.Origins.Generic.Definite_Article").Format(Lang.GetNPCNameValue(type)));
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			if ((int.TryParse(text, out int npcType) && npcType < NPCLoader.NPCCount) || NPCID.Search.TryGetId(text, out npcType)) {
				return new NPC_Name_Snippet(npcType, baseColor);
			}
			return new NPC_Name_Snippet(-1, baseColor);
		}
	}
	public class Item_Name_Handler : ITagHandler {
		public class Item_Name_Snippet : TextSnippet {
			readonly Item item;
			public Item_Name_Snippet(int type, Color color = default) : base() {
				if (type == -1) {
					Text = "Invalid Item type";
					return;
				}
				item = ContentSamples.ItemsByType[type];
				Text = Lang.GetItemNameValue(type);
				CheckForHover = true;
				Color = color;
			}
			public override void OnHover() {
				Main.LocalPlayer.mouseInterface = true;
				if (item is not null) {
					Main.hoverItemName = $"{item.Name} [i:{item.type}]";
					Main.HoverItem = item.Clone();
					Main.HoverItem.SetNameOverride(Main.hoverItemName);
					Main.instance.MouseText(Main.hoverItemName, item.rare, 0);
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			if ((int.TryParse(text, out int itemType) && itemType < ItemLoader.ItemCount) || ItemID.Search.TryGetId(text, out itemType)) {
				return new Item_Name_Snippet(itemType, baseColor);
			}
			return new Item_Name_Snippet(-1, baseColor);
		}
	}
	public class Item_Hint_Handler : ITagHandler {
		public class Item_Hint_Snippet : TextSnippet {
			readonly Item item;
			public Item_Hint_Snippet(string text, int type, Color color = default) : base() {
				Text = text;
				Color = color;
				if (type == -1) {
					Text = "Invalid Item type";
					return;
				}
				item = ContentSamples.ItemsByType[type];
				CheckForHover = true;
			}
			public override void OnHover() {
				Main.LocalPlayer.mouseInterface = true;
				if (item is not null) {
					Main.hoverItemName = $"{item.Name} [i:{item.type}]";
					Main.HoverItem = item.Clone();
					Main.HoverItem.SetNameOverride(Main.hoverItemName);
					Main.instance.MouseText(Main.hoverItemName, item.rare, 0);
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			if ((int.TryParse(options, out int itemType) && itemType < ItemLoader.ItemCount) || ItemID.Search.TryGetId(options, out itemType)) {
				return new Item_Hint_Snippet(text, itemType, baseColor);
			}
			return new Item_Hint_Snippet(text, -1, baseColor);
		}
	}
	public class Imperfect_Item_Name_Handler : ITagHandler {
		public class Imperfect_Item_Name_Snippet : TextSnippet {
			readonly Item item;
			public Imperfect_Item_Name_Snippet(int type, Color color = default) : base() {
				if (type == -1) {
					Text = "Invalid Item type";
					return;
				}
				item = ContentSamples.ItemsByType[type].Clone();
				item.Prefix(ModContent.PrefixType<Imperfect_Prefix>());
				Text = item.AffixName();
				CheckForHover = true;
				Color = color;
			}
			public override void OnHover() {
				Main.LocalPlayer.mouseInterface = true;
				if (item is not null) {
					Main.hoverItemName = $"{item.Name} [i:{item.type}]";
					Main.HoverItem = item.Clone();
					Main.HoverItem.SetNameOverride(Main.hoverItemName);
					Main.instance.MouseText(Main.hoverItemName, item.rare, 0);
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			if (baseColor == new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 255).MultiplyRGBA(Main.MouseTextColorReal)) {
				baseColor = Color.White;
			} else if (baseColor.A == Main.mouseTextColor) {
				baseColor *= 255f / Main.mouseTextColor;
			}
			if ((int.TryParse(text, out int itemType) && itemType < ItemLoader.ItemCount) || ItemID.Search.TryGetId(text, out itemType)) {
				return new Imperfect_Item_Name_Snippet(itemType, baseColor);
			}
			return new Imperfect_Item_Name_Snippet(-1, baseColor);
		}
	}
}
