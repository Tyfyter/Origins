using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	[LegacyName("Riven_Flesh")]
	public class Spug_Flesh : ComplexFrameTile, IRivenTile, IGlowingModTile {
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
				GlowTexture = Request<Texture2D>(Texture + "_Glow");
			}
			Origins.PotType.Add(Type, ((ushort)TileType<Riven_Pot>(), 0, 0));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;

			AddMapEntry(new Color(0, 125, 200));
			//soundType = SoundID.NPCDeath1;
			MinPick = 65;
			MineResist = 1.5f;
			DustType = Riven_Hive.DefaultTileDust;
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay("Origins/Tiles/MergerOverlays/Spug_Calcified_Overlay", TileType<Calcified_Riven_Flesh>());
			yield return new TileMergeOverlay("Origins/Tiles/MergerOverlays/Mud_Overlay", TileID.Sets.Mud);
		}
		public override void PostTileFrame(int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight) {
			if (WorldGen.genRand.NextBool(12) && !CheckOtherTilesGlow(i, j)) {
				Main.tile[i, j].TileFrameY += 270;
			}
		}
		bool CheckOtherTilesGlow(int i, int j) {
			if (Main.tile[i + 1, j].TileIsType(Type) && Main.tile[i + 1, j].TileFrameY > 270) {
				return true;
			}
			if (Main.tile[i - 1, j].TileIsType(Type) && Main.tile[i - 1, j].TileFrameY > 270) {
				return true;
			}
			if (Main.tile[i, j + 1].TileIsType(Type) && Main.tile[i, j + 1].TileFrameY > 270) {
				return true;
			}
			if (Main.tile[i, j - 1].TileIsType(Type) && Main.tile[i, j - 1].TileFrameY > 270) {
				return true;
			}
			return false;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
			base.PostDraw(i, j, spriteBatch);
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
		const string scar_layout =
		"_XXX____X_______" +
		"_XXX_X_X__X_____" +
		"_XXX_XX____X____" +
		"__X___X__X__ X__" +
		"_X_____X_       " +
		"_XX___X__X__X   " +
		"____X________   " +
		"X__X____X__X_   " +
		"__X____X_X___ __" +
		"___X_______X____" +
		"X_________X_____" +
		"__X__X_X____ ___" +
		"_X____       ___" +
		"______       ___" +
		"__X__X       ___";
		static bool HasScar(Tile tile) {
			if (tile.TileFrameY < 270) return false;
			return scar_layout[tile.TileFrameX / 18 + ((tile.TileFrameY % 270) / 18) * 16] == 'X';
		}
		public override void RandomUpdate(int i, int j) {
			int wrycoral = TileType<Hanging_Wrycoral>();
			int existing = 0;
			for (int k = -4; k <= 4; k++) {
				for (int l = -1; l <= 1; l++) {
					if (Main.tile[i + k, j + l].TileIsType(wrycoral)) existing++;
				}
			}
			Tile tile = Main.tile[i, j];
			Tile below = Main.tile[i, j + 1];
			if (!below.HasTile && tile.HasSolidFace(TileExtenstions.TileSide.Bottom) && WorldGen.genRand.NextBool(30)) {
				if (WorldGen.genRand.NextBool(5)) {
					if (TileExtenstions.CanActuallyPlace(i, j + 1, WorldGen.genRand.NextBool(Math.Max(20 - existing, 1)) ? wrycoral : TileType<Fuzzvine_Lorg>(), WorldGen.genRand.Next(3), 0, out TileObject objectData, onlyCheck: false, checkStay: true, cut: false)) {
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
			if (!above.HasTile && tile.BlockType == BlockType.Solid) {
				if (WorldGen.genRand.NextBool(250)) {
					above.SetToType((ushort)ModContent.TileType<Acetabularia>(), Main.tile[i, j].TileColor);
				} else if (WorldGen.genRand.NextBool(10) && TileExtenstions.CanActuallyPlace(i, j - 1, WorldGen.genRand.NextBool(3) ? TileType<Marrowick_Coral>() : TileType<Riven_Large_Foliage>(), 0, 0, out TileObject objectData, onlyCheck: false, checkStay: true)) {
					TileObject.Place(objectData);
				} else {
					above.SetToType((ushort)ModContent.TileType<Riven_Foliage>(), Main.tile[i, j].TileColor);
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
			Item.DefaultToPlaceableTile(ModContent.TileType<Spug_Flesh>());
		}
	}
}
