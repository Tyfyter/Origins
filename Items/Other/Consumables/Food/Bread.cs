using Origins.Items.Weapons.Demolitionist;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Bread : ModItem {
		public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ItemID.ArtisanLoaf] = ModContent.ItemType<Bread>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bread>()] = ItemID.ArtisanLoaf;
            Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MonsterLasagna);
			Item.holdStyle = ItemHoldStyleID.None;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 8;
			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.White;
		}
	}
}
