using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class The_Defiled_Will : ModItem {
		public const int max_uses = 1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("The Defiled Will");
			Tooltip.SetDefault("Will you continue the legacy?");
			SacrificeTotal = 30;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaCrystal);
			Item.rare = ItemRarityID.Gray;
			Item.UseSound = SoundID.Item139.WithPitch(0.2f);
		}
		public override bool? UseItem(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.defiledWill < max_uses) {
				originPlayer.defiledWill++;
				return true;
			}
			return false;
		}
	}
}
