using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other {
	public class Journal_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.FallenStar);
			Item.ammo = AmmoID.None;
			Item.maxStack = 1;
		}
		public override bool? UseItem(Player player) {
			ref bool isUnlocked = ref Main.LocalPlayer.GetModPlayer<OriginPlayer>().journalUnlocked;
			if (!isUnlocked) {
				isUnlocked = true;
				return true;
			}
			return false;
		}
	}
}
