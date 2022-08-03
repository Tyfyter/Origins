using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories.Eyndum_Cores;
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
	public class Body_Glow_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return Origins.BreastplateGlowMasks.ContainsKey(drawInfo.drawPlayer.body) || Origins.BreastplateGlowMasks.ContainsKey(-drawInfo.drawPlayer.body);
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
            Player drawPlayer = drawInfo.drawPlayer;
            AutoCastingAsset<Texture2D> texture;
            if (!Origins.BreastplateGlowMasks.TryGetValue(drawPlayer.Male ? drawPlayer.body : -drawPlayer.body, out texture)) {
                texture = Origins.BreastplateGlowMasks[drawPlayer.Male ? -drawPlayer.body : drawPlayer.body];
            }

			if (drawInfo.usesCompositeTorso) {
                Rectangle Frame = drawInfo.compTorsoFrame;

                Vector2 Position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (drawInfo.drawPlayer.bodyFrame.Width / 2) + (drawInfo.drawPlayer.width / 2)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
                Vector2 value = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
                value.Y -= 2f;
                Position += value * -drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();

                DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
                drawInfo.DrawDataCache.Add(item);
            } else {
                Rectangle Frame = drawPlayer.bodyFrame;

                Vector2 Position = new Vector2(((int)(drawInfo.Position.X - Main.screenPosition.X - Frame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - Frame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo.bodyVect;
                DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
                drawInfo.DrawDataCache.Add(item);
            }
        }
	}
}
