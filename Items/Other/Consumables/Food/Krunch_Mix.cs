using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Krunch_Mix : ModItem {
        public string[] Categories => new string[] {
            "Food"
        };
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.PotatoChips);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.4f;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 6;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Orange;
		}
		public override bool ConsumeItem(Player player) {
			player.AddBuff(BuffID.Regeneration, Item.buffTime);
			return true;
		}
	}
}
