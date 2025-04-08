using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Armor.Other;
using Origins.Reflection;
using Origins.World.BiomeData;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
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
					x + y * 787
				);
			}
		}
	}
	public class Tangela_Bramble_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tangela_Bramble>());
		}
	}
}
