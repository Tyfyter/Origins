using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles.Weapons {
	public class Gooey_Exaultion_P : ModProjectile, IElementalProjectile {
		public float frameCount = 15;
		public ushort Element => Elements.Acid;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.penetrate = 1;//when projectile.penetrate reaches 0 the projectile is destroyed
			Projectile.extraUpdates = 1;
			Projectile.width = Projectile.height = 10;
			Projectile.light = 0;
			Projectile.timeLeft = 180;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 0, 0.75f * Projectile.scale, 0.35f * Projectile.scale);
			if (++Projectile.frameCounter >= (int)frameCount) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= 2) {
					Projectile.frame = 0;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
			target.AddBuff(BuffID.OgreSpit, 480);
			Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 1.25f * Projectile.scale);
			dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
			dust.noGravity = false;
			dust.noLight = true;
		}
		public override bool PreDraw(ref Color lightColor) {
			//
			//float alpha = (float)Math.Pow(projectile.frameCounter/frameCount, 0.5f);
			//float alpha = projectile.frameCounter<=3 ? 0.5f : 1;
			bool m = false;//projectile.frameCounter <= 3;
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;//0f/projectile.frameCounter
			bool flip = Projectile.velocity.X < 0;
			float fade = (float)Math.Sqrt(Projectile.timeLeft / 300f);
			//projectile.frame^1: bitwise XOR with 1 to use the other frame's height
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * 14, 34, 14), new Color(1f, 1f, 1f, m ? 0.5f : 1f) * fade, Projectile.rotation - MathHelper.PiOver2, new Vector2(26, 7), Projectile.scale, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
			return false;
		}
	}
}