using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Riven_Foliage : ModTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = new Vector3(0.394f, 0.879f, 0.912f) * GlowValue;
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileLighted[Type] = true;
			AddMapEntry(new Color(20, 138, 220));

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);

			TileObjectData.newTile.AnchorValidTiles = new int[]{
				ModContent.TileType<Riven_Flesh>()
			};

			TileObjectData.addTile(Type);
			//soundType = SoundID.NPCKilled;
			DustType = Riven_Hive.DefaultTileDust;
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
		public override bool CanDrop(int i, int j) => false;
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.05f * GlowValue;
			g = 0.0375f * GlowValue;
			b = 0.015f * GlowValue;
		}
	}
}
