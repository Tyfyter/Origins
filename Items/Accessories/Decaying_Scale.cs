using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Decaying_Scale : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Decaying Scale");
			// Tooltip.SetDefault("Attacks inflict 'Toxic Shock' on enemies\nEffects are stronger while Acrid Armor is equipped");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 22);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Pink;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.decayingScale = true;
		}
	}
}
