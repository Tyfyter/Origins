using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Items.Other.Consumables {
	public class Mojo_Flask : ModItem {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mojo Flask");
			Tooltip.SetDefault("");
			Main.RegisterItemAnimation(Item.type, new DrawAnimationDelegated(GetFrame));
			SacrificeTotal = 1;
			ID = Type;
		}
		public static Rectangle GetFrame(Texture2D texture) {
			const int frameCount = 5;
			int frame = Main.LocalPlayer.GetModPlayer<OriginPlayer>().mojoFlaskCount;
			if (Main.LocalPlayer.ItemAnimationActive && Main.LocalPlayer.HeldItem.type == ID) {
				frame++;
			}
			return texture.Frame(frameCount, 1, Math.Min(frame, frameCount - 1), 0);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.HealingPotion);
			Item.maxStack = 1;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.healLife = 0;
			Item.value = Item.buyPrice(gold: 1);
			Item.consumable = false;
		}
		public override bool CanUseItem(Player player) {
			if (Main.LocalPlayer.GetModPlayer<OriginPlayer>().mojoFlaskCount > 0) {
				player.potionDelayTime = 5 * 60;
				return true;
			}
			return false;
		}
		public override bool? UseItem(Player player) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			originPlayer.mojoFlaskCount--;
			originPlayer.CorruptionAssimilation -= Math.Min(0.2f, originPlayer.CorruptionAssimilation);
			originPlayer.CrimsonAssimilation -= Math.Min(0.2f, originPlayer.CrimsonAssimilation);
			originPlayer.DefiledAssimilation -= Math.Min(0.2f, originPlayer.DefiledAssimilation);
			originPlayer.RivenAssimilation -= Math.Min(0.2f, originPlayer.RivenAssimilation);
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			float inventoryScale = Main.inventoryScale;
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				Main.LocalPlayer.GetModPlayer<OriginPlayer>().mojoFlaskCount.ToString(),
				(position - (new Vector2(52) * inventoryScale) / 2f + frame.Size() * scale / 2f) + new Vector2(10f, 26f) * inventoryScale,
				drawColor,
				0f,
				Vector2.Zero,
				new Vector2(inventoryScale),
				-1f,
				inventoryScale
			);
		}
	}
}
