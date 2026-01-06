using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Hanging_Wrycoral : OriginTile, IGlowingModTile {
		public override string Texture => base.Texture.Replace("Hanging_", "");
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = Request<Texture2D>(Texture + "_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			Main.tileNoFail[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = false;
			TileID.Sets.MultiTileSway[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.RandomStyleRange = 0;
			TileObjectData.addTile(Type);


			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(108, 200, 255), name);

			HitSound = SoundID.Grass;
			DustType = DustID.BlueCrystalShard;

			RegisterItemDrop(ItemType<Wrycoral_Item>());
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileObjectData.IsTopLeft(Main.tile[i, j])) {
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);
			}
			return false;
		}
		public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor) {
			glowTexture = this.GetGlowTexture(Main.tile[i, j].TileColor);
			glowColor = GlowColor;
		}
		public override void Load() => this.SetupGlowKeys();
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	[Obsolete("Retained for technical reasons; Use Hanging_Wrycoral instead", true)]
	public class Wrycoral : OriginTile { // can't get rid of the old stuff, rename it, or replace it for technical reasons
		public override string Texture => "Terraria/Images/Tiles_" + TileID.Cobweb;
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			Main.tileNoFail[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(128, 128, 128), name);

			HitSound = SoundID.Grass;
			DustType = DustID.WhiteTorch;

			RegisterItemDrop(ItemType<Wrycoral_Item>());
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileType = TileID.Cobweb;
			WorldGen.TileFrame(i, j, resetFrame, noBreak);
			tile.TileType = Type;
			return false;
		}
	}
	public class Wrycoral_Item : MaterialItem {
		public override int Value => Item.sellPrice(copper: 20);
		public override bool Hardmode => false;
		public override bool HasGlowmask => true;
		public override int Width => 12;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			OriginsSets.Items.EvilMaterialAchievement[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.DefaultToPlaceableTile(TileType<Hanging_Wrycoral>());
		}
	}
}
