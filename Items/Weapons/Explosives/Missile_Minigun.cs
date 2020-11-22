using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Microsoft.Xna.Framework.MathHelper;

namespace Origins.Items.Weapons.Explosives {
    public class Missile_Minigun : ModItem {
        public override string Texture => "Terraria/Item_"+ItemID.ProximityMineLauncher;

        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Missile Minigun");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.ProximityMineLauncher);
			item.damage = 70;
			item.useTime = 9;
			item.useAnimation = 9;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Missile_Minigun_P1>();
			item.rare = ItemRarityID.Lime;
            item.autoReuse = true;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			type = item.shoot+(type-item.shoot)/3;
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, 8);
            return false;
        }
    }
    public class Missile_Minigun_P1 : ModProjectile {

        const float force = 1;

        public override string Texture => "Terraria/Projectile_"+ProjectileID.RocketI;
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.RocketI);
            projectile.aiStyle = 0;
            projectile.penetrate = 1;
            projectile.width-=4;
            projectile.height-=4;
            projectile.scale = 0.75f;
        }
        public override void AI() {
            float angle = projectile.velocity.ToRotation();
            projectile.rotation = angle + PiOver2;
            float targetOffset = 0.9f;
            float targetAngle = 1;
            NPC target;
            float dist = 641;
            for(int i = 0; i < Main.npc.Length; i++) {
                target = Main.npc[i];
                if(!target.CanBeChasedBy()) continue;
                //float ta = (float)AngleDif((target.Center - projectile.Center).ToRotation(), angle);
                Vector2 toHit = (projectile.Center.Clamp(target.Hitbox.Add(target.velocity)) - projectile.Center);
                if(!Collision.CanHitLine(projectile.Center+projectile.velocity, 1, 1, projectile.Center+toHit, 1, 1))continue;
                float tdist = toHit.Length();
                float ta = (float)Math.Abs(AngleDif(toHit.ToRotation(), angle));
                if(tdist<=dist && ta<=targetOffset) {
                    targetAngle = ((target.Center+target.velocity) - projectile.Center).ToRotation();
                    targetOffset = ta;
                    dist = tdist;
                }
                /*if(!((Math.Abs(ta)>Math.Abs(targetOffset)) || (tdist>dist))) {
                    targetOffset = ta;
                    dist = tdist;
                }*/
            }
            if(dist<641) projectile.velocity = (projectile.velocity+new Vector2(force,0).RotatedBy(targetAngle)).SafeNormalize(Vector2.Zero)*projectile.velocity.Length();//projectile.velocity = projectile.velocity.RotatedBy(targetOffset);//Clamp(targetOffset, -0.05f, 0.05f)
            int num248 = Dust.NewDust(projectile.Center - projectile.velocity * 0.5f-new Vector2(0,4), 0, 0, 6, 0f, 0f, 100);
			Dust dust3 = Main.dust[num248];
			dust3.scale *= 1f + Main.rand.Next(10) * 0.1f;
			dust3.velocity *= 0.2f;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.RocketI;
            return true;
        }
        public override void Kill(int timeLeft) {
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 64;
			projectile.height = 64;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
        }
    }
    public class Missile_Minigun_P2 : Missile_Minigun_P1 {
        public override string Texture => "Terraria/Projectile_"+ProjectileID.RocketII;
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.RocketII;
            return true;
        }
    }
    public class Missile_Minigun_P3 : Missile_Minigun_P1 {
        public override string Texture => "Terraria/Projectile_"+ProjectileID.RocketIII;
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.RocketIII;
            return true;
        }
        public override void Kill(int timeLeft) {
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 96;
			projectile.height = 96;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
        }
    }
    public class Missile_Minigun_P4 : Missile_Minigun_P3 {
        public override string Texture => "Terraria/Projectile_"+ProjectileID.RocketIV;
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.RocketIV;
            return true;
        }
    }
}
