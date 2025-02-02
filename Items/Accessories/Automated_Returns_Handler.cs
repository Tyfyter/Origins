﻿using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Back)]
	public class Automated_Returns_Handler : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Movement",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.hasMagiluminescence = true;
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.turboReel2 = true;
			originPlayer.automatedReturnsHandler = true;

			player.blackBelt = true;
			player.dashType = 1;
			player.spikedBoots += 2;
		}
        public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ItemID.MasterNinjaGear)
            .AddIngredient(ModContent.ItemType<Super_Turbo_Reel>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
    }
}
