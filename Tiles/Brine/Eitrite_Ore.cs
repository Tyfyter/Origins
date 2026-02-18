using Newtonsoft.Json.Linq;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	public class Eitrite_Ore : OriginTile {
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileOreFinderPriority[Type] = 666;
			Main.tileSpelunker[Type] = true;
			TileID.Sets.Ore[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			AddMapEntry(new Color(8, 190, 151), CreateMapEntryName());
			MinPick = 180;
			HitSound = SoundID.Tink;
			DustType = DustID.Mythril;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
	}
	public class Eitrite_Ore_Item : ModItem, ICustomWikiStat {
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.ChlorophyteOre] = Type;
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.TitaniumOre;
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Eitrite_Ore>());
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(silver: 16);
		}
		public void ModifyWikiStats(JObject data) {
			string base_key = $"WikiGenerator.Stats.{Mod?.Name}.{Name}.";
			string key = base_key + "Crafting";
			data.AppendStat("Crafting", Language.GetTextValue(key), key);
			data.Add("Tier", 11.5);
		}
	}
}
