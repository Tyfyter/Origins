using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class DA_Mask : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("{$Defiled} Amalgamation Mask");
			if (Main.netMode != NetmodeID.Server) {
				Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Vanity/BossMasks/DA_Mask_Head_Glow");
			}
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
		}
	}
}
