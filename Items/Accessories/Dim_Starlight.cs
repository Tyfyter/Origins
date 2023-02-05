using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Dim_Starlight : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dim Starlight");
			Tooltip.SetDefault("Chance for mana stars to fall from critical hits");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.dimStarlight = true;
			float light = 0.1f + (originPlayer.dimStarlightCooldown / 1000f);
			Lighting.AddLight(player.Center, light, light, light);
		}
	}
}
