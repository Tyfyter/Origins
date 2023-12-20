using Origins.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Blue_Bovine : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Blue_Bovine_Buff.ID;
			Item.buffTime = 60 * 60 * 4;
			Item.value = Item.sellPrice(silver: 20);
		}
	}
}
