using Terraria;
using Terraria.ID;

namespace Origins.Tiles.Ashen {
	public class Scrap_Heap : ComplexFrameTile, IAshenTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this));
		}
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(FromHexRGB(0x2c212a));
			DustType = DustID.Copper;
			HitSound = SoundID.NPCHit18;
		}
	}
}
