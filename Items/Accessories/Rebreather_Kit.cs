using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Rebreather_Kit : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Rebreather Kit");
			Tooltip.SetDefault("Extends underwater breathing\nGain more breath as you move in water\nImmunity to ‘Suffocation’");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.YoYoGlove);
			Item.faceSlot = Rebreather.FaceSlot;
			Item.backSlot = Air_Tank.BackSlot;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().rebreather = true;
			player.buffImmune[BuffID.Suffocation] = true;
			player.breathMax += 257;
		}
		public override void AddRecipes() {

		}
	}
}
