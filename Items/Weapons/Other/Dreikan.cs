using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Dreikan : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dreikan");
			Tooltip.SetDefault("Like \"Drakin\" with a heavy Aussie accent");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.SniperRifle);
            item.damage = 66;
            item.crit = 26;
            item.useAnimation = 33;
            item.useTime = 11;
            item.width = 100;
            item.height = 24;
            item.shoot = ModContent.ProjectileType<Dreikan_Shot>();
            item.reuseDelay = 6;
            item.scale = 0.75f;
		}
        public override Vector2? HoldoutOffset() {
            return new Vector2(-16,2);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(type==ProjectileID.Bullet)type = item.shoot;
            Main.PlaySound(SoundID.Item, position, 40);
            Main.PlaySound(SoundID.Item, (int)position.X, (int)position.Y, 36, 0.75f);
            OriginGlobalProj.extraUpdatesNext = 2;
            return true;
        }
    }
    public class Dreikan_Shot : ModProjectile {
        public override string Texture => "Terraria/Projectile_286";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dreikan");
            Origins.ExplosiveProjectiles[projectile.type] = true;
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
            aiType = ProjectileID.ExplosiveBullet;
            projectile.light = 0;
        }
        public override void AI() {
	        Lighting.AddLight(projectile.Center, 0.5f, 0.25f, 0.05f);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.Daybreak, 30);
			target.immune[projectile.owner] = 5;
        }
        public override void Kill(int timeLeft) {
			Main.PlaySound(SoundID.Item14, projectile.position);
			for (int i = 0; i < 7; i++){
				Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
			}
			for (int i = 0; i < 3; i++){
				int num568 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2.5f);
				Main.dust[num568].noGravity = true;
				Dust dust1 = Main.dust[num568];
				Dust dust2 = dust1;
				dust2.velocity *= 3f;
				num568 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
				dust1 = Main.dust[num568];
				dust2 = dust1;
				dust2.velocity *= 2f;
			}
			Gore gore = Gore.NewGoreDirect(new Vector2(projectile.position.X - 10f, projectile.position.Y - 10f), default(Vector2), Main.rand.Next(61, 64));
			Gore gore2 = gore;
			gore2.velocity *= 0.3f;
			gore.velocity.X += Main.rand.Next(-10, 11) * 0.05f;
			gore.velocity.Y += Main.rand.Next(-10, 11) * 0.05f;
			if (projectile.owner == Main.myPlayer){
				projectile.localAI[1] = -1f;
				projectile.maxPenetrate = 0;
				projectile.position.X += projectile.width / 2;
				projectile.position.Y += projectile.height / 2;
				projectile.width = 80;
				projectile.height = 80;
				projectile.position.X -= projectile.width / 2;
				projectile.position.Y -= projectile.height / 2;
				projectile.Damage();
			}
        }
    }
}
