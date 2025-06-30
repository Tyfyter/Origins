using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Gores.NPCs {
	public class Amoebeye1_Gore : ModGore {
		AutoLoadingAsset<Texture2D> solidTexture;
		public override string Texture => base.Texture + "_Glow";
		public override void SetStaticDefaults() {
			solidTexture = base.Texture;
		}
		public override Color? GetAlpha(Gore gore, Color lightColor) {
			Vector2 origin = solidTexture.Value.Size() * 0.5f;
			Main.spriteBatch.Draw(
				solidTexture,
				gore.position - Main.screenPosition + origin + gore.drawOffset,
				null,
				lightColor,
				gore.rotation,
				origin,
				gore.scale,
				SpriteEffects.None,
			0f);
			return Riven_Hive.GetGlowAlpha(lightColor);
		}
	}
	public class Amoebeye2_Gore : Amoebeye1_Gore { }
	public class Amoebeye3_Gore : Amoebeye1_Gore { }
}
