using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo.Canisters {
	public class Grenade_Launcher_Canister_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Grenade_Launcher_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Grenade_Launcher_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			CanisterGlobalItem.RegisterForLauncher(ItemID.GrenadeLauncher, Type);
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GrenadeI);
		}
		public override void AI() {
			if (Projectile.ai[0] > 5f) {
				Projectile.velocity.Y -= 0.2f;
				this.DoGravity(0.2f);
			}
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 origin = OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
		}
	}
	public class Proximity_Mine_Launcher_Canister_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Proximity_Mine_Launcher_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Proximity_Mine_Launcher_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			CanisterGlobalItem.RegisterForLauncher(ItemID.ProximityMineLauncher, Type);
			ProjectileID.Sets.IsAMineThatDealsTripleDamageWhenStationary[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			if (Projectile.velocity.X > -0.2 && Projectile.velocity.X < 0.2 && Projectile.velocity.Y > -0.2 && Projectile.velocity.Y < 0.2) {
				Projectile.alpha += 2;
				if (Projectile.alpha > 200)
					Projectile.alpha = 200;
			} else {
				Projectile.alpha = 0;
				Dust dust = Dust.NewDustDirect(new Vector2(Projectile.position.X + 3f, Projectile.position.Y + 3f) - Projectile.velocity * 0.5f, Projectile.width - 8, Projectile.height - 8, DustID.Smoke, 0f, 0f, 100);
				dust.scale *= 1.6f + Main.rand.Next(5) * 0.1f;
				dust.velocity *= 0.05f;
				dust.noGravity = true;
			}
			this.DoGravity(0.2f);
			Projectile.velocity *= 0.97f;
			if (Projectile.velocity.X > -0.1 && Projectile.velocity.X < 0.1)
				Projectile.velocity.X = 0f;

			if (Projectile.velocity.Y > -0.1 && Projectile.velocity.Y < 0.1)
				Projectile.velocity.Y = 0f;
			Projectile.rotation += Projectile.velocity.X * 0.1f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.Kill();
		}
		public override bool PreKill(int timeLeft) {
			if (Projectile.velocity.Length() < 0.5f * 0.5f) Projectile.damage *= 3;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) Projectile.velocity.X = oldVelocity.X * -0.4f;
			if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0.7) Projectile.velocity.Y = oldVelocity.Y * -0.4f;
			return false;
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			lightColor *= (255 - projectile.alpha) / 255f;
			Vector2 origin = OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
		}
	}
}
