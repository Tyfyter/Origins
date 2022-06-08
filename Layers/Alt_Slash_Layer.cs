using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Alt_Slash_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.ItemAnimationActive && drawInfo.heldItem.useStyle == Terraria.ID.ItemUseStyleID.Swing && drawInfo.heldItem.ModItem is AnimatedModItem;
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.HeldItem, PlayerDrawLayers.ArmOverItem);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
            Player drawPlayer = drawInfo.drawPlayer;
            float num77 = drawPlayer.itemRotation + MathHelper.PiOver4 * drawPlayer.direction;
            Item item = drawPlayer.inventory[drawPlayer.selectedItem];
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;
            AnimatedModItem aItem = (AnimatedModItem)item.ModItem;
            Rectangle frame = aItem.Animation.GetFrame(itemTexture);
            Color currentColor = Lighting.GetColor((int)(drawInfo.Position.X + drawPlayer.width * 0.5) / 16, (int)((drawInfo.Position.Y + drawPlayer.height * 0.5) / 16.0));
            SpriteEffects spriteEffects = (drawPlayer.direction == 1 ? 0 : SpriteEffects.FlipHorizontally) | (drawPlayer.gravDir == 1f ? 0 : SpriteEffects.FlipVertically);
            DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y)), frame, drawPlayer.inventory[drawPlayer.selectedItem].GetAlpha(currentColor), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
            drawInfo.DrawDataCache.Add(value);
            if (drawPlayer.inventory[drawPlayer.selectedItem].color != default) {
                value = new DrawData(itemTexture, new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y)), frame, drawPlayer.inventory[drawPlayer.selectedItem].GetColor(currentColor), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(value);
            }
            if (drawPlayer.inventory[drawPlayer.selectedItem].glowMask != -1) {
                value = new DrawData(TextureAssets.GlowMask[drawPlayer.inventory[drawPlayer.selectedItem].glowMask].Value, new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y)), frame, aItem.GlowmaskTint ?? new Color(250, 250, 250, item.alpha), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(value);
            }
        }
	}
}
