using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Silicon_Ore : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			Main.tileOreFinderPriority[Type] = 120;
			Main.tileSpelunker[Type] = true;
			TileID.Sets.Ore[Type] = true;
			AddMapEntry(new Color(60, 60, 60), CreateMapEntryName());
			MineResist = 2;
			DustType = DustID.Lead;
			HitSound = SoundID.Tink;
		}
	}
	public class Silicon_Ore_Item : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Silicon_Ore>());
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(copper: 44);
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
		public void ModifyWikiStats(JObject data) {
			string base_key = $"WikiGenerator.Stats.{Mod?.Name}.{Name}.";
			string key = base_key + "Crafting";
			data.AppendStat("Crafting", Language.GetTextValue(key), key);
			data.Add("Tier", 4);
		}
	}
}
