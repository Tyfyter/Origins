using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Accessories.Eyndum_Cores;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Shinespark_Dash_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> horizontalTexture = "Origins/Projectiles/Misc/Speed_Booster_Shinespark";
		AutoLoadingAsset<Texture2D> diagonalTexture = "Origins/Projectiles/Misc/Speed_Booster_Shinespark_Diagonal";
		AutoLoadingAsset<Texture2D> verticalTexture = "Origins/Projectiles/Misc/Speed_Booster_Shinespark_Vertical";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.OriginPlayer().shineSparkDashTime > 0;
		public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 position = drawInfo.Position + drawPlayer.Size * 0.5f - Main.screenPosition;
			Vector2 direction = drawPlayer.OriginPlayer().shineSparkDashDirection;
			Texture2D texture = null;
			SpriteEffects effects = SpriteEffects.None;
			float rotation = 0;
			position += direction * 2;
			switch ((Math.Sign(direction.X), Math.Sign(direction.Y))) {
				case (1, 0):
				texture = horizontalTexture;
				break;
				case (-1, 0):
				texture = horizontalTexture;
				effects |= SpriteEffects.FlipHorizontally;
				break;

				case (1, -1):
				texture = diagonalTexture;
				break;
				case (-1, -1):
				texture = diagonalTexture;
				effects |= SpriteEffects.FlipHorizontally;
				break;
				case (1, 1):
				texture = diagonalTexture;
				rotation = MathHelper.PiOver2;
				break;
				case (-1, 1):
				texture = diagonalTexture;
				effects |= SpriteEffects.FlipHorizontally;
				rotation = -MathHelper.PiOver2;
				break;

				case (0, -1):
				texture = verticalTexture;
				effects = drawInfo.playerEffect;
				position += direction * 2;
				break;
				case (0, 1):
				texture = verticalTexture;
				effects = drawInfo.playerEffect | SpriteEffects.FlipVertically;
				position += direction * 2;
				break;
			}
			if (texture is null) return;
			Rectangle frame = texture.Frame(verticalFrames: 8, frameY: (int)(Main.timeForVisualEffects % 8));

			drawInfo.DrawDataCache.Add(new(
				texture,
				position,
				frame,
				Color.White * drawInfo.shadow,
				rotation,
				frame.Size() * 0.5f,
				1f,
				effects
			));
		}
	}
}
