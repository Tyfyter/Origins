using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Misc;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Abyssal_Anchor_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> chainTexture = "Origins/Items/Accessories/Abyssal_Anchor_P2";
		AutoLoadingAsset<Texture2D> endTexture = "Origins/Items/Accessories/Abyssal_Anchor_P";
		AutoLoadingAsset<Texture2D> endGlowTexture = "Origins/Items/Accessories/Abyssal_Anchor_P_Glow";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return !drawInfo.drawPlayer.dead && drawInfo.drawPlayer.OriginPlayer().abyssalAnchorVisual;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Backpacks);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			OriginPlayer originPlayer = drawInfo.drawPlayer.OriginPlayer();
			Physics.Chain chain = originPlayer.abyssalAnchorChain;
			if (chain is null) return;
			Vector2 startPoint = chain.anchor.WorldPosition;
			Vector2 extraChainDir = (startPoint - chain.links[0].position).SafeNormalize(default) * 10;
			if (extraChainDir != Vector2.Zero) {
				int tries = 0;
				Vector2 extraChainPoint = chain.links[0].position;
				while ((extraChainPoint - startPoint).LengthSquared() > 16 * 16 && ++tries < 200) {
					drawInfo.DrawDataCache.Add(new(
						chainTexture,
						extraChainPoint - Main.screenPosition,
						null,
						drawInfo.drawPlayer.GetImmuneAlphaPure(new Color(Lighting.GetSubLight(extraChainPoint)), drawInfo.shadow),
						extraChainDir.ToRotation(),
						chainTexture.Value.Size() * new Vector2(0.5f, 0.5f),
						1,
						SpriteEffects.None
					) {
						shader = originPlayer.abyssalAnchorDye
					});
					extraChainPoint += extraChainDir;
				}
			}
			for (int j = 0; j < chain.links.Length - 1; j++) {
				drawInfo.DrawDataCache.Add(new(
					chainTexture,
					chain.links[j].position - Main.screenPosition,
					null,
					drawInfo.drawPlayer.GetImmuneAlphaPure(new Color(Lighting.GetSubLight(chain.links[j].position)), drawInfo.shadow),
					(chain.links[j].position - startPoint).ToRotation(),
					chainTexture.Value.Size() * new Vector2(0.5f, 0.5f),
					1,
					SpriteEffects.None
				) {
					shader = originPlayer.abyssalAnchorDye
				});
				startPoint = chain.links[j].position;
			}
			drawInfo.DrawDataCache.Add(new(
				endTexture,
				startPoint - Main.screenPosition,
				null,
				drawInfo.drawPlayer.GetImmuneAlphaPure(new Color(Lighting.GetSubLight(startPoint)), drawInfo.shadow),
				(chain.links[^1].position - chain.links[^2].position).ToRotation(),
				endTexture.Value.Size() * new Vector2(0f, 0.5f),
				1,
				SpriteEffects.None
			) {
				shader = originPlayer.abyssalAnchorDye
			});
			drawInfo.DrawDataCache.Add(new(
				endGlowTexture,
				startPoint - Main.screenPosition,
				null,
				drawInfo.drawPlayer.GetImmuneAlphaPure(new Color(1f, 1f, 1f, 1f), drawInfo.shadow),
				(chain.links[^1].position - chain.links[^2].position).ToRotation(),
				endTexture.Value.Size() * new Vector2(0f, 0.5f),
				1,
				SpriteEffects.None
			) {
				shader = originPlayer.abyssalAnchorDye
			});
		}
	}
}
