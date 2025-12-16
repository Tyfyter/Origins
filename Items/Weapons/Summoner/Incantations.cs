using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
    public static class Incantations {
		public static Asset<Texture2D> GetSmallTexture(this ModItem item, string suffix = "") => ModContent.RequestIfExists<Texture2D>(item.Texture + "_Smol" + suffix, out Asset<Texture2D> asset) ? asset : null;
		public static void HoldItemFrame(Player player) {
			if (Main.menuMode is MenuID.FancyUI or MenuID.CharacterSelect) return;
			player.SetCompositeArmBack(
				true,
				player.ItemAnimationActive ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.Full,
				(-1f - Math.Clamp((player.velocity.Y - player.GetModPlayer<OriginPlayer>().AverageOldVelocity().Y) / 64, -0.1f, 0.1f)) * player.direction
			);
		}
		public static void DrawInHand(Texture2D smallTexture, ref PlayerDrawSet drawInfo, Color lightColor, Texture2D smallGlowTexture = null, Color? smallGlowColor = null) {
			//if (drawInfo.DrawDataCache[^1].texture.Bounds != new Rectangle(0, 0, 360, 224)) return;
			Vector2 pos = drawInfo.drawPlayer.GetCompositeArmPosition(true);
			pos.Y -= 4 * drawInfo.drawPlayer.gravDir;
			float rotation = drawInfo.drawPlayer.compositeBackArm.rotation * drawInfo.drawPlayer.gravDir + drawInfo.drawPlayer.direction;
			if (drawInfo.drawPlayer.mount?.Active == true && drawInfo.drawPlayer.mount.Type == MountID.Wolf) {
				pos = drawInfo.Position;
				pos.X += drawInfo.drawPlayer.width / 2 + 32 * drawInfo.drawPlayer.direction;
				pos.Y += 17;
				rotation = 0;
				pos.Floor();
			}
			DrawData data = new(
				smallTexture,
				pos - Main.screenPosition,
				null,
				lightColor,
				rotation,
				new Vector2(5 - 2 * drawInfo.drawPlayer.direction, 8),
				1,
				drawInfo.itemEffect
			);
			drawInfo.DrawDataCache.Add(data);
			if (smallGlowTexture is not null) {
				data.texture = smallGlowTexture;
				data.color = smallGlowColor ?? Color.White;
				drawInfo.DrawDataCache.Add(data);
			}
		}
	}
}
