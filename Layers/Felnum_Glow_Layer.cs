using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Felnum_Glow_Layer : PlayerDrawLayer {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.GetModPlayer<OriginPlayer>().felnumShock > 0;
		}
		public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.FaceAcc, PlayerDrawLayers.MountFront);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 Position;
			Rectangle? Frame;
			Texture2D Texture;
			DrawData item;
			int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock * 255) / drawPlayer.statLifeMax2, 255), 1);
			if (drawPlayer.head == Origins.FelnumHeadArmorID) {
				Position = new Vector2(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f, drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition + drawInfo.headVect;
				Frame = new Rectangle?(drawPlayer.bodyFrame);
				Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Felnum_Glow_Head").Value;
				item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0);
				item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
				drawInfo.DrawDataCache.Add(item);
				Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Felnum_Glow_Eye").Value;
				item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0);
				item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
				drawInfo.DrawDataCache.Add(item);
			} else if (drawInfo.fullHair || drawInfo.hatHair || drawPlayer.head == ArmorIDs.Head.FamiliarWig) {
				Position = new Vector2(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f, drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition + drawInfo.headVect;
				Frame = new Rectangle?(drawPlayer.bodyFrame);
				Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Felnum_Glow_Eye").Value;
				item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0);
				item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
				drawInfo.DrawDataCache.Add(item);
			}
			if (drawPlayer.body == Origins.FelnumBodyArmorID) {
				Position = new Vector2(((int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo.bodyVect;
				Frame = new Rectangle?(drawPlayer.bodyFrame);
				Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Felnum_Glow_Arms").Value;
				item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
				item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
				drawInfo.DrawDataCache.Add(item);

				Position = new Vector2(((int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo.bodyVect;
				Frame = new Rectangle?(drawPlayer.bodyFrame);
				Texture = ModContent.Request<Texture2D>(drawPlayer.Male ? "Origins/Items/Armor/Felnum/Felnum_Glow_Body" : "Origins/Items/Armor/Felnum/Felnum_Glow_FemaleBody").Value;
				item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
				item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
				drawInfo.DrawDataCache.Add(item);
			}
			if (drawPlayer.legs == Origins.FelnumLegsArmorID) {
				Position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo.legVect;
				Frame = new Rectangle?(drawPlayer.legFrame);
				Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Felnum_Glow_Legs").Value;
				item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.legRotation, drawInfo.legVect, 1f, drawInfo.playerEffect, 0);
				item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[2].type);
				drawInfo.DrawDataCache.Add(item);
			}
		}
	}
}
