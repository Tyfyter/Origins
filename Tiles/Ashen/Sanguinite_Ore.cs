using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Sanguinite_Ore : OriginTile, IComplexMineDamageTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileOreFinderPriority[Type] = 320;
			Main.tileSpelunker[Type] = true;
			TileID.Sets.Ore[Type] = true;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(255, 165, 0), name);
			DustType = DustID.Torch;
			//DustType = DustID.Palladium;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 1f;
			g = 0.6f;
			b = 0f;
		}
		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower < 55 && j > Main.worldSurface) {
				damage = 0;
			}
		}
	}
	public class Sanguinite_Ore_Item : ModItem, ICustomWikiStat {
		public string[] Categories => [
		   "Ore"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.DemoniteOre] = ItemID.CrimtaneOre;
			ItemID.Sets.ShimmerTransformToItem[ItemID.CrimtaneOre] = ItemType<Lost_Ore_Item>();
			ItemID.Sets.ShimmerTransformToItem[ItemType<Lost_Ore_Item>()] = ItemType<Encrusted_Ore_Item>();
			ItemID.Sets.ShimmerTransformToItem[ItemType<Encrusted_Ore_Item>()] = Type;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.DemoniteOre;
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Sanguinite_Ore>());
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 16);
		}
		public void ModifyWikiStats(JObject data) {
			string base_key = $"WikiGenerator.Stats.{Mod?.Name}.{Name}.";
			string key = base_key + "Crafting";
			data.AppendStat("Crafting", Language.GetTextValue(key), key);
			data.Add("Tier", 5);
			data["PickReq"] = 55;
		}
	}
}
