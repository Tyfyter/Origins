using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Foliage : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));
			HitSound = Origins.Sounds.DefiledIdle;

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);

			TileObjectData.newTile.AnchorValidTiles = new int[]{
				ModContent.TileType<Defiled_Grass>()
			};

			TileObjectData.addTile(Type);
			//soundType = SoundID.Grass;
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 1) spriteEffects = SpriteEffects.FlipHorizontally;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Main.tile[i, j].TileFrameX = (short)(Main.rand.Next(6) * 18);
			ushort anchorType = Main.tile[i, j + 1].TileType;
			if (!TileObjectData.GetTileData(Main.tile[i, j]).isValidTileAnchor(anchorType)) {
				if (TileID.Sets.Conversion.Grass[anchorType]) {
					switch (anchorType) {
						case TileID.Grass:
						Main.tile[i, j].TileType = TileID.Plants;
						return true;
						case TileID.CorruptGrass:
						Main.tile[i, j].TileType = TileID.CorruptPlants;
						return true;
						case TileID.CrimsonGrass:
						Main.tile[i, j].TileType = TileID.CrimsonPlants;
						return true;
						case TileID.HallowedGrass:
						Main.tile[i, j].TileType = TileID.HallowedPlants;
						return true;
					}
				} else {
					WorldGen.KillTile(i, j, noItem: WorldGen.gen);
				}
			}
			return false;
		}

		public override bool Drop(int i, int j) {
			return false;
		}
	}
}
