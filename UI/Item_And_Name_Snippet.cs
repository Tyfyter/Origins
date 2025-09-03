using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Item_And_Name_Handler : ITagHandler {
		internal static Vector2 origin;
		public class Item_And_Name_Snippet : TextSnippet {
			private Item item;
			private readonly bool locked;

			public Item_And_Name_Snippet(Item item, Color color, bool unlocked) : base(item.Name, color) {
				this.item = item;
				locked = !unlocked;
				CheckForHover = true;
			}

			public override void OnHover() {
				if (item is null) return;
				Main.HoverItem = item;
				Main.instance.MouseText(item.Name, item.rare, 0);
			}

			public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1f) {
				if (item is null) {
					size = default;
					return false;
				}
				if (!NetmodeActive.Server) {
					Main.instance.LoadItem(item.type);
					Main.itemAnimations[item.type]?.GetFrame(TextureAssets.Item[item.type].Value);
				}
				float num = scale * 0.75f;
				if (!justCheckingString) {
					float inventoryScale = Main.inventoryScale;
					Main.inventoryScale = num;
					ItemSlot.Draw(spriteBatch, ref item, 14, position - new Vector2(10f) * num, Color.Lerp(Color.White, Color.Black, locked.ToInt()));
					Main.inventoryScale = inventoryScale;
					spriteBatch.DrawString(FontAssets.MouseText.Value, item.Name, position + new Vector2(32f, 0) * num, Color.Lerp(Color.MultiplyRGBA(color), Color.Gray * 0.6f, locked.Mul(0.5f)));
				}
				size = new Vector2(32f) * num + FontAssets.MouseText.Value.MeasureString(item.Name) * Vector2.UnitX;
				return true;
			}

			public override float GetStringLength(DynamicSpriteFont font) {
				return (32f * 0.65f + font.MeasureString(item.Name).X) * Scale;
			}
		}
		public record struct Options(float Speed = 1f / 60f, float WiggleWidth = 16, float WiggleScale = 2);
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			Item item = new();
			if (int.TryParse(text, out int result) && result < ItemLoader.ItemCount) {
				item.netDefaults(result);
			} else if (ItemID.Search.TryGetId(text, out result)) {
				item.netDefaults(result);
			}
			bool unlocked = false;
			SnippetHelper.ParseOptions(options,
				SnippetOption.CreateFlagOption("u", () => unlocked = true),
				SnippetOption.CreateStringOption("text", text => {
					item.GetGlobalItem<AddConditionsTextGlobalItem>().texts = text.Split(',').Select(t => {
						string[] info = t.Split(";");
						return Language.GetText(info[0]).WithFormatArgs(info[1..]);
					}).ToArray();
				}, '/')
			);
			return new Item_And_Name_Snippet(item, baseColor, unlocked);
		}
		class AddConditionsTextGlobalItem : GlobalItem {
			public override string Name => $"{nameof(Item_And_Name_Snippet)}_{base.Name}";
			public override bool InstancePerEntity => true;
			public LocalizedText[] texts = null;
			public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
				if (texts is null) return;
				if (!ItemSlot.ShiftInUse) tooltips.RemoveRange(1, tooltips.Count - 1);
				for (int i = 0; i < texts.Length; i++) {
					tooltips.Add(new(Mod, "Condition" + i, texts[i].Value));
				}
			}
		}
	}
}
