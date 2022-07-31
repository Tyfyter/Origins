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
	public class Torso_Overhang_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.body == Origins.PlagueTexanJacketID;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Leggings);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
            Player drawPlayer = drawInfo.drawPlayer;
            AutoCastingAsset<Texture2D> texture = TextureAssets.ArmorBodyComposite[Origins.PlagueTexanJacketID];

            Vector2 Position = new Vector2(((int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo.bodyVect;
            Rectangle Frame = drawPlayer.bodyFrame;
            Position.Y += Frame.Height - 2;
            Frame.Y += Frame.Height - 2;
            Frame.Height = 2;
            DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
            drawInfo.DrawDataCache.Add(item);
        }
	}
}
