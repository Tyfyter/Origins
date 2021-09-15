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
    public class Bolter : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bolt Gun");
            Tooltip.SetDefault("Get boned");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Handgun);
            item.damage = 26;
			item.knockBack = 5;
			item.crit = 4;
            item.useTime = item.useAnimation = 17;
			item.shoot = ProjectileID.Bullet;
            item.shootSpeed = 8;
            item.width = 38;
            item.height = 18;
            item.autoReuse = true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Projectile p = Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
            if(p.penetrate>0) {
                p.penetrate++;
                p.localNPCHitCooldown = 10;
                p.usesLocalNPCImmunity = true;
            }
            return false;
        }
    }
}
