﻿using Origins.Items.Armor.Fiberglass;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Pants_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.legs == Fiberglass_Legs.SlotID;
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.Skin, PlayerDrawLayers.Leggings);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			drawInfo.hidesBottomSkin = false;
			drawInfo.drawPlayer.legs = 0;
			PlayerDrawLayers.DrawPlayer_13_Leggings(ref drawInfo);

			drawInfo.hidesBottomSkin = true;
			drawInfo.drawPlayer.legs = Fiberglass_Legs.SlotID;
			PlayerDrawLayers.DrawPlayer_13_Leggings(ref drawInfo);
			/*Vector2 Position = drawInfo.Position;
            int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
            drawData = new DrawData(TextureAssets.Players[skinVariant, 11].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo.legVect, new Rectangle?(drawPlayer.legFrame), drawInfo.colorPants, drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);
            drawData = new DrawData(TextureAssets.Players[skinVariant, 12].Value, new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo.legVect, new Rectangle?(drawPlayer.legFrame), drawInfo.colorShoes, drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0);
            drawInfo.DrawDataCache.Add(drawData);*/
		}
	}
}
