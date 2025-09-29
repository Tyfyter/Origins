using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Misc;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Binding_Book_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> endTexture = "Origins/Items/Accessories/Binding_Book_P";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return !drawInfo.drawPlayer.dead && drawInfo.drawPlayer.GetModPlayer<OriginPlayer>().bindingBookVisual;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Backpacks);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			OriginPlayer originPlayer = drawInfo.drawPlayer.GetModPlayer<OriginPlayer>();
			Physics.Chain[] chains = originPlayer.bindingBookChains;
			for (int i = 0; i < chains.Length; i++) {
				Physics.Chain chain = chains[i];
				if (chain is null) continue;
				Vector2 startPoint = chain.anchor.WorldPosition;
				for (int j = 0; j < chain.links.Length; j++) {
					drawInfo.DrawDataCache.Add(new(
						TextureAssets.Chain.Value,
						chain.links[j].position - Main.screenPosition,
						null,
						drawInfo.drawPlayer.GetImmuneAlphaPure(new(Lighting.GetSubLight(chain.links[j].position)), drawInfo.shadow),
						(chain.links[j].position - startPoint).ToRotation() + MathHelper.PiOver2,
						Vector2.Zero,
						1,
						SpriteEffects.None
					) {
						shader = originPlayer.bindingBookDye
					});
					startPoint = chain.links[j].position;
				}
				Rectangle frame = endTexture.Value.Frame(verticalFrames: 4, frameY: ((int)((Main.timeForVisualEffects + i * 2) / 6) + i) % 4);
				drawInfo.DrawDataCache.Add(new(
					endTexture,
					startPoint - Main.screenPosition,
					frame,
					drawInfo.drawPlayer.GetImmuneAlphaPure(new(1f, 1f, 1f, 0.75f), drawInfo.shadow),
					0,
					frame.Size() * new Vector2(0.4f, 0.7f),
					1,
					SpriteEffects.None
				) {
					shader = originPlayer.bindingBookDye
				});
			}
		}
	}
}
