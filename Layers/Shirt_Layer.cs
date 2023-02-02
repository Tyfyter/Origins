using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories.Eyndum_Cores;
using Origins.Items.Armor.Fiberglass;
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
	public class Shirt_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.body == Fiberglass_Body.SlotID;
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.ArmorLongCoat, PlayerDrawLayers.Torso);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			drawInfo.hidesTopSkin = false;
			drawInfo.drawPlayer.body = 0;
			PlayerDrawLayers.DrawPlayer_17_Torso(ref drawInfo);
			PlayerDrawLayers.DrawPlayer_28_ArmOverItem(ref drawInfo);

			//drawInfo.hidesTopSkin = true;
			drawInfo.drawPlayer.body = Fiberglass_Body.SlotID;
			//PlayerDrawLayers.DrawPlayer_17_Torso(ref drawInfo);
			/*Player drawPlayer = drawInfo.drawPlayer;
            Vector2 Position = drawInfo.Position;
            int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
            drawData = new DrawData(TextureAssets.Players[skinVariant, 14].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo.legVect, new Rectangle?(drawPlayer.legFrame), drawInfo.colorShirt, drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);
            if (!drawPlayer.Male) {
                drawData = new DrawData(TextureAssets.Players[skinVariant, 4].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo.colorUnderShirt, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
                drawInfo.DrawDataCache.Add(drawData);
                drawData = new DrawData(TextureAssets.Players[skinVariant, 6].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo.colorShirt, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
                drawInfo.DrawDataCache.Add(drawData);
            } else {
                drawData = new DrawData(TextureAssets.Players[skinVariant, 4].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo.colorUnderShirt, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
                drawInfo.DrawDataCache.Add(drawData);
                drawData = new DrawData(TextureAssets.Players[skinVariant, 6].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo.colorShirt, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
                drawInfo.DrawDataCache.Add(drawData);
            }
            drawData = new DrawData(TextureAssets.Players[skinVariant, 5].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo.colorArmorBody, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);*/
		}
	}
}
