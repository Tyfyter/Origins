using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.Graphics.Shaders;

namespace Origins.Items.Weapons.Explosives {
    public class Fallout : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fallout");
			Tooltip.SetDefault("");
            glowmask = Origins.AddGlowMask(this);
        }
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.ProximityMineLauncher);
			item.damage = 250;
			item.useTime = 90;
			item.useAnimation = 90;
			item.value = 5000;
            item.shootSpeed*=0.75f;
            item.shoot = ModContent.ProjectileType<Fallout_P1>();
			item.rare = ItemRarityID.Lime;
            item.autoReuse = true;
            item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			//type = item.shoot+(type-item.shoot)/3;
            Projectile.NewProjectile(position, new Vector2(speedX, speedY), item.shoot, damage, knockBack, player.whoAmI, 0, type-item.shoot+ProjectileID.RocketI);
            return false;
        }
    }
    public class Fallout_P1 : ModProjectile {
        public override string Texture => "Terraria/Projectile_"+ProjectileID.RocketI;
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.RocketI);
            projectile.aiStyle = 0;
            projectile.penetrate = 1;
            projectile.width+=4;
            projectile.height+=4;
            projectile.scale = 1.25f;
        }
        public override void AI() {
            projectile.rotation = projectile.velocity.ToRotation() + PiOver2;
            int num248 = Dust.NewDust(projectile.Center - projectile.velocity * 0.5f-new Vector2(0,4), 0, 0, 6, 0f, 0f, 100);
			Dust dust3 = Main.dust[num248];
			dust3.scale *= 1f + Main.rand.Next(10) * 0.1f;
			dust3.velocity *= 0.2f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.Kill();
            return false;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.RocketI;
            return true;
        }
        public override void Kill(int timeLeft) {
            projectile.ai[0] = 1;
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 640;
			projectile.height = 640;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<Fallout_Field>(), projectile.damage, projectile.knockBack, projectile.owner, 0, projectile.ai[1]);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            return projectile.ai[0]>0?(bool?)((projectile.Center.Clamp(targetHitbox)-projectile.Center).Length()<=320):null;
        }
    }
    public class Fallout_Field : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public List<Vector2> targets = new List<Vector2>(){};
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.RocketI);
            projectile.aiStyle = 0;
            projectile.timeLeft = 900;
            projectile.tileCollide = false;
			projectile.width = 640;
			projectile.height = 640;
        }
        public override void AI() {
            if(projectile.timeLeft%15==0) {
                Vector2 offset = Main.rand.NextVector2Circular(160, 160)+Main.rand.NextVector2Circular(160, 160);
                if(targets.Count>0&&!Main.rand.NextBool(3))offset = Main.rand.Next(targets);
                Projectile.NewProjectile(projectile.Center+offset, Vector2.Zero, ModContent.ProjectileType<Fallout_Cloud>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.ai[1]);
                targets.Clear();
            }
            for(int i = 0; i < 6; i++) {
                Dust dust = Dust.NewDustDirect(projectile.Center+Main.rand.NextVector2Circular(140,140)+Main.rand.NextVector2Circular(140,140), 0, 0, 226, 0, 0, 100, new Color(0, 255, 0), 0.75f*projectile.scale);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.noGravity = true;
                dust.noLight = true;
            }
        }
        public override void Kill(int timeLeft) {
            projectile.ai[0] = 1;
			projectile.Damage();
        }
        public override bool? CanHitNPC(NPC target) {
            if((target.Center-projectile.Center).Length()<320)targets.Add(target.Center-projectile.Center);
            return projectile.ai[0]>0?null:(bool?)false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            return (projectile.Center.Clamp(targetHitbox)-projectile.Center).Length()<=320;
        }
    }
    public class Fallout_Cloud : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.RocketI);
            projectile.aiStyle = 0;
            projectile.timeLeft = 0;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = (int)projectile.ai[0];
            return true;
        }
        public override void Kill(int timeLeft) {
            for(int i = 0; i < 7; i++) {
                Dust dust = Dust.NewDustDirect(projectile.position, 10, 10, 226, 0, 0, 100, new Color(0, 255, 0), 1.25f*projectile.scale);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.noGravity = true;
                dust.noLight = true;
            }
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = projectile.ai[0]>ProjectileID.RocketII? 128: 96;
			projectile.height = projectile.width;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            return (projectile.Center.Clamp(targetHitbox)-projectile.Center).Length()<=(projectile.ai[0]>ProjectileID.RocketII? 72:48);
        }
    }
}
