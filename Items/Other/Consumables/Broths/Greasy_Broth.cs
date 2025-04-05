using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Greasy_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(124, 67, 128),
				new Color(89, 52, 96),
				new Color(38, 73, 66)
			];
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Oiled, 360);
			if (MinionGlobalProjectile.IsArtifact(proj)) {
				ref int staticBrothEffectCooldown = ref Main.player[proj.owner].OriginPlayer().staticBrothEffectCooldown;
				if (staticBrothEffectCooldown <= 0) {
					Projectile.NewProjectile(Terraria.Entity.InheritSource(proj), proj.Center, Vector2.Zero, Greasy_Gas.ID, 0, 0);
					staticBrothEffectCooldown = 15;
				}
			}
		}
	}
	public class Greasy_Gas : ModProjectile {
		public static int ID { get; private set; }
		public override string Texture => "Origins/Projectiles/Misc/Smonk";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 74;
			Projectile.height = 68;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			Projectile.netImportant = true;
			Projectile.timeLeft = 60;
			Projectile.scale -= 0.5f;
		}
		public override bool? CanCutTiles() {
			return false;
		}
		public override void AI() {
			#region General behavior
			Projectile.scale += 0.02f;
			Projectile.Opacity -= 0.01f;
			Rectangle hitbox = Projectile.getRect().Scaled(Projectile.scale * 2).Recentered(Projectile.Center);
			//hitbox.DrawDebugOutline();
			Projectile.ai[0] = hitbox.Center().X;
			Projectile.ai[1] = hitbox.BottomLeft().Y;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (hitbox.Intersects(npc.getRect()) && npc.chaseable && !npc.immortal && !npc.friendly) {
					npc.AddBuff(BuffID.OnFire, 360);
				}
			}
			#endregion

			#region Animation and visuals
			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			if (++Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}

			// Some visuals here
			#endregion
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				texture,
				new Vector2(Projectile.ai[0], Projectile.ai[1] - 4) - Main.screenPosition,
				frame,
				Projectile.GetAlpha(lightColor.MultiplyRGBA(new Color(89, 52, 96, 200))),
				Projectile.rotation,
				new Vector2(frame.Width / 2, frame.Height - 4),
				Projectile.scale,
				SpriteEffects.None,
			0);
			return false;
		}
	}
}
