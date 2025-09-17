using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Other {
	public class Pocket_Dimension_Monolith : MonolithBase, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (IsEnabled(tile)) color = Vector3.Max(color, new Vector3(0.394f));
		}
		public override int Frames => 4;
		public override Color MapColor => new(157, 157, 157);
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			base.SetStaticDefaults();
			TileID.Sets.HasSlopeFrames[Type] = true;
			Main.tileLighted[Type] = true;
		}
		public override void ApplyEffectEquipped(Player player) {
			player.OriginPlayer().pocketDimensionMonolithActive = true;
		}
		public override void ApplyEffect() {
			SC_Scene_Effect.monolithTileActive = true;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.01f;
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			int frameTime = (int)tile.Slope;
			int height = Height * 18;
			if (tile.TileFrameY >= height) {
				frameYOffset = tile.TileFrameNumber * height;
				if (tile.TileFrameNumber < 3) {
					if (frameTime >= 5) {
						tile.TileFrameNumber++;
						tile.Slope = 0;
					} else {
						tile.Slope = (SlopeType)(frameTime + 1);
					}
				} else {
					frameYOffset += Main.tileFrame[type] * Height * 18;
				}
			} else if (tile.TileFrameNumber > 0) {
				frameYOffset += (tile.TileFrameNumber - 1) * height;
				if (tile.TileFrameNumber > 0) {
					if (frameTime >= 5) {
						tile.TileFrameNumber--;
						tile.Slope = 0;
					} else {
						tile.Slope = (SlopeType)(frameTime + 1);
					}
				}
			}
		}
		public override void OnLoad() {
			this.SetupGlowKeys();
			Item.OnAddRecipes += (item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.ShimmerMonolith)
				.AddIngredient<Aetherite_Ore_Item>(15)
				.AddTile(TileID.Anvils)
				.Register();
			});
		}
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}
