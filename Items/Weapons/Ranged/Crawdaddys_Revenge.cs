using Origins.Dev;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Crawdaddys_Revenge : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Gun
		];
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.crawdadNetworkEffect = true;
			});
		}
		public override void SetDefaults() {
			Item.DefaultToRangedWeapon(ProjectileID.Bullet, AmmoID.Bullet, 10, 9.1f, true);
			Item.damage = 30;
			Item.knockBack = 2;
			Item.UseSound = SoundID.Item11;
			Item.width = 86;
			Item.height = 22;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 2, silver: 50);
		}
		public override Vector2? HoldoutOffset() => new(-10, 1);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(0.06f);
		}
	}
	public class Crawdaddys_Revenge_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 4;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.frame < 3) return false;
			return base.Colliding(projHitbox, targetHitbox);
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.velocity.X > 0) {
				Projectile.spriteDirection = -1;
			} else {
				Projectile.rotation += MathHelper.Pi;
			}
			if (++Projectile.frameCounter >= 4) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame--;
					Projectile.timeLeft = Math.Min(Projectile.timeLeft, 8);
				}
			}
		}
	}
}
