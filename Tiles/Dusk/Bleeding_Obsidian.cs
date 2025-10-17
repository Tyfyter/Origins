using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Dusk {
	public class Bleeding_Obsidian : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.Ore[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			Main.tileMerge[Type][TileType<Dusk_Stone>()] = true;
			Main.tileMerge[Type][TileType<Dusk_Stone_Liquid>()] = true;
			AddMapEntry(new Color(57, 10, 75));
			MinPick = 180;
			MineResist = 8;
			DustType = DustID.ShimmerTorch;
		}
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			yield return new Item(ItemType<Bleeding_Obsidian_Shard>(), Main.rand.Next(4, 7));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			float m = 0.05f;
			r = 37.2f * m;
			g = 6.7f * m;
			b = 49.2f * m;
		}
	}
	public class Bleeding_Obsidian_Item : ModItem, ICustomWikiStat {
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Bleeding_Obsidian>());
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 6)
			.AddIngredient(ItemID.Obsidian, 6)
			.AddIngredient(ItemID.SoulofNight)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
		public void ModifyWikiStats(JObject data) {
			string base_key = $"WikiGenerator.Stats.{Mod?.Name}.{Name}.";
			string key = base_key + "Crafting";
			data.AppendStat("Crafting", Language.GetTextValue(key), key);
			data.Add("Tier", 11);
		}
	}
}
