using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Legs_Glow_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return Origins.LeggingGlowMasks.ContainsKey(drawInfo.drawPlayer.legs);
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Texture2D texture = Origins.LeggingGlowMasks[drawPlayer.legs];

			Vector2 Position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo.legVect;
			Rectangle? Frame = new Rectangle?(drawPlayer.legFrame);
			DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0);
			item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[2].type);
			drawInfo.DrawDataCache.Add(item);
		}
	}
}
