using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.NPCs {
	public abstract class Glowing_Mod_NPC : ModNPC {
		public virtual string GlowTexturePath => Texture + "_Glow";
		private Asset<Texture2D> _glowTexture;
		public Texture2D GlowTexture => (_glowTexture ??= (ModContent.RequestIfExists<Texture2D>(GlowTexturePath, out var asset) ? asset : null))?.Value;
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (GlowTexture is not null) {
				SpriteEffects spriteEffects = SpriteEffects.None;
				if (NPC.spriteDirection == 1) {
					spriteEffects = SpriteEffects.FlipHorizontally;
				}
				Vector2 halfSize = new Vector2(GlowTexture.Width / 2, GlowTexture.Height / Main.npcFrameCount[NPC.type] / 2);
				spriteBatch.Draw(
					GlowTexture,
					new Vector2(NPC.position.X - screenPos.X + (NPC.width / 2) - GlowTexture.Width * NPC.scale / 2f + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - GlowTexture.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + halfSize.Y * NPC.scale + Main.NPCAddHeight(NPC) + NPC.gfxOffY),
					NPC.frame,
					Color.White,
					NPC.rotation,
					halfSize,
					NPC.scale,
					spriteEffects,
				0);
			}
		}
	}
}
