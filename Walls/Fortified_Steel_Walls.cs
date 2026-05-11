using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Utilities;

namespace Origins.Walls {
	public abstract class PatternedWall : OriginsWall {
		AutoLoadingTexture[] patternTextures;
		CustomTilePaintLoader.CustomTileVariationKey[] paintKeys;
		public override void Load() {
			paintKeys = new CustomTilePaintLoader.CustomTileVariationKey[BGCount];
			for (int i = 0; i < BGCount; i++) {
				paintKeys[i] = CustomTilePaintLoader.CreateKey();
			}
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			if (BGCount == 1) {
				patternTextures = [GetType().GetDefaultTMLName() + "_BG"];
			} else {
				patternTextures = new AutoLoadingTexture[BGCount];
				for (int i = 0; i < BGCount; i++) {
					patternTextures[i] = GetType().GetDefaultTMLName() + "_BG" + i;
				}
			}
		}
		public abstract int BGCount { get; }
		// Way simpler than for tiles since walls can't be sloped and the edges cover everything that's supposed to be covered outside of the 16x16 area
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			AutoLoadingTexture patternTexture;
			CustomTilePaintLoader.CustomTileVariationKey paintKey;
			if (BGCount > 1) {
				int x = (i * 16) / patternTextures[0].Value.Width;
				int y = (j * 16) / patternTextures[0].Value.Height;
				int rand = new FastRandom((ulong)(x + 2654435769u + ((long)y << 6)) + ((ulong)y >> 2)).Next(BGCount);
				patternTexture = patternTextures[rand];
				paintKey = paintKeys[rand];
			} else {
				patternTexture = patternTextures[0];
				paintKey = paintKeys[0];
			}
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
	}
	public abstract class Fortified_Steel_Wall : PatternedWall, IComplexMineDamageWall {
		public override WallVersion WallVersions => WallVersion.Natural | WallVersion.Safe | WallVersion.Placed_Unsafe;
		public abstract int HammerPower { get; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			if (WallVersion == WallVersion.Natural) Origins.WallHammerRequirement[Type] = HammerPower;
		}
		bool IComplexMineDamageWall.CanMine(Player self, Item item, int i, int j) {
			return WallVersion != WallVersion.Natural || CanMineNatural(self, item, i, j);
		}
		public abstract bool CanMineNatural(Player self, Item item, int i, int j);
	}
	public class Fortified_Steel_Wall1 : Fortified_Steel_Wall {
		public override Color MapColor => FromHexRGB(0x3b2b21);
		public override int HammerPower => 55;
		public override SoundStyle? HitSound => SoundID.Tink;
		public override int BGCount => 3;
		public override bool CanMineNatural(Player self, Item item, int i, int j) => NPC.downedBoss2;
	}
	public class Fortified_Steel_Wall2 : Fortified_Steel_Wall {
		public override Color MapColor => FromHexRGB(0x170c0f);
		public override int BGCount => 1;
		public override int HammerPower => 80;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
		public override bool CanMineNatural(Player self, Item item, int i, int j) => Main.hardMode;
	}
	public class Molten_Steel_Wall : Fortified_Steel_Wall {
		public override Color MapColor => new(254, 194, 20);
		public override int HammerPower => 80;
		public override int BGCount => 1;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			if (WallVersion == WallVersion.Natural) OriginsSets.Walls.GeneratesLiquid[Type] = LiquidID.Lava;
		}
		public override bool CanMineNatural(Player self, Item item, int i, int j) => NPC.downedGolemBoss;
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 1;
			g = 0.8f;
			b = 0.1f;
		}
	}
	public class Fortified_Steel_Wall3 : Fortified_Steel_Wall {
		public override Color MapColor => FromHexRGB(0x170c0f);
		public override int HammerPower => 100;
		public override int BGCount => 1;
		public override bool CanMineNatural(Player self, Item item, int i, int j) => NPC.downedGolemBoss;
	}
}
