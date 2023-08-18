using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Riven_Lesion : ModTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => (float)(Math.Sin(Main.GlobalTimeWrappedHourly) + 2) * 0.5f;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (tile.TileFrameY == 0) color = new Vector3(0.394f, 0.879f, 0.912f) * GlowValue;
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = Mod.Assets.Request<Texture2D>("Tiles/Riven/Riven_Lesion_Glow");
			}
			Main.tileSpelunker[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.AnchorInvalidTiles = new[] { (int)TileID.MagicalIceBlock };
			TileObjectData.newTile.StyleHorizontal = false;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Riven Lesion");
			AddMapEntry(new Color(217, 95, 54), name);
			AdjTiles = new int[] { TileID.ShadowOrbs };
			//soundType = SoundID.NPCKilled;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			return true;
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			World.BiomeData.Riven_Hive.CheckLesion(i, j, Type);
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.05f * GlowValue;
			g = 0.0375f * GlowValue;
			b = 0.015f * GlowValue;
		}
	}
	public class Riven_Lesion_Item : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Riven_Lesion>();
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) {
				bool CheckPos(int x, int y) {
					return !Main.tile[x, y].HasTile && !Main.tile[x, y - 1].HasTile && Main.tile[x, y + 1].HasTile && Main.tile[x, y + 1].Slope == SlopeType.Solid;
				}
				Point current = new(Player.tileTargetX, Player.tileTargetY);
				if (CheckPos(current.X, current.Y) && CheckPos(current.X - 1, current.Y) && CheckPos(current.X + 1, current.Y) && CheckPos(current.X + 2, current.Y)) {
					int minX = current.X;
					int maxX = current.X;
					for (int x = -1; x > -3; x--) {
						if (CheckPos(current.X + x, current.Y)) {
							if (x <= -2) minX--;
							for (int y = -1; y > 3; y++) Framing.GetTileSafely(current.X + x, current.Y + y).TileColor = (byte)-x;
						} else {
							for (int y = -1; y > 3; y++) Framing.GetTileSafely(current.X + x, current.Y + y).TileColor = PaintID.NegativePaint;
							break;
						}
					}
					for (int x = 1; x < 4; x++) {
						if (CheckPos(current.X + x, current.Y)) {
							if (x >= 2) maxX++;
							for (int y = -1; y > 3; y++) Framing.GetTileSafely(current.X + x, current.Y + y).TileColor = (byte)x;
						} else {
							for (int y = -1; y > 3; y++) Framing.GetTileSafely(current.X + x, current.Y + y).TileColor = PaintID.NegativePaint;
							break;
						}
					}
					for (int x = minX; x < maxX; x++) {
						Framing.GetTileSafely(x, current.Y + 4).TileColor = PaintID.DeepCyanPaint;
					}
					Framing.GetTileSafely(current.X, current.Y + 1).TileColor = PaintID.DeepRedPaint;
					WorldGen.PlaceTile(Main.rand.Next(minX, maxX), Player.tileTargetY, Item.createTile);
				} else {
					Framing.GetTileSafely(current).TileColor = PaintID.NegativePaint;
				}
				return false;
			}
			return true;
		}
	}
}
