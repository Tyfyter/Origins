using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Terraria;

namespace Origins.Gores.NPCs {
	public class Aqueoua_Gore1 : GoreDust {
		protected override Rectangle Frame => new(0, 0, 34, 26);
		AutoLoadingAsset<Texture2D> goreTexture;
		public override string Texture => base.Texture;
		public override void SetStaticDefaults() {
			goreTexture = base.Texture + "_Glow";
		}
		public override bool PreDraw(Dust dust) {
			Color color = Lighting.GetColor(dust.position.ToTileCoordinates()) * ((255f - dust.alpha) / 255f);
			Main.spriteBatch.Draw(
				Texture2D.Value,
				dust.position - Main.screenPosition,
				dust.frame,
				color,
				dust.rotation,
				dust.frame.Size() * 0.5f,
				dust.scale,
				SpriteEffects.None,
			0f);
			Main.spriteBatch.Draw(
				goreTexture,
				dust.position - Main.screenPosition,
				dust.frame,
				Riven_Hive.GetGlowAlpha(color),
				dust.rotation,
				dust.frame.Size() * 0.5f,
				dust.scale,
				SpriteEffects.None,
			0f);
			return false;
		}
	}
	public class Aqueoua_Gore2 : Aqueoua_Gore1 {
		protected override Rectangle Frame => new(0, 0, 24, 28);
	}
}
