using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
    public class Chambersite : OriginTile {
		public override void SetStaticDefaults() { //TODO: gemstone properties
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			AddMapEntry(new Color(10, 60, 25));
			MinPick = 35;
			MineResist = 3;
		}
	}
    public class Chambersite_Item : MaterialItem {
		public override int ResearchUnlockCount => 15;
		public override bool Hardmode => true;
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Chambersite>());
            Item.value = Item.sellPrice(silver: 13);
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = Item.CommonMaxStack;
        }
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.CrystalShard, 16)
			.AddCondition(Language.GetOrRegister("Mods.Origins.Conditions.ShimmerDecrafting"), () => false)
			.AddDecraftCondition(Condition.Hardmode)
			.Register();

			Recipe.Create(ItemID.CrystalShard, 16)
			.AddIngredient(Type)
			.AddCondition(Language.GetOrRegister("Mods.Origins.Conditions.ShimmerDecrafting"), () => false)
			.Register();
		}
	}
}
