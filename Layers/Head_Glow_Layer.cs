using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Head_Glow_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return Origins.HelmetGlowMasks.ContainsKey(drawInfo.drawPlayer.head);
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Texture2D texture = Origins.HelmetGlowMasks[drawPlayer.head];

			Vector2 Position = new Vector2((int)(drawInfo.Position.X + (float)drawPlayer.width / 2f - (float)drawPlayer.bodyFrame.Width / 2f - Main.screenPosition.X), (int)(drawInfo.Position.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y)) + drawPlayer.headPosition + drawInfo.headVect;
			Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
			DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0);
			item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
			drawInfo.DrawDataCache.Add(item);
		}
	}
}
