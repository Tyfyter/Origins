using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace Origins.Projectiles.Weapons {
	public class Lava_Shot : ModProjectile {
		public static DamageClass damageType;
		public override void Unload() {
			damageType = null;
		}
		public float frameCount = 15;
		public override string Texture => "Origins/Projectiles/Weapons/Lava_Cast_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 2;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Flamelash);
			Projectile.light = 0;
			Projectile.aiStyle = 1;
			Projectile.extraUpdates++;
			Projectile.timeLeft = 300;
			Projectile.DamageType = damageType ?? DamageClass.Magic;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 0.75f, 0.35f, 0f);
			if (++Projectile.frameCounter >= (int)frameCount) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= 2) {
					Projectile.frame = 0;
				}
			}
			if (Projectile.wet) {
				Projectile.timeLeft--;
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BreatheBubble);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, hit.Crit ? 600 : 300);
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item167, Projectile.position);
			for (int i = 0; i < 8; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Lava);
				dust.noGravity = false;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			//
			//float alpha = (float)Math.Pow(projectile.frameCounter/frameCount, 0.5f);
			//float alpha = projectile.frameCounter<=3 ? 0.5f : 1;
			bool m = Projectile.frameCounter <= 3;
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;//0f/projectile.frameCounter
			bool flip = Projectile.velocity.X < 0;
			float fade = (float)Math.Sqrt(Projectile.timeLeft / 300f);
			//projectile.frame^1: bitwise XOR with 1 to use the other frame's height
			if (m) Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, (Projectile.frame ^ 1) * 26, 56, 26), new Color(1f, 1f, 1f, 0.5f) * fade, Projectile.rotation - MathHelper.PiOver2, new Vector2(44, flip ? 13 : 10), Projectile.scale, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * 26, 56, 26), new Color(1f, 1f, 1f, m ? 0.5f : 1f) * fade, Projectile.rotation - MathHelper.PiOver2, new Vector2(44, flip ? 13 : 10), Projectile.scale, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
			return false;
		}
	}
}
