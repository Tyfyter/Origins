using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.NPCs;

namespace Origins.Projectiles.Misc {
	public class Felnum_Shock_Leader : ModProjectile {
		public static int ID { get; private set; }
		public Entity Parent { get; internal set; }
		public event Action OnStrike;
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Felnum Shock");
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 30;
			Projectile.extraUpdates = 29;
			Projectile.width = Projectile.height = 2;
			Projectile.penetrate = 1;
			Projectile.light = 0;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.X -= 2;
			hitbox.Y -= 2;
			hitbox.Width += 4;
			hitbox.Height += 4;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.ModifyHitInfo += (ref NPC.HitInfo info) => {
				if (info.Damage >= target.life) {
					info.Damage = target.life - 1;
					target.GetGlobalNPC<OriginGlobalNPC>().shockTime = 15;
				}
			};
			modifiers.ScalingArmorPenetration += 1;
		}

		public override void OnKill(int timeLeft) {
			if (timeLeft > 0) {
				if (!(OnStrike is null)) OnStrike();
				SoundEngine.PlaySound(SoundID.Item60.WithVolume(0.65f).WithPitch(1f), Projectile.Center);
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Felnum_Shock_Arc.ID, Projectile.damage, 0, Projectile.owner, Projectile.ai[0], Projectile.ai[1]);
				if (proj.ModProjectile is Felnum_Shock_Arc shock) {
					shock.Parent = Parent;
				}
			}
		}
	}
	public class Felnum_Shock_Arc : ModProjectile {
		public static int ID { get; private set; }
		public Entity Parent { get; internal set; }
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetStaticDefaults() {
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.timeLeft = 10;
			Projectile.width = Projectile.height = 0;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.localAI[0] = (float)Math.Pow(Main.rand.NextFloat(-4, 4), 2);
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X -= 4;
			Projectile.position.Y -= 4;
			Projectile.width = Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.Damage();
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 arcStart = (Parent?.position ?? Vector2.Zero) + new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Main.spriteBatch.DrawLightningArcBetween(
				arcStart - Main.screenPosition,
				Projectile.position - Main.screenPosition,
				Projectile.localAI[0]);
			//projectile.localAI[0] *= 0.8f;
			for (int i = 0; i < 16; i++) {
				Lighting.AddLight(Vector2.Lerp(Projectile.Center, arcStart, i / 16f), 0.15f, 0.4f, 0.43f);
			}
			return false;
		}
	}
}
