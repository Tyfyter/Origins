using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Journal;
using Origins.Tiles.Riven;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	[LegacyName("Defiled_Ore")]
    public class Lost_Ore : OriginTile, IComplexMineDamageTile {
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileOreFinderPriority[Type] = 320;
			Main.tileSpelunker[Type] = true;
			TileID.Sets.Ore[Type] = true;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(225, 225, 225), name);
			mergeID = TileID.Demonite;
			DustType = DustID.WhiteTorch;
            HitSound = SoundID.Tink;
        }
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.25f;
		}
		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower < 55 && j > Main.worldSurface) {
				damage = 0;
			}
		}
	}
	[LegacyName("Defiled_Ore_Item")]
	public class Lost_Ore_Item : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Ore"
		];
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Lost_Ore>());
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 14);
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
