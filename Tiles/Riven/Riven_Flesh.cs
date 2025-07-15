using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using Origins.World.BiomeData;
using System.Reflection.Metadata;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Riven_Flesh : OriginTile, IRivenTile, IGlowingModTile {
        public string[] Categories => [
            "Stone"
        ];
        public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (HasScar(tile)) {
				//Lighting.AddLight((int)(id % Main.maxTilesX) * 16, (int)(id / Main.maxTilesX) * 16, 1, 0.05f, 0.055f);
				color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
			}
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Riven_Flesh_Glow");
			}
			Origins.PotType.Add(Type, ((ushort)TileType<Riven_Pot>(), 0, 0));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			/*Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
            Main.tileMerge[Type][TileID.Stone] = true;
            for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
            }*/
			AddMapEntry(new Color(0, 125, 200));
			//soundType = SoundID.NPCDeath1;
			MinPick = 65;
			MineResist = 1.5f;
			DustType = Riven_Hive.DefaultTileDust;
		}
		bool recursion = false;
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			if (recursion) {
				return true;
			}
			recursion = true;
			WorldGen.TileFrame(i, j, resetFrame, noBreak);
			recursion = false;
			Tile tile = Main.tile[i, j];
			if ((!WorldGen.genRand.NextBool(tile.TileFrameX == 54 ? 12 : 8)) || CheckOtherTilesGlow(i, j)) {
				Main.tile[i, j].TileFrameY += 90;
			}
			return false;
		}
		bool CheckOtherTilesGlow(int i, int j) {
			if (Main.tile[i + 1, j].TileIsType(Type) && Main.tile[i + 1, j].TileFrameY < 90) {
				return true;
			}
			if (Main.tile[i - 1, j].TileIsType(Type) && Main.tile[i - 1, j].TileFrameY < 90) {
				return true;
			}
			if (Main.tile[i, j + 1].TileIsType(Type) && Main.tile[i, j + 1].TileFrameY < 90) {
				return true;
			}
			if (Main.tile[i, j - 1].TileIsType(Type) && Main.tile[i, j - 1].TileFrameY < 90) {
				return true;
			}
			return false;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0;
			Tile tile = Main.tile[i, j];
			if (OriginsModIntegrations.FancyLighting is not null && HasScar(tile)) {
				r = 0.02f * GlowValue;
				g = 0.15f * GlowValue;
				b = 0.2f * GlowValue;
			}
		}
		static bool HasScar(Tile tile) {
			if (tile.TileFrameY >= 90) return false;
			switch ((tile.TileFrameX / 18, tile.TileFrameY / 18)) {
				case (1, 0):
				case (2, 0):
				case (3, 0):

				case (1, 1):
				case (2, 1):
				case (3, 1):

				case (1, 2):
				case (2, 2):
				case (3, 2):

				case (1, 4):
				case (2, 3):
				case (5, 1):
				case (5, 2):
				case (6, 2):
				case (6, 3):
				case (7, 1):
				case (7, 4):
				case (8, 0):
				case (8, 3):
				case (9, 3):
				case (10, 1):
				case (11, 2):
				return true;
			}
			return false;
		}
		static bool CheckOtherTilesActive(int i, int j) {
			return !(Main.tile[i + 1, j].HasTile && Main.tile[i + 1, j].HasTile && Main.tile[i + 1, j].HasTile && Main.tile[i + 1, j].HasTile);
		}
		public override void RandomUpdate(int i, int j) {
			int wrycoral = TileType<Hanging_Wrycoral>();
			int existing = 0;
			for (int k = -4; k <= 4; k++) {
				for (int l = -1; l <= 1; l++) {
					if (Main.tile[i + k, j + l].TileIsType(wrycoral)) existing++;
				}
			}
			Tile below = Main.tile[i, j + 1];
			if (!below.HasTile && WorldGen.genRand.NextBool(50)) {
				if (WorldGen.genRand.NextBool(50 - existing)) {
					if (TileExtenstions.CanActuallyPlace(i, j + 1, wrycoral, 0, 0, out TileObject objectData, onlyCheck: false, checkStay: true)) {
						TileObject.Place(objectData);
					}
				} else {
					below.TileType = (ushort)TileType<Fuzzvine>();
					below.HasTile = true;
					WorldGen.TileFrame(i, j + 1, true);
				}
				//Main.LocalPlayer.Teleport(new Vector2(i, j).ToWorldCoordinates(), 1);
			}
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (WorldGen.genRand.NextBool(250)) {
					above.ResetToType((ushort)ModContent.TileType<Acetabularia>());
				} else if (WorldGen.genRand.NextBool(10) && TileExtenstions.CanActuallyPlace(i, j - 1, WorldGen.genRand.NextBool(3) ? TileType<Marrowick_Coral>() : TileType<Riven_Large_Foliage>(), 0, 0, out TileObject objectData, onlyCheck: false, checkStay: true)) {
					TileObject.Place(objectData);
				} else {
					above.ResetToType((ushort)ModContent.TileType<Riven_Foliage>());
				}
				WorldGen.TileFrame(i, j - 1);
			}
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Riven_Flesh_Item : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Spug_Flesh_Entry).Name;
		public class Spug_Flesh_Entry : JournalEntry {
			public override string TextKey => "Spug_Flesh";
			public override JournalSortIndex SortIndex => new("Riven", 11);
		}
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.StoneBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Riven_Flesh>());
		}
	}
}
