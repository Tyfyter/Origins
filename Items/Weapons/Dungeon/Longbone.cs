using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Dungeon {
    public class Longbone : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bolterbow");
            Tooltip.SetDefault("maybe placeholder sprite, just removed skull from unused vanilla sprite");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.GoldBow);
            item.damage = 10;
			item.knockBack = 5;
			item.crit = 4;
            item.useTime = item.useAnimation = 16;
			item.shoot = ModContent.ProjectileType<Bone_Bolt>();
            item.shootSpeed = 9;
            item.width = 24;
            item.height = 56;
            item.autoReuse = false;
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-8f,0);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(type == ProjectileID.WoodenArrowFriendly)type = item.shoot;
            return true;
        }
    }
    public class Bone_Bolt : ModProjectile {
        public override string Texture => "Terraria/Projectile_117";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bone Bolt");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.timeLeft = 45;
        }
        public override void Kill(int timeLeft) {
            int t = ModContent.ProjectileType<Bone_Shard>();
            Projectile.NewProjectile(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner);
            Projectile.NewProjectile(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner);
            Projectile.NewProjectile(projectile.Center, projectile.velocity.RotatedByRandom(0.3f), t, projectile.damage/5, 2, projectile.owner);
        }
    }
    public class Bone_Shard : ModProjectile {
        public override string Texture => "Terraria/Projectile_21";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bone Shard");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
        }
    }
}
