using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Jam_Sandwich : ModItem {
		static short glowmask;
        public string[] Categories => new string[] {
            "Food"
        };
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShrimpPoBoy);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.scale = 0.6f;
			Item.buffType = BuffID.WellFed2;
			Item.buffTime = 60 * 60 * 10;
			Item.value = Item.sellPrice(silver: 10);
			Item.rare = ItemRarityID.Orange;
			Item.glowMask = glowmask;
		}
	}
}
