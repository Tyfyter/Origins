using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.BossMasks {
    [AutoloadEquip(EquipType.Head)]
	public class PA_Mask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Primordial Amoeba Mask");
			Tooltip.SetDefault("");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Vanity/BossMasks/PA_Mask_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
		}
	}
}
