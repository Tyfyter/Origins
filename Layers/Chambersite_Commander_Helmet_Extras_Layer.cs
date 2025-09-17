﻿using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Chambersite;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Chambersite_Commander_Helmet_Extras_Layer : PlayerDrawLayer {
		public AutoLoadingAsset<Texture2D> texture = "Origins/Items/Armor/Chambersite/Chambersite_Helmet_Tail";
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			return drawInfo.drawPlayer.head == Chambersite_Helmet.HeadSlot;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeadBack);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;

			Vector2 Position = new Vector2((int)(drawInfo.Position.X + (float)drawPlayer.width / 2f - (float)drawPlayer.bodyFrame.Width / 2f - Main.screenPosition.X), (int)(drawInfo.Position.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y)) + drawPlayer.headPosition + drawInfo.headVect;
			Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
			DrawData item = new DrawData(texture, Position, Frame, drawInfo.colorArmorHead, drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0);
			item.shader = drawPlayer.cHead;
			drawInfo.DrawDataCache.Add(item);
		}
	}
}
