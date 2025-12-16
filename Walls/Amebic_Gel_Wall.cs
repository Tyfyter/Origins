using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Tiles;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	public class Amebic_Gel_Wall : OriginsWall, IGlowingModWall {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public static float GlowValue => Riven_Hive.NormalGlowValue.GetValue() + 0.2f;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
		public override Color MapColor => new(0, 160, 160);
		public override int TileItem => ItemType<Amoeba_Fluid_Item>();
		public override int DustType => DustID.GemEmerald;
		public override SoundStyle? HitSound => SoundID.NPCHit13;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallLight[Type] = true;
			if (!Main.dedServ) {
				GlowTexture = Request<Texture2D>(Texture);
			}
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			float glowValue = GlowValue * 0.1f;
			r = 0.394f * glowValue;
			g = 0.879f * glowValue;
			b = 0.912f * glowValue;
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Framing.GetTileSafely(i, j);
			Color color = Lighting.GetColor(i, j);
			if (tile.IsWallFullbright) {
				color = Color.White;
			}
			Main.instance.LoadWall(Type);
			Rectangle value = new(tile.WallFrameX, tile.WallFrameY + Main.wallFrame[Type] * 180, 32, 32);
			Texture2D tileDrawTexture2 = CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(GlowPaintKey, tile.WallColor, TextureAssets.Wall[Type]);
			Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + (Main.drawToScreen ? Vector2.Zero : new(Main.offScreenRange));
			spriteBatch.Draw(
				tileDrawTexture2,
				position - 8 * Vector2.One,
				value,
				Riven_Hive.GetGlowAlpha(color),
				0f,
				Vector2.Zero,
				1f,
				SpriteEffects.None,
			0f);

			int quality = (int)(120f * (1f - Main.gfxQuality) + 40f * Main.gfxQuality);
			if (color.R > (int)(quality * 0.4f) || color.G > (int)(quality * 0.35f) || color.B > (int)(quality * 0.3f)) {
				int wallBlend = Main.wallBlend[tile.WallType];
				bool wallLeft = Main.tile[i - 1, j].WallType > WallID.None && Main.wallBlend[Main.tile[i - 1, j].WallType] != wallBlend;
				bool wallRight = Main.tile[i + 1, j].WallType > WallID.None && Main.wallBlend[Main.tile[i + 1, j].WallType] != wallBlend;
				bool wallUp = Main.tile[i, j - 1].WallType > WallID.None && Main.wallBlend[Main.tile[i, j - 1].WallType] != wallBlend;
				bool wallDown = Main.tile[i, j + 1].WallType > WallID.None && Main.wallBlend[Main.tile[i, j + 1].WallType] != wallBlend;
				if (wallLeft) {
					spriteBatch.Draw(TextureAssets.WallOutline.Value, position, new Rectangle(0, 0, 2, 16), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
				if (wallRight) {
					spriteBatch.Draw(TextureAssets.WallOutline.Value, position + new Vector2(14, 0), new Rectangle(14, 0, 2, 16), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
				if (wallUp) {
					spriteBatch.Draw(TextureAssets.WallOutline.Value, position, new Rectangle(0, 0, 16, 2), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
				if (wallDown) {
					spriteBatch.Draw(TextureAssets.WallOutline.Value, position + new Vector2(0, 14), new Rectangle(0, 14, 16, 2), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
			}
			return false;
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public override void Load() => GlowPaintKey = CustomTilePaintLoader.CreateKey();
	}
}
