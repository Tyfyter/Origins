using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Lunatics_Rune : ModItem {
		public static int ChargeThreshold => 3 * 60;
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationSwitching(OriginsModIntegrations.CheckAprilFools, NoDrawAnimation.AtAll, new DrawAnimationRandom(3, 20)));
			AprilFoolsTextures.AddItem(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.mana = 120;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) { }
		public bool CheckMana(Player player) {
			player.manaRegenDelay = player.maxRegenDelay;
			float reduce = player.manaCost;
			float mult = 1;

			CombinedHooks.ModifyManaCost(player, Item, ref reduce, ref mult);
			int mana = Main.rand.RandomRound(Item.mana * reduce * mult / ChargeThreshold);

			if (player.statMana < mana) {
				CombinedHooks.OnMissingMana(player, Item, mana);
				if (player.statMana < mana && player.manaFlower)
					player.QuickMana();
			}

			if (player.statMana < mana) return false;
			CombinedHooks.OnConsumeMana(player, Item, mana);
			player.statMana -= mana;
			return true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (player.whoAmI != Main.myPlayer) return;
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.lunaticsRune = true;
			ref int charge = ref originPlayer.lunaticsRuneCharge;
			if (Keybindings.LunaticsRune.Current && (charge >= ChargeThreshold || CheckMana(player))) {
				originPlayer.lunaticsRuneRotation += 0.02f;
				charge.Warmup(ChargeThreshold);
				float moveMult = 1 - float.Pow(charge / (float)ChargeThreshold, 2);
				player.velocity *= moveMult * moveMult;
				player.gravity *= moveMult;
				originPlayer.moveSpeedMult *= moveMult * moveMult;
				if (player.velocity.Y == 0) player.velocity.Y = float.Epsilon;
			} else {
				if (charge >= ChargeThreshold) {
					switch (Main.rand.Next(4)) {
						default:
						player.AddBuff(BuffID.RapidHealing, 8 * 60);
						break;
					}
				}
				charge = 0;
			}
		}
	}
}
