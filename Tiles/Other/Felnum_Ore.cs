using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Journal;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
    public class Felnum_Ore : OriginTile {
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileOreFinderPriority[Type] = 450;
			Main.tileSpelunker[Type] = true;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(160, 116, 42), name);
            MinPick = 65;
            MineResist = 3;
			DustType = DustID.Electric;
			HitSound = SoundID.Tink;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (!Main.tile[i, j].HasTile) return;
			float v = (float)Math.Sin((Main.time - i) / 45) * 2;
			if (v >= 1.99f && Main.rand.Next(0, 4) == 0) Dust.NewDust(new Vector2(i * 16 - Main.rand.Next(0, 16), j * 16 - Main.rand.Next(0, 16)), 30, 30, DustID.Electric, 0f, 0f, 0, Color.White, 0.5f);//Main.NewText($"{i},{j},{v}");
			if (v < 0) v = 0;
			r = 0.4f - (0.4f * v);
			g = 0.3f + (0.2f * v);
			b = 0.1f + (0.3f * v);
		}
	}
	public class Felnum_Ore_Item : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Felnum_Mat_Entry).Name;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Felnum_Ore>());
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient(ItemID.CopperOre, 50)
			.AddIngredient(ItemID.FallenStar, 4)
			.AddTile(TileID.CrystalBall)
			.Register();
		}
		public void ModifyWikiStats(JObject data) {
			string base_key = $"WikiGenerator.Stats.{Mod?.Name}.{Name}.";
			string key = base_key + "Crafting";
			data.AppendStat("Crafting", Language.GetTextValue(key), key);
			data.Add("Tier", 8);
		}
	}
	public class Felnum_Mat_Entry : JournalEntry {
		public override string TextKey => "Felnum";
	}
}
