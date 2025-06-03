using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Items.Other.Consumables {
	public class Mojo_Flask : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Potion"
		];
		public override LocalizedText Tooltip => Language.GetOrRegister(Mod.GetLocalizationKey($"{LocalizationCategory}.{nameof(Mojo_Flask)}.{nameof(Tooltip)}"));
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Item.type, new DrawAnimationDelegated(GetFrameGetter(Type, FlaskUseCount)));
		}
		public virtual int CooldownTime => 5 * 60;
		public virtual int FlaskUseCount => 5;
		public static Func<Texture2D, Rectangle> GetFrameGetter(int type, int chargeCount) => (Texture2D texture) => {
			int frame = 5;
			if (!Main.gameMenu) {
				frame = chargeCount - Main.CurrentPlayer.OriginPlayer().mojoFlaskChargesUsed;
				if (Main.CurrentPlayer.ItemAnimationActive && Main.CurrentPlayer.HeldItem.type == type) {
					frame++;
				}
			}
			return texture.Frame(chargeCount + 1, 1, Math.Clamp(frame, 0, chargeCount), 0, -1);
		};
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
				switch (tooltips[i].Name) {
					case "BuffTime":
					tooltips[i] = new TooltipLine(
						Mod,
						"Cooldown",
						OriginExtensions.GetCooldownText(CooldownTime)
					);
					break;
				}
				tooltips[i].Text = tooltips[i].Text.Replace("{FlaskCureAmount}", Item.buffTime.ToString()).Replace("{FlaskUseCount}", FlaskUseCount.ToString());
			}
			if (OriginsModIntegrations.GoToKeybindKeybindPressed) {
				OriginsModIntegrations.GoToKeybind(Keybindings.UseMojoFlask);
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
			if (player.HasBuff(ModContent.BuffType<Mojo_Flask_Cooldown>())) return false;
			return player.OriginPlayer().mojoFlaskChargesUsed < FlaskUseCount;
		}
		public override bool? UseItem(Player player) {
			player.OriginPlayer().mojoFlaskChargesUsed++;
			player.AddBuff(ModContent.BuffType<Mojo_Flask_Cooldown>(), CooldownTime);
			player.AddBuff(Purifying_Buff.ID, Item.buffTime);
			return true;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (Main.gameMenu) return;
			float inventoryScale = Main.inventoryScale;
			int buffIndex = Main.LocalPlayer.FindBuffIndex(Item.buffType);
			if (buffIndex >= 0) {
				Color color3 = drawColor * (Main.LocalPlayer.buffTime[buffIndex] / (float)Item.buffTime);
				spriteBatch.Draw(TextureAssets.Cd.Value, position, null, color3, 0f, TextureAssets.Cd.Value.Size() * 0.5f, scale, SpriteEffects.None, 0f);
			}
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				Math.Max(FlaskUseCount - Main.LocalPlayer.OriginPlayer().mojoFlaskChargesUsed, 0).ToString(),
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
