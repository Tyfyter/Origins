using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Felnum {
	public class Shock_Grenade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Shock Grenade");
			Tooltip.SetDefault("Quite shocking");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Grenade);
            //item.maxStack = 999;
            item.damage = 32;
			item.value*=4;
            item.shoot = ModContent.ProjectileType<Shock_Grenade_P>();
			item.shootSpeed*=1.5f;
            item.knockBack = 15f;
            item.ammo = ItemID.Grenade;
			item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
        public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref int damage, ref float knockback) {
            damage-=16;
        }
    }
    public class Shock_Grenade_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Felnum/Shock_Grenade";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Grenade);
            projectile.timeLeft = 135;
            projectile.penetrate = 1;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Grenade;
            return true;
        }
        public override void Kill(int timeLeft) {
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 128;
			projectile.height = 128;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
			Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
            int t = ModContent.ProjectileType<Shock_Grenade_Shock>();
            for(int i = Main.rand.Next(2); i < 3; i++)Projectile.NewProjectile(projectile.Center, Vector2.Zero, t, (int)((projectile.damage-32)*1.5f)+16, 6, projectile.owner);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            Vector2 dest = Vector2.Lerp(target.Center, new Vector2(target.position.X+Main.rand.NextFloat(target.width),target.position.Y+Main.rand.NextFloat(target.height)), 0.5f);
            for(int i = 0; i < 16; i++) {
                Dust.NewDustPerfect(Vector2.Lerp(projectile.Center, dest, i/16f), 226, Main.rand.NextVector2Circular(1,1), Scale:0.5f);
            }
        }
    }
    public class Shock_Grenade_Shock  : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public override bool CloneNewInstances => true;
        Vector2 closest;
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bullet);
            projectile.aiStyle = 0;
            projectile.timeLeft = 3;
            projectile.width = projectile.height = 20;
            projectile.penetrate = 2;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 30;
        }
        public override void AI() {
            if(projectile.penetrate == 1) {
                projectile.penetrate = 2;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            closest = projectile.Center.Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
            return (projectile.Center-closest).Length()<=96;
        }
        public override bool? CanHitNPC(NPC target) {
            return projectile.penetrate>1?base.CanHitNPC(target):false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            projectile.damage-=(int)((projectile.Center-closest).Length()/16f);
            if(Main.rand.Next(5)!=0)projectile.timeLeft+=crit?2:1;
            Vector2 dest = projectile.Center;
            projectile.Center = Vector2.Lerp(closest, new Vector2(target.position.X+Main.rand.NextFloat(target.width),target.position.Y+Main.rand.NextFloat(target.height)), 0.5f);
            for(int i = 0; i < 16; i++) {
                Dust.NewDustPerfect(Vector2.Lerp(projectile.Center, dest, i/16f), 226, Main.rand.NextVector2Circular(1,1), Scale:0.5f);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            Vector2 drawOrigin = new Vector2(1, 1);
            Rectangle drawRect = new Rectangle(
                (int)Math.Round(projectile.Center.X - Main.screenPosition.X),
                (int)Math.Round(projectile.Center.Y - Main.screenPosition.Y),
                (int)Math.Round((projectile.oldPosition - projectile.position).Length()),
                2);

            spriteBatch.Draw(mod.GetTexture("Projectiles/Pixel"), drawRect, null, Color.Aquamarine, (projectile.oldPosition - projectile.position).ToRotation(), Vector2.Zero, SpriteEffects.None, 0);
            Vector2 dest = (projectile.oldPosition - projectile.position)+new Vector2(projectile.width,projectile.height)/2;
            for(int i = 0; i < 16; i++) {
                Dust.NewDustPerfect(Vector2.Lerp(projectile.Center, dest, i/16f), 226, Main.rand.NextVector2Circular(1,1), Scale:0.5f);
            }
            return false;
        }
    }
}
