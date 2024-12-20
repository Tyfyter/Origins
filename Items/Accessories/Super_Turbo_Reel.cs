﻿using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
    public class Super_Turbo_Reel : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.hasMagiluminescence = true;
			player.GetModPlayer<OriginPlayer>().turboReel2 = true;
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ItemID.Magiluminescence)
            .AddIngredient(ModContent.ItemType<Turbo_Reel>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
    }
}
