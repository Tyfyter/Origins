using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Mysterious_Spray : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 26);
			Item.rare = ItemRarityID.Master;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 6);
		}
		public static void EquippedEffect(Player player) {
			const float hit_ramp_up_factor = 1 / 180f;
			int factor = (int)((30 / ((player.statLife / (float)player.statLifeMax2) * 3.5f + 0.5f)) * MathF.Min(player.GetModPlayer<OriginPlayer>().lifeRegenTimeSinceHit * hit_ramp_up_factor, 1f));
			player.lifeRegen += factor;
		}
		public static void VanityEffect(Player player) {
			player.GetModPlayer<OriginPlayer>().mysteriousSprayMult *= player.statLife / (float)player.statLifeMax2;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			EquippedEffect(player);
			if (!hideVisual) {
				VanityEffect(player);
			}
		}
		public override void UpdateVanity(Player player) {
			VanityEffect(player);
		}
	}
}
