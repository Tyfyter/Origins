using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
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
			mergeID = TileID.Demonite;
		}
		public override bool CreateDust(int i, int j, ref int type) {
			type = DustID.Torch;
			return true;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 1f;
			g = 0.6f;
			b = 0f;
		}
		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower >= 55 || j <= Main.worldSurface) {
				damage = (int)(minePower / MineResist);
			}
		}
	}
	public class Sanguinite_Ore_Item : ModItem {
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DemoniteOre);
			Item.createTile = TileType<Sanguinite_Ore>();
		}
	}
}
