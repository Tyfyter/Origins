using Microsoft.Xna.Framework;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Dreikan : ModItem {
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				proj.extraUpdates += 2;
			});
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 66;
			Item.crit = 26;
			Item.useAnimation = 33;
			Item.useTime = 11;
			Item.width = 100;
			Item.height = 24;
			Item.shoot = ModContent.ProjectileType<Dreikan_Shot>();
			Item.reuseDelay = 6;
			Item.scale = 0.75f;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-16, 2);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) type = Item.shoot;
			SoundEngine.PlaySound(SoundID.Item40, position);
			SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), position);
		}
	}
	public class Dreikan_Shot : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_286";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			AIType = ProjectileID.ExplosiveBullet;
			Projectile.light = 0;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0.05f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Daybreak, 30);
			target.immune[Projectile.owner] = 5;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			for (int i = 0; i < 7; i++) {
				Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
			}
			for (int i = 0; i < 3; i++) {
				int num568 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default(Color), 2.5f);
				Main.dust[num568].noGravity = true;
				Dust dust1 = Main.dust[num568];
				Dust dust2 = dust1;
				dust2.velocity *= 3f;
				num568 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default(Color), 1.5f);
				dust1 = Main.dust[num568];
				dust2 = dust1;
				dust2.velocity *= 2f;
			}
			Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.position.X - 10f, Projectile.position.Y - 10f), default(Vector2), Main.rand.Next(61, 64));
			Gore gore2 = gore;
			gore2.velocity *= 0.3f;
			gore.velocity.X += Main.rand.Next(-10, 11) * 0.05f;
			gore.velocity.Y += Main.rand.Next(-10, 11) * 0.05f;
			if (Projectile.owner == Main.myPlayer) {
				Projectile.localAI[1] = -1f;
				Projectile.maxPenetrate = 0;
				Projectile.position.X += Projectile.width / 2;
				Projectile.position.Y += Projectile.height / 2;
				Projectile.width = 80;
				Projectile.height = 80;
				Projectile.position.X -= Projectile.width / 2;
				Projectile.position.Y -= Projectile.height / 2;
				Projectile.Damage();
			}
		}
	}
}
