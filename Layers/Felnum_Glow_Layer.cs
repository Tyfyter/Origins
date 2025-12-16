using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
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
			Vector2 Position = default;
			Rectangle Frame;
			Texture2D Texture;
			DrawData item;
			int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock * 255) / drawPlayer.statLifeMax2, 255), 1);
			if (drawPlayer.head == Origins.AncientFelnumHeadArmorID || drawInfo.fullHair || drawInfo.hatHair || drawPlayer.head == ArmorIDs.Head.FamiliarWig) {
				Vector2 bodyOffset = new(-drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4);
				Position = (drawInfo.Position - Main.screenPosition + bodyOffset).Floor() + drawInfo.drawPlayer.headPosition + drawInfo.headVect;
				Frame = drawPlayer.bodyFrame;

				Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Felnum_Glow_Eye").Value;
				item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0);
				item.shader = drawInfo.cHead;
				drawInfo.DrawDataCache.Add(item);
			}
			if (drawPlayer.body == Origins.FelnumBodyArmorID) {// since the tML support for torso glowmasks doesn't seem to work
				Frame = drawInfo.compTorsoFrame;
				int armorAdjust = drawInfo.armorAdjust;
				Frame.X += armorAdjust;
				Frame.Width -= armorAdjust;
				Position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (drawInfo.drawPlayer.bodyFrame.Width / 2) + (drawInfo.drawPlayer.width / 2)) + armorAdjust, (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
				Vector2 headgearOffset = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
				headgearOffset.Y -= 2f;
				Position += headgearOffset * -drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
				Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Felnum_Breastplate_Body_Glow").Value;
				item = new DrawData(Texture, Position, drawInfo.compTorsoFrame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
				item.shader = drawInfo.cBody;
				drawInfo.DrawDataCache.Add(item);
			} else if (drawPlayer.body == Origins.AncientFelnumBodyArmorID) {// since the tML support for torso glowmasks doesn't seem to work
				Frame = drawInfo.compTorsoFrame;
				int armorAdjust = drawInfo.armorAdjust;
				Frame.X += armorAdjust;
				Frame.Width -= armorAdjust;
				Position = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (drawInfo.drawPlayer.bodyFrame.Width / 2) + (drawInfo.drawPlayer.width / 2)) + armorAdjust, (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
				Vector2 headgearOffset = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
				headgearOffset.Y -= 2f;
				Position += headgearOffset * -drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
				Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Ancient_Felnum_Breastplate_Body_Glow").Value;
				item = new DrawData(Texture, Position, drawInfo.compTorsoFrame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
				item.shader = drawInfo.cBody;
				drawInfo.DrawDataCache.Add(item);
			}
			//Texture = ModContent.Request<Texture2D>("Origins/Items/Armor/Felnum/Felnum_Glow_Body").Value;
			//item = new DrawData(Texture, Position, drawInfo.compTorsoFrame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0);
			//drawInfo.DrawDataCache.Add(item);
		}
	}
}
