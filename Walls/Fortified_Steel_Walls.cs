using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Tiles;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;

namespace Origins.Walls {
	public class Fortified_Steel_Wall1 : OriginsWall, IComplexMineDamageWall {
		public override Color MapColor => FromHexRGB(0x3b2b21);
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
		AutoLoadingAsset<Texture2D> patternTexture;
		CustomTilePaintLoader.CustomTileVariationKey paintKey;
		public override void Load() => paintKey = CustomTilePaintLoader.CreateKey();
		public virtual int HammerPower => 55;
		public override SoundStyle? HitSound => SoundID.Tink;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			patternTexture = GetType().GetDefaultTMLName() + "_BG";
			if (WallVersion == WallVersion.Natural) Origins.WallHammerRequirement[Type] = HammerPower;
		}
		// Way simpler than for tiles since walls can't be sloped and the edges cover everything that's supposed to be covered outside of the 16x16 area
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			Texture2D texture = CustomTilePaintLoader.TryGetTileAndRequestIfNotReady(paintKey, Main.tile[i, j].WallColor, patternTexture);
			Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition;
			if (!Main.drawToScreen) {
				pos.X += Main.offScreenRange;
				pos.Y += Main.offScreenRange;
			}
			Lighting.GetCornerColors(i, j, out VertexColors vertices);
			Vector4 destination = new(pos, 16, 16);
			Rectangle source = new((i * 16) % texture.Width, (j * 16) % texture.Height, 16, 16);
			Main.tileBatch.Draw(
				patternTexture,
				destination,
				source,
				vertices
			);
			return base.PreDraw(i, j, spriteBatch);
		}
		bool IComplexMineDamageWall.CanMine(Player self, Item item, int i, int j) {
			return WallVersion != WallVersion.Natural || CanMine(self, item, i, j);
		}
		public virtual bool CanMine(Player self, Item item, int i, int j) => NPC.downedBoss2;
	}
	public class Molten_Steel_Wall : Fortified_Steel_Wall1 {
		public override Color MapColor => new(254, 194, 20);
		public override int HammerPower => 100;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			if (WallVersion == WallVersion.Natural) OriginsSets.Walls.GeneratesLiquid[Type] = LiquidID.Lava;
		}
		public override bool CanMine(Player self, Item item, int i, int j) => NPC.downedGolemBoss;
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 1;
			g = 0.8f;
			b = 0.1f;
		}
	}
	public class Fortified_Steel_Wall3 : Fortified_Steel_Wall1 {
		public override Color MapColor => FromHexRGB(0x170c0f);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			if (WallVersion == WallVersion.Natural) Origins.WallHammerRequirement[Type] = 100;
		}
		public override bool CanMine(Player self, Item item, int i, int j) => NPC.downedGolemBoss;
	}
}
