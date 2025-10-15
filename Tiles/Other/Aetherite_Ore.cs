using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Journal;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
    public class Aetherite_Ore : OriginTile, IComplexMineDamageTile {
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileOreFinderPriority[Type] = 320;
			Main.tileSpelunker[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.Ore[Type] = true;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(44, 57, 126), name);
			mergeID = TileID.Gold;
			DustType = DustID.PurpleCrystalShard;
            HitSound = SoundID.Tink;
        }
	}
	public class Aetherite_Ore_Item : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Aetherite_Ore_Entry).Name;
		public class Aetherite_Ore_Entry : JournalEntry {
			public override string TextKey => "Aetherite_Ore";
			public override JournalSortIndex SortIndex => new("Arabel", 6);
		}
		public string[] Categories => [
			WikiCategories.Ore
		];
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Aetherite_Ore>());
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(silver: 14);
		}
		public void ModifyWikiStats(JObject data) {
			string base_key = $"WikiGenerator.Stats.{Mod?.Name}.{Name}.";
			string key = base_key + "Crafting";
			data.AppendStat("Crafting", Language.GetTextValue(key), key);
			data.Add("Tier", 6);
			data["PickReq"] = 0;
		}
	}
}
