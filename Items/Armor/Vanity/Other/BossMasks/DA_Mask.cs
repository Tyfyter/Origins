using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Other.BossMasks {
    [AutoloadEquip(EquipType.Head)]
	public class DA_Mask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Defiled Amalgamation Mask");
			Tooltip.SetDefault("");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Vanity/Other/BossMasks/DA_Mask_Head_Glow");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
		}
	}
}
