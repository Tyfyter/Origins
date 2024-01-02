using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories.Eyndum_Cores;
using Origins.Items.Armor.Fiberglass;
using Origins.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Binding_Book_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> endTexture = "Origins/Items/Accessories/Binding_Book_P";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<OriginPlayer>().bindingBookVisual;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Backpacks);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Physics.Chain[] chains = drawInfo.drawPlayer.GetModPlayer<OriginPlayer>().bindingBookChains;
			for (int i = 0; i < chains.Length; i++) {
				Physics.Chain chain = chains[i];
				if (chain is null || chain.links[0].position.HasNaNs() || chain.links[0].position.DistanceSQ(drawInfo.drawPlayer.position) > 512 * 512) {
					Vector2 offset = Vector2.Zero;
					Vector2 gravMod = Vector2.One;
					switch (i) {
						case 0:
						offset = new Vector2(8, -4);
						gravMod = new(1.1f, 0.7f);
						break;

						case 1:
						offset = new Vector2(-8, 0);
						gravMod.X = -1;
						break;

						case 2:
						offset = new Vector2(6, 10);
						gravMod = new(0.5f, 1.2f);
						break;
					}
					var anchor = new Physics.EntityAnchorPoint() {
						entity = drawInfo.drawPlayer,
						offset = offset
					};
					const float spring = 0.5f;
					chains[i] = chain = new Physics.Chain() {
						anchor = anchor,
						links = new Physics.Chain.Link[] {
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 6, null, drag: 0.93f, spring: spring),
							new(anchor.WorldPosition, default, 8, new Physics.Gravity[] { new Physics.EntityDirectionGravity(new Vector2(0.12f, -0.28f) * gravMod, drawInfo.drawPlayer) }, drag: 0.93f, spring: spring)
						}
					};
				}
				Vector2[] deltas = chain.Update();
				if (OriginsModIntegrations.CheckAprilFools()) {
					for (int j = 0; j < deltas.Length; j++) {
						drawInfo.drawPlayer.velocity -= deltas[j] * 0.004f;
					}
				}
				Vector2 startPoint = chain.anchor.WorldPosition;
				for (int j = 0; j < chain.links.Length; j++) {
					/*Rectangle drawRect = new Rectangle(
						(int)Math.Round(startPoint.X - Main.screenPosition.X),
						(int)Math.Round(startPoint.Y - Main.screenPosition.Y),
						(int)Math.Round((chain.links[j].position - startPoint).Length()),
						1);

					drawInfo.DrawDataCache.Add(new(
						Origins.instance.Assets.Request<Texture2D>("Projectiles/Pixel").Value,
						drawRect,
						null,
						new Color((~(j >> 0) & 1) * 255, (~(j >> 1) & 1) * 255, (~(j >> 2) & 1) * 255),
						(chain.links[j].position - startPoint).ToRotation(),
						Vector2.Zero,
						SpriteEffects.None
					));*/
					drawInfo.DrawDataCache.Add(new(
						TextureAssets.Chain.Value,
						chain.links[j].position - Main.screenPosition,
						null,
						new(Lighting.GetSubLight(chain.links[j].position)),
						(chain.links[j].position - startPoint).ToRotation() + MathHelper.PiOver2,
						Vector2.Zero,
						1,
						SpriteEffects.None
					));
					startPoint = chain.links[j].position;
				}
				Rectangle frame = endTexture.Value.Frame(verticalFrames: 4, frameY: ((int)((Main.timeForVisualEffects + i * 2) / 6) + i) % 4);
				drawInfo.DrawDataCache.Add(new(
					endTexture,
					startPoint - Main.screenPosition,
					frame,
					new Color(1f, 1f, 1f, 0.75f),
					0,
					frame.Size() * new Vector2(0.4f, 0.7f),
					1,
					SpriteEffects.None
				));
			}
		}
	}
}
