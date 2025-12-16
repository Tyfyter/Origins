using Microsoft.Xna.Framework;
using Origins.Items.Accessories.Eyndum_Cores;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Eyndum_Core_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<OriginPlayer>().eyndumCore?.Value?.ModItem is Eyndum_Core;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Color color = ((Eyndum_Core)drawPlayer.GetModPlayer<OriginPlayer>().eyndumCore.Value.ModItem).CoreGlowColor;
			Vector2 Position = new Vector2(((int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo.bodyVect;
			Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
			DrawData item = new(Origins.eyndumCoreTexture, Position, Frame, color, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
			item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
			drawInfo.DrawDataCache.Add(item);
		}
	}
}
