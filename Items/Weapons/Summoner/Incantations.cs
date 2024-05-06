using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
    public static class Incantations {
		public static Asset<Texture2D> GetSmallTexture(this ModItem item, string suffix = "") => ModContent.RequestIfExists<Texture2D>(item.Texture + "_Smol" + suffix, out var asset) ? asset : null;
		public static void HoldItemFrame(Player player) {
			if (Main.menuMode is MenuID.FancyUI or MenuID.CharacterSelect) return;
			player.SetCompositeArmBack(
				true,
				player.ItemAnimationActive ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.Full,
				(-1f - Math.Clamp((player.velocity.Y - player.GetModPlayer<OriginPlayer>().AverageOldVelocity().Y) / 64, -0.1f, 0.1f)) * player.direction
			);
		}
		public static void DrawInHand(Texture2D smallTexture, ref PlayerDrawSet drawInfo, Color lightColor, Texture2D smallGlowTexture = null, Color? smallGlowColor = null) {
			if (drawInfo.DrawDataCache[^1].texture.Bounds != new Rectangle(0, 0, 360, 224)) return;
			Vector2 pos = drawInfo.drawPlayer.GetBackHandPosition(drawInfo.drawPlayer.compositeBackArm.stretch, drawInfo.drawPlayer.compositeBackArm.rotation);
			pos += drawInfo.DrawDataCache[^1].position - drawInfo.drawPlayer.position - new Vector2(10 + 6 * drawInfo.drawPlayer.direction, 24);
			DrawData data = new(
				smallTexture,
				pos,
				null,
				lightColor,
				drawInfo.drawPlayer.compositeBackArm.rotation + drawInfo.drawPlayer.direction,
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
