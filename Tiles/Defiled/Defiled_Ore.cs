using Microsoft.Xna.Framework;
using Origins.Journal;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
    public class Defiled_Ore : OriginTile, IComplexMineDamageTile {
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
	public class Defiled_Ore_Item : ModItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Lost_Ore_Entry).Name;

        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DemoniteOre);
			Item.createTile = TileType<Defiled_Ore>();
		}
    }
	public class Lost_Ore_Entry : JournalEntry {
		public override string TextKey => "Lost_Ore";
		public override ArmorShaderData TextShader => null;
	}
}
