using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria;
using PegasusLib;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

namespace Origins.Dusts {
	public class Felnum_Enemy_Death_Dust : ModDust {
		public override string Texture => "Origins/Items/Weapons/Summoner/Maelstrom_Incantation_Large_P";
		public override void OnSpawn(Dust dust) {
			dust.alpha = 0;
			dust.frame = new(0, 0, 0, 0);
		}
		public override bool Update(Dust dust) {
			if (--dust.alpha <= -8) {
				if (dust.alpha > -12) {
					if (dust.alpha == -8) {
						SoundEngine.PlaySound(SoundID.Item122.WithPitchRange(0.9f, 1.1f).WithVolume(2), dust.position);
						SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithPitchRange(0.0f, 0.2f).WithVolume(0.75f), dust.position);
					}
					//Projectile.scale += 0.5f;
				} else {
					if (dust.alpha < -16) {
						dust.active = false;
					}
				}
				float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
				Vector2 targetEnd = OriginExtensions.Vec2FromPolar(angle, Main.rand.NextFloat(18, 24)) + dust.position;
				Vector2 targetStart = OriginExtensions.Vec2FromPolar(angle, Main.rand.NextFloat(2)) + dust.position;
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					Projectile.NewProjectile(
						Entity.GetSource_None(),
						targetEnd,
						default,
						Projectiles.Misc.Felnum_Shock_Arc.ID,
						0,
						0,
						Owner: Main.myPlayer,
						ai0: targetStart.X,
						ai1: targetStart.Y
					);
				}
				dust.frame.X++;
				if (dust.frame.X >= 5) {
					dust.frame.X = 0;
					dust.frame.Y++;
					if (dust.frame.Y >= 4) {
						dust.frame.Y = 0;
					}
				}
			} else {
				dust.scale = 2.5f;
			}
			return false;
		}
		public override bool PreDraw(Dust dust) {
			const int large_frames = 3;
			Texture2D texture = Texture2D.Value;
			int frame = (int)(dust.alpha * (float)large_frames / -32f);
			int frameCount = large_frames;
			float scale = 1;

			int width = texture.Width;
			int frameHeight = texture.Height / frameCount;
			int frameY = frameHeight * frame;
			Main.EntitySpriteDraw(
				texture,
				dust.position - Main.screenPosition,
				new Rectangle(0, frameY, width, frameHeight),
				Color.White,
				dust.rotation,
				new Vector2(width * 0.5f, frameHeight * 0.5f),
				scale,
				0,
				0
			);
			return false;
		}
	}
}
