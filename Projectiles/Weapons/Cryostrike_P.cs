using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using static Origins.OriginExtensions;

namespace Origins.Projectiles.Weapons {
	public class Cryostrike_P : ModProjectile {
		public static float margin = 0.5f;
		float stabVel = 1;
		float drawOffsetY;
		public override string Texture => "Origins/Projectiles/Weapons/Icicle_P";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.coldDamage = true;
			Projectile.penetrate = -1;//when projectile.penetrate reaches 0 the projectile is destroyed
			Projectile.extraUpdates = 1;
			Projectile.aiStyle = ProjAIStyleID.Arrow;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
			drawOffsetY = 0;//-34;
			DrawOriginOffsetX = -0.5f;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.aiStyle == 0) {
				if (drawOffsetY > -20) {
					drawOffsetY -= stabVel;
				} else {
					drawOffsetY = -20;
				}
				Projectile.velocity = Vector2.Zero;
			}
			DrawOriginOffsetY = (int)drawOffsetY;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.aiStyle != 0) {
				Projectile.aiStyle = 0;
				Projectile.knockBack = 0.1f;
				Projectile.timeLeft = 180;
				Projectile.usesLocalNPCImmunity = true;
				stabVel = Projectile.velocity.Length() / 2;//(oldVelocity-projectile.velocity).Length();
			}
			return false;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			if (Projectile.aiStyle == 0) {
				hitbox = hitbox.Add(new Vector2(0, -1.5f).RotatedBy(Projectile.rotation) * (-34 - drawOffsetY));
			}
		}
		public override bool? CanHitNPC(NPC target) {
			return (Projectile.aiStyle != 0 || PokeAngle(target.velocity)) ? null : new bool?(false);
		}
		bool PokeAngle(Vector2 velocity) {
			return NormDot(velocity, Vec2FromPolar(Projectile.rotation - MathHelper.PiOver2)) > 1f - margin;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Frostburn, 180);
		}
	}
}
