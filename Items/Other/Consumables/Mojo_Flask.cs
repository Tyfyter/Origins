using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Items.Other.Consumables {
    public class Mojo_Flask : ModItem {
		public const int cooldown_time = 5 * 60;
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Mojo Flask");
			// Tooltip.SetDefault("Mitigates all assimilation by 20% each for five uses");
			Main.RegisterItemAnimation(Item.type, new DrawAnimationDelegated(GetFrame));
			Item.ResearchUnlockCount = 1;
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
			Item.value = Item.sellPrice(silver: 40);
			Item.potion = false;
			Item.buffTime = 20;
			Item.consumable = false;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "BuffTime") {
					tooltips[i] = new TooltipLine(
						Mod,
						"Cooldown",
						OriginExtensions.GetCooldownText(cooldown_time)
					);
					break;
				}
			}
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			if (player.DpadRadial.SelectedBinding != -1) {
				player.releaseUseItem = true;
				player.controlUseItem = true;
				player.DpadRadial.ChangeSelection(-1);
			}
		}
		public override bool CanUseItem(Player player) {
			if (player.HasBuff(Item.buffType)) return false;
			if (player.GetModPlayer<OriginPlayer>().mojoFlaskCount > 0) {
				return true;
			}
			return false;
		}
		public override bool? UseItem(Player player) {
			player.GetModPlayer<OriginPlayer>().mojoFlaskCount--;
			player.AddBuff(ModContent.BuffType<Mojo_Flask_Cooldown>(), cooldown_time);
			player.AddBuff(Purifying_Buff.ID, Item.buffTime);
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			float inventoryScale = Main.inventoryScale;
			int buffIndex = Main.LocalPlayer.FindBuffIndex(Item.buffType);
			if (buffIndex >= 0) {
				Color color3 = drawColor * (Main.LocalPlayer.buffTime[buffIndex] / (float)Item.buffTime);
				spriteBatch.Draw(TextureAssets.Cd.Value, position, null, color3, 0f, TextureAssets.Cd.Value.Size() * 0.5f, scale, SpriteEffects.None, 0f);
			}
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				Main.LocalPlayer.GetModPlayer<OriginPlayer>().mojoFlaskCount.ToString(),
				position + origin * scale * new Vector2(0.75f, 0.4f),
				drawColor,
				0f,
				Vector2.Zero,
				new Vector2(inventoryScale),
				-1f,
				inventoryScale
			);
		}
	}
	public class Mojo_Flask_Cooldown : ModBuff {
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			BuffID.Sets.LongerExpertDebuff[Type] = false;
		}
	}
}
