using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Silicon : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			AddMapEntry(new Color(60, 60, 60));
			MinPick = 35;
			MineResist = 3;
		}
	}
	public class Silicon_Item : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Ore"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneBlock);
			Item.value = Item.sellPrice(copper: 44);
			Item.createTile = ModContent.TileType<Silicon>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SandBlock, 3)
			.AddTile(TileID.GlassKiln)
			.Register();

			Recipe.Create(Type)
			.AddIngredient(ItemID.EbonsandBlock, 3)
			.AddTile(TileID.GlassKiln)
			.Register();

			Recipe.Create(Type)
			.AddIngredient(ItemID.CrimsandBlock, 3)
			.AddTile(TileID.GlassKiln)
			.Register();

			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Sand_Item>(), 3)
			.AddTile(TileID.GlassKiln)
			.Register();

			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Silica_Item>(), 3)
			.AddTile(TileID.GlassKiln)
			.Register();

			/*Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Ashen_Sand_Item>(), 3)
			.AddTile(TileID.GlassKiln)
			.Register();*/
		}
	}
}
