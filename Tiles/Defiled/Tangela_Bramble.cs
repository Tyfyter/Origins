using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Graphics;
using Origins.World.BiomeData;
using PegasusLib;
using ReLogic.Threading;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Tangela_Bramble : ModTile {
		public override void SetStaticDefaults() {
			Main.tileSpelunker[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			Main.tileFrameImportant[Type] = true;
			//Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.addTile(Type);
			HitSound = Origins.Sounds.DefiledIdle;
			DustType = Defiled_Wastelands.DefaultTileDust;

			RegisterItemDrop(ModContent.ItemType<Tangela_Bramble_Item>(), -1);
		}
		public override bool IsTileDangerous(int i, int j, Player player) => true;
		public static void StandInside(Player player) {
			player.AddBuff(Rasterized_Debuff.ID, 16);
		}
		public static AutoLoadingAsset<Texture2D> tangelaTexture = typeof(Tangela_Bramble).GetDefaultTMLName() + "_Tangela";
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
		}
		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (TangelaVisual.DrawOver || TileDrawing.IsVisible(tile)) {
				Vector2 position = new Vector2(i * 16f, j * 16f) - Main.screenPosition;
				TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(tile), out int x, out int y);
				TangelaVisual.DrawTangela(
					tangelaTexture,
					position,
					new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
					0,
					Vector2.Zero,
					Vector2.One,
					SpriteEffects.None,
					x + y * 787,
					new(i * 16f, j * 16f)
				);
			}
		}
	}
	public class Tangela_Bramble_Map_System : ModSystem {
		int chunkNum = 0;
		List<Point>[] chunks = new List<Point>[100];
		public override void Load() {
			On_MapIconOverlay.Draw += On_MapIconOverlay_Draw;
		}
		public override void ClearWorld() {
			for (int i = 0; i < chunks.Length; i++) {
				(chunks[i] ??= []).Clear();
			}
		}
		public override void PostUpdateWorld() {
			if (chunkNum >= chunks.Length) chunkNum = 0;
			int perChunk = Main.maxTilesX / chunks.Length;
			int minX = perChunk * chunkNum;
			int maxX = perChunk * (chunkNum + 1);
			if (chunkNum >= chunks.Length - 1) {
				maxX = Main.maxTilesX;
			}
			List<Point> chunk = chunks[chunkNum] ??= [];
			chunk.Clear();
			ushort tangelaBramble = (ushort)ModContent.TileType<Tangela_Bramble>();
			FastParallel.For(minX, maxX, (min, max, _) => {
				for (int i = min; i < max; i++) {
					for (int j = 0; j < Main.maxTilesY; j++) {
						if (Framing.GetTileSafely(i, j).TileIsType(tangelaBramble)) {
							chunk.Add(new(i, j));
						}
					}
				}
			});
			chunkNum++;
		}
		void DrawBrambles(Vector2 mapPosition, Vector2 mapOffset, Rectangle? clippingRect, float mapScale, float drawScale) {
			const int pixels = 8;
			for (int i = 0; i < chunks.Length; i++) {
				List<Point> chunk = chunks[i];
				if (chunk is null) continue;
				for (int j = 0; j < chunk.Count; j++) {
					if (!Main.Map.IsRevealed(chunk[j].X, chunk[j].Y)) continue;
					Vector2 position = (chunk[j].ToVector2() - mapPosition) * mapScale + mapOffset;
					TangelaVisual.DrawTangela(
						TextureAssets.MagicPixel.Value,
						position,
						new Rectangle(0, 0, pixels, pixels),
						0f,
						default,
						Vector2.One * mapScale / pixels,
						SpriteEffects.None,
						Main.ActiveWorldFileData.Seed,
						chunk[j].ToVector2() * pixels
					);
				}
			}
		}
		private static void On_MapIconOverlay_Draw(On_MapIconOverlay.orig_Draw orig, MapIconOverlay self, Vector2 mapPosition, Vector2 mapOffset, Rectangle? clippingRect, float mapScale, float drawScale, ref string text) {
			ModContent.GetInstance<Tangela_Bramble_Map_System>()?.DrawBrambles(mapPosition, mapOffset, clippingRect, mapScale, drawScale);
			orig(self, mapPosition, mapOffset, clippingRect, mapScale, drawScale, ref text);
		}
	}
	public class Tangela_Bramble_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tangela_Bramble>());
		}
	}
}
