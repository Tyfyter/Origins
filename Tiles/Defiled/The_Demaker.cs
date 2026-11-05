using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.World.BiomeData;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class The_Demaker : ModTile, IGlowingModTile {
		public const int BaseTileID = TileID.FleshCloningVat;
		public static int ID { get; private set; }
		public override void Load() {
			 new TileItem(this)
			.WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.FleshCloningVaat);
				item.createTile = Type;
			})
			.RegisterItem();
			this.SetupGlowKeys();
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;

			TileID.Sets.DisableSmartCursor[Type] = TileID.Sets.DisableSmartCursor[BaseTileID];
			TileID.Sets.AvoidedByNPCs[Type] = TileID.Sets.AvoidedByNPCs[BaseTileID];

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(BaseTileID, 0));
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.SetHeight(4);
			TileObjectData.newTile.SetOriginBottomCenter();
			AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum() + 2 * TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);

			AddMapEntry(new(75, 96, 161), TileItem.Get(this).DisplayName);
			AdjTiles = [BaseTileID, Type];
			HitSound = SoundID.NPCHit18;
			DustType = Ashen_Biome.DefaultTileDust;
			ID = Type;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 7) {
				frameCounter = 0;
				frame = ++frame % 10;
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public void FancyLightingGlowColor(Tile tile, int x, int y, ref Vector3 color) {
			switch (tile.TileFrameY) {
				case 18 * 3:
				return;
			}
			color.DoFancyGlow(new(0.8f), tile.TileColor);
		}
		public static AutoLoadingAsset<Texture2D> tangelaTexture = typeof(The_Demaker).GetDefaultTMLName() + "_Tangela";
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			switch (Framing.GetTileSafely(i, j).TileFrameY) {
				case 18 * 3:
				return;
			}
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);
		}
		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (TangelaVisual.DrawOver || TileDrawing.IsVisible(tile)) {
				Vector2 position = new Vector2(i * 16f, j * 16f) - Main.screenPosition;
				TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(tile), out int x, out int y);
				short tileFrameX = tile.TileFrameX;
				short tileFrameY = tile.TileFrameY;
				Main.instance.TilesRenderer.GetTileDrawData(i, j, tile, tile.TileType, ref tileFrameX, ref tileFrameY, out _, out _, out _, out _, out int addFrX, out int addFrY, out _, out _, out _, out _);
				tileFrameX += (short)addFrX;
				tileFrameY += (short)addFrY;
				TangelaVisual.DrawTangela(
					tangelaTexture,
					position,
					new Rectangle(tileFrameX, tileFrameY, 16, 16),
					0,
					Vector2.Zero,
					Vector2.One,
					SpriteEffects.None,
					x + y * 787,
					new(i * 16f, j * 16f)
				);
			}
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
	}
}
