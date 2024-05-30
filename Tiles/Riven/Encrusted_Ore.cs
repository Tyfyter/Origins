using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
    public class Encrusted_Ore : OriginTile, IComplexMineDamageTile {
        public string[] Categories => new string[] {
            "Ore"
        };
		public static float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileOreFinderPriority[Type] = 320;
			Main.tileSpelunker[Type] = true;
			TileID.Sets.Ore[Type] = true;
			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(40, 148, 207), name);
			mergeID = TileID.Crimtane;
            HitSound = SoundID.Tink;
        }
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.02f * GlowValue;
			g = 0.15f * GlowValue;
			b = 0.2f * GlowValue;
		}
		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower < 55 && j > Main.worldSurface) {
				damage = 0;
			}
		}
	}
	[LegacyName("Infested_Ore_Item")]
	public class Encrusted_Ore_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrimtaneOre);
			Item.createTile = TileType<Encrusted_Ore>();
		}
    }
}
