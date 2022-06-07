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
	public class Pants_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<OriginPlayer>().drawPants;
		}
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Leggings);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
            Player drawPlayer = drawInfo.drawPlayer;
            Vector2 Position = drawInfo.Position;
            int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
            drawData = new DrawData(TextureAssets.Players[skinVariant, 11].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo.legVect, new Rectangle?(drawPlayer.legFrame), drawInfo.colorPants, drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);
            drawData = new DrawData(TextureAssets.Players[skinVariant, 12].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo.legVect, new Rectangle?(drawPlayer.legFrame), drawInfo.colorShoes, drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);
        }
	}
}
