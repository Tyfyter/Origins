using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Medium_Foliage : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));
			HitSound = Origins.Sounds.DefiledIdle;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);

			TileObjectData.newTile.AnchorValidTiles = new int[]{
				ModContent.TileType<Defiled_Grass>()
			};

			TileObjectData.addTile(Type);
			//soundType = SoundID.Grass;
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 1) spriteEffects = SpriteEffects.FlipHorizontally;
		}

		public override bool Drop(int i, int j) {
			return false;
		}
	}
}
