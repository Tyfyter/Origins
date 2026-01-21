using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI.Snippets {
	public class Quest_Reward_Item_List_Handler : ITagHandler {
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
					if (!locked) color *= 0.666f;
					float inventoryScale = Main.inventoryScale;
					Main.inventoryScale = num;
					ItemSlot.Draw(spriteBatch, ref item, 14, position - new Vector2(10f) * num, Color.White * (1 - locked.Mul(0.334f)));
					Main.inventoryScale = inventoryScale;
					spriteBatch.DrawString(FontAssets.MouseText.Value, item.Name, position + new Vector2(32f, 0) * num, Color.MultiplyRGBA(color));
					if (!locked) ChatManager.DrawColorCodedString(spriteBatch, StrikethroughFont.Font, "☐" + item.Name, position, new Color(color.R, color.G, color.B, 255), 0, Vector2.Zero, new(scale));
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
			if (int.TryParse(text, out int result) && result < ItemLoader.ItemCount) item.netDefaults(result);
			else if (ItemID.Search.TryGetId(text, out result)) item.netDefaults(result);
			bool unlocked = false;
			options.ParseOptions(SnippetOption.CreateFlagOption("u", () => unlocked = true),
				SnippetOption.CreateStringOption("text", text => {
					item.GetGlobalItem<AddConditionsTextGlobalItem>().conditions = text.Split(',').TrySelect<string, Condition>(conditions.TryGetValue).ToArray();
				})
			);
			return new Item_And_Name_Snippet(item, baseColor, unlocked);
		}
		internal static Dictionary<string, Condition> conditions = [];
		class AddConditionsTextGlobalItem : GlobalItem {
			public override string Name => $"{nameof(Item_And_Name_Snippet)}_{base.Name}";
			public override bool InstancePerEntity => true;
			[CloneByReference] public Condition[] conditions = null;
			public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
				if (conditions is null) return;
				if (!ItemSlot.ShiftInUse) tooltips.RemoveRange(1, tooltips.Count - 1);
				for (int i = 0; i < conditions.Length; i++) tooltips.Add(new(Mod, "Condition" + i, (conditions[i].IsMet() ? "☑" : "☐") + conditions[i].Description));
			}
		}
	}
}
